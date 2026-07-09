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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Attributes;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using Nop.Web.Models.ShoppingCart;
using NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;
using NopAdvance.Plugin.Misc.PublicAPI.Factories;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure.Extensions;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses.Payments;
using NopAdvance.Plugin.Misc.PublicAPI.Services;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers;

/// <summary>
/// Checkout methods
/// </summary>
public partial class PublicCheckoutController : BaseAPIController
{
    #region Fields

    private readonly PaymentSettings _paymentSettings;
    private readonly AddressSettings _addressSettings;
    private readonly OrderSettings _orderSettings;
    private readonly ShippingSettings _shippingSettings;
    private readonly RewardPointsSettings _rewardPointsSettings;
    private readonly TaxSettings _taxSettings;
    private readonly ICustomerService _customerService;
    private readonly IWorkContext _workContext;
    private readonly ICountryService _countryService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IAddressModelFactory _addressModelFactory;
    private readonly IAddressService _addressService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly ICheckoutModelFactory _checkoutModelFactory;
    private readonly IStoreContext _storeContext;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IAttributeParser<AddressAttribute, AddressAttributeValue> _addressAttributeParser;
    private readonly IShippingService _shippingService;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly IPaymentService _paymentService;
    private readonly ILocalizationService _localizationService;
    private readonly IOrderService _orderService;
    private readonly ILogger _logger;
    private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
    private readonly IAPICommonModelFactory _apiCommonModelFactory;
    private readonly IPluginPaymentService _pluginPaymentService;
    private readonly ITaxService _taxService;

    #endregion

    #region Ctor

    public PublicCheckoutController(PaymentSettings paymentSettings,
        AddressSettings addressSettings,
        OrderSettings orderSettings,
        ShippingSettings shippingSettings,
        RewardPointsSettings rewardPointsSettings,
        ICustomerService customerService,
        IWorkContext workContext,
        ICountryService countryService,
        IStoreMappingService storeMappingService,
        IAddressModelFactory addressModelFactory,
        IAddressService addressService,
        IShoppingCartService shoppingCartService,
        ICheckoutModelFactory checkoutModelFactory,
        IStoreContext storeContext,
        IGenericAttributeService genericAttributeService,
        IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
        IShippingService shippingService,
        IOrderProcessingService orderProcessingService,
        IPaymentPluginManager paymentPluginManager,
        IPaymentService paymentService,
        ILocalizationService localizationService,
        IOrderService orderService,
        ILogger logger,
        IShoppingCartModelFactory shoppingCartModelFactory,
        IAPICommonModelFactory apiCommonModelFactory,
        IPluginPaymentService pluginPaymentService,
        TaxSettings taxSettings,
        ITaxService taxService)
    {
        _paymentSettings = paymentSettings;
        _addressSettings = addressSettings;
        _orderSettings = orderSettings;
        _shippingSettings = shippingSettings;
        _rewardPointsSettings = rewardPointsSettings;
        _customerService = customerService;
        _workContext = workContext;
        _countryService = countryService;
        _storeMappingService = storeMappingService;
        _addressModelFactory = addressModelFactory;
        _addressService = addressService;
        _shoppingCartService = shoppingCartService;
        _checkoutModelFactory = checkoutModelFactory;
        _storeContext = storeContext;
        _genericAttributeService = genericAttributeService;
        _addressAttributeParser = addressAttributeParser;
        _shippingService = shippingService;
        _orderProcessingService = orderProcessingService;
        _paymentPluginManager = paymentPluginManager;
        _paymentService = paymentService;
        _localizationService = localizationService;
        _orderService = orderService;
        _logger = logger;
        _shoppingCartModelFactory = shoppingCartModelFactory;
        _apiCommonModelFactory = apiCommonModelFactory;
        _pluginPaymentService = pluginPaymentService;
        _taxSettings = taxSettings;
        _taxService = taxService;
    }

    #endregion

    #region Utilities

    protected virtual async Task<bool> IsMinimumOrderPlacementIntervalValidAsync()
    {
        //prevent 2 orders being placed within an X seconds time frame
        if (_orderSettings.MinimumOrderPlacementInterval == 0)
            return true;

        var lastOrder = (await _orderService.SearchOrdersAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
            customerId: (await _workContext.GetCurrentCustomerAsync()).Id, pageSize: 1))
            .FirstOrDefault();
        if (lastOrder == null)
            return true;

