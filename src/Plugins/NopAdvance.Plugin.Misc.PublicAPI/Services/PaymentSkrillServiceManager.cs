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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;

namespace NopAdvance.Plugin.Misc.PublicAPI.Services;

public partial class PaymentSkrillServiceManager
{
    #region Properties

    private static string QuickCheckoutWebhookRouteName => "Plugin.Payments.Skrill.QuickCheckoutWebhook";
    private static string ReferralId => "124956815";
    private static string QuickCheckoutServiceUrl => "https://pay.skrill.com";
    private static string CheckoutCompletedRouteName => "CheckoutCompleted";
    private static string OrderDetailsRouteName => "OrderDetails";
    private static string UserAgent => $"nopCommerce-{NopVersion.CURRENT_VERSION}";

    #endregion

    #region Fields

    private readonly SkrillSettings _settings;
    private readonly CurrencySettings _currencySettings;
    private readonly ILogger _logger;
    private readonly IWorkContext _workContext;
    private readonly ILocalizationService _localizationService;
    private readonly ICustomerService _customerService;
    private readonly ICurrencyService _currencyService;
    private readonly ICountryService _countryService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IWebHelper _webHelper;
    private readonly IProductService _productService;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IStoreContext _storeContext;
    private readonly IAddressService _addressService;
    private readonly IOrderService _orderService;
    private readonly ILanguageService _languageService;

    #endregion

    #region Ctor

