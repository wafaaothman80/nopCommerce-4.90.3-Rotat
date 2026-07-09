// ***	 ** ****** ****** ****** ******* **     ** ****** ***   ** **** ****
// ****  ** **  ** **  ** **  **  **  **  **   **  **  ** ****  ** *    *  
// ** ** ** **  ** ****** ******  **  **   ** **   ****** ** ** ** *    ***
// **  **** **  ** **	  **  **  **  **    ***    **  ** **  **** *    *  
// **   *** ****** **	  **  ** *******     *     **  ** **   *** **** ****
// ***************************************************************************
// *                                                                         *
// *    NopCommerce Public RESTful API Plugin by NopAdvance team             *
// *    Copyright (c) NopAdvance LLP. All Rights Reserved.                   *
// *                                                                         *
// ***************************************************************************
// *                                                                         *
// *    This software is licensed for use under the terms accepted during    *
// *    the purchase of this product. A non-exclusive, non-transferable      *
// *    right is granted to use this product on the website for which it was *
// *    licensed.                                                            *
// *                                                                         *
// *    Companies purchasing this product for their customers are permitted, *
// *    provided the use complies with the terms outlined in the EULA:       *
// *    https://store.nopadvance.com/eula.                                   *
// *                                                                         *
// *    You may not reverse engineer, decompile, modify, or distribute this  *
// *    software without explicit permission from NopAdvance LLP. Any        *
// *    violation will result in the termination of your license and may     *
// *    lead to legal action.                                                *
// *                                                                         *
// ***************************************************************************
// *    Contact: contact@nopadvance.com                                      *
// *    Website: https://nopadvance.com                                      *
// ***************************************************************************
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Pickup;
using Nop.Services.Tax;
using Nop.Web.Models.Checkout;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses.Payments;

namespace NopAdvance.Plugin.Misc.PublicAPI.Factories;

public partial class APICommonModelFactory : IAPICommonModelFactory
{
    #region Fields

    private readonly LocalizationSettings _localizationSettings;
    private readonly ShippingSettings _shippingSettings;
    private readonly ILanguageService _languageService;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    private readonly IWebHelper _webHelper;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly IPickupPluginManager _pickupPluginManager;
    private readonly IShippingService _shippingService;
    private readonly ICountryService _countryService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly ITaxService _taxService;
    private readonly ICurrencyService _currencyService;
    private readonly IOrderTotalCalculationService _orderTotalCalculationService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IShippingPluginManager _shippingPluginManager;
    private readonly IAddressService _addressService;

    #endregion

    #region Ctor

    public APICommonModelFactory(LocalizationSettings localizationSettings,
        ShippingSettings shippingSettings,
        ILanguageService languageService,
        IStoreContext storeContext,
        IWorkContext workContext,
        IWebHelper webHelper,
        ILocalizationService localizationService,
        ISettingService settingService,
        IPickupPluginManager pickupPluginManager,
        IShippingService shippingService,
        ICountryService countryService,
        IStateProvinceService stateProvinceService,
        IShoppingCartService shoppingCartService,
        ITaxService taxService,
        ICurrencyService currencyService,
        IOrderTotalCalculationService orderTotalCalculationService,
        IPriceFormatter priceFormatter,
        IShippingPluginManager shippingPluginManager,
        IAddressService addressService)
    {
        _localizationSettings = localizationSettings;
        _shippingSettings = shippingSettings;
        _languageService = languageService;
        _storeContext = storeContext;
        _workContext = workContext;
        _webHelper = webHelper;
        _localizationService = localizationService;
        _settingService = settingService;
        _pickupPluginManager = pickupPluginManager;
        _shippingService = shippingService;
        _countryService = countryService;
        _stateProvinceService = stateProvinceService;
        _shoppingCartService = shoppingCartService;
        _taxService = taxService;
        _currencyService = currencyService;
        _orderTotalCalculationService = orderTotalCalculationService;
        _priceFormatter = priceFormatter;
        _shippingPluginManager = shippingPluginManager;
        _addressService = addressService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare the language selector model
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the language selector model
    /// </returns>
    public virtual async Task<LanguageResponse> PrepareLanguageSelectorModelAsync()
    {
        var useImages = _localizationSettings.UseImagesForLanguageSelection;
        var storeLocation = "";
        if (useImages)
            storeLocation = _webHelper.GetStoreLocation() + "images/flags/";

        var availableLanguages = (await _languageService
                .GetAllLanguagesAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id))
                .Select(x => new LanguageResponseModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    FlagImageFileName = useImages ? storeLocation + x.FlagImageFileName : x.FlagImageFileName,
                    UniqueSeoCode = x.UniqueSeoCode,
                }).ToList();

