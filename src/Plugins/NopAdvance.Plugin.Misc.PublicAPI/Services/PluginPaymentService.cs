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
using Microsoft.AspNetCore.WebUtilities;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Html;
using Nop.Services.Orders;
using Nop.Services.Tax;
using System.Globalization;
using System.Net;
using System.Text;

namespace NopAdvance.Plugin.Misc.PublicAPI.Services;

public class PluginPaymentService : IPluginPaymentService
{
    #region Properties

    //PayPal Standard
    private static string NopCommercePartnerCode => "nopCommerce_SP";
    private static string OrderTotalSentToPayPal => "OrderTotalSentToPayPal";

    //Two Checkout
    private static string ServiceUrl => "https://www.2checkout.com/checkout/purchase";

    #endregion

    #region Fields

    private readonly PayPalStandardPaymentSettings _payPalStandardPaymentSettings;
    private readonly CurrencySettings _currencySettings;
    private readonly PaymentSettings _paymentSettings;
    private readonly TwoCheckoutPaymentSettings _twoCheckoutPaymentSettings;
    private readonly PaymentSkrillServiceManager _paymentSkrillServiceManager;
    private readonly IWebHelper _webHelper;
    private readonly IAddressService _addressService;
    private readonly ICurrencyService _currencyService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly ICountryService _countryService;
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly IAttributeParser<CheckoutAttribute,CheckoutAttributeValue> _checkoutAttributeParser;
    private readonly ICustomerService _customerService;
    private readonly ITaxService _taxService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IHtmlFormatter _htmlFormatter;

    #endregion

    #region Ctor

