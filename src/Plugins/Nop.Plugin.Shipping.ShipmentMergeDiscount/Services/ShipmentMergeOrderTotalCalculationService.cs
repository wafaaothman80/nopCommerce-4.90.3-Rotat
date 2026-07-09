using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Orders;

namespace Nop.Plugin.Shipping.ShipmentMergeDiscount.Services;

/// <summary>
/// Decorator for <see cref="IOrderTotalCalculationService"/> that injects a shipping
/// consolidation discount into the checkout pipeline.
///
/// NopCommerce checkout calls <see cref="AdjustShippingRateAsync"/> to apply discounts to
/// a shipping rate before displaying it in the order summary. That is the primary
/// interception point. The two <c>GetShoppingCartShippingTotal*</c> overloads are also
/// intercepted so the mini-cart and shopping-cart page show the correct reduced total.
///
/// All other methods are forwarded unchanged to the inner implementation.
/// </summary>
public class ShipmentMergeOrderTotalCalculationService : IOrderTotalCalculationService
{
    #region Fields

    private readonly IOrderTotalCalculationService _inner;
    private readonly IShipmentMergeDiscountService _mergeDiscountService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly IAddressService _addressService;
    private readonly IGenericAttributeService _genericAttributeService;

    #endregion

    #region Ctor

    public ShipmentMergeOrderTotalCalculationService(
        IOrderTotalCalculationService inner,
        IShipmentMergeDiscountService mergeDiscountService,
        IWorkContext workContext,
        IStoreContext storeContext,
        ISettingService settingService,
        IAddressService addressService,
        IGenericAttributeService genericAttributeService)
    {
        _inner = inner;
        _mergeDiscountService = mergeDiscountService;
        _workContext = workContext;
        _storeContext = storeContext;
        _settingService = settingService;
        _addressService = addressService;
        _genericAttributeService = genericAttributeService;
    }

    #endregion

    #region Utilities

    private async Task<Nop.Core.Domain.Common.Address?> ResolveShippingAddressAsync(Customer customer)
    {
        if (customer.ShippingAddressId == null)
            return null;
        return await _addressService.GetAddressByIdAsync(customer.ShippingAddressId.Value);
    }

    /// <summary>
    /// CALCULATE + STORE path (used only by <see cref="AdjustShippingRateAsync"/>).
    /// Runs the full merge-discount calculation, persists the result, reduces the rate,
    /// and appends a synthetic <see cref="Discount"/> for checkout display.
    /// </summary>
    private async Task<decimal> ApplyMergeDiscountAsync(
        decimal shippingRate,
        IList<ShoppingCartItem> cart,
        List<Discount> appliedDiscounts)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;

        var settings = await _settingService.LoadSettingAsync<ShipmentMergeDiscountSettings>(storeId);
        if (!settings.Enabled)
            return shippingRate;

        var shippingAddress = await ResolveShippingAddressAsync(customer);

        var discountAmount = await _mergeDiscountService.CalculateMergeDiscountAsync(
            customer, shippingAddress, cart, shippingRate, storeId);

        if (discountAmount <= 0)
            return shippingRate;

        appliedDiscounts.Add(new Discount
        {
            Name = ShipmentMergeDiscountDefaults.DiscountName,
            DiscountType = DiscountType.AssignedToShipping,
            UsePercentage = false,
            DiscountAmount = discountAmount,
            IsCumulative = false,
            RequiresCouponCode = false
        });

