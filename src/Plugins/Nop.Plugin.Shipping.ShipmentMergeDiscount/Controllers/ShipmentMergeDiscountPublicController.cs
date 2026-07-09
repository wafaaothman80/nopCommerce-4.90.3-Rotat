using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Plugin.Shipping.ShipmentMergeDiscount.Services;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Shipping.ShipmentMergeDiscount.Controllers;

public class ShipmentMergeDiscountPublicController : BasePluginController
{
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly ICurrencyService _currencyService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IShipmentMergeDiscountService _mergeDiscountService;
    private readonly ISettingService _settingService;
    private readonly IAddressService _addressService;
    public ShipmentMergeDiscountPublicController(
        IGenericAttributeService genericAttributeService,
        IWorkContext workContext,
        IStoreContext storeContext,
        ICurrencyService currencyService,
        IPriceFormatter priceFormatter,
        IShipmentMergeDiscountService mergeDiscountService,
        ISettingService settingService,
        IAddressService addressService)
    {
        _genericAttributeService = genericAttributeService;
        _workContext = workContext;
        _storeContext = storeContext;
        _currencyService = currencyService;
        _priceFormatter = priceFormatter;
        _mergeDiscountService = mergeDiscountService;
        _settingService = settingService;
        _addressService = addressService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAmount()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var storeId = store.Id;

        var settings = await _settingService.LoadSettingAsync<ShipmentMergeDiscountSettings>(storeId);

        if (!settings.Enabled)
            return Json(new { amount = (string?)null });

        Nop.Core.Domain.Common.Address? shippingAddress = null;
        if (customer.ShippingAddressId.HasValue)
            shippingAddress = await _addressService.GetAddressByIdAsync(customer.ShippingAddressId.Value);

        var isEligible = await _mergeDiscountService.IsEligibleForMergeDiscountAsync(
            customer, shippingAddress, storeId);

        if (!isEligible)
            return Json(new { amount = (string?)null });

        // Use the shipping rate stored by GetShoppingCartTotalAsync — this is written only
        // during total calculation and reflects the actual rate used for the discount.
        // Falls back to SelectedShippingOptionAttribute if the stored rate is not yet set
        // (e.g. very first page load before any total calculation has run).
        var shippingRate = await _genericAttributeService.GetAttributeAsync<decimal>(
            customer, ShipmentMergeDiscountDefaults.CustomerSelectedShippingRateAttribute, storeId);

        if (shippingRate <= 0)
        {
            var selectedOption = await _genericAttributeService
                .GetAttributeAsync<Nop.Core.Domain.Shipping.ShippingOption>(
                    customer, NopCustomerDefaults.SelectedShippingOptionAttribute, storeId);
            shippingRate = selectedOption?.Rate ?? 0m;
        }

        if (shippingRate <= 0)
            return Json(new { amount = (string?)null });

        decimal discountBase = settings.UsePercentage
            ? shippingRate * Math.Clamp(settings.DiscountPercentage, 0m, 100m) / 100m
            : settings.DiscountAmount;

        if (settings.MaxDiscountAmount > 0 && discountBase > settings.MaxDiscountAmount)
            discountBase = settings.MaxDiscountAmount;

        discountBase = Math.Min(discountBase, shippingRate);

        if (discountBase <= 0)
            return Json(new { amount = (string?)null });

        var currency = await _workContext.GetWorkingCurrencyAsync();
        var converted = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(discountBase, currency);
        var formatted = await _priceFormatter.FormatShippingPriceAsync(converted, true);

        return Json(new { amount = formatted });
    }

    /// <summary>
    /// Debug endpoint — returns how many orders currently qualify for the merge discount.
    /// Visit /shipmentmergediscount/eligiblecount to see the count.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> EligibleCount()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var storeId = store.Id;

        var settings = await _settingService.LoadSettingAsync<ShipmentMergeDiscountSettings>(storeId);

        Nop.Core.Domain.Common.Address? shippingAddress = null;
        if (customer.ShippingAddressId.HasValue)
            shippingAddress = await _addressService.GetAddressByIdAsync(customer.ShippingAddressId.Value);

        var eligibleCount = await _mergeDiscountService.GetEligibleOrderCountAsync(
            customer, shippingAddress, storeId);

        return Json(new
        {
            customerId = customer.Id,
            shippingAddressId = customer.ShippingAddressId,
            minimumRequired = settings.MinimumEligibleOrders,
            strictAddressMatch = settings.StrictAddressMatch,
            eligibleOrdersFound = eligibleCount,
            isEligible = eligibleCount >= settings.MinimumEligibleOrders,
            suggestion = settings.StrictAddressMatch
                ? "Strict matching is ON — only exact address matches count"
                : "Strict matching is OFF — any order with same City+Zip+Country counts. Enable 'Require strict address match' in plugin settings to be more precise."
        });
    }
}
