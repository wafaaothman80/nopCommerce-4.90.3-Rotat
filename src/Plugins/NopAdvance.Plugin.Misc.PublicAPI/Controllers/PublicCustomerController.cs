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
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Attributes;
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Common;
using Nop.Web.Models.Customer;
using NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;
using NopAdvance.Plugin.Misc.PublicAPI.Filters;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure.Extensions;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;
using NopAdvance.Plugin.Misc.PublicAPI.Services;
using Nop.Core.Domain.Messages;
using System.Reflection;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers;

/// <summary>
/// Customer methods
/// </summary>
public partial class PublicCustomerController : BaseAPIController
{
    #region Fields

    private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
    private readonly MediaSettings _mediaSettings;
    private readonly CustomerSettings _customerSettings;
    private readonly GdprSettings _gdprSettings;
    private readonly DateTimeSettings _dateTimeSettings;
    private readonly TaxSettings _taxSettings;
    private readonly LocalizationSettings _localizationSettings;
    private readonly ForumSettings _forumSettings;
    private readonly AddressSettings _addressSettings;
    private readonly IAuthenticationService _authenticationService;
    private readonly ICustomerService _customerService;
    private readonly IPermissionService _permissionService;
    private readonly IAPIService _apiService;
    private readonly ICustomerModelFactory _customerModelFactory;
    private readonly IWorkContext _workContext;
    private readonly ICustomerRegistrationService _customerRegistrationService;
    private readonly ILocalizationService _localizationService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IEventPublisher _eventPublisher;
    private readonly IWorkflowMessageService _workflowMessageService;
    private readonly IGdprService _gdprService;
    private readonly IStoreContext _storeContext;
    private readonly IAttributeParser<CustomerAttribute, CustomerAttributeValue> _customerAttributeParser;
    private readonly IAttributeService<CustomerAttribute, CustomerAttributeValue> _customerAttributeService;
    private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
    private readonly ITaxService _taxService;
    private readonly IAddressService _addressService;
    private readonly ICountryService _countryService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly ILogger _logger;
    private readonly IAddressModelFactory _addressModelFactory;
    private readonly IAttributeParser<AddressAttribute, AddressAttributeValue> _addressAttributeParser;
    private readonly IPictureService _pictureService;
    private readonly IGiftCardService _giftCardService;
    private readonly ICurrencyService _currencyService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IExportManager _exportManager;
    private readonly IExternalAuthenticationService _externalAuthenticationService;
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    #endregion

    #region Ctor

    public PublicCustomerController(ExternalAuthenticationSettings externalAuthenticationSettings,
        MediaSettings mediaSettings,
        CustomerSettings customerSettings,
        GdprSettings gdprSettings,
        DateTimeSettings dateTimeSettings,
        TaxSettings taxSettings,
        LocalizationSettings localizationSettings,
        ForumSettings forumSettings,
        AddressSettings addressSettings,
        IAuthenticationService authenticationService,
        ICustomerService customerService,
        IPermissionService permissionService,
        IAPIService apiService,
        ICustomerModelFactory customerModelFactory,
        IWorkContext workContext,
        ICustomerRegistrationService customerRegistrationService,
        ILocalizationService localizationService,
        ICustomerActivityService customerActivityService,
        IGenericAttributeService genericAttributeService,
        IEventPublisher eventPublisher,
        IWorkflowMessageService workflowMessageService,
        IGdprService gdprService,
        IStoreContext storeContext,
        IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
        IAttributeService<CustomerAttribute,CustomerAttributeValue> customerAttributeService,
        INewsLetterSubscriptionService newsLetterSubscriptionService,
        ITaxService taxService,
        IAddressService addressService,
        ICountryService countryService,
        IStateProvinceService stateProvinceService,
        ILogger logger,
        IAddressModelFactory addressModelFactory,
        IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
        IPictureService pictureService,
        IGiftCardService giftCardService,
        ICurrencyService currencyService,
        IPriceFormatter priceFormatter,
        IExportManager exportManager,
        IExternalAuthenticationService externalAuthenticationService,
        IOrderService orderService,
        IProductService productService,
        IShoppingCartService shoppingCartService,
        IHttpContextAccessor httpContextAccessor)
    {
        _externalAuthenticationSettings = externalAuthenticationSettings;
        _mediaSettings = mediaSettings;
        _customerSettings = customerSettings;
        _gdprSettings = gdprSettings;
        _dateTimeSettings = dateTimeSettings;
        _taxSettings = taxSettings;
        _localizationSettings = localizationSettings;
        _forumSettings = forumSettings;
        _addressSettings = addressSettings;
        _authenticationService = authenticationService;
        _customerService = customerService;
        _permissionService = permissionService;
        _apiService = apiService;
        _customerModelFactory = customerModelFactory;
        _workContext = workContext;
        _customerRegistrationService = customerRegistrationService;
        _localizationService = localizationService;
        _customerActivityService = customerActivityService;
        _genericAttributeService = genericAttributeService;
        _eventPublisher = eventPublisher;
        _workflowMessageService = workflowMessageService;
        _gdprService = gdprService;
        _storeContext = storeContext;
        _customerAttributeParser = customerAttributeParser;
        _customerAttributeService = customerAttributeService;
        _newsLetterSubscriptionService = newsLetterSubscriptionService;
        _taxService = taxService;
        _addressService = addressService;
        _countryService = countryService;
        _stateProvinceService = stateProvinceService;
        _logger = logger;
        _addressModelFactory = addressModelFactory;
        _addressAttributeParser = addressAttributeParser;
        _pictureService = pictureService;
        _giftCardService = giftCardService;
        _currencyService = currencyService;
        _priceFormatter = priceFormatter;
        _exportManager = exportManager;
        _externalAuthenticationService = externalAuthenticationService;
        _orderService = orderService;
        _productService = productService;
        _shoppingCartService = shoppingCartService;
        _httpContextAccessor = httpContextAccessor;
    }

    #endregion

    #region Utilities

