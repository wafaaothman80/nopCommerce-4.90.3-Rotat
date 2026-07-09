using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Services.Shipping;

namespace Nop.Plugin.Shipping.ShipmentMergeDiscount.Services;

/// <summary>
/// Default implementation of <see cref="IShipmentMergeDiscountService"/>.
/// </summary>
public class ShipmentMergeDiscountService : IShipmentMergeDiscountService
{
    #region Fields

    private readonly IAddressService _addressService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly ISettingService _settingService;
    private readonly IShippingService _shippingService;

    #endregion

    #region Ctor

    public ShipmentMergeDiscountService(
        IAddressService addressService,
        IGenericAttributeService genericAttributeService,
        IOrderService orderService,
        IProductService productService,
        ISettingService settingService,
        IShippingService shippingService)
    {
        _addressService = addressService;
        _genericAttributeService = genericAttributeService;
        _orderService = orderService;
        _productService = productService;
        _settingService = settingService;
        _shippingService = shippingService;
    }

    #endregion

    #region Utilities

    private async Task<Address?> GetCustomerShippingAddressAsync(Customer customer)
    {
        if (customer.ShippingAddressId == null)
            return null;
        return await _addressService.GetAddressByIdAsync(customer.ShippingAddressId.Value);
    }

    /// <summary>Loose (city + postal + country) or strict (all fields) address comparison.</summary>
    private static bool AddressesMatch(Address a, Address b, bool strict)
    {
        if (strict)
        {
            return string.Equals(a.Address1, b.Address1, StringComparison.OrdinalIgnoreCase)
                && string.Equals(a.Address2, b.Address2, StringComparison.OrdinalIgnoreCase)
                && string.Equals(a.City, b.City, StringComparison.OrdinalIgnoreCase)
                && string.Equals(a.ZipPostalCode, b.ZipPostalCode, StringComparison.OrdinalIgnoreCase)
                && a.CountryId == b.CountryId
                && a.StateProvinceId == b.StateProvinceId;
        }
        return string.Equals(a.City, b.City, StringComparison.OrdinalIgnoreCase)
            && string.Equals(a.ZipPostalCode, b.ZipPostalCode, StringComparison.OrdinalIgnoreCase)
            && a.CountryId == b.CountryId;
    }