        return Math.Max(0m, shippingRate - discountAmount);
    }

    /// <summary>
    /// READ path — returns the discount already calculated and stored by
    /// <see cref="ApplyMergeDiscountAsync"/> without re-running the calculation.
    /// Used by total/shipping-total display methods to stay consistent with the rate
    /// shown in the shipping-method selector.
    /// </summary>
    private async Task<decimal> GetStoredDiscountAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
        return await _genericAttributeService.GetAttributeAsync<decimal>(
            customer,
            ShipmentMergeDiscountDefaults.CustomerMergeDiscountAmountAttribute,
            storeId);
    }

    #endregion

    #region IOrderTotalCalculationService — intercepted methods

    /// <summary>
    /// Calculates and stores the merge discount without reducing the displayed rate.
    /// The original carrier rate is returned so the shipping-method selector shows
    /// the full price. The discount is surfaced via the widget component and deducted
    /// from the order total in <see cref="GetShoppingCartTotalAsync"/>.
    ///
    /// IMPORTANT: NopCommerce (and ROPC) call this method once for EVERY available
    /// shipping option when enumerating methods. We only store the discount when the
    /// rate being evaluated matches the customer's currently-selected option, so that
    /// a non-selected option's lower rate cannot overwrite the stored value.
    /// </summary>
    public async Task<(decimal adjustedShippingRate, List<Discount> appliedDiscounts)>
        AdjustShippingRateAsync(decimal shippingRate, IList<ShoppingCartItem> cart,
            bool applyToPickupInStore = false)
    {
        // Let core pipeline apply free-shipping, other discounts, etc. first.
        var (adjustedRate, appliedDiscounts) =
            await _inner.AdjustShippingRateAsync(shippingRate, cart, applyToPickupInStore);

        if (applyToPickupInStore || adjustedRate <= 0)
            return (adjustedRate, appliedDiscounts);

        appliedDiscounts ??= new List<Discount>();

        // Only run the merge-discount calculation (which stores the result) when this
        // rate corresponds to the customer's selected shipping option. Calls for other
        // options are silently skipped so they cannot corrupt the stored value.
        var customer = await _workContext.GetCurrentCustomerAsync();
        var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
        var selectedOption = await _genericAttributeService
            .GetAttributeAsync<ShippingOption>(
                customer, NopCustomerDefaults.SelectedShippingOptionAttribute, storeId);

        bool isSelectedRate = selectedOption == null                               // nothing selected yet — allow first call through
                              || Math.Abs(adjustedRate - selectedOption.Rate) < 0.01m; // rate matches selected option

        if (!isSelectedRate)
            return (adjustedRate, appliedDiscounts);

        await ApplyMergeDiscountAsync(adjustedRate, cart, appliedDiscounts);

        return (adjustedRate, appliedDiscounts);
    }

    /// <summary>
    /// Returns the ORIGINAL (undiscounted) shipping total so the order-summary
    /// shipping line shows the full carrier rate. The merge discount is shown
    /// separately by the widget, and the Order Total is reduced via
    /// <see cref="GetShoppingCartTotalAsync"/>.
    /// </summary>
    public Task<(decimal? shippingTotal, decimal taxRate, List<Discount> appliedDiscounts)>
        GetShoppingCartShippingTotalAsync(IList<ShoppingCartItem> cart, bool includingTax)
        => _inner.GetShoppingCartShippingTotalAsync(cart, includingTax);

    /// <summary>No-param overload — also returns the original rate unchanged.</summary>
    public Task<decimal?> GetShoppingCartShippingTotalAsync(IList<ShoppingCartItem> cart)
        => _inner.GetShoppingCartShippingTotalAsync(cart);

    /// <summary>Both-tax overload — also returns original rates unchanged.</summary>
    public Task<(decimal? shippingTotalInclTax, decimal? shippingTotaExclTax, decimal taxRate, List<Discount> appliedDiscounts)>
        GetShoppingCartShippingTotalsAsync(IList<ShoppingCartItem> cart)
        => _inner.GetShoppingCartShippingTotalsAsync(cart);

    #endregion

    #region IOrderTotalCalculationService — delegated methods

    public Task<(decimal discountAmount, List<Discount> appliedDiscounts, decimal subTotalWithoutDiscount, decimal subTotalWithDiscount, SortedDictionary<decimal, decimal> taxRates)>
        GetShoppingCartSubTotalAsync(IList<ShoppingCartItem> cart, bool includingTax)
        => _inner.GetShoppingCartSubTotalAsync(cart, includingTax);

    public Task<(decimal discountAmountInclTax, decimal discountAmountExclTax, List<Discount> appliedDiscounts, decimal subTotalWithoutDiscountInclTax, decimal subTotalWithoutDiscountExclTax, decimal subTotalWithDiscountInclTax, decimal subTotalWithDiscountExclTax, SortedDictionary<decimal, decimal> taxRates)>
        GetShoppingCartSubTotalsAsync(IList<ShoppingCartItem> cart)
        => _inner.GetShoppingCartSubTotalsAsync(cart);

    public Task<bool> IsFreeShippingAsync(IList<ShoppingCartItem> cart, decimal? subTotal = null)
        => _inner.IsFreeShippingAsync(cart, subTotal);

    public Task<(decimal taxTotal, SortedDictionary<decimal, decimal> taxRates)>
        GetTaxTotalAsync(IList<ShoppingCartItem> cart, bool usePaymentMethodAdditionalFee = true)
        => _inner.GetTaxTotalAsync(cart, usePaymentMethodAdditionalFee);

    public async Task<(decimal? shoppingCartTotal, decimal discountAmount, List<Discount> appliedDiscounts, List<AppliedGiftCard> appliedGiftCards, int redeemedRewardPoints, decimal redeemedRewardPointsAmount)>
        GetShoppingCartTotalAsync(IList<ShoppingCartItem> cart, bool? useRewardPoints = null, bool usePaymentMethodAdditionalFee = true)
    {
        var (total, discountAmount, appliedDiscounts, giftCards, rewardPoints, rewardPointsAmount) =
            await _inner.GetShoppingCartTotalAsync(cart, useRewardPoints, usePaymentMethodAdditionalFee);

        if (!total.HasValue)
            return (total, discountAmount, appliedDiscounts, giftCards, rewardPoints, rewardPointsAmount);

        var customer = await _workContext.GetCurrentCustomerAsync();
        var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
        var settings = await _settingService.LoadSettingAsync<ShipmentMergeDiscountSettings>(storeId);

        if (!settings.Enabled)
            return (total, discountAmount, appliedDiscounts, giftCards, rewardPoints, rewardPointsAmount);

        // Always re-evaluate eligibility with the CURRENT settings so that changes to
        // MinimumEligibleOrders take effect immediately without a full checkout restart.
        var shippingAddress = await ResolveShippingAddressAsync(customer);
        var isEligible = await _mergeDiscountService.IsEligibleForMergeDiscountAsync(
            customer, shippingAddress, storeId);

        if (!isEligible)
        {
            // Clear both stored attributes so the widget shows nothing.
            await _genericAttributeService.SaveAttributeAsync<decimal>(
                customer, ShipmentMergeDiscountDefaults.CustomerMergeDiscountAmountAttribute, 0m, storeId);
            await _genericAttributeService.SaveAttributeAsync<decimal>(
                customer, ShipmentMergeDiscountDefaults.CustomerMergeDiscountDisplayAttribute, 0m, storeId);
            return (total, discountAmount, appliedDiscounts, giftCards, rewardPoints, rewardPointsAmount);
        }

        // Get the actual shipping total to compute a fresh configured-minimum discount.
        var shippingBase = await _inner.GetShoppingCartShippingTotalAsync(cart);
        if (!shippingBase.HasValue || shippingBase.Value <= 0)
            return (total, discountAmount, appliedDiscounts, giftCards, rewardPoints, rewardPointsAmount);

        // Configured percentage/amount is the guaranteed minimum discount.
        decimal configuredMin = settings.UsePercentage
            ? shippingBase.Value * Math.Clamp(settings.DiscountPercentage, 0m, 100m) / 100m
            : settings.DiscountAmount;

        // The stored value may be larger (formula gave a better saving for the customer).
        var stored = await GetStoredDiscountAsync();
        decimal mergeDiscount = Math.Max(configuredMin, stored);

        // Apply optional absolute cap and clamp to shipping cost.
        if (settings.MaxDiscountAmount > 0 && mergeDiscount > settings.MaxDiscountAmount)
            mergeDiscount = settings.MaxDiscountAmount;
        mergeDiscount = Math.Clamp(mergeDiscount, 0m, shippingBase.Value);

        if (mergeDiscount <= 0)
            return (total, discountAmount, appliedDiscounts, giftCards, rewardPoints, rewardPointsAmount);

        // Persist the discount and the shipping rate used — both written only here so ROPC's
        // AdjustShippingRateAsync calls (one per shipping option) cannot corrupt them.
        await _genericAttributeService.SaveAttributeAsync(
            customer, ShipmentMergeDiscountDefaults.CustomerMergeDiscountAmountAttribute, mergeDiscount, storeId);
        await _genericAttributeService.SaveAttributeAsync(
            customer, ShipmentMergeDiscountDefaults.CustomerMergeDiscountDisplayAttribute, mergeDiscount, storeId);
        await _genericAttributeService.SaveAttributeAsync(
            customer, ShipmentMergeDiscountDefaults.CustomerSelectedShippingRateAttribute, shippingBase.Value, storeId);

        return (Math.Max(0m, total.Value - mergeDiscount), discountAmount, appliedDiscounts, giftCards, rewardPoints, rewardPointsAmount);
    }

    public Task<decimal> CalculatePaymentAdditionalFeeAsync(IList<ShoppingCartItem> cart, decimal fee, bool usePercentage)
        => _inner.CalculatePaymentAdditionalFeeAsync(cart, fee, usePercentage);

    public Task UpdateOrderTotalsAsync(UpdateOrderParameters updateOrderParameters, IList<ShoppingCartItem> restoredCart)
        => _inner.UpdateOrderTotalsAsync(updateOrderParameters, restoredCart);

    public Task<decimal> ConvertRewardPointsToAmountAsync(int rewardPoints)
        => _inner.ConvertRewardPointsToAmountAsync(rewardPoints);

    public bool CheckMinimumRewardPointsToUseRequirement(int rewardPoints)
        => _inner.CheckMinimumRewardPointsToUseRequirement(rewardPoints);

    public decimal CalculateApplicableOrderTotalForRewardPoints(decimal orderShippingInclTax, decimal orderTotal)
        => _inner.CalculateApplicableOrderTotalForRewardPoints(orderShippingInclTax, orderTotal);

    public Task<int> CalculateRewardPointsAsync(Customer customer, decimal amount)
        => _inner.CalculateRewardPointsAsync(customer, amount);

    #endregion
}