        var model = new LanguageResponse
        {
            CurrentLanguageId = (await _workContext.GetWorkingLanguageAsync()).Id,
            AvailableLanguages = availableLanguages,
            UseImages = useImages
        };

        return model;
    }

    /// <summary>
    /// Prepare the authorize net response
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the authorize net response
    /// </returns>
    public virtual Task<AuthorizeNetResponse> PrepareAuthorizeNetResponseAsync()
    {
        var model = new AuthorizeNetResponse
        {
            PaymentMethodName = PaymentMethodDefaults.AUTHORIZE_NET,
            PaymentMethodType = PaymentMethodType.Standard
        };

        //years
        for (var i = 0; i < 15; i++)
        {
            var year = Convert.ToString(DateTime.Now.Year + i);
            model.ExpireYears.Add(new SelectListItem
            {
                Text = year,
                Value = year
            });
        }

        //months
        for (var i = 1; i <= 12; i++)
        {
            var text = i < 10 ? "0" + i : i.ToString();
            model.ExpireMonths.Add(new SelectListItem
            {
                Text = text,
                Value = i.ToString()
            });
        }

        return Task.FromResult(model);
    }

    /// <summary>
    /// Prepare the manual response
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the manual response
    /// </returns>
    public virtual Task<ManualResponse> PrepareManualResponseAsync()
    {
        var model = new ManualResponse()
        {
            CreditCardTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Visa", Value = "visa" },
                new SelectListItem { Text = "Master card", Value = "MasterCard" },
                new SelectListItem { Text = "Discover", Value = "Discover" },
                new SelectListItem { Text = "Amex", Value = "Amex" },
            },
            PaymentMethodName = PaymentMethodDefaults.MANUAL,
            PaymentMethodType = PaymentMethodType.Standard
        };

        //years
        for (var i = 0; i < 15; i++)
        {
            var year = (DateTime.Now.Year + i).ToString();
            model.ExpireYears.Add(new SelectListItem { Text = year, Value = year, });
        }

        //months
        for (var i = 1; i <= 12; i++)
            model.ExpireMonths.Add(new SelectListItem { Text = i.ToString("D2"), Value = i.ToString(), });

        return Task.FromResult(model);
    }

    /// <summary>
    /// Prepare the check money order response
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the check money order response
    /// </returns>
    public virtual async Task<CheckMoneyOrderResponse> PrepareCheckMoneyOrderResponseAsync()
    {
        var key = "checkmoneyorderpaymentsettings.descriptiontext";
        var descriptionText = string.Empty;

        var setting = await _settingService.GetSettingAsync(key, storeId: (await _storeContext.GetCurrentStoreAsync()).Id, loadSharedValueIfNotFound: true);
        if (setting != null)
            descriptionText = await _localizationService.GetLocalizedAsync(setting, x => x.Value, (await _workContext.GetWorkingLanguageAsync()).Id);

        var model = new CheckMoneyOrderResponse
        {
            PaymentMethodName = PaymentMethodDefaults.CHECK_MONEY_ORDER,
            PaymentMethodType = PaymentMethodType.Standard,
            DescriptionText = descriptionText
        };

        return model;
    }

    /// <summary>
    /// Prepare the brain tree response
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the brain tree response
    /// </returns>
    public virtual Task<BrainTreeResponse> PrepareBrainTreeResponseAsync()
    {
        var model = new BrainTreeResponse
        {
            PaymentMethodName = PaymentMethodDefaults.BRAIN_TREE,
            PaymentMethodType = PaymentMethodType.Standard
        };
        for (var i = 0; i < 15; i++)
        {
            var year = Convert.ToString(DateTime.Now.Year + i);
            model.ExpireYears.Add(new SelectListItem { Text = year, Value = year, });
        }

        for (var i = 1; i <= 12; i++)
        {
            var text = i < 10 ? "0" + i : i.ToString();
            model.ExpireMonths.Add(new SelectListItem { Text = text, Value = i.ToString(), });
        }

        return Task.FromResult(model);
    }

    /// <summary>
    /// Prepare the purchase order response
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the purchase order response
    /// </returns>
    public virtual Task<PurchaseOrderResponse> PreparePurchaseOrderResponseAsync()
    {
        return Task.FromResult(new PurchaseOrderResponse
        {
            PaymentMethodName = PaymentMethodDefaults.PURCHASE_ORDER,
            PaymentMethodType = PaymentMethodType.Standard
        });
    }

    /// <summary>
    /// Prepares the checkout pickup points model
    /// </summary>
    /// <param name="cart">Cart</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the checkout pickup points model
    /// </returns>
    public virtual async Task<CheckoutPickupPointsModel> PrepareCheckoutPickupPointsModelAsync(IList<ShoppingCartItem> cart)
    {
        var model = new CheckoutPickupPointsModel
        {
            AllowPickupInStore = _shippingSettings.AllowPickupInStore
        };

        if (!model.AllowPickupInStore)
            return model;

        model.DisplayPickupPointsOnMap = _shippingSettings.DisplayPickupPointsOnMap;
        model.GoogleMapsApiKey = _shippingSettings.GoogleMapsApiKey;
        var pickupPointProviders = await _pickupPluginManager.LoadActivePluginsAsync(await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);
        if (pickupPointProviders.Any())
        {
            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
            var customer = await _workContext.GetCurrentCustomerAsync();
            var address = customer.BillingAddressId.HasValue
                ? await _addressService.GetAddressByIdAsync(customer.BillingAddressId.Value)
                : null;
            var pickupPointsResponse = await _shippingService.GetPickupPointsAsync(cart,
                address, customer, storeId: (await _storeContext.GetCurrentStoreAsync()).Id);
            if (pickupPointsResponse.Success)
                model.PickupPoints = await pickupPointsResponse.PickupPoints.SelectAwait(async point =>
                {
                    var country = await _countryService.GetCountryByTwoLetterIsoCodeAsync(point.CountryCode);
                    var state = await _stateProvinceService.GetStateProvinceByAbbreviationAsync(point.StateAbbreviation, country?.Id);

                    var pickupPointModel = new CheckoutPickupPointModel
                    {
                        Id = point.Id,
                        Name = point.Name,
                        Description = point.Description,
                        ProviderSystemName = point.ProviderSystemName,
                        Address = point.Address,
                        City = point.City,
                        County = point.County,
                        StateName = state != null ? await _localizationService.GetLocalizedAsync(state, x => x.Name, languageId) : string.Empty,
                        CountryName = country != null ? await _localizationService.GetLocalizedAsync(country, x => x.Name, languageId) : string.Empty,
                        ZipPostalCode = point.ZipPostalCode,
                        Latitude = point.Latitude,
                        Longitude = point.Longitude,
                        OpeningHours = point.OpeningHours
                    };

                    var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
                    var amount = await _orderTotalCalculationService.IsFreeShippingAsync(cart) ? 0 : point.PickupFee;

                    if (amount > 0)
                    {
                        (amount, _) = await _taxService.GetShippingPriceAsync(amount, await _workContext.GetCurrentCustomerAsync());
                        amount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(amount, await _workContext.GetWorkingCurrencyAsync());
                        pickupPointModel.PickupFee = await _priceFormatter.FormatShippingPriceAsync(amount, true);
                    }

                    //adjust rate
                    var (shippingTotal, _) = await _orderTotalCalculationService.AdjustShippingRateAsync(point.PickupFee, cart, true);
                    var (rateBase, _) = await _taxService.GetShippingPriceAsync(shippingTotal, await _workContext.GetCurrentCustomerAsync());
                    var rate = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(rateBase, await _workContext.GetWorkingCurrencyAsync());
                    pickupPointModel.PickupFee = await _priceFormatter.FormatShippingPriceAsync(rate, true);

                    return pickupPointModel;
                }).ToListAsync();
            else
                foreach (var error in pickupPointsResponse.Errors)
                    model.Warnings.Add(error);
        }

        //only available pickup points
        var shippingProviders = await _shippingPluginManager.LoadActivePluginsAsync(await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);
        if (!shippingProviders.Any())
        {
            if (!pickupPointProviders.Any())
            {
                model.Warnings.Add(await _localizationService.GetResourceAsync("Checkout.ShippingIsNotAllowed"));
                model.Warnings.Add(await _localizationService.GetResourceAsync("Checkout.PickupPoints.NotAvailable"));
            }
            model.PickupInStoreOnly = true;
            model.PickupInStore = true;
            return model;
        }

        return model;
    }

    #endregion
}