        var interval = DateTime.UtcNow - lastOrder.CreatedOnUtc;
        return interval.TotalSeconds > _orderSettings.MinimumOrderPlacementInterval;
    }

    protected virtual async Task<(string, PlaceOrderResult, string)> PlaceOrderAsync(ProcessPaymentRequest processPaymentRequest)
    {
        try
        {
            //prevent 2 orders being placed within an X seconds time frame
            if (!await IsMinimumOrderPlacementIntervalValidAsync())
                return (await _localizationService.GetResourceAsync("Checkout.MinOrderPlacementInterval"), null, "");

            processPaymentRequest.StoreId = (await _storeContext.GetCurrentStoreAsync()).Id;
            processPaymentRequest.CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;
            processPaymentRequest.PaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.SelectedPaymentMethodAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);

            var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);
            var redirectUrl = string.Empty;
            if (placeOrderResult.Success)
            {
                var customer = await _customerService.GetCustomerByIdAsync(placeOrderResult.PlacedOrder.CustomerId);
                var paymentMethod = await _paymentPluginManager
                    .LoadPluginBySystemNameAsync(placeOrderResult.PlacedOrder.PaymentMethodSystemName, customer, placeOrderResult.PlacedOrder.StoreId);

                if (paymentMethod == null)
                    return ("Payment method couldn't be loaded", null, "");

                if (paymentMethod.PaymentMethodType == PaymentMethodType.Standard)
                    await _paymentService.PostProcessPaymentAsync(new PostProcessPaymentRequest { Order = placeOrderResult.PlacedOrder });
                else if (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection)
                {
                    //already paid or order.OrderTotal == decimal.Zero
                    if (placeOrderResult.PlacedOrder.PaymentStatus == PaymentStatus.Paid)
                        return (string.Empty, placeOrderResult, redirectUrl);

                    switch (placeOrderResult.PlacedOrder.PaymentMethodSystemName)
                    {
                        case PaymentMethodDefaults.PAY_PAL_STANDARD:
                            redirectUrl = await _pluginPaymentService.GetPayPalStandardRedirectionUrl(placeOrderResult.PlacedOrder);
                            break;
                        case PaymentMethodDefaults.SKRILL:
                            redirectUrl = await _pluginPaymentService.GetSkrillRedirectionUrl(placeOrderResult.PlacedOrder);
                            break;
                        case PaymentMethodDefaults.TWO_CHECKOUT:
                            redirectUrl = await _pluginPaymentService.GetTwoCheckoutRedirectionUrl(placeOrderResult.PlacedOrder);
                            break;
                    }
                }

            }
            return (string.Empty, placeOrderResult, redirectUrl);

        }
        catch (Exception exc)
        {
            await _logger.WarningAsync(exc.Message, exc);
            return (exc.Message, null, "");
        }
    }

    protected virtual async Task<ValidatePaymentInfoResponse> ValidatePaymentInfoAsync(IList<ShoppingCartItem> cart,
        IDictionary<string, string> paymentInfo, Guid? previousOrderGuid, DateTime? previousOrderGuidGeneratedOnUtc,
        bool validatePaymentWorkflow = false)
    {
        var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
        var response = new ValidatePaymentInfoResponse
        {
            IsPaymentWorkflowRequired = isPaymentWorkflowRequired
        };
        if (isPaymentWorkflowRequired)
        {
            //load payment method
            var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.SelectedPaymentMethodAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (string.IsNullOrEmpty(paymentMethodSystemName))
            {
                response.AddError(MessageDefaults.PAYMENT_METHOD_REQUIRED);
                return response;
            }

            var paymentMethod = await _paymentPluginManager
                .LoadPluginBySystemNameAsync(paymentMethodSystemName, await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);
           
            if (paymentMethod == null)
            {
                response.AddError(MessageDefaults.PAYMENT_METHOD_NOT_FOUND);
                return response;
            }
                
            var form = new FormCollection(ConvertToFormCollection(paymentInfo));
            var warnings = await paymentMethod.ValidatePaymentFormAsync(form);
            if (warnings.Count <= 0)
            {
                //set previous order GUID (if exists)
                var (orderGuid, orderGuidGeneratedOnUtc) = _pluginPaymentService.GenerateOrderGuid(previousOrderGuid,
                    previousOrderGuidGeneratedOnUtc);
                response.OrderGuid = orderGuid;
                response.OrderGuidGeneratedOnUtc = orderGuidGeneratedOnUtc;
            }
            else
                foreach (var warning in warnings)
                    response.AddError(warning);
        }
        else
            if (validatePaymentWorkflow)
            response.AddError("Payment workflow is not required");

        return response;
    }

    protected virtual async Task<ConfirmOrderResponse> ConfirmOrderAsync(ValidatePaymentInfoResponse validatePaymentInfoResponse,
        IDictionary<string, string> paymentInfo, Guid? previousOrderGuid, DateTime? previousOrderGuidGeneratedOnUtc)
    {
        var result = new ConfirmOrderResponse();
        ProcessPaymentRequest processPaymentRequest;
        if (validatePaymentInfoResponse.IsPaymentWorkflowRequired)
        {
            var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.SelectedPaymentMethodAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
            var paymentMethod = await _paymentPluginManager
                .LoadPluginBySystemNameAsync(paymentMethodSystemName, await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);
            if (paymentMethod == null)
                result.Errors.Add(string.Format(MessageDefaults.NOT_FOUND, nameof(paymentMethod)));

            var form = new FormCollection(ConvertToFormCollection(paymentInfo));
            processPaymentRequest = await paymentMethod.GetPaymentInfoAsync(form);
            processPaymentRequest.OrderGuid = validatePaymentInfoResponse.OrderGuid.Value;
            processPaymentRequest.OrderGuidGeneratedOnUtc = validatePaymentInfoResponse.OrderGuidGeneratedOnUtc;
        }
        else
        {
            var (orderGuid, orderGuidGeneratedOnUtc) = _pluginPaymentService.GenerateOrderGuid(previousOrderGuid,
                        previousOrderGuidGeneratedOnUtc);

            processPaymentRequest = new ProcessPaymentRequest
            {
                OrderGuid = orderGuid,
                OrderGuidGeneratedOnUtc = orderGuidGeneratedOnUtc
            };
        }
        var (placeOrderError, placeOrderResult, redirectUrl) = await PlaceOrderAsync(processPaymentRequest);
        if (!string.IsNullOrEmpty(placeOrderError))
            result.Errors.Add(placeOrderError);

        result.RedirectionUrl = redirectUrl;

        if (placeOrderResult.Success)
            result.OrderId = placeOrderResult.PlacedOrder.Id;
        else
            foreach (var error in placeOrderResult.Errors)
                result.Errors.Add(error);
        return result;
    }

    /// <summary>	
    /// Saves the pickup option	
    /// </summary>	
    /// <param name="pickupPoint">The pickup option</param>	
    /// <returns>A task that represents the asynchronous operation</returns>	
    protected virtual async Task SavePickupOptionAsync(PickupPoint pickupPoint)
    {
        var name = !string.IsNullOrEmpty(pickupPoint.Name) ?
            string.Format(await _localizationService.GetResourceAsync("Checkout.PickupPoints.Name"), pickupPoint.Name) :
            await _localizationService.GetResourceAsync("Checkout.PickupPoints.NullName");
        var pickUpInStoreShippingOption = new ShippingOption
        {
            Name = name,
            Rate = pickupPoint.PickupFee,
            Description = pickupPoint.Description,
            ShippingRateComputationMethodSystemName = pickupPoint.ProviderSystemName,
            IsPickupInStore = true
        };
        await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedShippingOptionAttribute, pickUpInStoreShippingOption, (await _storeContext.GetCurrentStoreAsync()).Id);
        await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedPickupPointAttribute, pickupPoint, (await _storeContext.GetCurrentStoreAsync()).Id);
    }

    /// <summary>
    /// Save customer VAT number
    /// </summary>
    /// <param name="fullVatNumber">The full VAT number</param>
    /// <param name="customer">The customer</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the Vat number error if exists
    /// </returns>
    protected virtual async Task<string> SaveCustomerVatNumberAsync(string fullVatNumber, Customer customer)
    {
        var (vatNumberStatus, _, _) = await _taxService.GetVatNumberStatusAsync(fullVatNumber);
        customer.VatNumberStatus = vatNumberStatus;
        customer.VatNumber = fullVatNumber;
        await _customerService.UpdateCustomerAsync(customer);

        if (vatNumberStatus != VatNumberStatus.Valid && !string.IsNullOrEmpty(fullVatNumber))
        {
            var warning = await _localizationService.GetResourceAsync("Checkout.VatNumber.Warning");
            return string.Format(warning, await _localizationService.GetLocalizedEnumAsync(vatNumberStatus));
        }

        return string.Empty;
    }
    
    #endregion

    #region Methods

    /// <summary>
    /// Get order summary
    /// </summary>
    /// <param name="validateCheckoutAttributes">Validate checkout attributes? (optional)</param>
    /// <param name="prepareOrderReviewData">Prepare order review data? (optional)</param>
    [HttpGet]
    [ProducesResponseType(typeof(ShoppingCartModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetOrderSummary(bool? validateCheckoutAttributes, bool? prepareOrderReviewData)
    {
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        var model = new ShoppingCartModel();
        model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart,
            isEditable: false,
            validateCheckoutAttributes: validateCheckoutAttributes.GetValueOrDefault(),
            prepareAndDisplayOrderReviewData: prepareOrderReviewData.GetValueOrDefault());
        return Ok(model);
    }

    #region Billing

    /// <summary>
    /// Get all billing addresses
    /// </summary>
    /// <param name="prepareAdd">Prepare add new billing address model?</param>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BillingAddressResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetBillingAddresses(bool prepareAdd = false)
    {
        //validation
        if (_orderSettings.CheckoutDisabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (!cart.Any())
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "cartItems"));

        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var addresses = await (await _customerService.GetAddressesByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id))
            .WhereAwait(async a => !a.CountryId.HasValue || await _countryService.GetCountryByAddressAsync(a) is Country country &&
                //published
                country.Published &&
                //allow billing
                country.AllowsBilling &&
                //enabled for the current store
                await _storeMappingService.AuthorizeAsync(country))
            .ToListAsync();

        var model = new BillingAddressResponse();

        var customer = await _workContext.GetCurrentCustomerAsync();
        if (await _customerService.IsGuestAsync(customer) && _taxSettings.EuVatEnabled)
        {
            model.VatNumber = customer.VatNumber;
            model.EuVatEnabled = true;
            model.EuVatEnabledForGuests = _taxSettings.EuVatEnabledForGuests;
        }

        foreach (var address in addresses)
        {
            var addressModel = new AddressModel();
            await _addressModelFactory.PrepareAddressModelAsync(addressModel,
                address: address,
                excludeProperties: false,
                addressSettings: _addressSettings);

            if (await _addressService.IsAddressValidAsync(address))
                model.Addresses.Add(addressModel);
            else
                model.InvalidAddresses.Add(addressModel);
        }

        if (prepareAdd)
        {
            model.NewAddress = new AddressModel();
            await _addressModelFactory.PrepareAddressModelAsync(model.NewAddress,
                address: null,
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesForBillingAsync((await _workContext.GetWorkingLanguageAsync()).Id),
                prePopulateWithCustomerFields: true,
                customer: await _workContext.GetCurrentCustomerAsync());
        }

        return Ok(model);
    }

    /// <summary>
    /// Select billing address
    /// </summary>
    /// <param name="addressId">The address identifier</param>
    /// <param name="shipToSameAddress">Ship to the same address?</param>
    [HttpGet("{addressId}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> SelectBillingAddress(int addressId, bool shipToSameAddress = false)
    {
        //validation
        if (_orderSettings.CheckoutDisabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var address = await _customerService.GetCustomerAddressAsync((await _workContext.GetCurrentCustomerAsync()).Id, addressId);

        if (address == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(address)));

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
        if (!cart.Any())
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "cartItems"));

        (await _workContext.GetCurrentCustomerAsync()).BillingAddressId = address.Id;
        await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());

        //ship to the same address?
        //by default Shipping is available if the country is not specified
        var shippingAllowed = !_addressSettings.CountryEnabled || ((await _countryService.GetCountryByAddressAsync(address))?.AllowsShipping ?? false);
        if (_shippingSettings.ShipToSameAddress && shipToSameAddress && await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart) && shippingAllowed)
        {
            (await _workContext.GetCurrentCustomerAsync()).ShippingAddressId = (await _workContext.GetCurrentCustomerAsync()).BillingAddressId;
            await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());
            //reset selected shipping method (in case if "pick up in store" was selected)
            await _genericAttributeService.SaveAttributeAsync<ShippingOption>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedShippingOptionAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);
            await _genericAttributeService.SaveAttributeAsync<PickupPoint>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedPickupPointAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);
        }

        return Ok();
    }

    /// <summary>
    /// Add a new billing address
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> AddBillingAddress(AddBillingAddressRequest request)
    {
        //validation
        if (_orderSettings.CheckoutDisabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (!cart.Any())
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "cartItems"));

        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        // Allow VAT number to be entered in guest checkout
        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && _taxSettings.EuVatEnabled && _taxSettings.EuVatEnabledForGuests)
        {
            var warning = await SaveCustomerVatNumberAsync(request.VatNumber, await _workContext.GetCurrentCustomerAsync());
            if (!string.IsNullOrEmpty(warning))
                ModelState.AddModelError("", warning);
        }

        //custom address attributes
        var form = new FormCollection(ConvertToFormCollection(request.CustomAddressAttributes));
        var customAttributes = await _addressAttributeParser.ParseCustomAttributesAsync(form, NopCommonDefaults.AddressAttributeControlName);
        var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
        foreach (var error in customAttributeWarnings)
            ModelState.AddModelError("", error);

        var newAddress = request;

        if (ModelState.IsValid)
        {
            //try to find an address with the same values (don't duplicate records)
            var address = _addressService.FindAddress((await _customerService.GetAddressesByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id)).ToList(),
                newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                newAddress.Address1, newAddress.Address2, newAddress.City,
                newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                newAddress.CountryId, customAttributes);

            if (address == null)
            {
                //address is not found. let's create a new one
                address = newAddress.ToEntity();
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;

                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;

                await _addressService.InsertAddressAsync(address);

                await _customerService.InsertCustomerAddressAsync(await _workContext.GetCurrentCustomerAsync(), address);
            }

            (await _workContext.GetCurrentCustomerAsync()).BillingAddressId = address.Id;

            await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());

            //ship to the same address?
            if (_shippingSettings.ShipToSameAddress && request.ShipToSameAddress && await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
            {
                (await _workContext.GetCurrentCustomerAsync()).ShippingAddressId = (await _workContext.GetCurrentCustomerAsync()).BillingAddressId;
                await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());

                //reset selected shipping method (in case if "pick up in store" was selected)
                await _genericAttributeService.SaveAttributeAsync<ShippingOption>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedShippingOptionAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);
                await _genericAttributeService.SaveAttributeAsync<PickupPoint>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedPickupPointAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);

                //limitation - "Ship to the same address" doesn't properly work in "pick up in store only" case (when no shipping plugins are available) 
                return Ok(address.Id);
            }

            return Ok(address.Id);
        }

        return PrepareBadRequest(ModelState);
    }

    #endregion

    #region Shipping

    /// <summary>
    /// Get all pickup points
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(CheckoutPickupPointsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetPickupPoints()
    {
        if (_orderSettings.CheckoutDisabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (!cart.Any())
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "cartItems"));

        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
            return BadRequest("Shipping is not required");

        //model
        var model = await _apiCommonModelFactory.PrepareCheckoutPickupPointsModelAsync(cart);
        return Ok(model);
    }

    /// <summary>
    /// Get all shipping addresses
    /// </summary>
    /// <param name="prepareAdd">Prepare add new shipping address model?</param>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AddressesResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetShippingAddresses(bool prepareAdd = false)
    {
        //validation
        if (_orderSettings.CheckoutDisabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (!cart.Any())
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "cartItems"));

        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
            return BadRequest("Shipping is not required");

        var addresses = await (await _customerService.GetAddressesByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id))
            .WhereAwait(async a => !a.CountryId.HasValue || await _countryService.GetCountryByAddressAsync(a) is Country country &&
                //published
                country.Published &&
                //allow shipping
                country.AllowsShipping &&
                //enabled for the current store
                await _storeMappingService.AuthorizeAsync(country))
            .ToListAsync();

        var model = new AddressesResponse();
        foreach (var address in addresses)
        {
            var addressModel = new AddressModel();
            await _addressModelFactory.PrepareAddressModelAsync(addressModel,
                address: address,
                excludeProperties: false,
                addressSettings: _addressSettings);

            if (await _addressService.IsAddressValidAsync(address))
                model.Addresses.Add(addressModel);
            else
                model.InvalidAddresses.Add(addressModel);
        }

        if (prepareAdd)
        {
            model.NewAddress = new AddressModel();
            await _addressModelFactory.PrepareAddressModelAsync(model.NewAddress,
            address: null,
            excludeProperties: false,
            addressSettings: _addressSettings,
            loadCountries: async () => await _countryService.GetAllCountriesForShippingAsync((await _workContext.GetWorkingLanguageAsync()).Id),
            prePopulateWithCustomerFields: true,
            customer: await _workContext.GetCurrentCustomerAsync());
        }

        return Ok(model);
    }

    /// <summary>
    /// Select shipping address
    /// </summary>
    /// <param name="addressId">The address identifier</param>
    [HttpGet("{addressId}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> SelectShippingAddress(int addressId)
    {
        //validation
        if (_orderSettings.CheckoutDisabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var address = await _customerService.GetCustomerAddressAsync((await _workContext.GetCurrentCustomerAsync()).Id, addressId);

        if (address == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(address)));

        (await _workContext.GetCurrentCustomerAsync()).ShippingAddressId = address.Id;
        await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());

        if (_shippingSettings.AllowPickupInStore)
            //set value indicating that "pick up in store" option has not been chosen
            await _genericAttributeService.SaveAttributeAsync<PickupPoint>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedPickupPointAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);

        return Ok();
    }

    /// <summary>
    /// Add a new shipping address
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> AddShippingAddress(AddressRequest request)
    {
        //validation
        if (_orderSettings.CheckoutDisabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (!cart.Any())
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "cartItems"));

        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        //custom address attributes
        var form = new FormCollection(ConvertToFormCollection(request.CustomAddressAttributes));
        var customAttributes = await _addressAttributeParser.ParseCustomAttributesAsync(form, NopCommonDefaults.AddressAttributeControlName);
        var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
        foreach (var error in customAttributeWarnings)
            ModelState.AddModelError("", error);

        var newAddress = request;

        if (ModelState.IsValid)
        {
            //try to find an address with the same values (don't duplicate records)
            var address = _addressService.FindAddress((await _customerService.GetAddressesByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id)).ToList(),
                newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                newAddress.Address1, newAddress.Address2, newAddress.City,
                newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                newAddress.CountryId, customAttributes);

            if (address == null)
            {
                address = newAddress.ToEntity();
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;

                await _addressService.InsertAddressAsync(address);

                await _customerService.InsertCustomerAddressAsync(await _workContext.GetCurrentCustomerAsync(), address);

            }

            (await _workContext.GetCurrentCustomerAsync()).ShippingAddressId = address.Id;
            await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());

            return Ok(address.Id);
        }

        return PrepareBadRequest(ModelState);
    }

    /// <summary>
    /// Get all shipping methods
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(CheckoutShippingMethodModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetShippingMethods()
    {
        //validation
        if (_orderSettings.CheckoutDisabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (!cart.Any())
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "cartItems"));

        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
        {
            await _genericAttributeService.SaveAttributeAsync<ShippingOption>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedShippingOptionAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);
            return BadRequest("Shipping is not required");
        }

        //check if pickup point is selected on the shipping address step
        if (!_orderSettings.DisplayPickupInStoreOnShippingMethodPage)
        {
            var selectedPickUpPoint = await _genericAttributeService
                .GetAttributeAsync<PickupPoint>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedPickupPointAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (selectedPickUpPoint != null)
                return Ok();
        }

        var model = await _checkoutModelFactory.PrepareShippingMethodModelAsync(cart, await _customerService.GetCustomerShippingAddressAsync(await _workContext.GetCurrentCustomerAsync()));
        return Ok(model);
    }

    /// <summary>
    /// Select shipping method
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> SelectShippingMethod(SelectShippingMethodRequest request)
    {
        //validation
        if (_orderSettings.CheckoutDisabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (!cart.Any())
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "cartItems"));

        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
        {
            await _genericAttributeService.SaveAttributeAsync<ShippingOption>(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.SelectedShippingOptionAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);
            return BadRequest("Shipping is not required");
        }

        if (_shippingSettings.AllowPickupInStore && !_orderSettings.DisplayPickupInStoreOnShippingMethodPage)
        {
            if (request.IsPickup)
            {
                var pickupPoint = request.PickupPointName.Split(new[] { "___" }, StringSplitOptions.None);

                var customer = await _workContext.GetCurrentCustomerAsync();
                var address = customer.BillingAddressId.HasValue
               ? await _addressService.GetAddressByIdAsync(customer.BillingAddressId.Value)
               : null;

                var selectedPoint = (await _shippingService.GetPickupPointsAsync(cart,
                    address, customer, pickupPoint[1], (await _storeContext.GetCurrentStoreAsync()).Id)).PickupPoints.FirstOrDefault(x => x.Id.Equals(pickupPoint[0]));

                if (selectedPoint == null)
                    return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(selectedPoint)));

                await SavePickupOptionAsync(selectedPoint);

                return Ok();
            }

            //set value indicating that "pick up in store" option has not been chosen
            await _genericAttributeService.SaveAttributeAsync<PickupPoint>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedPickupPointAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);
        }

        //parse selected method 
        if (string.IsNullOrEmpty(request.ShippingMethodName))
            return BadRequest("ShippingMethodName is required");
        var splittedOption = request.ShippingMethodName.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries);
        if (splittedOption.Length != 2)
            return BadRequest(message: "Invalid ShippingMethodName");
        var selectedName = splittedOption[0];
        var shippingRateComputationMethodSystemName = splittedOption[1];

        //find it
        //performance optimization. try cache first
        var shippingOptions = await _genericAttributeService.GetAttributeAsync<List<ShippingOption>>(await _workContext.GetCurrentCustomerAsync(),
            NopCustomerDefaults.OfferedShippingOptionsAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
        if (shippingOptions == null || !shippingOptions.Any())
            //not found? let's load them using shipping service
            shippingOptions = (await _shippingService.GetShippingOptionsAsync(cart, await _customerService.GetCustomerShippingAddressAsync(await _workContext.GetCurrentCustomerAsync()),
                await _workContext.GetCurrentCustomerAsync(), shippingRateComputationMethodSystemName, (await _storeContext.GetCurrentStoreAsync()).Id)).ShippingOptions.ToList();
        else
            //loaded cached results. let's filter result by a chosen shipping rate computation method
            shippingOptions = shippingOptions.Where(so => so.ShippingRateComputationMethodSystemName.Equals(shippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase))
                .ToList();

        var shippingOption = shippingOptions
            .Find(so => !string.IsNullOrEmpty(so.Name) && so.Name.Equals(selectedName, StringComparison.InvariantCultureIgnoreCase));
        if (shippingOption == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(shippingOption)));

        //save
        await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedShippingOptionAttribute, shippingOption, (await _storeContext.GetCurrentStoreAsync()).Id);

        return Ok();
    }

    #endregion

    /// <summary>
    /// Get all payment methods
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(CheckoutPaymentMethodModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetPaymentMethods()
    {
        //validation
        if (_orderSettings.CheckoutDisabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (!cart.Any())
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "cartItems"));

        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        //Check whether payment workflow is required
        //we ignore reward points during cart total calculation
        var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart, false);
        if (!isPaymentWorkflowRequired)
        {
            await _genericAttributeService.SaveAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.SelectedPaymentMethodAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);
            return BadRequest("Payment workflow is not required");
        }

        //filter by country
        var filterByCountryId = 0;
        if (_addressSettings.CountryEnabled)
            filterByCountryId = (await _customerService.GetCustomerBillingAddressAsync(await _workContext.GetCurrentCustomerAsync()))?.CountryId ?? 0;

        //model
        var paymentMethodModel = await _checkoutModelFactory.PreparePaymentMethodModelAsync(cart, filterByCountryId);

        return Ok(paymentMethodModel);
    }

    /// <summary>
    /// Select payment method
    /// </summary>
    /// <param name="paymentMethodName">Payment method name (provider system name)</param>
    /// <param name="useRewardPoints">Use reward points?</param>
    [HttpGet("{paymentMethodName}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> SelectPaymentMethod(string paymentMethodName, bool useRewardPoints = false)
    {
        //validation
        if (_orderSettings.CheckoutDisabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (!cart.Any())
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "cartItems"));

        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        //reward points
        if (_rewardPointsSettings.Enabled)
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.UseRewardPointsDuringCheckoutAttribute, useRewardPoints,
                (await _storeContext.GetCurrentStoreAsync()).Id);

        //Check whether payment workflow is required
        var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
        if (!isPaymentWorkflowRequired)
        {
            await _genericAttributeService.SaveAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.SelectedPaymentMethodAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);
            return BadRequest("Payment workflow is not required");
        }
        //payment method 
        if (string.IsNullOrEmpty(paymentMethodName))
            return BadRequest("Invalid payment method");

        if (!await _paymentPluginManager.IsPluginActiveAsync(paymentMethodName, await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id))
            return BadRequest("Invalid payment method");

        //save
        await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
            NopCustomerDefaults.SelectedPaymentMethodAttribute, paymentMethodName, (await _storeContext.GetCurrentStoreAsync()).Id);

        return Ok();
    }

    /// <summary>
    /// Prepare payment info model
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetPaymentInfo()
    {
        //validation
        if (_orderSettings.CheckoutDisabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (!cart.Any())
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "cartItems"));

        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        //Check whether payment workflow is required
        var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
        if (!isPaymentWorkflowRequired)
            return BadRequest("Payment workflow is not required");

        //load payment method
        var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
            NopCustomerDefaults.SelectedPaymentMethodAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
        var paymentMethod = await _paymentPluginManager
            .LoadPluginBySystemNameAsync(paymentMethodSystemName, await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);
        if (paymentMethod == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(paymentMethod)));

        if (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection)
        {
            var response = new RedirectionResponse
            {
                PaymentMethodType = PaymentMethodType.Redirection
            };
            switch (paymentMethodSystemName)
            {
                case PaymentMethodDefaults.PAY_PAL_STANDARD:
                    {
                        response.PaymentMethodName = PaymentMethodDefaults.PAY_PAL_STANDARD;
                        response.RedirectionTip = await _localizationService.GetResourceAsync("Plugins.Payments.PayPalStandard.Fields.RedirectionTip");
                    }
                    break;
                case PaymentMethodDefaults.SKRILL:
                    {
                        response.PaymentMethodName = PaymentMethodDefaults.SKRILL;
                        response.RedirectionTip = "";
                    }
                    break;
                case PaymentMethodDefaults.TWO_CHECKOUT:
                    {
                        response.PaymentMethodName = PaymentMethodDefaults.TWO_CHECKOUT;
                        response.RedirectionTip = await _localizationService.GetResourceAsync("Plugins.Payments.2Checkout.RedirectionTip");
                    }
                    break;
            }

            //Check whether payment info should be skipped
            if (paymentMethod.SkipPaymentInfo || _paymentSettings.SkipPaymentInfoStepForRedirectionPaymentMethods)
                response.RedirectToConfirmOrder = true;
            return Ok(response);
        }

        return paymentMethodSystemName switch
        {
            PaymentMethodDefaults.MANUAL => Ok(await _apiCommonModelFactory.PrepareManualResponseAsync()),
            PaymentMethodDefaults.AUTHORIZE_NET => Ok(await _apiCommonModelFactory.PrepareAuthorizeNetResponseAsync()),
            PaymentMethodDefaults.CHECK_MONEY_ORDER => Ok(await _apiCommonModelFactory.PrepareCheckMoneyOrderResponseAsync()),
            PaymentMethodDefaults.BRAIN_TREE => Ok(await _apiCommonModelFactory.PrepareBrainTreeResponseAsync()),
            PaymentMethodDefaults.PURCHASE_ORDER => Ok(await _apiCommonModelFactory.PreparePurchaseOrderResponseAsync()),
            _ => Ok(),
        };
    }

    /// <summary>
    /// Validate the payment info
    /// </summary>
    /// <param name="confirmOrder">True to confirm after validation success</param>
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidatePaymentInfoResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> ValidatePaymentInfo(PaymentInfoRequest request, bool confirmOrder = false)
    {
        //validation
        if (_orderSettings.CheckoutDisabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (!cart.Any())
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "cartItems"));

        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        if (request == null)
            request = new PaymentInfoRequest();

        var validationResult = await ValidatePaymentInfoAsync(cart, request.PaymentInfo, request.PreviousOrderGuid,
            request.PreviousOrderGuidGeneratedOnUtc, true);

        if (confirmOrder)
            return Ok(await ConfirmOrderAsync(validationResult, request.PaymentInfo, request.PreviousOrderGuid,
            request.PreviousOrderGuidGeneratedOnUtc));

        return Ok(validationResult);
    }

    /// <summary>
    /// Prepare confirm order model
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(CheckoutConfirmModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetConfirmOrder()
    {
        //validation
        if (_orderSettings.CheckoutDisabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (!cart.Any())
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "cartItems"));

        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        //model
        var model = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);
        return Ok(model);
    }

    /// <summary>
    /// Confirm and pay the order
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> ConfirmOrder(PaymentInfoRequest request)
    {
        //validation
        if (_orderSettings.CheckoutDisabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (!cart.Any())
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "cartItems"));

        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var validationResult = await ValidatePaymentInfoAsync(cart, request.PaymentInfo, request.PreviousOrderGuid,
            request.PreviousOrderGuidGeneratedOnUtc);

        if (!validationResult.Success)
            return BadRequest(validationResult.Errors);

        return Ok(await ConfirmOrderAsync(validationResult, request.PaymentInfo, request.PreviousOrderGuid,
            request.PreviousOrderGuidGeneratedOnUtc));
    }

    /// <summary>
    /// Get completed order info
    /// </summary>
    /// <param name="orderId">The order identifier (If not provided then will pick the last placed order)</param>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(CheckoutCompletedModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetOrderCompleted(int? orderId)
    {
        //validation
        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        Order order = null;
        if (orderId.HasValue)
            //load order by identifier (if provided)
            order = await _orderService.GetOrderByIdAsync(orderId.Value);
        if (order == null)
            order = (await _orderService.SearchOrdersAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
            customerId: (await _workContext.GetCurrentCustomerAsync()).Id, pageSize: 1))
                .FirstOrDefault();
        if (order == null || order.Deleted || (await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(order)));

        //disable "order completed" page?
        if (_orderSettings.DisableOrderCompletedPage)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        //model
        var model = await _checkoutModelFactory.PrepareCheckoutCompletedModelAsync(order);
        return Ok(model);
    }

    #endregion
}