    public PaymentSkrillServiceManager(SkrillSettings settings,
        CurrencySettings currencySettings,
        ILogger logger,
        IWorkContext workContext,
        ILocalizationService localizationService,
        ICustomerService customerService,
        ICurrencyService currencyService,
        ICountryService countryService,
        IStateProvinceService stateProvinceService,
        IUrlHelperFactory urlHelperFactory,
        IWebHelper webHelper,
        IProductService productService,
        IActionContextAccessor actionContextAccessor,
        IStoreContext storeContext,
        IAddressService addressService,
        IOrderService orderService,
        ILanguageService languageService)
    {
        _settings = settings;
        _currencySettings = currencySettings;
        _logger = logger;
        _workContext = workContext;
        _localizationService = localizationService;
        _customerService = customerService;
        _currencyService = currencyService;
        _countryService = countryService;
        _stateProvinceService = stateProvinceService;
        _urlHelperFactory = urlHelperFactory;
        _webHelper = webHelper;
        _productService = productService;
        _actionContextAccessor = actionContextAccessor;
        _storeContext = storeContext;
        _addressService = addressService;
        _orderService = orderService;
        _languageService = languageService;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Check whether the plugin is configured
    /// </summary>
    /// <returns>Result</returns>
    protected virtual bool IsConfigured()
    {
        //merchant email and secret word are required to request services
        return !string.IsNullOrEmpty(_settings.MerchantEmail) && !string.IsNullOrEmpty(_settings.SecretWord);
    }

    /// <summary>
    /// Handle function and get result
    /// </summary>
    /// <typeparam name="TResult">Result type</typeparam>
    /// <param name="function">Function</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result; error message if exists
    /// </returns>
    protected virtual async Task<(TResult Result, string ErrorMessage)> HandleFunctionAsync<TResult>(Func<TResult> function)
    {
        try
        {
            //ensure that plugin is configured
            if (!IsConfigured())
                throw new NopException("Plugin not configured");

            //invoke function
            return (function(), default);
        }
        catch (Exception exception)
        {
            //log errors
            var errorMessage = $"{PaymentMethodDefaults.SKRILL} error: {Environment.NewLine}{exception.Message}";
            await _logger.ErrorAsync(errorMessage, exception, await _workContext.GetCurrentCustomerAsync());

            return (default, errorMessage);
        }
    }

    /// <summary>
    /// Prepare parameters to request the Quick Checkout service
    /// </summary>
    /// <param name="order">Order</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains URL to request
    /// </returns>
    protected virtual async Task<string> PrepareSessionRequestUrlAsync(Order order)
    {
        if (order == null)
            throw new NopException("Order is not set");

        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
        if (customer == null)
            throw new NopException("Order customer is not set");

        var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
        if (billingAddress == null)
            throw new NopException("Order billing address is not set");

        var billingCountryThreeLetterIsoCode = string.Empty;
        if (billingAddress.CountryId.HasValue)
            billingCountryThreeLetterIsoCode = (await _countryService.GetCountryByIdAsync(billingAddress.CountryId.Value))?.ThreeLetterIsoCode;

        var billingStateProvinceName = string.Empty;
        if (billingAddress.StateProvinceId.HasValue)
            billingStateProvinceName = (await _stateProvinceService.GetStateProvinceByIdAsync(billingAddress.StateProvinceId.Value))?.Name;

        var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
        if (currency == null)
            throw new NopException("Primary store currency is not set");

        //prepare URLs
        var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
        var successUrl = urlHelper.RouteUrl(CheckoutCompletedRouteName, new { orderId = order.Id }, _webHelper.GetCurrentRequestProtocol());
        var failUrl = urlHelper.RouteUrl(OrderDetailsRouteName, new { orderId = order.Id }, _webHelper.GetCurrentRequestProtocol());
        var webhookUrl = urlHelper.RouteUrl(QuickCheckoutWebhookRouteName, null, _webHelper.GetCurrentRequestProtocol());

        //prepare some customer details
        var customerLanguage = await _languageService.GetLanguageByIdAsync(order.CustomerLanguageId) ?? await _workContext.GetWorkingLanguageAsync();
        var customerDateOfBirth = customer.DateOfBirth.HasValue ? customer.DateOfBirth?.ToString("ddMMyyyy") : string.Empty;

        //prepare some item details
        var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
        var item1 = orderItems.FirstOrDefault();
        var product1 = item1 != null ? await _productService.GetProductByIdAsync(item1.ProductId) : null;
        var detailDescription1 = "Product name:";
        var detailText1 = product1 != null ? await _localizationService.GetLocalizedAsync(product1, entity => entity.Name) : string.Empty;

        var item2 = orderItems.Skip(1).FirstOrDefault();
        var product2 = item2 != null ? await _productService.GetProductByIdAsync(item2.ProductId) : null;
        var detailDescription2 = product2 != null ? detailDescription1 : "Product description:";
        var detailText2 = product2 != null
            ? await _localizationService.GetLocalizedAsync(product2, entity => entity.Name)
            : product1 != null ? await _localizationService.GetLocalizedAsync(product1, entity => entity.ShortDescription) : string.Empty;

        var item3 = orderItems.Skip(2).FirstOrDefault();
        var product3 = item3 != null ? await _productService.GetProductByIdAsync(item3.ProductId) : null;
        var detailDescription3 = product3 != null ? detailDescription1 : product2 != null ? string.Empty : "Quantity:";
        var detailText3 = product3 != null
            ? await _localizationService.GetLocalizedAsync(product3, entity => entity.Name)
            : product2 != null
            ? string.Empty
            : item1?.Quantity.ToString();

        var item4 = orderItems.Skip(3).FirstOrDefault();
        var product4 = item4 != null ? await _productService.GetProductByIdAsync(item4.ProductId) : null;
        var detailDescription4 = product4 != null ? detailDescription1 : string.Empty;
        var detailText4 = product4 != null ? await _localizationService.GetLocalizedAsync(product4, entity => entity.Name) : string.Empty;

        var item5 = orderItems.Skip(4).FirstOrDefault();
        var product5 = item5 != null ? await _productService.GetProductByIdAsync(item5.ProductId) : null;
        var detailDescription5 = product5 != null ? detailDescription1 : string.Empty;
        var detailText5 = product5 != null ? await _localizationService.GetLocalizedAsync(product5, entity => entity.Name) : string.Empty;

        var store = await _storeContext.GetCurrentStoreAsync();
        //prepare URL to request
        var url = QueryHelpers.AddQueryString(QuickCheckoutServiceUrl, new Dictionary<string, string>
        {
            //merchant details
            ["pay_to_email"] = CommonHelper.EnsureMaximumLength(_settings.MerchantEmail, 50) ?? string.Empty,
            ["recipient_description"] = CommonHelper.EnsureMaximumLength(store.Name, 30) ?? string.Empty,
            ["transaction_id"] = order.OrderGuid.ToString(),
            ["return_url"] = CommonHelper.EnsureMaximumLength(successUrl, 240) ?? string.Empty,
            ["return_url_text"] = CommonHelper.EnsureMaximumLength($"Back to {store.Name}", 35) ?? string.Empty,
            //["return_url_target"] = "1", //default value
            ["cancel_url"] = CommonHelper.EnsureMaximumLength(failUrl, 240) ?? string.Empty,
            //["cancel_url_target"] = "1", //default value
            ["status_url"] = CommonHelper.EnsureMaximumLength(webhookUrl, 400) ?? string.Empty,
            //["status_url2"] = null, //single webhook handler is enough
            ["language"] = CommonHelper.EnsureMaximumLength(customerLanguage?.UniqueSeoCode ?? "EN", 2) ?? string.Empty,
            //["logo_url"] = null, //not used, only store name will be shown
            ["prepare_only"] = "1", //first, prepare the order details
            ["dynamic_descriptor "] = CommonHelper.EnsureMaximumLength(store.Name, 25) ?? string.Empty,
            //["sid"] = null, //used in the next request
            //["rid"] = CommonHelper.EnsureMaximumLength(Defaults.ReferralId, 100) ?? string.Empty, //according to Skrill managers "referral ID" should be passed in additional merchant fields, well ok
            //["ext_ref_id"] = CommonHelper.EnsureMaximumLength(Defaults.UserAgent, 100) ?? string.Empty, //according to Skrill managers "referral ID" should be passed in additional merchant fields, well ok
            ["merchant_fields"] = CommonHelper.EnsureMaximumLength("platform,platform_version", 240),
            ["platform"] = CommonHelper.EnsureMaximumLength(ReferralId, 240),
            ["platform_version"] = CommonHelper.EnsureMaximumLength(NopVersion.CURRENT_VERSION, 240),

            //customer details
            ["pay_from_email"] = CommonHelper.EnsureMaximumLength(billingAddress.Email, 100) ?? string.Empty,
            ["firstname"] = CommonHelper.EnsureMaximumLength(billingAddress.FirstName, 20) ?? string.Empty,
            ["lastname"] = CommonHelper.EnsureMaximumLength(billingAddress.LastName, 50) ?? string.Empty,
            ["date_of_birth"] = CommonHelper.EnsureMaximumLength(customerDateOfBirth, 8) ?? string.Empty,
            ["address"] = CommonHelper.EnsureMaximumLength(billingAddress.Address1, 100) ?? string.Empty,
            ["address2"] = CommonHelper.EnsureMaximumLength(billingAddress.Address2, 100) ?? string.Empty,
            ["phone_number"] = CommonHelper.EnsureMaximumLength(billingAddress.PhoneNumber, 20) ?? string.Empty,
            ["postal_code"] = CommonHelper.EnsureMaximumLength(billingAddress.ZipPostalCode, 9) ?? string.Empty,
            ["city"] = CommonHelper.EnsureMaximumLength(billingAddress.City, 50) ?? string.Empty,
            ["state"] = CommonHelper.EnsureMaximumLength(billingStateProvinceName, 50) ?? string.Empty,
            ["country"] = CommonHelper.EnsureMaximumLength(billingCountryThreeLetterIsoCode, 3) ?? string.Empty,
            //["neteller_account"] = null, //not used
            //["neteller_secure_id"] = null, // not used

            //payment details
            ["amount"] = CommonHelper.EnsureMaximumLength(order.OrderTotal.ToString("F").TrimEnd('0').TrimEnd('0').TrimEnd('.'), 19) ?? string.Empty,
            ["currency"] = CommonHelper.EnsureMaximumLength(currency?.CurrencyCode, 3) ?? string.Empty,
            ["amount2_description "] = CommonHelper.EnsureMaximumLength("Item total:", 240),
            ["amount2"] = CommonHelper.EnsureMaximumLength(order.OrderSubtotalExclTax.ToString("F").TrimEnd('0').TrimEnd('0').TrimEnd('.'), 19) ?? string.Empty,
            ["amount3_description "] = CommonHelper.EnsureMaximumLength("Shipping total:", 240) ?? string.Empty,
            ["amount3"] = CommonHelper.EnsureMaximumLength(order.OrderShippingExclTax.ToString("F").TrimEnd('0').TrimEnd('0').TrimEnd('.'), 19) ?? string.Empty,
            ["amount4_description "] = CommonHelper.EnsureMaximumLength("Tax total:", 240) ?? string.Empty,
            ["amount4"] = CommonHelper.EnsureMaximumLength(order.OrderTax.ToString("F").TrimEnd('0').TrimEnd('0').TrimEnd('.'), 19) ?? string.Empty,
            ["detail1_description"] = CommonHelper.EnsureMaximumLength(detailDescription1, 240) ?? string.Empty,
            ["detail1_text"] = CommonHelper.EnsureMaximumLength(detailText1, 240) ?? string.Empty,
            ["detail2_description"] = CommonHelper.EnsureMaximumLength(detailDescription2, 240) ?? string.Empty,
            ["detail2_text"] = CommonHelper.EnsureMaximumLength(detailText2, 240) ?? string.Empty,
            ["detail3_description"] = CommonHelper.EnsureMaximumLength(detailDescription3, 240) ?? string.Empty,
            ["detail3_text"] = CommonHelper.EnsureMaximumLength(detailText3, 240) ?? string.Empty,
            ["detail4_description"] = CommonHelper.EnsureMaximumLength(detailDescription4, 240) ?? string.Empty,
            ["detail4_text"] = CommonHelper.EnsureMaximumLength(detailText4, 240) ?? string.Empty,
            ["detail5_description"] = CommonHelper.EnsureMaximumLength(detailDescription5, 240) ?? string.Empty,
            ["detail5_text"] = CommonHelper.EnsureMaximumLength(detailText5, 240) ?? string.Empty
        });

        return url;
    }

    /// <summary>
    /// Prepare URL to complete checkout
    /// </summary>
    /// <param name="sessionRequestUrl">URL to request session details</param>
    /// <returns>URL</returns>
    protected virtual async Task<string> PrepareCheckoutUrlAsync(string sessionRequestUrl)
    {
        //first prepare checkout and get session id
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(_settings.RequestTimeout ?? 10)
        };
        client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, UserAgent);
        client.DefaultRequestHeaders.Add(HeaderNames.Accept, "*/*");
        var response = await client.GetAsync(sessionRequestUrl);
        var sessionResponse = await response.Content.ReadAsStringAsync();
        var sessionError = new { code = string.Empty, message = string.Empty };
        try
        {
            sessionError = JsonConvert.DeserializeAnonymousType(sessionResponse, sessionError);
        }
        catch { }
        if (!string.IsNullOrEmpty(sessionError?.code))
            throw new NopException($"{sessionError.code} - {sessionError.message}");

        //and now build URL to redirect
        var sessionId = sessionResponse;
        return QueryHelpers.AddQueryString(QuickCheckoutServiceUrl, new Dictionary<string, string>
        {
            ["sid"] = CommonHelper.EnsureMaximumLength(sessionId, 32),
        });
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare checkout URL to redirect customer
    /// </summary>
    /// <param name="order">Order</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains URL
    /// </returns>
    public virtual async Task<string> PrepareCheckoutUrlAsync(Order order)
    {
        var (result, _) = await HandleFunctionAsync(async () =>
        {
            var sessionRequestUrl = await PrepareSessionRequestUrlAsync(order);
            return await PrepareCheckoutUrlAsync(sessionRequestUrl);
        });

        return await result;
    }

    #endregion
}