    public PluginPaymentService(PayPalStandardPaymentSettings payPalStandardPaymentSettings,
        CurrencySettings currencySettings,
        PaymentSettings paymentSettings,
        TwoCheckoutPaymentSettings twoCheckoutPaymentSettings,
        PaymentSkrillServiceManager paymentSkrillServiceManager,
        IWebHelper webHelper,
        IAddressService addressService,
        ICurrencyService currencyService,
        IStateProvinceService stateProvinceService,
        ICountryService countryService,
        IOrderService orderService,
        IProductService productService,
        IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
        ICustomerService customerService,
        ITaxService taxService,
        IGenericAttributeService genericAttributeService,
        IHtmlFormatter htmlFormatter)
    {
        _payPalStandardPaymentSettings = payPalStandardPaymentSettings;
        _currencySettings = currencySettings;
        _paymentSettings = paymentSettings;
        _twoCheckoutPaymentSettings = twoCheckoutPaymentSettings;
        _paymentSkrillServiceManager = paymentSkrillServiceManager;
        _webHelper = webHelper;
        _addressService = addressService;
        _currencyService = currencyService;
        _stateProvinceService = stateProvinceService;
        _countryService = countryService;
        _orderService = orderService;
        _productService = productService;
        _checkoutAttributeParser = checkoutAttributeParser;
        _customerService = customerService;
        _taxService = taxService;
        _genericAttributeService = genericAttributeService;
        _htmlFormatter = htmlFormatter;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Create common query parameters for the request
    /// </summary>
    /// <param name="order">Order</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the created query parameters
    /// </returns>
    protected virtual async Task<IDictionary<string, string>> CreateQueryParametersAsync(Order order)
    {
        //get store location
        var storeLocation = _webHelper.GetStoreLocation();

        //choosing correct order address
        var orderAddress = await _addressService.GetAddressByIdAsync(
            (order.PickupInStore ? order.PickupAddressId : order.ShippingAddressId) ?? 0);

        //create query parameters
        return new Dictionary<string, string>
        {
            //PayPal ID or an email address associated with your PayPal account
            ["business"] = _payPalStandardPaymentSettings.BusinessEmail,

            //the character set and character encoding
            ["charset"] = "utf-8",

            //set return method to "2" (the customer redirected to the return URL by using the POST method, and all payment variables are included)
            ["rm"] = "2",

            ["bn"] = NopCommercePartnerCode,
            ["currency_code"] = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId))?.CurrencyCode,

            //order identifier
            ["invoice"] = order.CustomOrderNumber,
            ["custom"] = order.OrderGuid.ToString(),

            //PDT, IPN and cancel URL
            ["return"] = $"{storeLocation}Plugins/PaymentPayPalStandard/PDTHandler",
            ["notify_url"] = $"{storeLocation}Plugins/PaymentPayPalStandard/IPNHandler",
            ["cancel_return"] = $"{storeLocation}Plugins/PaymentPayPalStandard/CancelOrder",

            //shipping address, if exists
            ["no_shipping"] = order.ShippingStatus == ShippingStatus.ShippingNotRequired ? "1" : "2",
            ["address_override"] = order.ShippingStatus == ShippingStatus.ShippingNotRequired ? "0" : "1",
            ["first_name"] = orderAddress?.FirstName,
            ["last_name"] = orderAddress?.LastName,
            ["address1"] = orderAddress?.Address1,
            ["address2"] = orderAddress?.Address2,
            ["city"] = orderAddress?.City,
            ["state"] = (await _stateProvinceService.GetStateProvinceByAddressAsync(orderAddress))?.Abbreviation,
            ["country"] = (await _countryService.GetCountryByAddressAsync(orderAddress))?.TwoLetterIsoCode,
            ["zip"] = orderAddress?.ZipPostalCode,
            ["email"] = orderAddress?.Email
        };
    }

    /// <summary>
    /// Add order items to the request query parameters
    /// </summary>
    /// <param name="parameters">Query parameters</param>
    /// <param name="order">Order</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected virtual async Task AddItemsParametersAsync(IDictionary<string, string> parameters, Order order)
    {
        //upload order items
        parameters.Add("cmd", "_cart");
        parameters.Add("upload", "1");

        var cartTotal = decimal.Zero;
        var roundedCartTotal = decimal.Zero;
        var itemCount = 1;

        //add shopping cart items
        foreach (var item in await _orderService.GetOrderItemsAsync(order.Id))
        {
            var roundedItemPrice = Math.Round(item.UnitPriceExclTax, 2);

            var product = await _productService.GetProductByIdAsync(item.ProductId);

            //add query parameters
            parameters.Add($"item_name_{itemCount}", product.Name);
            parameters.Add($"amount_{itemCount}", roundedItemPrice.ToString("0.00", CultureInfo.InvariantCulture));
            parameters.Add($"quantity_{itemCount}", item.Quantity.ToString());

            cartTotal += item.PriceExclTax;
            roundedCartTotal += roundedItemPrice * item.Quantity;
            itemCount++;
        }

        //add checkout attributes as order items
        var checkoutAttributeValues = _checkoutAttributeParser.ParseAttributeValues(order.CheckoutAttributesXml);
        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

        await foreach (var (attribute, values) in checkoutAttributeValues)
            await foreach (var attributeValue in values)
            {
                var (attributePrice, _) = await _taxService.GetCheckoutAttributePriceAsync(attribute, attributeValue, false, customer);
                var roundedAttributePrice = Math.Round(attributePrice, 2);

                //add query parameters
                if (attribute == null)
                    continue;

                parameters.Add($"item_name_{itemCount}", attribute.Name);
                parameters.Add($"amount_{itemCount}", roundedAttributePrice.ToString("0.00", CultureInfo.InvariantCulture));
                parameters.Add($"quantity_{itemCount}", "1");

                cartTotal += attributePrice;
                roundedCartTotal += roundedAttributePrice;
                itemCount++;
            }

        //add shipping fee as a separate order item, if it has price
        var roundedShippingPrice = Math.Round(order.OrderShippingExclTax, 2);
        if (roundedShippingPrice > decimal.Zero)
        {
            parameters.Add($"item_name_{itemCount}", "Shipping fee");
            parameters.Add($"amount_{itemCount}", roundedShippingPrice.ToString("0.00", CultureInfo.InvariantCulture));
            parameters.Add($"quantity_{itemCount}", "1");

            cartTotal += order.OrderShippingExclTax;
            roundedCartTotal += roundedShippingPrice;
            itemCount++;
        }

        //add payment method additional fee as a separate order item, if it has price
        var roundedPaymentMethodPrice = Math.Round(order.PaymentMethodAdditionalFeeExclTax, 2);
        if (roundedPaymentMethodPrice > decimal.Zero)
        {
            parameters.Add($"item_name_{itemCount}", "Payment method fee");
            parameters.Add($"amount_{itemCount}", roundedPaymentMethodPrice.ToString("0.00", CultureInfo.InvariantCulture));
            parameters.Add($"quantity_{itemCount}", "1");

            cartTotal += order.PaymentMethodAdditionalFeeExclTax;
            roundedCartTotal += roundedPaymentMethodPrice;
            itemCount++;
        }

        //add tax as a separate order item, if it has positive amount
        var roundedTaxAmount = Math.Round(order.OrderTax, 2);
        if (roundedTaxAmount > decimal.Zero)
        {
            parameters.Add($"item_name_{itemCount}", "Tax amount");
            parameters.Add($"amount_{itemCount}", roundedTaxAmount.ToString("0.00", CultureInfo.InvariantCulture));
            parameters.Add($"quantity_{itemCount}", "1");

            cartTotal += order.OrderTax;
            roundedCartTotal += roundedTaxAmount;
        }

        if (cartTotal > order.OrderTotal)
        {
            //get the difference between what the order total is and what it should be and use that as the "discount"
            var discountTotal = Math.Round(cartTotal - order.OrderTotal, 2);
            roundedCartTotal -= discountTotal;

            //gift card or rewarded point amount applied to cart in nopCommerce - shows in PayPal as "discount"
            parameters.Add("discount_amount_cart", discountTotal.ToString("0.00", CultureInfo.InvariantCulture));
        }

        //save order total that actually sent to PayPal (used for PDT order total validation)
        await _genericAttributeService.SaveAttributeAsync(order, OrderTotalSentToPayPal, roundedCartTotal);
    }

    /// <summary>
    /// Add order total to the request query parameters
    /// </summary>
    /// <param name="parameters">Query parameters</param>
    /// <param name="order">Order</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected virtual async Task AddOrderTotalParametersAsync(IDictionary<string, string> parameters, Order order)
    {
        //round order total
        var roundedOrderTotal = Math.Round(order.OrderTotal, 2);

        parameters.Add("cmd", "_xclick");
        parameters.Add("item_name", $"Order Number {order.CustomOrderNumber}");
        parameters.Add("amount", roundedOrderTotal.ToString("0.00", CultureInfo.InvariantCulture));

        //save order total that actually sent to PayPal (used for PDT order total validation)
        await _genericAttributeService.SaveAttributeAsync(order, OrderTotalSentToPayPal, roundedOrderTotal);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Generate order guid
    /// </summary>
    /// <param name="previousOrderGuid">previousOrderGuid</param>
    /// <param name="previousOrderGuidGeneratedOnUtc">previousOrderGuidGeneratedOnUtc</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the order guid and order guid generated date-time utc
    /// </returns>
    public virtual (Guid, DateTime) GenerateOrderGuid(Guid? previousOrderGuid, DateTime? previousOrderGuidGeneratedOnUtc)
    {
        Guid? orderGuid = null;
        var orderGuidGeneratedOnUtc = DateTime.UtcNow;
        //we should use the same GUID for multiple payment attempts
        //this way a payment gateway can prevent security issues such as credit card brute-force attacks
        //in order to avoid any possible limitations by payment gateway we reset GUID periodically
        if (_paymentSettings.RegenerateOrderGuidInterval > 0 && previousOrderGuid.HasValue
            && previousOrderGuidGeneratedOnUtc.HasValue)
        {
            var order = _orderService.GetOrderByGuidAsync(previousOrderGuid.Value);
            if (order == null)
            {
                var interval = DateTime.UtcNow - previousOrderGuidGeneratedOnUtc.Value;
                if (interval.TotalSeconds < _paymentSettings.RegenerateOrderGuidInterval)
                {
                    orderGuid = previousOrderGuid;
                    orderGuidGeneratedOnUtc = previousOrderGuidGeneratedOnUtc.Value;
                }
            }
        }

        if (!orderGuid.HasValue)
        {
            orderGuid = Guid.NewGuid();
            orderGuidGeneratedOnUtc = DateTime.UtcNow;
        }

        return (orderGuid.Value, orderGuidGeneratedOnUtc);
    }

    /// <summary>
    /// Get PayPalStandard redirection Url
    /// </summary>
    /// <param name="order">order</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the paypal standard redirect url
    /// </returns>
    public virtual async Task<string> GetPayPalStandardRedirectionUrl(Order order)
    {
        var baseUrl = _payPalStandardPaymentSettings.UseSandbox ?
            "https://www.sandbox.paypal.com/us/cgi-bin/webscr" :
            "https://www.paypal.com/us/cgi-bin/webscr";

        //create common query parameters for the request
        var queryParameters = await CreateQueryParametersAsync(order);

        //whether to include order items in a transaction
        if (_payPalStandardPaymentSettings.PassProductNamesAndTotals)
        {
            //add order items query parameters to the request
            var parameters = new Dictionary<string, string>(queryParameters);
            await AddItemsParametersAsync(parameters, order);

            //remove null values from parameters
            parameters = parameters.Where(parameter => !string.IsNullOrEmpty(parameter.Value))
                .ToDictionary(parameter => parameter.Key, parameter => parameter.Value);

            //ensure redirect URL doesn't exceed 2K chars to avoid "too long URL" exception
            var redirectUrl = QueryHelpers.AddQueryString(baseUrl, parameters);
            if (redirectUrl.Length <= 2048)
                return redirectUrl;
        }

        //or add only an order total query parameters to the request
        await AddOrderTotalParametersAsync(queryParameters, order);

        //remove null values from parameters
        queryParameters = queryParameters.Where(parameter => !string.IsNullOrEmpty(parameter.Value))
            .ToDictionary(parameter => parameter.Key, parameter => parameter.Value);

        var url = QueryHelpers.AddQueryString(baseUrl, queryParameters);
        return baseUrl;
    }

    /// <summary>
    /// Get skrill redirection Url
    /// </summary>
    /// <param name="order">order</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the skrill redirect url
    /// </returns>
    public virtual async Task<string> GetSkrillRedirectionUrl(Order order)
    {
        return await _paymentSkrillServiceManager.PrepareCheckoutUrlAsync(order);
    }

    /// <summary>
    /// Get two checkout redirection Url
    /// </summary>
    /// <param name="order">order</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the two checkout redirect url
    /// </returns>
    public virtual async Task<string> GetTwoCheckoutRedirectionUrl(Order order)
    {
        var builder = new StringBuilder();

        builder.AppendFormat("{0}?id_type=1", ServiceUrl);

        //products
        var orderProducts = await _orderService.GetOrderItemsAsync(order.Id);
        for (var i = 0; i < orderProducts.Count; i++)
        {
            var pNum = i + 1;
            var orderItem = orderProducts[i];
            var product = await _productService.GetProductByIdAsync(orderProducts[i].ProductId);

            var cProd = $"c_prod_{pNum}";
            var cProdValue = $"{product.Sku},{orderItem.Quantity}";
            builder.AppendFormat("&{0}={1}", cProd, cProdValue);

            var cName = $"c_name_{pNum}";
            var cNameValue = product.Name;
            builder.AppendFormat("&{0}={1}", WebUtility.UrlEncode(cName), WebUtility.UrlEncode(cNameValue));

            var cDescription = $"c_description_{pNum}";
            var cDescriptionValue = cNameValue;
            if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                cDescriptionValue = _htmlFormatter.StripTags($"{cDescriptionValue}. {orderItem.AttributeDescription}");
            builder.AppendFormat("&{0}={1}", WebUtility.UrlEncode(cDescription), WebUtility.UrlEncode(cDescriptionValue));

            var cPrice = $"c_price_{pNum}";
            var cPriceValue = orderItem.UnitPriceInclTax.ToString("0.00", CultureInfo.InvariantCulture);
            builder.AppendFormat("&{0}={1}", cPrice, cPriceValue);

            var cTangible = $"c_tangible_{pNum}";
            var cTangibleValue = product.IsDownload ? "N" : "Y";
            builder.AppendFormat("&{0}={1}", cTangible, cTangibleValue);
        }

        builder.AppendFormat("&x_login={0}", _twoCheckoutPaymentSettings.AccountNumber);
        builder.AppendFormat("&sid={0}", _twoCheckoutPaymentSettings.AccountNumber);
        builder.AppendFormat("&x_amount={0}", order.OrderTotal.ToString("0.00", CultureInfo.InvariantCulture));
        var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
        builder.AppendFormat("&currency_code={0}", currency?.CurrencyCode);
        builder.AppendFormat("&x_invoice_num={0}", order.CustomOrderNumber);

        if (_twoCheckoutPaymentSettings.UseSandbox)
            builder.AppendFormat("&demo=Y");

        var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
        if (billingAddress != null)
        {
            var country = await _countryService.GetCountryByIdAsync(billingAddress.CountryId ?? 0);
            var state = await _stateProvinceService.GetStateProvinceByIdAsync(billingAddress.StateProvinceId ?? 0);
            builder.AppendFormat("&x_First_Name={0}", WebUtility.UrlEncode(billingAddress.FirstName ?? string.Empty));
            builder.AppendFormat("&x_Last_Name={0}", WebUtility.UrlEncode(billingAddress.LastName ?? string.Empty));
            builder.AppendFormat("&x_Address={0}", WebUtility.UrlEncode(billingAddress.Address1 ?? string.Empty));
            builder.AppendFormat("&x_City={0}", WebUtility.UrlEncode(billingAddress.City ?? string.Empty));
            builder.AppendFormat("&x_State={0}", WebUtility.UrlEncode(state?.Abbreviation ?? string.Empty));
            builder.AppendFormat("&x_Country={0}", WebUtility.UrlEncode(country?.ThreeLetterIsoCode ?? string.Empty));
            builder.AppendFormat("&x_Zip={0}", WebUtility.UrlEncode(billingAddress.ZipPostalCode ?? string.Empty));
            builder.AppendFormat("&x_EMail={0}", WebUtility.UrlEncode(billingAddress.Email ?? string.Empty));
            builder.AppendFormat("&x_Phone={0}", WebUtility.UrlEncode(CommonHelper.EnsureNumericOnly(billingAddress.PhoneNumber) ?? string.Empty));
        }

        var redirectUrl = builder.ToString();
        return redirectUrl;
    }

    #endregion
}