    /// <summary>
    /// Returns all pending/processing, not-yet-shipped orders for this customer whose
    /// shipping address matches <paramref name="targetAddress"/>.
    /// </summary>
    private async Task<List<Order>> GetEligibleOrdersAsync(
        Customer customer, Address targetAddress, int storeId,
        bool strictMatch)
    {
        var orders = await _orderService.SearchOrdersAsync(
            storeId: 0,          // 0 = search across all stores
            customerId: customer.Id,
            osIds: new List<int>
            {
                (int)OrderStatus.Pending,
                (int)OrderStatus.Processing
            },
            ssIds: new List<int>
            {
                (int)ShippingStatus.NotYetShipped,
                (int)ShippingStatus.PartiallyShipped
            });

        var result = new List<Order>();
        foreach (var order in orders)
        {
            if (order.ShippingAddressId == null)
                continue;

            var orderAddress = await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value);
            if (orderAddress != null && AddressesMatch(orderAddress, targetAddress, strictMatch))
                result.Add(order);
        }
        return result;
    }

    /// <summary>
    /// Attempts to calculate Shipping1and2 — the cost of shipping ALL items (current cart
    /// AND previous eligible orders) as a single consolidated shipment.
    ///
    /// Strategy: builds a combined list of <see cref="ShoppingCartItem"/>-backed package items
    /// and calls <see cref="IShippingService.GetShippingOptionsAsync"/> restricted to the
    /// customer's currently-selected shipping rate computation method. The option whose name
    /// matches the selected option is used.
    ///
    /// Returns null when no shipping option has been selected yet or no matching option
    /// is found in the response (falling back to percentage/fixed discount).
    /// </summary>
    private async Task<decimal?> TryGetCombinedShippingCostAsync(
        Customer customer,
        Address shippingAddress,
        IList<ShoppingCartItem> currentCart,
        List<Order> eligibleOrders,
        int storeId)
    {
        // Read the customer's currently-selected shipping option (serialised in generic attrs).
        var selectedOption = await _genericAttributeService.GetAttributeAsync<Nop.Core.Domain.Shipping.ShippingOption>(
            customer,
            NopCustomerDefaults.SelectedShippingOptionAttribute,
            storeId);

        if (selectedOption == null)
            return null;

        // ── Build a combined cart (current items + previous order items) ─────────────
        // We create lightweight ShoppingCartItem shells for previous-order products so we
        // can pass them through the normal shipping pipeline without touching the database.
        var combinedCart = new List<ShoppingCartItem>(currentCart);

        foreach (var order in eligibleOrders)
        {
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
            foreach (var item in orderItems)
            {
                // Minimal ShoppingCartItem — the shipping service only needs ProductId,
                // Quantity, and AttributesXml to compute item weight.
                combinedCart.Add(new ShoppingCartItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    AttributesXml = item.AttributesXml,
                    StoreId = storeId,
                    CustomerId = customer.Id,
                    ShoppingCartTypeId = (int)ShoppingCartType.ShoppingCart
                });
            }
        }

        // ── Ask the selected rate computation method for combined options ─────────────
        var response = await _shippingService.GetShippingOptionsAsync(
            combinedCart,
            shippingAddress,
            customer,
            allowedShippingRateComputationMethodSystemName:
                selectedOption.ShippingRateComputationMethodSystemName,
            storeId: storeId);

        if (!response.Success || response.ShippingOptions == null || !response.ShippingOptions.Any())
            return null;

        // Match by name — e.g. "Sea Shipping", "Standard", etc.
        var matched = response.ShippingOptions
            .FirstOrDefault(o => string.Equals(o.Name, selectedOption.Name,
                StringComparison.OrdinalIgnoreCase));

        return matched?.Rate;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public async Task<int> GetEligibleOrderCountAsync(Customer customer, Address? shippingAddress, int storeId)
    {
        var settings = await _settingService.LoadSettingAsync<ShipmentMergeDiscountSettings>(storeId);
        var targetAddress = shippingAddress ?? await GetCustomerShippingAddressAsync(customer);
        if (targetAddress == null)
            return 0;
        var eligible = await GetEligibleOrdersAsync(customer, targetAddress, storeId, settings.StrictAddressMatch);
        return eligible.Count;
    }

    /// <inheritdoc />
    public async Task<bool> IsEligibleForMergeDiscountAsync(
        Customer customer, Address? shippingAddress, int storeId)
    {
        var settings = await _settingService.LoadSettingAsync<ShipmentMergeDiscountSettings>(storeId);
        if (!settings.Enabled)
            return false;

        var targetAddress = shippingAddress ?? await GetCustomerShippingAddressAsync(customer);
        if (targetAddress == null)
            return false;

        var eligible = await GetEligibleOrdersAsync(
            customer, targetAddress, storeId, settings.StrictAddressMatch);

        return eligible.Count >= settings.MinimumEligibleOrders;
    }

    /// <inheritdoc />
    public async Task<decimal> CalculateMergeDiscountAsync(
        Customer customer,
        Address? shippingAddress,
        IList<ShoppingCartItem> currentCart,
        decimal currentShippingCost,
        int storeId)
    {
        if (currentShippingCost <= 0)
            return 0m;

        var settings = await _settingService.LoadSettingAsync<ShipmentMergeDiscountSettings>(storeId);
        if (!settings.Enabled)
            return 0m;

        var targetAddress = shippingAddress ?? await GetCustomerShippingAddressAsync(customer);
        if (targetAddress == null)
        {
            await ClearStoredDiscountAsync(customer, storeId);
            return 0m;
        }

        var eligibleOrders = await GetEligibleOrdersAsync(
            customer, targetAddress, storeId, settings.StrictAddressMatch);

        if (eligibleOrders.Count < settings.MinimumEligibleOrders)
        {
            await ClearStoredDiscountAsync(customer, storeId);
            return 0m;
        }

        // ── Formula: Discount = Shipping2 − (Shipping1and2 − Shipping1) ──────────────
        //
        // Shipping2     = currentShippingCost  (current cart shipped separately)
        // Shipping1     = sum of previous eligible orders' shipping costs already paid
        // Shipping1and2 = what the carrier charges for the combined shipment

        // Shipping1: sum of already-paid shipping on eligible orders (excl tax).
        var shipping1 = eligibleOrders.Sum(o => o.OrderShippingExclTax);

        decimal discountAmount;

        var shipping1and2 = await TryGetCombinedShippingCostAsync(
            customer, targetAddress, currentCart, eligibleOrders, storeId);

        if (shipping1and2.HasValue)
        {
            var combinedShippingCost = shipping1and2.Value;
            var previousOrdersShippingPaid = shipping1;
            var currentOrderShippingCost = currentShippingCost;

            var additionalShipping = Math.Max(
                0m,
                combinedShippingCost - previousOrdersShippingPaid);

            var shipmentMergeDiscount = currentOrderShippingCost - additionalShipping;

            shipmentMergeDiscount = Math.Max(0m, shipmentMergeDiscount);

            discountAmount = shipmentMergeDiscount;
        }
        else
        {
            // Fallback when combined rate is unavailable (option not yet selected, etc.).
            // Use the configured percentage or fixed amount.
            if (settings.UsePercentage)
            {
                var pct = Math.Clamp(settings.DiscountPercentage, 0m, 100m);
                discountAmount = currentShippingCost * pct / 100m;
            }
            else
            {
                discountAmount = settings.DiscountAmount;
            }
        }

        // The configured percentage/amount is the MINIMUM guaranteed discount.
        // If the formula gives more than configured, use the formula (larger saving for customer).
        // If the formula gives less than configured, use configured (merchant guarantee).
        decimal configuredMin;
        if (settings.UsePercentage)
            configuredMin = currentShippingCost * Math.Clamp(settings.DiscountPercentage, 0m, 100m) / 100m;
        else
            configuredMin = settings.DiscountAmount;

        if (configuredMin > 0)
            discountAmount = Math.Max(discountAmount, configuredMin);

        // Apply optional absolute cap.
        if (settings.MaxDiscountAmount > 0 && discountAmount > settings.MaxDiscountAmount)
            discountAmount = settings.MaxDiscountAmount;

        discountAmount = Math.Clamp(discountAmount, 0m, currentShippingCost);

        // Persist for reporting and order-creation pipeline.
        await _genericAttributeService.SaveAttributeAsync(
            customer,
            ShipmentMergeDiscountDefaults.CustomerMergeDiscountAmountAttribute,
            discountAmount,
            storeId);

        return discountAmount;
    }

    private async Task ClearStoredDiscountAsync(Customer customer, int storeId)
    {
        await _genericAttributeService.SaveAttributeAsync<decimal>(
            customer,
            ShipmentMergeDiscountDefaults.CustomerMergeDiscountAmountAttribute,
            0m,
            storeId);
    }

    #endregion
}