    protected virtual DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        var expiryDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        expiryDateTimeUtc = expiryDateTimeUtc.AddSeconds(unixTimeStamp).ToUniversalTime();
        return expiryDateTimeUtc;
    }

    protected virtual async Task<string> ParseCustomCustomerAttributesAsync(IFormCollection form)
    {
        if (form == null)
            throw new ArgumentNullException(nameof(form));

        var attributesXml = "";
        var attributes = await _customerAttributeService.GetAllAttributesAsync();
        foreach (var attribute in attributes)
        {
            var controlId = $"{NopCustomerServicesDefaults.CustomerAttributePrefix}{attribute.Id}";
            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var selectedAttributeId = int.Parse(ctrlAttributes);
                            if (selectedAttributeId > 0)
                                attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }
                    }
                    break;
                case AttributeControlType.Checkboxes:
                    {
                        var cblAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(cblAttributes))
                            foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId > 0)
                                    attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                    }
                    break;
                case AttributeControlType.ReadonlyCheckboxes:
                    {
                        //load read-only (already server-side selected) values
                        var attributeValues = await _customerAttributeService.GetAttributeValuesAsync(attribute.Id);
                        foreach (var selectedAttributeId in attributeValues
                            .Where(v => v.IsPreSelected)
                            .Select(v => v.Id)
                            .ToList())
                            attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                attribute, selectedAttributeId.ToString());
                    }
                    break;
                case AttributeControlType.TextBox:
                case AttributeControlType.MultilineTextbox:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var enteredText = ctrlAttributes.ToString().Trim();
                            attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                attribute, enteredText);
                        }
                    }
                    break;
                case AttributeControlType.Datepicker:
                case AttributeControlType.ColorSquares:
                case AttributeControlType.ImageSquares:
                case AttributeControlType.FileUpload:
                //not supported customer attributes
                default:
                    break;
            }
        }

        return attributesXml;
    }

    protected virtual void ValidateRequiredConsents(List<GdprConsent> consents, IFormCollection form)
    {
        foreach (var consent in consents)
        {
            var controlId = $"consent{consent.Id}";
            var cbConsent = form[controlId];
            if (StringValues.IsNullOrEmpty(cbConsent) || !cbConsent.ToString().Equals("on"))
                ModelState.AddModelError("", consent.RequiredMessage);
        }
    }

    protected virtual async Task LogGdprAsync(Customer customer, CustomerInfoModel oldCustomerInfoModel, CustomerInfoRequest newCustomerInfoModel, IFormCollection form)
    {
        try
        {
            //consents
            var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayOnCustomerInfoPage).ToList();
            foreach (var consent in consents)
            {
                var previousConsentValue = await _gdprService.IsConsentAcceptedAsync(consent.Id, (await _workContext.GetCurrentCustomerAsync()).Id);
                var controlId = $"consent{consent.Id}";
                var cbConsent = form[controlId];
                if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                    //agree
                    if (!previousConsentValue.HasValue || !previousConsentValue.Value)
                        await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                    else
                    //disagree
                    if (!previousConsentValue.HasValue || previousConsentValue.Value)
                        await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
            }

            if (_gdprSettings.LogNewsletterConsent && _customerSettings.NewsletterEnabled)
            {
                var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;

                // old state from DB (before update)
                var oldSub = await GetNewsletterSubscriptionAsync(customer.Email, storeId);
                var oldIsSubscribed = oldSub?.Active ?? false;

                // new state from request
                var newIsSubscribed = newCustomerInfoModel.Newsletter;

                if (oldIsSubscribed && !newIsSubscribed)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentDisagree,
                        await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));

                if (!oldIsSubscribed && newIsSubscribed)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree,
                        await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
            }


            //user profile changes
            if (!_gdprSettings.LogUserProfileChanges)
                return;

            if (oldCustomerInfoModel.Gender != newCustomerInfoModel.Gender)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Gender")} = {newCustomerInfoModel.Gender}");

            if (oldCustomerInfoModel.FirstName != newCustomerInfoModel.FirstName)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.FirstName")} = {newCustomerInfoModel.FirstName}");

            if (oldCustomerInfoModel.LastName != newCustomerInfoModel.LastName)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.LastName")} = {newCustomerInfoModel.LastName}");

            if (oldCustomerInfoModel.ParseDateOfBirth() != newCustomerInfoModel.ParseDateOfBirth())
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.DateOfBirth")} = {newCustomerInfoModel.ParseDateOfBirth()}");

            if (oldCustomerInfoModel.Email != newCustomerInfoModel.Email)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Email")} = {newCustomerInfoModel.Email}");

            if (oldCustomerInfoModel.Company != newCustomerInfoModel.Company)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Company")} = {newCustomerInfoModel.Company}");

            if (oldCustomerInfoModel.StreetAddress != newCustomerInfoModel.StreetAddress)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StreetAddress")} = {newCustomerInfoModel.StreetAddress}");

            if (oldCustomerInfoModel.StreetAddress2 != newCustomerInfoModel.StreetAddress2)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StreetAddress2")} = {newCustomerInfoModel.StreetAddress2}");

            if (oldCustomerInfoModel.ZipPostalCode != newCustomerInfoModel.ZipPostalCode)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.ZipPostalCode")} = {newCustomerInfoModel.ZipPostalCode}");

            if (oldCustomerInfoModel.City != newCustomerInfoModel.City)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.City")} = {newCustomerInfoModel.City}");

            if (oldCustomerInfoModel.County != newCustomerInfoModel.County)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.County")} = {newCustomerInfoModel.County}");

            if (oldCustomerInfoModel.CountryId != newCustomerInfoModel.CountryId)
            {
                var countryName = (await _countryService.GetCountryByIdAsync(newCustomerInfoModel.CountryId))?.Name;
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Country")} = {countryName}");
            }

            if (oldCustomerInfoModel.StateProvinceId != newCustomerInfoModel.StateProvinceId)
            {
                var stateProvinceName = (await _stateProvinceService.GetStateProvinceByIdAsync(newCustomerInfoModel.StateProvinceId))?.Name;
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StateProvince")} = {stateProvinceName}");
            }
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync(exception.Message, exception, customer);
        }
    }
    protected virtual async Task<NewsLetterSubscription> GetNewsletterSubscriptionAsync(string email, int storeId)
    {
        // Some nop versions have async, some have sync. Support both.
        var svcType = _newsLetterSubscriptionService.GetType();

        // Try async method name 1
        var m1 = svcType.GetMethod("GetNewsLetterSubscriptionByEmailAndStoreIdAsync");
        if (m1 != null)
        {
            var task = (Task)m1.Invoke(_newsLetterSubscriptionService, new object[] { email, storeId });
            await task.ConfigureAwait(false);
            return (NewsLetterSubscription)task.GetType().GetProperty("Result")!.GetValue(task)!;
        }

        // Try sync method
        var m2 = svcType.GetMethod("GetNewsLetterSubscriptionByEmailAndStoreId");
        if (m2 != null)
            return (NewsLetterSubscription)m2.Invoke(_newsLetterSubscriptionService, new object[] { email, storeId })!;

        // Fallback: search all subscriptions (works if service exposes GetAllNewsLetterSubscriptionsAsync)
        var m3 = svcType.GetMethod("GetAllNewsLetterSubscriptionsAsync");
        if (m3 != null)
        {
            var task = (Task)m3.Invoke(_newsLetterSubscriptionService, new object[] { email, storeId, true })!;
            await task.ConfigureAwait(false);
            var result = task.GetType().GetProperty("Result")!.GetValue(task);
            // result is usually IPagedList<NewsLetterSubscription> or IList<NewsLetterSubscription>
            var enumerable = (System.Collections.IEnumerable)result!;
            foreach (var item in enumerable)
            {
                var sub = (NewsLetterSubscription)item;
                if (sub.Email?.Equals(email, StringComparison.OrdinalIgnoreCase) == true && sub.StoreId == storeId)
                    return sub;
            }
        }

        throw new MissingMethodException("No compatible newsletter subscription lookup method found on INewsLetterSubscriptionService for this nopCommerce version.");
    }

    protected virtual async Task<LoginResponse> SignInCustomerAsync(Customer customer, int applicationId)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var previousCustomerId = 0;
        if (!currentCustomer.IsSystemAccount && currentCustomer?.Id != customer.Id)
        {
            previousCustomerId = currentCustomer.Id;

            //migrate shopping cart
            await _shoppingCartService.MigrateShoppingCartAsync(currentCustomer, customer, true);

            await _workContext.SetCurrentCustomerAsync(customer);
        }

        //sign in new customer
        await _authenticationService.SignInAsync(customer, false);

        //raise event       
        await _eventPublisher.PublishAsync(new CustomerLoggedinEvent(customer));

        //activity log
        await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Login",
            await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Login"), customer);

        if (previousCustomerId > 0)
        {
            var accessTokenId = _apiService.GetTokenId(_httpContextAccessor.HttpContext.Request.Headers[AuthenticationDefaults.AUTHORIZATION_KEY_NAME].FirstOrDefault());
            if (accessTokenId.HasValue)
            {
                var refreshToken = await _apiService.GetAPIRefreshTokenAsync(applicationId, previousCustomerId, accessTokenId.Value);
                if (refreshToken != null)
                    await _apiService.DeleteAPIRefreshTokenAsync(refreshToken);
            }
        }

        var tokens = await _apiService.GenerateTokensAsync(customer, applicationId);
        var response = new LoginResponse
        {
            CustomerId = tokens.CustomerId,
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            IsImpersonationAllowed = await _permissionService.AuthorizeAsync(StandardPermission.Customers.CUSTOMERS_IMPERSONATION, customer),
        };

        return response;
    }

    protected virtual async Task<RegisterResponse> RegisterCustomerAsync(Customer customer, bool login, int applicationId)
    {
        var accessTokenId = _apiService.GetTokenId(_httpContextAccessor.HttpContext.Request.Headers[AuthenticationDefaults.AUTHORIZATION_KEY_NAME].FirstOrDefault());
        if (accessTokenId.HasValue)
        {
            var refreshToken = await _apiService.GetAPIRefreshTokenAsync(applicationId, customer.Id, accessTokenId.Value);
            if (refreshToken != null)
                await _apiService.DeleteAPIRefreshTokenAsync(refreshToken);
        }

        var result = await _customerModelFactory.PrepareRegisterResultModelAsync((int)UserRegistrationType.Standard, "");
        var response = new RegisterResponse
        {
            Message = result.Result,
            CustomerId = customer.Id
        };

        if (login)
        {
            var tokens = await _apiService.GenerateTokensAsync(customer, applicationId);
            response.AccessToken = tokens.AccessToken;
            response.RefreshToken = tokens.RefreshToken;

            //raise event       
            await _eventPublisher.PublishAsync(new CustomerLoggedinEvent(customer));

            //activity log
            await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Login",
                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Login"), customer);
        }

        return response;
    }

    #endregion

    #region Methods

    #region Impersonation

    /// <summary>
    /// Impersonate a customer
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ImpersonateResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> Impersonate(ImpersonationRequest request)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Customers.CUSTOMERS_IMPERSONATION))
            return Unauthorized();

        var customer = await _customerService.GetCustomerByEmailAsync(request.UserEmail);

        if (customer == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(customer)));

        if (!customer.Active)
            return BadRequest(await _localizationService.GetResourceAsync("Admin.Customers.Customers.Impersonate.Inactive"));

        //ensure that a non-admin user cannot impersonate as an administrator
        //otherwise, that user can simply impersonate as an administrator and gain additional administrative privileges
        if (!await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()) && await _customerService.IsAdminAsync(customer))
            return BadRequest(await _localizationService.GetResourceAsync("Admin.Customers.Customers.NonAdminNotImpersonateAsAdminError"));

        //activity log
        await _customerActivityService.InsertActivityAsync("Impersonation.Started",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Started.StoreOwner"), customer.Email, customer.Id), customer);
        await _customerActivityService.InsertActivityAsync(customer, "Impersonation.Started",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Started.Customer"), (await _workContext.GetCurrentCustomerAsync()).Email, (await _workContext.GetCurrentCustomerAsync()).Id), await _workContext.GetCurrentCustomerAsync());

        //ensure login is not required
        customer.RequireReLogin = false;
        await _customerService.UpdateCustomerAsync(customer);
        await _genericAttributeService.SaveAttributeAsync<int?>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.ImpersonatedCustomerIdAttribute, customer.Id);

        var response = new ImpersonateResponse
        {
            ImpersonatedCustomerName = await _customerService.IsRegisteredAsync(customer) ? await _customerService.FormatUsernameAsync(customer) : string.Empty
        };

        return Ok(response);
    }

    #endregion

    #region Login / Logout

    /// <summary>
    /// Get guest token to continue as a guest
    /// </summary>
    [HttpGet]
    [Authorize(true)]
    [CheckAccessPublicStore(true)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetGuestToken()
    {
        var guestRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.GuestsRoleName);
        var guestUserAllowed = await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.PUBLIC_STORE_ALLOW_NAVIGATION,
            guestRole.Id);
        if (!guestUserAllowed)
            return BadRequest("Guest users are disabled.");

        var customer = await _customerService.InsertGuestCustomerAsync();
        await _workContext.SetCurrentCustomerAsync(customer);

        var applicationId = await GetApplicationIdAsync(_apiService, _httpContextAccessor);
        if (applicationId == 0)
            return BadRequest(MessageDefaults.INVALID_API_KEY);

        var response = await _apiService.GenerateTokensAsync(customer, applicationId);

        return Ok(response);
    }

    /// <summary>
    /// Prepare login model
    /// </summary>
    /// <param name="checkoutAsGuest">Checkout as guest? (optional)</param>
    [HttpGet]
    [CheckAccessClosedStore(true)]
    [CheckAccessPublicStore(true)]
    [ProducesResponseType(typeof(LoginModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetLogin(bool? checkoutAsGuest)
    {
        var model = await _customerModelFactory.PrepareLoginModelAsync(checkoutAsGuest);

        return Ok(model);
    }

    /// <summary>
    /// Login
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [CheckAccessClosedStore(true)]
    [CheckAccessPublicStore(true)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> Login(LoginRequest request)
    {
        if (ModelState.IsValid)
        {
            var message = string.Empty;
            var loginResult = await _customerRegistrationService.ValidateCustomerAsync(request.UsernameOrEmail, request.Password);
            switch (loginResult)
            {
                case CustomerLoginResults.Successful:
                    {
                        var customer = _customerSettings.UsernamesEnabled
                            ? await _customerService.GetCustomerByUsernameAsync(request.UsernameOrEmail)
                            : await _customerService.GetCustomerByEmailAsync(request.UsernameOrEmail);

                        if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.PUBLIC_STORE_ALLOW_NAVIGATION, customer))
                            return BadRequest("Public store navigation is disabled");

                        var applicationId = await GetApplicationIdAsync(_apiService, _httpContextAccessor);
                        if (applicationId == 0)
                            return BadRequest(MessageDefaults.INVALID_API_KEY);

                        return Ok(await SignInCustomerAsync(customer, applicationId));
                    }
                case CustomerLoginResults.MultiFactorAuthenticationRequired:
                    break;
                case CustomerLoginResults.CustomerNotExist:
                    message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist");
                    break;
                case CustomerLoginResults.Deleted:
                    message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.Deleted");
                    break;
                case CustomerLoginResults.NotActive:
                    message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotActive");
                    break;
                case CustomerLoginResults.NotRegistered:
                    message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotRegistered");
                    break;
                case CustomerLoginResults.LockedOut:
                    message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.LockedOut");
                    break;
                case CustomerLoginResults.WrongPassword:
                default:
                    message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials");
                    break;
            }
            return BadRequest(message);
        }
        return PrepareBadRequest(ModelState);

    }

    /// <summary>
    /// Logout
    /// </summary>
    [HttpPost]
    [CheckAccessClosedStore(true)]
    [CheckAccessPublicStore(true)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> Logout()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (_workContext.OriginalCustomerIfImpersonated != null)
        {
            //activity log
            await _customerActivityService.InsertActivityAsync(_workContext.OriginalCustomerIfImpersonated, "Impersonation.Finished",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Finished.StoreOwner"),
                    customer.Email, customer.Id),
                customer);

            await _customerActivityService.InsertActivityAsync("Impersonation.Finished",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Finished.Customer"),
                    _workContext.OriginalCustomerIfImpersonated.Email, _workContext.OriginalCustomerIfImpersonated.Id),
                _workContext.OriginalCustomerIfImpersonated);

            //logout impersonated customer
            await _genericAttributeService
                .SaveAttributeAsync<int?>(_workContext.OriginalCustomerIfImpersonated, NopCustomerDefaults.ImpersonatedCustomerIdAttribute, null);

            return Ok();
        }

        //activity log
        await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Logout",
            await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Logout"), customer);

        //standard logout 
        var accessTokenId = _apiService.GetTokenId(_httpContextAccessor.HttpContext.Request.Headers[AuthenticationDefaults.AUTHORIZATION_KEY_NAME].FirstOrDefault());
        if (accessTokenId.HasValue)
        {
            var applicationId = await GetApplicationIdAsync(_apiService, _httpContextAccessor);
            if (applicationId == 0)
                return BadRequest(MessageDefaults.INVALID_API_KEY);

            var tokenEntity = await _apiService.GetAPIRefreshTokenAsync(applicationId, customer.Id, accessTokenId.Value);
            if (tokenEntity != null)
                await _apiService.DeleteAPIRefreshTokenAsync(tokenEntity);
        }

        await _authenticationService.SignOutAsync();

        //raise logged out event       
        await _eventPublisher.PublishAsync(new CustomerLoggedOutEvent(customer));

        return Ok();
    }

    /// <summary>
    /// Refresh access token on expiration
    /// </summary>
    [HttpPost]
    [Authorize(true)]
    [CheckAccessClosedStore(true)]
    [CheckAccessPublicStore(true)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
    {
        var authorization = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ");
        if (authorization != null && authorization.Length > 1 && authorization[0] == JwtBearerDefaults.AuthenticationScheme &&
               !string.IsNullOrEmpty(authorization[1]))
        {
            var accessToken = authorization[1];
            var (principal, error) = _apiService.GetPrincipalFromToken(accessToken, false);
            if (principal != null)
            {
                var tokenIdClaim = principal.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Jti);
                if (tokenIdClaim != null && Guid.TryParse(tokenIdClaim.Value, out var tokenId))
                {
                    var claimsIdentity = principal.Identity as ClaimsIdentity;
                    if (!int.TryParse(claimsIdentity.FindFirst(AuthenticationDefaults.CLAIMS_CUSTOMER_ID)?.Value, out var customerId))
                        return Unauthorized();

                    var applicationId = await GetApplicationIdAsync(_apiService, _httpContextAccessor);
                    if (applicationId == 0)
                        return BadRequest(MessageDefaults.INVALID_API_KEY);

                    var refreshToken = await _apiService.GetAPIRefreshTokenAsync(applicationId, _workContext.OriginalCustomerIfImpersonated?.Id ?? customerId, tokenId);
                    if (refreshToken == null || refreshToken.Token != request.RefreshToken || refreshToken.IsUsed ||
                        refreshToken.IsRevoked)
                        return BadRequest("Invalid refresh token.");

                    if (refreshToken.ExpiryInUtc < DateTime.UtcNow)
                        return BadRequest("Refresh token has been expired.");

                    //Check token expiration
                    var expiryDateUnix = double.Parse(principal.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                    var expiryDate = UnixTimeStampToDateTime(expiryDateUnix);
                    if (expiryDate > DateTime.UtcNow)
                        return BadRequest("Access token has not expired yet.");

                    var customer = await _customerService.GetCustomerByIdAsync(customerId);
                    if (customer == null)
                        return NotFound(string.Format(MessageDefaults.NOT_FOUND, "Customer"));

                    refreshToken.IsUsed = true;
                    await _apiService.UpdateAPIRefreshTokenAsync(refreshToken);

                    var tokens = await _apiService.GenerateTokensAsync(customer, applicationId);

                    var response = new RefreshTokenResponse
                    {
                        CustomerId = tokens.CustomerId,
                        AccessToken = tokens.AccessToken,
                        RefreshToken = tokens.RefreshToken,
                        IsCustomerImpersonated = _workContext.OriginalCustomerIfImpersonated != null
                    };

                    if (response.IsCustomerImpersonated)
                        response.ImpersonatedCustomerName = await _customerService.IsRegisteredAsync(customer) ? await _customerService.FormatUsernameAsync(customer) : string.Empty;

                    return Ok(response);
                }
            }
        }
        return Unauthorized();
    }

    #endregion

    #region Password recovery

    /// <summary>
    /// Prepare password recovery model
    /// </summary>
    [HttpGet]
    [CheckAccessPublicStore(true)]
    [CheckAccessClosedStore(true)]
    [ProducesResponseType(typeof(PasswordRecoveryModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetPasswordRecovery()
    {
        var model = new PasswordRecoveryModel();
        model = await _customerModelFactory.PreparePasswordRecoveryModelAsync(model);

        return Ok(model);
    }

    /// <summary>
    /// Submit password recovery request
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> PasswordRecovery(PasswordRecoveryRequest request)
    {
        if (ModelState.IsValid)
        {
            var customer = await _customerService.GetCustomerByEmailAsync(request.Email.Trim());
            if (customer != null && customer.Active && !customer.Deleted)
            {
                //save token and current date
                var passwordRecoveryToken = Guid.NewGuid();
                await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute,
                    passwordRecoveryToken.ToString());
                DateTime? generatedDateTime = DateTime.UtcNow;
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.PasswordRecoveryTokenDateGeneratedAttribute, generatedDateTime);

                //send email
                await _workflowMessageService.SendCustomerPasswordRecoveryMessageAsync(customer,
                    (await _workContext.GetWorkingLanguageAsync()).Id);

                return Ok(await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailHasBeenSent"));
            }
            else
                return BadRequest(await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailNotFound"));
        }
        return PrepareBadRequest(ModelState);
    }

    #endregion

    #region Register

    /// <summary>
    /// Prepare the customer registration model
    /// </summary>
    [HttpGet]
    [CheckAccessPublicStore(true)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RegisterModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetRegister()
    {
        //check whether registration is allowed
        if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = new RegisterModel();
        model = await _customerModelFactory.PrepareRegisterModelAsync(model, false, setDefaultValues: true);

        return Ok(model);
    }

    /// <summary>
    /// Submit the customer register request
    /// </summary>
    [HttpPost]
    [CheckAccessPublicStore(true)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> Register(RegisterRequest request)
    {
        //check whether registration is allowed
        if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var applicationId = await GetApplicationIdAsync(_apiService, _httpContextAccessor);
        if (applicationId == 0)
            return BadRequest(MessageDefaults.INVALID_API_KEY);

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        if (await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
        {
            //Already registered customer. 
            var accessTokenId = _apiService.GetTokenId(_httpContextAccessor.HttpContext.Request.Headers[AuthenticationDefaults.AUTHORIZATION_KEY_NAME].FirstOrDefault());
            if (accessTokenId.HasValue)
            {
                var tokenEntity = await _apiService.GetAPIRefreshTokenAsync(applicationId, (await _workContext.GetCurrentCustomerAsync()).Id, accessTokenId.Value);
                if (tokenEntity != null)
                    await _apiService.DeleteAPIRefreshTokenAsync(tokenEntity);
            }

            await _authenticationService.SignOutAsync();

            //raise logged out event       
            await _eventPublisher.PublishAsync(new CustomerLoggedOutEvent(await _workContext.GetCurrentCustomerAsync()));

            //Save a new record
            await _workContext.SetCurrentCustomerAsync(await _customerService.InsertGuestCustomerAsync());
        }
        var customer = await _workContext.GetCurrentCustomerAsync();
        customer.RegisteredInStoreId = (await _storeContext.GetCurrentStoreAsync()).Id;

        //custom customer attributes
        var customerAttributesXml = await ParseCustomCustomerAttributesAsync(new FormCollection(ConvertToFormCollection(request.CustomerAttributes)));
        var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
        foreach (var error in customerAttributeWarnings)
            ModelState.AddModelError("", error);

        //GDPR
        if (_gdprSettings.GdprEnabled)
        {
            var consents = (await _gdprService
                .GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration && consent.IsRequired).ToList();

            ValidateRequiredConsents(consents, new FormCollection(ConvertToFormCollection(request.GdprConsents)));
        }

        if (ModelState.IsValid)
        {
            var customerUserName = request.Username?.Trim();
            var customerEmail = request.Email?.Trim();

            var isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
            var registrationRequest = new CustomerRegistrationRequest(customer,
                customerEmail,
                _customerSettings.UsernamesEnabled ? customerUserName : customerEmail,
                request.Password,
                _customerSettings.DefaultPasswordFormat,
                (await _storeContext.GetCurrentStoreAsync()).Id,
                isApproved);
            var registrationResult = await _customerRegistrationService.RegisterCustomerAsync(registrationRequest);
            if (registrationResult.Success)
            {
                //properties
                if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    customer.TimeZoneId = request.TimeZoneId;

                //VAT number
                if (_taxSettings.EuVatEnabled)
                {
                    customer.VatNumber = request.VatNumber;

                    var (vatNumberStatus, _, vatAddress) = await _taxService.GetVatNumberStatusAsync(request.VatNumber);
                    customer.VatNumberStatusId = (int)vatNumberStatus;
                    //send VAT number admin notification
                    if (!string.IsNullOrEmpty(request.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                        await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer, request.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                }

                //form fields
                if (_customerSettings.GenderEnabled)
                    customer.Gender = request.Gender;
                if (_customerSettings.FirstNameEnabled)
                    customer.FirstName = request.FirstName;
                if (_customerSettings.LastNameEnabled)
                    customer.LastName = request.LastName;
                if (_customerSettings.DateOfBirthEnabled)
                    customer.DateOfBirth = request.ParseDateOfBirth();
                if (_customerSettings.CompanyEnabled)
                    customer.Company = request.Company;
                if (_customerSettings.StreetAddressEnabled)
                    customer.StreetAddress = request.StreetAddress;
                if (_customerSettings.StreetAddress2Enabled)
                    customer.StreetAddress2 = request.StreetAddress2;
                if (_customerSettings.ZipPostalCodeEnabled)
                    customer.ZipPostalCode = request.ZipPostalCode;
                if (_customerSettings.CityEnabled)
                    customer.City = request.City;
                if (_customerSettings.CountyEnabled)
                    customer.County = request.County;
                if (_customerSettings.CountryEnabled)
                    customer.CountryId = request.CountryId;
                if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                    customer.StateProvinceId = request.StateProvinceId;
                if (_customerSettings.PhoneEnabled)
                    customer.Phone = request.Phone;
                if (_customerSettings.FaxEnabled)
                    customer.Fax = request.Fax;

                //save customer attributes
                customer.CustomCustomerAttributesXML = customerAttributesXml;
                await _customerService.UpdateCustomerAsync(customer);

                //newsletter
                if (_customerSettings.NewsletterEnabled)
                {
                    var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
                    var existing = await GetNewsletterSubscriptionAsync(customer.Email, storeId);

                    if (request.Newsletter)
                    {
                        if (existing != null)
                        {
                            existing.Active = true;
                            await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(existing);
                        }
                        else
                        {
                            await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new NewsLetterSubscription
                            {
                                NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                Email = customer.Email,
                                Active = true,
                                StoreId = storeId,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                        }
                    }
                    else
                    {
                        if (existing != null)
                            await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(existing);
                    }
                }


                if (_customerSettings.AcceptPrivacyPolicyEnabled)
                    //privacy policy is required
                    //GDPR
                    if (_gdprSettings.GdprEnabled && _gdprSettings.LogPrivacyPolicyConsent)
                        await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.PrivacyPolicy"));

                //GDPR
                if (_gdprSettings.GdprEnabled)
                {
                    var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration).ToList();
                    foreach (var consent in consents)
                    {
                        var controlId = $"consent{consent.Id}";
                        var cbConsent = request.GdprConsents[controlId];
                        if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                            //agree
                            await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                        else
                            //disagree
                            await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                    }
                }

                //insert default address (if possible)
                var defaultAddress = new Address
                {
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    Company = customer.Company,
                    CountryId = customer.CountryId > 0
                        ? customer.CountryId
                        : null,
                    StateProvinceId = customer.StateProvinceId > 0
                        ? customer.StateProvinceId
                        : null,
                    County = customer.County,
                    City = customer.City,
                    Address1 = customer.StreetAddress,
                    Address2 = customer.StreetAddress2,
                    ZipPostalCode = customer.ZipPostalCode,
                    PhoneNumber = customer.Phone,
                    FaxNumber = customer.Fax,
                    CreatedOnUtc = customer.CreatedOnUtc
                };
                if (await _addressService.IsAddressValidAsync(defaultAddress))
                {
                    //some validation
                    if (defaultAddress.CountryId == 0)
                        defaultAddress.CountryId = null;
                    if (defaultAddress.StateProvinceId == 0)
                        defaultAddress.StateProvinceId = null;

                    await _addressService.InsertAddressAsync(defaultAddress);

                    await _customerService.InsertCustomerAddressAsync(customer, defaultAddress);

                    customer.BillingAddressId = defaultAddress.Id;
                    customer.ShippingAddressId = defaultAddress.Id;

                    await _customerService.UpdateCustomerAsync(customer);
                }

                //notifications
                if (_customerSettings.NotifyNewCustomerRegistration)
                    await _workflowMessageService.SendCustomerRegisteredStoreOwnerNotificationMessageAsync(customer,
                        _localizationSettings.DefaultAdminLanguageId);

                //raise event       
                await _eventPublisher.PublishAsync(new CustomerRegisteredEvent(customer));

                switch (_customerSettings.UserRegistrationType)
                {
                    case UserRegistrationType.EmailValidation:
                        {
                            //email validation message
                            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
                            await _workflowMessageService.SendCustomerEmailValidationMessageAsync(customer, (await _workContext.GetWorkingLanguageAsync()).Id);

                            var result = await _customerModelFactory.PrepareRegisterResultModelAsync((int)UserRegistrationType.EmailValidation, "");
                            return Ok(new RegisterResponse { Message = result.Result });
                        }
                    case UserRegistrationType.AdminApproval:
                        {
                            var result = await _customerModelFactory.PrepareRegisterResultModelAsync((int)UserRegistrationType.AdminApproval, "");
                            return Ok(new RegisterResponse { Message = result.Result });
                        }
                    case UserRegistrationType.Standard:
                        {
                            //send customer welcome message
                            await _workflowMessageService.SendCustomerWelcomeMessageAsync(customer, (await _workContext.GetWorkingLanguageAsync()).Id);

                            //raise event       
                            await _eventPublisher.PublishAsync(new CustomerActivatedEvent(customer));

                            return Ok(await RegisterCustomerAsync(customer, request.LoginAfterRegistration, applicationId));
                        }
                }
            }

            //errors
            foreach (var error in registrationResult.Errors)
                ModelState.AddModelError("", error);
        }

        return PrepareBadRequest(ModelState);
    }

    /// <summary>
    /// Check username availability
    /// </summary>
    /// <param name="username">Username</param>
    [HttpPost("{username}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> CheckUsernameAvailability(string username)
    {
        if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
            return BadRequest(await _localizationService.GetResourceAsync("Account.Register.Result.Disabled"));

        var usernameAvailable = false;
        var statusText = await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.NotAvailable");

        if (!UsernamePropertyValidator<string, string>.IsValid(username, _customerSettings))
            statusText = await _localizationService.GetResourceAsync("Account.Fields.Username.NotValid");
        else if (_customerSettings.UsernamesEnabled && !string.IsNullOrWhiteSpace(username))
            if (await _workContext.GetCurrentCustomerAsync() != null &&
                (await _workContext.GetCurrentCustomerAsync()).Username != null &&
                (await _workContext.GetCurrentCustomerAsync()).Username.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                statusText = await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.CurrentUsername");
            else
            {
                var customer = await _customerService.GetCustomerByUsernameAsync(username);
                if (customer == null)
                {
                    statusText = await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.Available");
                    usernameAvailable = true;
                }
            }

        if (usernameAvailable)
            return Ok(statusText);

        return BadRequest(statusText);
    }

    #endregion

    #region My account / Info

    /// <summary>
    /// Prepare customer navigation model
    /// </summary>
    /// <param name="selectedTab">Selected/Current tab</param>
    [HttpGet]
    [ProducesResponseType(typeof(CustomerNavigationResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetCustomerNavigation(CustomerNavigationEnum selectedTab = CustomerNavigationEnum.Info)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        var model = await _customerModelFactory.PrepareCustomerNavigationModelAsync((int)selectedTab);
        var response = new CustomerNavigationResponse
        {
            SelectedTab = (CustomerNavigationEnum)model.SelectedTab
        };

        for (var i = 0; i < model.CustomerNavigationItems.Count; i++)
        {
            var item = model.CustomerNavigationItems[i];
            var customerNavigationItem = new CustomerNavigationItemResponse
            {
                ItemClass = item.ItemClass,
                RouteName = item.RouteName,
                Tab = (CustomerNavigationEnum)item.Tab,
                Title = item.Title
            };
            response.CustomerNavigationItems.Add(customerNavigationItem);
        }

        return Ok(response);
    }

    /// <summary>
    /// Get the customer info
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(CustomerInfoModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetInfo()
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        var model = new CustomerInfoModel();
        model = await _customerModelFactory.PrepareCustomerInfoModelAsync(model, await _workContext.GetCurrentCustomerAsync(), false);
        return Ok(model);
    }

    /// <summary>
    /// Update the customer info
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> UpdateInfo(CustomerInfoRequest request)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        var oldCustomerModel = new CustomerInfoModel();

        var customer = await _workContext.GetCurrentCustomerAsync();

        //get customer info model before changes for gdpr log
        if (_gdprSettings.GdprEnabled & _gdprSettings.LogUserProfileChanges)
            oldCustomerModel = await _customerModelFactory.PrepareCustomerInfoModelAsync(oldCustomerModel, customer, false);

        //custom customer attributes
        var customerAttributesXml = await ParseCustomCustomerAttributesAsync(new FormCollection(ConvertToFormCollection(request.CustomerAttributes)));
        var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
        foreach (var error in customerAttributeWarnings)
            ModelState.AddModelError("", error);

        //GDPR
        if (_gdprSettings.GdprEnabled)
        {
            var consents = (await _gdprService
                .GetAllConsentsAsync()).Where(consent => consent.DisplayOnCustomerInfoPage && consent.IsRequired).ToList();

            ValidateRequiredConsents(consents, new FormCollection(ConvertToFormCollection(request.GdprConsents)));
        }

        try
        {
            if (ModelState.IsValid)
            {
                //username 
                if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames)
                {
                    var userName = request.Username.Trim();
                    if (!customer.Username.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
                        //change username
                        await _customerRegistrationService.SetUsernameAsync(customer, userName);
                }
                //email
                var email = request.Email.Trim();
                if (!customer.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase))
                {
                    //change email
                    var requireValidation = _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation;
                    await _customerRegistrationService.SetEmailAsync(customer, email, requireValidation);
                }

                //properties
                if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    customer.TimeZoneId = request.TimeZoneId;
                //VAT number
                if (_taxSettings.EuVatEnabled)
                {
                    var prevVatNumber = customer.VatNumber;
                    customer.VatNumber = request.VatNumber;

                    if (prevVatNumber != request.VatNumber)
                    {
                        var (vatNumberStatus, _, vatAddress) = await _taxService.GetVatNumberStatusAsync(request.VatNumber);
                        customer.VatNumberStatusId = (int)vatNumberStatus;

                        //send VAT number admin notification
                        if (!string.IsNullOrEmpty(request.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                            await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer,
                                request.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                    }
                }

                //form fields
                if (_customerSettings.GenderEnabled)
                    customer.Gender = request.Gender;
                if (_customerSettings.FirstNameEnabled)
                    customer.FirstName = request.FirstName;
                if (_customerSettings.LastNameEnabled)
                    customer.LastName = request.LastName;
                if (_customerSettings.DateOfBirthEnabled)
                    customer.DateOfBirth = request.ParseDateOfBirth();
                if (_customerSettings.CompanyEnabled)
                    customer.Company = request.Company;
                if (_customerSettings.StreetAddressEnabled)
                    customer.StreetAddress = request.StreetAddress;
                if (_customerSettings.StreetAddress2Enabled)
                    customer.StreetAddress2 = request.StreetAddress2;
                if (_customerSettings.ZipPostalCodeEnabled)
                    customer.ZipPostalCode = request.ZipPostalCode;
                if (_customerSettings.CityEnabled)
                    customer.City = request.City;
                if (_customerSettings.CountyEnabled)
                    customer.County = request.County;
                if (_customerSettings.CountryEnabled)
                    customer.CountryId = request.CountryId;
                if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                    customer.StateProvinceId = request.StateProvinceId;
                if (_customerSettings.PhoneEnabled)
                    customer.Phone = request.Phone;
                if (_customerSettings.FaxEnabled)
                    customer.Fax = request.Fax;

                customer.CustomCustomerAttributesXML = customerAttributesXml;
                await _customerService.UpdateCustomerAsync(customer);

                //newsletter
                if (_customerSettings.NewsletterEnabled)
                {

                  
                    var newsletter = await FindNewsletterSubscriptionAsync(customer.Email, (await _storeContext.GetCurrentStoreAsync()).Id);

                    if (newsletter != null)
                        if (request.Newsletter)
                        {
                            newsletter.Active = true;
                            await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);
                        }
                        else
                            await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(newsletter);
                    else
                        if (request.Newsletter)
                        await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new NewsLetterSubscription
                        {
                            NewsLetterSubscriptionGuid = Guid.NewGuid(),
                            Email = customer.Email,
                            Active = true,
                            StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                }

                if (_forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SignatureAttribute, request.Signature);

                //GDPR
                if (_gdprSettings.GdprEnabled)
                    await LogGdprAsync(customer, oldCustomerModel, request, new FormCollection(ConvertToFormCollection(request.GdprConsents)));

                return Ok();
            }
        }
        catch (Exception exc)
        {
            ModelState.AddModelError("", exc.Message);
        }

        return PrepareBadRequest(ModelState);
    }


protected virtual async Task<NewsLetterSubscription> FindNewsletterSubscriptionAsync(string email, int storeId)
{
    // Try common method names across nop versions using reflection,
    // so the plugin compiles even if the interface differs.

    var service = _newsLetterSubscriptionService;
    var t = service.GetType();

    // 1) sync: GetNewsLetterSubscriptionByEmailAndStoreId(string,int)
    var sync = t.GetMethod("GetNewsLetterSubscriptionByEmailAndStoreId",
        BindingFlags.Instance | BindingFlags.Public);
    if (sync != null)
        return (NewsLetterSubscription)sync.Invoke(service, new object[] { email, storeId });

    // 2) async alternative: GetNewsLetterSubscriptionByEmailAndStoreIdAsync(string,int)
    // (we DO NOT reference it at compile-time; reflection only)
    var async2 = t.GetMethod("GetNewsLetterSubscriptionByEmailAndStoreIdAsync",
        BindingFlags.Instance | BindingFlags.Public);
    if (async2 != null)
    {
        var taskObj = async2.Invoke(service, new object[] { email, storeId });
        if (taskObj is Task task)
        {
            await task.ConfigureAwait(false);
            var resultProp = task.GetType().GetProperty("Result");
            return (NewsLetterSubscription)resultProp.GetValue(task);
        }
    }

    // 3) fallback: GetAllNewsLetterSubscriptionsAsync(...)
    // Many versions have something like:
    // GetAllNewsLetterSubscriptionsAsync(string email = null, int storeId = 0, bool? isActive = null, int pageIndex=0, int pageSize=int.MaxValue)
    var getAllAsync = t.GetMethod("GetAllNewsLetterSubscriptionsAsync",
        BindingFlags.Instance | BindingFlags.Public);

    if (getAllAsync != null)
    {
        // Try to call it in a flexible way by matching parameters count
        var ps = getAllAsync.GetParameters();

        // Build best-effort args
        var args = new object[ps.Length];
        for (int i = 0; i < ps.Length; i++)
        {
            var p = ps[i];
            if (p.ParameterType == typeof(string) && p.Name?.ToLower().Contains("email") == true)
                args[i] = email;
            else if (p.ParameterType == typeof(int) && p.Name?.ToLower().Contains("store") == true)
                args[i] = storeId;
            else if ((p.ParameterType == typeof(bool?) || p.ParameterType == typeof(bool)) && p.Name?.ToLower().Contains("active") == true)
                args[i] = null;
            else if (p.ParameterType == typeof(int) && p.Name?.ToLower().Contains("pageindex") == true)
                args[i] = 0;
            else if (p.ParameterType == typeof(int) && p.Name?.ToLower().Contains("pagesize") == true)
                args[i] = int.MaxValue;
            else
                args[i] = p.HasDefaultValue ? p.DefaultValue : null;
        }

        var taskObj = getAllAsync.Invoke(service, args);
        if (taskObj is Task task)
        {
            await task.ConfigureAwait(false);
            var result = task.GetType().GetProperty("Result")?.GetValue(task);

            if (result is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is NewsLetterSubscription sub &&
                        sub.StoreId == storeId &&
                        sub.Email?.Equals(email, StringComparison.OrdinalIgnoreCase) == true)
                        return sub;
                }
            }
        }
    }

    return null;
}

/// <summary>
/// Delete the external authentication record
/// </summary>
/// <param name="id">The external authentication record identifier</param>
[HttpDelete("{id}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> DeleteExternalAssociation(int id)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (!_externalAuthenticationSettings.AllowCustomersToRemoveAssociations)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        //ensure it's our record
        var ear = await _externalAuthenticationService.GetExternalAuthenticationRecordByIdAsync(id);

        if (ear == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "External authentication record"));

        await _externalAuthenticationService.DeleteExternalAuthenticationRecordAsync(ear);

        return Ok();
    }

    #endregion

    #region My account / Addresses

    /// <summary>
    /// Get customer addresses
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(CustomerAddressListModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetAddresses()
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        var model = await _customerModelFactory.PrepareCustomerAddressListModelAsync();
        return Ok(model);
    }

    /// <summary>
    /// Delete the customer address
    /// </summary>
    /// <param name="addressId">The address identifier</param>
    [HttpDelete("{addressId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> DeleteAddress(int addressId)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        var customer = await _workContext.GetCurrentCustomerAsync();

        //find address (ensure that it belongs to the current customer)
        var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
        if (address == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(address)));

        await _customerService.RemoveCustomerAddressAsync(customer, address);
        await _customerService.UpdateCustomerAsync(customer);
        //now delete the address record
        await _addressService.DeleteAddressAsync(address);
        return Ok();
    }

    /// <summary>
    /// Prepare the add new customer address model
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(AddressModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetAddAddress()
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        var model = new AddressModel();
        await _addressModelFactory.PrepareAddressModelAsync(model,
            address: null,
            excludeProperties: false,
            addressSettings: _addressSettings,
            loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));

        return Ok(model);
    }

    /// <summary>
    /// Add a new customer address
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> AddAddress(AddressRequest request)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        //custom address attributes
        var customAttributes = await _addressAttributeParser.ParseCustomAttributesAsync(new FormCollection(ConvertToFormCollection(request.CustomAddressAttributes)), NopCommonDefaults.AddressAttributeControlName);
        var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
        foreach (var error in customAttributeWarnings)
            ModelState.AddModelError("", error);

        if (ModelState.IsValid)
        {
            var address = request.ToEntity();
            address.CustomAttributes = customAttributes;
            address.CreatedOnUtc = DateTime.UtcNow;
            //some validation
            if (address.CountryId == 0)
                address.CountryId = null;
            if (address.StateProvinceId == 0)
                address.StateProvinceId = null;


            await _addressService.InsertAddressAsync(address);

            await _customerService.InsertCustomerAddressAsync(await _workContext.GetCurrentCustomerAsync(), address);

            return Ok(address.Id);
        }

        return PrepareBadRequest(ModelState);
    }

    /// <summary>
    /// Get specific customer address
    /// </summary>
    /// <param name="addressId">The address identifier</param>
    [HttpGet("{addressId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(CustomerAddressEditModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetAddress(int addressId)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        var customer = await _workContext.GetCurrentCustomerAsync();
        //find address (ensure that it belongs to the current customer)
        var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
        if (address == null)
            //address is not found
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(address)));

        var model = new CustomerAddressEditModel();
        await _addressModelFactory.PrepareAddressModelAsync(model.Address,
            address: address,
            excludeProperties: false,
            addressSettings: _addressSettings,
            loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));

        return Ok(model);
    }

    /// <summary>
    /// Update the customer address
    /// </summary>
    /// <param name="addressId">The address identifier</param>
    /// <returns></returns>
    [HttpPut("{addressId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> UpdateAddress(int addressId, AddressRequest request)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        var customer = await _workContext.GetCurrentCustomerAsync();
        //find address (ensure that it belongs to the current customer)
        var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
        if (address == null)
            //address is not found
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(address)));

        //custom address attributes
        var customAttributes = await _addressAttributeParser.ParseCustomAttributesAsync(new FormCollection(ConvertToFormCollection(request.CustomAddressAttributes)), NopCommonDefaults.AddressAttributeControlName);
        var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
        foreach (var error in customAttributeWarnings)
            ModelState.AddModelError("", error);

        if (ModelState.IsValid)
        {
            address = request.ToEntity(address);
            address.CustomAttributes = customAttributes;
            await _addressService.UpdateAddressAsync(address);

            return Ok();
        }

        return PrepareBadRequest(ModelState);
    }

    #endregion

    #region My account / Downloadable products

    /// <summary>
    /// Get the customer downloadable products
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomerDownloadableProductsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetDownloadableProducts()
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (_customerSettings.HideDownloadableProductsTab)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = await _customerModelFactory.PrepareCustomerDownloadableProductsModelAsync();

        return Ok(model);
    }

    /// <summary>
    /// Get user agreement
    /// </summary>
    /// <param name="orderItemGuid">The order item guid identifier</param>
    [HttpGet("{orderItemGuid}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(UserAgreementModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetUserAgreement(Guid orderItemGuid)
    {
        var orderItem = await _orderService.GetOrderItemByGuidAsync(orderItemGuid);
        if (orderItem == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(orderItem)));

        var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
        if (product == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(product)));

        if (!product.HasUserAgreement)
            return NotFound("This product doesn't have user agreement");

        var model = await _customerModelFactory.PrepareUserAgreementModelAsync(orderItem, product);

        return Ok(model);
    }

    #endregion

    #region My account / Change password

    /// <summary>
    /// Prepare change password model
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ChangePasswordModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetChangePassword()
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        var model = await _customerModelFactory.PrepareChangePasswordModelAsync(await _workContext.GetCurrentCustomerAsync());

        //display the cause of the change password 
        if (await _customerService.IsPasswordExpiredAsync(await _workContext.GetCurrentCustomerAsync()))
            ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Account.ChangePassword.PasswordIsExpired"));

        return Ok(model);
    }

    /// <summary>
    /// Submit the change password request
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> ChangePassword(Models.Requests.ChangePasswordRequest request)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (ModelState.IsValid)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            var changePasswordRequest = new Nop.Services.Customers.ChangePasswordRequest(customer.Email,
                true, _customerSettings.DefaultPasswordFormat, request.NewPassword, request.OldPassword);
            var changePasswordResult = await _customerRegistrationService.ChangePasswordAsync(changePasswordRequest);
            if (changePasswordResult.Success)
                return Ok(await _localizationService.GetResourceAsync("Account.ChangePassword.Success"));

            foreach (var error in changePasswordResult.Errors)
                ModelState.AddModelError("", error);
        }
        return PrepareBadRequest(ModelState);
    }

    #endregion

    #region My account / Avatar

    /// <summary>
    /// Get customer avatar
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomerAvatarModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetAvatar()
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (!_customerSettings.AllowCustomersToUploadAvatars)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = new CustomerAvatarModel();
        model = await _customerModelFactory.PrepareCustomerAvatarModelAsync(model);

        return Ok(model);
    }

    /// <summary>
    /// Upload customer avatar
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomerAvatarModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> UploadAvatar(UploadAvatarRequest request)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (!_customerSettings.AllowCustomersToUploadAvatars)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var customer = await _workContext.GetCurrentCustomerAsync();

        if (ModelState.IsValid)
            try
            {
                var (customerPictureBinary, mimeType, pictureFileLength) = PluginCommonHelper.ConvertBase64ToFile(request.AvatarBase64String, request.AvatarFileName);
                var customerAvatar = await _pictureService.GetPictureByIdAsync(await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute));
                if (customerPictureBinary != null)
                {
                    var avatarMaxSize = _customerSettings.AvatarMaximumSizeBytes;
                    if (pictureFileLength > avatarMaxSize)
                        throw new NopException(string.Format(await _localizationService.GetResourceAsync("Account.Avatar.MaximumUploadedFileSize"), avatarMaxSize));

                    if (customerAvatar != null)
                        customerAvatar = await _pictureService.UpdatePictureAsync(customerAvatar.Id, customerPictureBinary, mimeType, null);
                    else
                        customerAvatar = await _pictureService.InsertPictureAsync(customerPictureBinary, mimeType, null);
                }

                var customerAvatarId = 0;
                if (customerAvatar != null)
                    customerAvatarId = customerAvatar.Id;

                await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AvatarPictureIdAttribute, customerAvatarId);

                var model = new CustomerAvatarModel
                {
                    AvatarUrl = await _pictureService.GetPictureUrlAsync(
                    await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute),
                    _mediaSettings.AvatarPictureSize,
                    false)
                };

                return Ok(model);
            }
            catch (Exception exc)
            {
                ModelState.AddModelError("", exc.Message);
            }

        return PrepareBadRequest(ModelState);
    }

    /// <summary>
    /// Remove customer avatar
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> DeleteAvatar()
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (!_customerSettings.AllowCustomersToUploadAvatars)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var customer = await _workContext.GetCurrentCustomerAsync();

        var customerAvatar = await _pictureService.GetPictureByIdAsync(await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute));
        if (customerAvatar != null)
            await _pictureService.DeletePictureAsync(customerAvatar);
        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AvatarPictureIdAttribute, 0);

        return Ok();
    }

    #endregion

    #region GDPR tools

    /// <summary>
    /// Prepare GDPR tools model
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(GdprToolsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetGdprTools()
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (!_gdprSettings.GdprEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = await _customerModelFactory.PrepareGdprToolsModelAsync();

        return Ok(model);
    }

    /// <summary>
    /// Export customer GDPR info
    /// </summary>
    /// <response code="200">Exports the GDPR information as xlsx file</response>
    [HttpPost]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> ExportGdprTools()
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (!_gdprSettings.GdprEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        //log
        await _gdprService.InsertLogAsync(await _workContext.GetCurrentCustomerAsync(), 0, GdprRequestType.ExportData, await _localizationService.GetResourceAsync("Gdpr.Exported"));

        //export
        var bytes = await _exportManager.ExportCustomerGdprInfoToXlsxAsync(await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);

        return File(bytes, Nop.Core.MimeTypes.TextXlsx, "customerdata.xlsx");
    }

    /// <summary>
    /// Delete request of GDPR info 
    /// </summary>
    /// <returns></returns>
    [HttpDelete]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(GdprToolsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> DeleteGdprTools()
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (!_gdprSettings.GdprEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        //log
        await _gdprService.InsertLogAsync(await _workContext.GetCurrentCustomerAsync(), 0, GdprRequestType.DeleteCustomer, await _localizationService.GetResourceAsync("Gdpr.DeleteRequested"));

        var model = await _customerModelFactory.PrepareGdprToolsModelAsync();
        model.Result = await _localizationService.GetResourceAsync("Gdpr.DeleteRequested.Success");
        return Ok(model);
    }

    #endregion

    #region Check gift card balance

    /// <summary>
    /// Prepare check gift card balance model
    /// </summary>
    [HttpGet]
    [CheckAccessClosedStore(true)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CheckGiftCardBalanceModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetCheckGiftCardBalance()
    {
        if (!_customerSettings.AllowCustomersToCheckGiftCardBalance)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = await _customerModelFactory.PrepareCheckGiftCardBalanceModelAsync();

        return Ok(model);
    }

    /// <summary>
    /// Check gift card balance
    /// </summary>
    /// <param name="giftCardCode">Gift card code</param>
    [HttpPost("{giftCardCode}")]
    [CheckAccessClosedStore(true)]
    [ProducesResponseType(typeof(CheckGiftCardBalanceModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> CheckGiftCardBalance(string giftCardCode)
    {
        var model = new CheckGiftCardBalanceModel();
        if (ModelState.IsValid)
        {
            var giftCard = (await _giftCardService.GetAllGiftCardsAsync(giftCardCouponCode: giftCardCode)).FirstOrDefault();
            if (giftCard != null && await _giftCardService.IsGiftCardValidAsync(giftCard))
            {
                var remainingAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(await _giftCardService.GetGiftCardRemainingAmountAsync(giftCard), await _workContext.GetWorkingCurrencyAsync());
                model.Result = await _priceFormatter.FormatPriceAsync(remainingAmount, true, false);
            }
            else
                model.Message = await _localizationService.GetResourceAsync("CheckGiftCardBalance.GiftCardCouponCode.Invalid");
        }
        return Ok(model);
    }

    #endregion

    #endregion
}
