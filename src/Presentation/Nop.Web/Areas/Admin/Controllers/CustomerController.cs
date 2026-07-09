using System.Data;
using System.Globalization;
using System.Net;
using System.Text;
using LinqToDB.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Data;
using Nop.Data.DataProviders;
using Nop.Services.Directory;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Attributes;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.ExportImport;
using Nop.Services.Forums;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Services.Logging;
namespace Nop.Web.Areas.Admin.Controllers;

public partial class CustomerController : BaseAdminController
{
    #region Fields

    protected readonly CustomerSettings _customerSettings;
    protected readonly DateTimeSettings _dateTimeSettings;
    protected readonly EmailAccountSettings _emailAccountSettings;
    protected readonly ForumSettings _forumSettings;
    protected readonly GdprSettings _gdprSettings;
    protected readonly IAddressService _addressService;
    protected readonly IAttributeParser<AddressAttribute, AddressAttributeValue> _addressAttributeParser;
    protected readonly IAttributeParser<CustomerAttribute, CustomerAttributeValue> _customerAttributeParser;
    protected readonly IAttributeService<CustomerAttribute, CustomerAttributeValue> _customerAttributeService;
    protected readonly ICustomerActivityService _customerActivityService;
    protected readonly ICustomerModelFactory _customerModelFactory;
    protected readonly ICustomerRegistrationService _customerRegistrationService;
    protected readonly ICustomerService _customerService;
    protected readonly IDateTimeHelper _dateTimeHelper;
    protected readonly IEmailAccountService _emailAccountService;
    protected readonly IEventPublisher _eventPublisher;
    protected readonly IExportManager _exportManager;
    protected readonly IForumService _forumService;
    protected readonly IGdprService _gdprService;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly IImportManager _importManager;
    protected readonly ILocalizationService _localizationService;
    protected readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
    protected readonly INotificationService _notificationService;
    protected readonly IPermissionService _permissionService;
    protected readonly IQueuedEmailService _queuedEmailService;
    protected readonly IRewardPointService _rewardPointService;
    protected readonly IStoreContext _storeContext;
    protected readonly ITaxService _taxService;
    protected readonly IWorkContext _workContext;
    protected readonly IWorkflowMessageService _workflowMessageService;
    protected readonly TaxSettings _taxSettings;
    private static readonly char[] _separator = [','];
    protected readonly HttpClient _httpClient;
    private static NetSuiteApiConfig _config;
    private static readonly RestClient _restClient;
    protected readonly Nop.Services.Logging.ILogger _logger;
    protected readonly ICountryService _countryService;
    protected readonly IStateProvinceService _stateProvinceService;

    #endregion

    #region Ctor

    public CustomerController(HttpClient httpClient,
        CustomerSettings customerSettings,
        DateTimeSettings dateTimeSettings,
        EmailAccountSettings emailAccountSettings,
        ForumSettings forumSettings,
        GdprSettings gdprSettings,
        IAddressService addressService,
        IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
        IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
        IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
        ICustomerActivityService customerActivityService,
        ICustomerModelFactory customerModelFactory,
        ICustomerRegistrationService customerRegistrationService,
        ICustomerService customerService,
        IDateTimeHelper dateTimeHelper,
        IEmailAccountService emailAccountService,
        IEventPublisher eventPublisher,
        IExportManager exportManager,
        IForumService forumService,
        IGdprService gdprService,
        IGenericAttributeService genericAttributeService,
        IImportManager importManager,
        ILocalizationService localizationService,
        INewsLetterSubscriptionService newsLetterSubscriptionService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IQueuedEmailService queuedEmailService,
        IRewardPointService rewardPointService,
        IStoreContext storeContext,
        ITaxService taxService,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService,
        TaxSettings taxSettings,
        Nop.Services.Logging.ILogger logger,
        ICountryService countryService,
        IStateProvinceService stateProvinceService)
    {
        _httpClient = httpClient;
        _customerSettings = customerSettings;
        _dateTimeSettings = dateTimeSettings;
        _emailAccountSettings = emailAccountSettings;
        _forumSettings = forumSettings;
        _gdprSettings = gdprSettings;
        _addressService = addressService;
        _addressAttributeParser = addressAttributeParser;
        _customerAttributeParser = customerAttributeParser;
        _customerAttributeService = customerAttributeService;
        _customerActivityService = customerActivityService;
        _customerModelFactory = customerModelFactory;
        _customerRegistrationService = customerRegistrationService;
        _customerService = customerService;
        _dateTimeHelper = dateTimeHelper;
        _emailAccountService = emailAccountService;
        _eventPublisher = eventPublisher;
        _exportManager = exportManager;
        _forumService = forumService;
        _gdprService = gdprService;
        _genericAttributeService = genericAttributeService;
        _importManager = importManager;
        _localizationService = localizationService;
        _newsLetterSubscriptionService = newsLetterSubscriptionService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _queuedEmailService = queuedEmailService;
        _rewardPointService = rewardPointService;
        _storeContext = storeContext;
        _taxService = taxService;
        _workContext = workContext;
        _workflowMessageService = workflowMessageService;
        _taxSettings = taxSettings;
        _logger = logger;
        _countryService = countryService;
        _stateProvinceService = stateProvinceService;
    }

    static CustomerController()
    {
        _config = new NetSuiteApiConfig();
        _restClient = CreateRestClient();
    }

    private static RestClient CreateRestClient()
    {
        var client = new RestClient();
        var oAuth1 = OAuth1Authenticator.ForAccessToken(
            consumerKey: _config.ClientId,
            consumerSecret: _config.ClientSecret,
            token: _config.TokenId,
            tokenSecret: _config.TokenSecret,
            OAuthSignatureMethod.HmacSha256);
        oAuth1.Realm = _config.AccountId;
        client.Authenticator = oAuth1;
        return client;
    }

    #endregion

    #region Utilities

    protected virtual async Task<string> ValidateCustomerRolesAsync(IList<CustomerRole> customerRoles, IList<CustomerRole> existingCustomerRoles)
    {
        ArgumentNullException.ThrowIfNull(customerRoles);

        ArgumentNullException.ThrowIfNull(existingCustomerRoles);

        //check ACL permission to manage customer roles
        var rolesToAdd = customerRoles.Except(existingCustomerRoles, new CustomerRoleComparerByName());
        var rolesToDelete = existingCustomerRoles.Except(customerRoles, new CustomerRoleComparerByName());
        if (rolesToAdd.Any(role => role.SystemName != NopCustomerDefaults.RegisteredRoleName) || rolesToDelete.Any())
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_ACL))
                return await _localizationService.GetResourceAsync("Admin.Customers.Customers.CustomerRolesManagingError");
        }

        //ensure a customer is not added to both 'Guests' and 'Registered' customer roles
        //ensure that a customer is in at least one required role ('Guests' and 'Registered')
        var isInGuestsRole = customerRoles.FirstOrDefault(cr => cr.SystemName == NopCustomerDefaults.GuestsRoleName) != null;
        var isInRegisteredRole = customerRoles.FirstOrDefault(cr => cr.SystemName == NopCustomerDefaults.RegisteredRoleName) != null;
        if (isInGuestsRole && isInRegisteredRole)
            return await _localizationService.GetResourceAsync("Admin.Customers.Customers.GuestsAndRegisteredRolesError");
        if (!isInGuestsRole && !isInRegisteredRole)
            return await _localizationService.GetResourceAsync("Admin.Customers.Customers.AddCustomerToGuestsOrRegisteredRoleError");

        //no errors
        return string.Empty;
    }

    protected virtual async Task<string> ParseCustomCustomerAttributesAsync(IFormCollection form)
    {
        ArgumentNullException.ThrowIfNull(form);

        var attributesXml = string.Empty;
        var customerAttributes = await _customerAttributeService.GetAllAttributesAsync();
        foreach (var attribute in customerAttributes)
        {
            var controlId = $"{NopCustomerServicesDefaults.CustomerAttributePrefix}{attribute.Id}";
            StringValues ctrlAttributes;

            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                    ctrlAttributes = form[controlId];
                    if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                    {
                        var selectedAttributeId = int.Parse(ctrlAttributes);
                        if (selectedAttributeId > 0)
                            attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                attribute, selectedAttributeId.ToString());
                    }

                    break;
                case AttributeControlType.Checkboxes:
                    var cblAttributes = form[controlId];
                    if (!StringValues.IsNullOrEmpty(cblAttributes))
                    {
                        foreach (var item in cblAttributes.ToString()
                                     .Split(_separator, StringSplitOptions.RemoveEmptyEntries))
                        {
                            var selectedAttributeId = int.Parse(item);
                            if (selectedAttributeId > 0)
                                attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }
                    }

                    break;
                case AttributeControlType.ReadonlyCheckboxes:
                    //load read-only (already server-side selected) values
                    var attributeValues = await _customerAttributeService.GetAttributeValuesAsync(attribute.Id);
                    foreach (var selectedAttributeId in attributeValues
                                 .Where(v => v.IsPreSelected)
                                 .Select(v => v.Id)
                                 .ToList())
                    {
                        attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                            attribute, selectedAttributeId.ToString());
                    }

                    break;
                case AttributeControlType.TextBox:
                case AttributeControlType.MultilineTextbox:
                    ctrlAttributes = form[controlId];
                    if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                    {
                        var enteredText = ctrlAttributes.ToString().Trim();
                        attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                            attribute, enteredText);
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

    protected virtual async Task<bool> SecondAdminAccountExistsAsync(Customer customer)
    {
        var customers = await _customerService.GetAllCustomersAsync(customerRoleIds: [(await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName)).Id]);

        return customers.Any(c => c.Active && c.Id != customer.Id);
    }

    private static async Task<List<SalesRepEntry>> BuildSalesRepArrayAsync(CustomerModel model, int fallbackErpId)
    {
        var result = new List<SalesRepEntry>();

        if (model.SelectedAccountManagerIds == null || !model.SelectedAccountManagerIds.Any())
        {
            result.Add(new SalesRepEntry { id = fallbackErpId.ToString(), is_primary = true });
            return result;
        }

        try
        {
            MsSqlNopDataProvider db = new MsSqlNopDataProvider();
            // Load all AMs to map Account_Manager.Id → ERPAccountManagerId
            var allAMs = await db.QueryAsync<AccountManagerDto>(
                "SELECT Id, AccountManagerName, ERPAccountManagerId FROM Account_Manager WHERE Deleted = 0");

            foreach (var amId in model.SelectedAccountManagerIds)
            {
                var am = allAMs.FirstOrDefault(a => a.Id == amId);
                // ERPAccountManagerId must be set — skip if 0
                if (am == null || am.ERPAccountManagerId <= 0) continue;

                // is_primary = true only for the admin-selected primary AM
                bool isPrimary = amId == model.PrimaryAccountManagerId;

                result.Add(new SalesRepEntry
                {
                    id = am.ERPAccountManagerId.ToString(),  // ← always ERPAccountManagerId
                    is_primary = isPrimary
                });
            }

            // Ensure at least one is_primary = true (first item if none selected as primary)
            if (result.Any() && !result.Any(r => r.is_primary))
                result[0].is_primary = true;
        }
        catch
        {
            result.Clear();
            result.Add(new SalesRepEntry { id = fallbackErpId.ToString(), is_primary = true });
        }

        return result.Any() ? result : new List<SalesRepEntry> { new SalesRepEntry { id = fallbackErpId.ToString(), is_primary = true } };
    }

    private class SalesRepEntry
    {
        public string id { get; set; }
        public bool is_primary { get; set; }
    }

    public static IList<SelectListItem> GetAvailableERPActions()
    {
        return new List<SelectListItem>
        {
            new SelectListItem { Text = "Create new ERP customer", Selected = true, Value = "1" },
            new SelectListItem { Text = "Update existing ERP customer", Selected = true, Value = "2" }
        };
    }

    #endregion

    #region Customers

    public virtual IActionResult Index()
    {
        return RedirectToAction("List");
    }

    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    public virtual async Task<IActionResult> List()
    {
        //prepare model
        var model = await _customerModelFactory.PrepareCustomerSearchModelAsync(new CustomerSearchModel());

        return View(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    public virtual async Task<IActionResult> CustomerList(CustomerSearchModel searchModel)
    {
        //prepare model
        var model = await _customerModelFactory.PrepareCustomerListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    public virtual async Task<IActionResult> CheckCustomerERPStatus(int customerId)
    {
        var customer = await _customerService.GetCustomerByIdAsync(customerId);
        if (customer == null)
            return Json(new { success = false });

        string cIdsByPhone = "";
        string cIdsByEmail = "";
        int erpCustomerId = customer.ERPCustomerId ?? 0;

        try
        {
            MsSqlNopDataProvider db = new MsSqlNopDataProvider();
            var erpData = await db.QueryProcAsync<CustomerModel>("GetCustomerERPRegistered", new DataParameter("@CID", customerId));
            var first = erpData.FirstOrDefault();
            if (first != null)
            {
                erpCustomerId = first.ERPCustomerId > 0 ? first.ERPCustomerId : erpCustomerId;
                cIdsByPhone = first.ERPRegisteredCIdsByPhone ?? "";
                cIdsByEmail = first.ERPRegisteredCIdsByEmail ?? "";
            }
        }
        catch { }

        if (string.IsNullOrEmpty(cIdsByPhone) && !string.IsNullOrEmpty(customer.Phone))
        {
            try
            {
                var restClient = CreateRestClient();
                var req = new RestRequest($"{_config.ApiRoot}&", Method.GET);
                req.AddQueryParameter("objType", "22");
                req.AddQueryParameter("objId", "0");
                req.AddQueryParameter("params", "+" + customer.Phone);
                var resp = await restClient.ExecuteAsync(req);
                var customers = JsonConvert.DeserializeObject<List<CustomerByPhoneOrEmail>>(resp.Content);
                foreach (var item in customers ?? new List<CustomerByPhoneOrEmail>())
                    cIdsByPhone += item.ns_id + " ,";
            }
            catch { }
        }

        if (string.IsNullOrEmpty(cIdsByEmail) && !string.IsNullOrEmpty(customer.Email))
        {
            try
            {
                var url = _config.ApiRoot + "&objType=22&objId=0&params=" + customer.Email;
                var httpReq = new RestRequest(url, Method.GET);
                var httpResp = await _restClient.ExecuteAsync(httpReq);
                var customers = JsonConvert.DeserializeObject<List<CustomerByPhoneOrEmail>>(httpResp.Content);
                foreach (var item in customers ?? new List<CustomerByPhoneOrEmail>())
                    cIdsByEmail += item.ns_id + " ,";
            }
            catch { }
        }

        try
        {
            MsSqlNopDataProvider dbSave = new MsSqlNopDataProvider();
            var erpId = await dbSave.QueryProcAsync<int>("CustomerSaveERPRegistered",
                new DataParameter("@CID", customerId),
                new DataParameter("@ERPRegisteredCIdsByPhone", cIdsByPhone),
                new DataParameter("@ERPRegisteredCIdsByEmail", cIdsByEmail),
                new DataParameter("@ERPCustomerId", erpCustomerId));
            erpCustomerId = erpId.FirstOrDefault();
        }
        catch { }

        return Json(new
        {
            success = true,
            erpCustomerId,
            isRegisteredByEmail = !string.IsNullOrEmpty(cIdsByEmail),
            isRegisteredByPhone = !string.IsNullOrEmpty(cIdsByPhone),
            emailIds = cIdsByEmail,
            phoneIds = cIdsByPhone
        });
    }

    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Create()
    {
        //prepare model
        var model = await _customerModelFactory.PrepareCustomerModelAsync(new CustomerModel(), null);

        model.AvailableERPActions = GetAvailableERPActions();

        var codFactors = await _customerService.GetAllCODFactorsAsync();
        model.AvailableCODCountries = codFactors
            .Select(c => new SelectListItem { Text = c.Name, Value = (c.CountryID + "," + c.FactorID).ToString() })
            .ToList();
        model.AvailableCODCountries.Insert(0, new SelectListItem { Text = "Select COD Country", Value = "" });

        var customerTypes = await _customerService.GetAllCustomerTypeAsync();
        model.AvailableCustomerType = customerTypes?.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.TypeName
        }).ToList() ?? new List<SelectListItem>();
        model.AvailableCustomerType.Insert(0, new SelectListItem { Text = "Select Customer Type", Value = "" });

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Create(CustomerModel model, bool continueEditing, IFormCollection form)
    {
        if (!string.IsNullOrWhiteSpace(model.Email) && await _customerService.GetCustomerByEmailAsync(model.Email) != null)
            ModelState.AddModelError(string.Empty, "Email is already registered");

        if (!string.IsNullOrWhiteSpace(model.Username) && _customerSettings.UsernamesEnabled &&
            await _customerService.GetCustomerByUsernameAsync(model.Username) != null)
        {
            ModelState.AddModelError(string.Empty, "Username is already registered");
        }

        //validate customer roles
        var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
        var newCustomerRoles = new List<CustomerRole>();
        foreach (var customerRole in allCustomerRoles)
            if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                newCustomerRoles.Add(customerRole);
        var customerRolesError = await ValidateCustomerRolesAsync(newCustomerRoles, new List<CustomerRole>());
        if (!string.IsNullOrEmpty(customerRolesError))
        {
            ModelState.AddModelError(string.Empty, customerRolesError);
            _notificationService.ErrorNotification(customerRolesError);
        }

        // Ensure that valid email address is entered if Registered role is checked to avoid registered customers with empty email address
        if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == NopCustomerDefaults.RegisteredRoleName) != null &&
            !CommonHelper.IsValidEmail(model.Email))
        {
            ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"));

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"));
        }

        //custom customer attributes
        var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);
        if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == NopCustomerDefaults.RegisteredRoleName) != null)
        {
            var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        if (ModelState.IsValid)
        {
            //fill entity from model
            var customer = model.ToEntity<Customer>();
            var currentStore = await _storeContext.GetCurrentStoreAsync();

            customer.CustomerGuid = Guid.NewGuid();
            customer.CreatedOnUtc = DateTime.UtcNow;
            customer.LastActivityDateUtc = DateTime.UtcNow;
            customer.RegisteredInStoreId = currentStore.Id;

            //form fields
            if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                customer.TimeZoneId = model.TimeZoneId;
            if (_customerSettings.GenderEnabled)
                customer.Gender = model.Gender;
            if (_customerSettings.FirstNameEnabled)
                customer.FirstName = model.FirstName;
            if (_customerSettings.LastNameEnabled)
                customer.LastName = model.LastName;
            if (_customerSettings.DateOfBirthEnabled)
                customer.DateOfBirth = model.DateOfBirth;
            if (_customerSettings.CompanyEnabled)
                customer.Company = model.Company;
            if (_customerSettings.StreetAddressEnabled)
                customer.StreetAddress = model.StreetAddress;
            if (_customerSettings.StreetAddress2Enabled)
                customer.StreetAddress2 = model.StreetAddress2;
            if (_customerSettings.ZipPostalCodeEnabled)
                customer.ZipPostalCode = model.ZipPostalCode;
            if (_customerSettings.CityEnabled)
                customer.City = model.City;
            if (_customerSettings.CountyEnabled)
                customer.County = model.County;
            if (_customerSettings.CountryEnabled)
                customer.CountryId = model.CountryId;
            if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                customer.StateProvinceId = model.StateProvinceId;
            if (_customerSettings.PhoneEnabled)
                customer.Phone = model.Phone;
            if (_customerSettings.FaxEnabled)
                customer.Fax = model.Fax;
            customer.CustomCustomerAttributesXML = customerAttributesXml;

            if (model.ERPAction == 2)
                customer.ERPCustomerIdToUpdate = model.ERPCustomerIdToUpdate;

            await _customerService.InsertCustomerAsync(customer);

            //password
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                var changePassRequest = new ChangePasswordRequest(model.Email, false, _customerSettings.DefaultPasswordFormat, model.Password);
                var changePassResult = await _customerRegistrationService.ChangePasswordAsync(changePassRequest);
                if (!changePassResult.Success)
                {
                    foreach (var changePassError in changePassResult.Errors)
                        _notificationService.ErrorNotification(changePassError);
                }
            }

            //customer roles
            foreach (var customerRole in newCustomerRoles)
            {
                //ensure that the current customer cannot add to "Administrators" system role if he's not an admin himself
                if (customerRole.SystemName == NopCustomerDefaults.AdministratorsRoleName && !await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
                    continue;

                await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = customerRole.Id });
            }

            await _customerService.UpdateCustomerAsync(customer);

            //ensure that a customer with a vendor associated is not in "Administrators" role
            //otherwise, he won't have access to other functionality in admin area
            if (await _customerService.IsAdminAsync(customer) && customer.VendorId > 0)
            {
                customer.VendorId = 0;
                await _customerService.UpdateCustomerAsync(customer);

                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.AdminCouldNotbeVendor"));
            }

            //ensure that a customer in the Vendors role has a vendor account associated.
            //otherwise, he will have access to ALL products
            if (await _customerService.IsVendorAsync(customer) && customer.VendorId == 0)
            {
                var vendorRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.VendorsRoleName);
                await _customerService.RemoveCustomerRoleMappingAsync(customer, vendorRole);

                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.CannotBeInVendoRoleWithoutVendorAssociated"));
            }

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewCustomer",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewCustomer"), customer.Id), customer);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.Added"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        //prepare model
        model = await _customerModelFactory.PrepareCustomerModelAsync(model, null, true);
        model.AvailableERPActions = GetAvailableERPActions();
        var codFactorsErr = await _customerService.GetAllCODFactorsAsync();
        model.AvailableCODCountries = codFactorsErr.Select(c => new SelectListItem { Text = c.Name, Value = (c.CountryID + "," + c.FactorID).ToString() }).ToList();
        model.AvailableCODCountries.Insert(0, new SelectListItem { Text = "Select COD Country", Value = "" });
        var customerTypesErr = await _customerService.GetAllCustomerTypeAsync();
        model.AvailableCustomerType = customerTypesErr?.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.TypeName }).ToList() ?? new List<SelectListItem>();
        model.AvailableCustomerType.Insert(0, new SelectListItem { Text = "Select Customer Type", Value = "" });

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    public virtual async Task<IActionResult> Edit(int id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var currentCustomerRoleIds = await _customerService.GetCustomerRoleIdsAsync(currentCustomer);

        //wafaa - Check access: Admin (role 1) can access any customer
        //wafaa - Account Manager (role 6) can only access customers assigned to them in AccountManager_CustomerMapping
        if (!currentCustomerRoleIds.Contains(1))
        {
            if (currentCustomerRoleIds.Contains(6))
            {
                //wafaa - Check if this customer is assigned to the current account manager
                MsSqlNopDataProvider dbCheck = new MsSqlNopDataProvider();
                var isAssigned = await dbCheck.QueryAsync<int>(@"
                    SELECT COUNT(1)
                    FROM AccountManager_CustomerMapping m
                    INNER JOIN Account_Manager am ON am.Id = m.AccountManagerId
                    WHERE m.Customer_Id = @CustomerId
                      AND am.Customer_Id = @CurrentCustomerId
                      AND am.Active = 1
                      AND am.Deleted = 0",
                    new DataParameter("@CustomerId", id),
                    new DataParameter("@CurrentCustomerId", currentCustomer.Id));

                if (isAssigned.FirstOrDefault() == 0)
                    return AccessDeniedView();
            }
            else
            {
                return AccessDeniedView();
            }
        }

        if (customer == null || customer.Deleted)
            return RedirectToAction("List");

        var model = await _customerModelFactory.PrepareCustomerModelAsync(null, customer);

        model.AvailableERPActions = GetAvailableERPActions();

        var codFactors = await _customerService.GetAllCODFactorsAsync();
        model.AvailableCODCountries = codFactors.Select(c => new SelectListItem
        {
            Value = (c.CountryID + "," + c.FactorID).ToString(),
            Text = c.Name,
            Selected = model.SelectedCODCountryId != null && model.SelectedCODCountryId == (c.CountryID + "," + c.FactorID).ToString()
        }).ToList();
        model.AvailableCODCountries.Insert(0, new SelectListItem { Text = "Select COD Country", Value = "" });

        var customerTypes = await _customerService.GetAllCustomerTypeAsync();
        model.AvailableCustomerType = customerTypes?.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.TypeName,
            Selected = model.SelectedCustomerTypeId > 0 && model.SelectedCustomerTypeId == c.Id
        }).ToList() ?? new List<SelectListItem>();
        model.AvailableCustomerType.Insert(0, new SelectListItem { Text = "Select Customer Type", Value = "" });

        string CIdsByEmail = "";
        string CIdsByPhone = "";

        try
        {
            MsSqlNopDataProvider db2 = new MsSqlNopDataProvider();
            var erpCustomer = await db2.QueryProcAsync<CustomerModel>("GetCustomerERPRegistered", new DataParameter("@CID", customer.Id));
            var first = erpCustomer.FirstOrDefault();
            model.ERPCustomerId = first?.ERPCustomerId ?? 0;
            model.ERPRegisteredCIdsByEmail = first?.ERPRegisteredCIdsByEmail;
            model.ERPIsRegisteredEmail = !string.IsNullOrEmpty(model.ERPRegisteredCIdsByEmail);
            model.ERPRegisteredCIdsByPhone = first?.ERPRegisteredCIdsByPhone;
            model.ERPIsRegisteredPhone = !string.IsNullOrEmpty(model.ERPRegisteredCIdsByPhone);
        }
        catch { }

        try
        {
            var restClient = CreateRestClient();
            var request = new RestRequest($"{_config.ApiRoot}&", Method.GET);
            request.AddQueryParameter("objType", "22");
            request.AddQueryParameter("objId", "0");
            if (!string.IsNullOrEmpty(model.Username))
                request.AddQueryParameter("params", "+" + model.Username);
            var response = await restClient.ExecuteAsync(request);
            var customers = JsonConvert.DeserializeObject<List<CustomerByPhoneOrEmail>>(response.Content);
            foreach (var item in customers ?? new List<CustomerByPhoneOrEmail>())
                CIdsByPhone += item.ns_id + " ,";
            if (customers?.Count > 0)
            {
                model.ERPIsRegisteredPhone = true;
                model.ERPRegisteredCIdsByPhone = CIdsByPhone;
            }
        }
        catch { }

        try
        {
            var url = _config.ApiRoot + "&objType=22&objId=0&params=" + (model.Email ?? "");
            var httpRequest = new RestRequest(url, Method.GET);
            var httpResponse = await _restClient.ExecuteAsync(httpRequest);
            var customers = JsonConvert.DeserializeObject<List<CustomerByPhoneOrEmail>>(httpResponse.Content);
            foreach (var item in customers ?? new List<CustomerByPhoneOrEmail>())
                CIdsByEmail += item.ns_id + " ,";
            if (customers?.Count > 0)
            {
                model.ERPIsRegisteredEmail = true;
                model.ERPRegisteredCIdsByEmail = CIdsByEmail;
            }
        }
        catch { }

        try
        {
            MsSqlNopDataProvider db3 = new MsSqlNopDataProvider();
            var erpId = await db3.QueryProcAsync<int>("CustomerSaveERPRegistered",
                new DataParameter("@CID", customer.Id),
                new DataParameter("@ERPRegisteredCIdsByPhone", model.ERPRegisteredCIdsByPhone),
                new DataParameter("@ERPRegisteredCIdsByEmail", model.ERPRegisteredCIdsByEmail),
                new DataParameter("@ERPCustomerId", model.ERPCustomerId));
            model.ERPCustomerId = erpId.FirstOrDefault();
        }
        catch { }

        // ERPCustomerIdToUpdate: fill with ERPCustomerId if not already set
        // Must be after ERPCustomerId is resolved from DB
        if (model.ERPCustomerIdToUpdate == 0 && model.ERPCustomerId > 0)
            model.ERPCustomerIdToUpdate = model.ERPCustomerId;

        // Load all active account managers for multi-select dropdown
        try
        {
            MsSqlNopDataProvider dbAm = new MsSqlNopDataProvider();
            var allAMs = await dbAm.QueryAsync<AccountManagerDto>(
                "SELECT Id, AccountManagerName, ERPAccountManagerId FROM Account_Manager WHERE Active = 1 AND Deleted = 0 ORDER BY AccountManagerName");

            model.AvailableAccountManagers = allAMs.Select(am => new SelectListItem
            {
                Value = am.Id.ToString(),
                Text = am.AccountManagerName
            }).ToList();

            var selectedAMIds = new List<int>();
            int primaryAMId = 0;

            // Priority 1: call ERP objType=88 — select ALL reps, detect isPrimary
            if (model.ERPCustomerId > 0)
            {
                try
                {
                    var salesRepReq = new RestRequest(_config.ApiRoot + "&objType=88&objId=" + model.ERPCustomerId, Method.GET);
                    var salesRepResp = await _restClient.ExecuteAsync(salesRepReq);
                    if (salesRepResp.IsSuccessful && !string.IsNullOrEmpty(salesRepResp.Content))
                    {
                        var salesRepData = JsonConvert.DeserializeObject<SalesRepResponse>(salesRepResp.Content);
                        if (salesRepData?.success == true && salesRepData.data?.Count > 0)
                        {
                            foreach (var rep in salesRepData.data)
                            {
                                var matched = allAMs.FirstOrDefault(am => am.ERPAccountManagerId == rep.id);
                                if (matched?.Id > 0)
                                {
                                    selectedAMIds.Add(matched.Id);
                                    if (rep.isPrimary && primaryAMId == 0)
                                        primaryAMId = matched.Id;
                                }
                            }
                            // If no primary found, use first
                            if (primaryAMId == 0 && selectedAMIds.Any())
                                primaryAMId = selectedAMIds.First();
                        }
                    }
                }
                catch { }
            }

            // Priority 2: if no reps from ERP, find AM by customer country → region
            if (!selectedAMIds.Any() && customer.CountryId > 0)
            {
                try
                {
                    MsSqlNopDataProvider dbRegion = new MsSqlNopDataProvider();
                    var amByRegion = await dbRegion.QueryAsync<int>(@"
                        SELECT DISTINCT arm.AccountManagerId
                        FROM CountryRigionMapping crm
                        INNER JOIN AccountManagerRigionMapping arm ON arm.RigionId = crm.RigionId
                        INNER JOIN Account_Manager am ON am.Id = arm.AccountManagerId
                        WHERE crm.CountryId = @CountryId AND am.Active = 1 AND am.Deleted = 0",
                        new DataParameter("@CountryId", customer.CountryId));
                    selectedAMIds.AddRange(amByRegion);
                    if (selectedAMIds.Any()) primaryAMId = selectedAMIds.First();
                }
                catch { }
            }

            // Priority 3: fallback — load currently saved mapping from DB
            if (!selectedAMIds.Any())
            {
                try
                {
                    MsSqlNopDataProvider dbMapping = new MsSqlNopDataProvider();
                    var savedAMs = await dbMapping.QueryAsync<int>(
                        "SELECT AccountManagerId FROM AccountManager_CustomerMapping WHERE Customer_Id = @CId",
                        new DataParameter("@CId", customer.Id));
                    selectedAMIds.AddRange(savedAMs);
                    if (selectedAMIds.Any()) primaryAMId = selectedAMIds.First();
                }
                catch { }
            }

            model.SelectedAccountManagerIds = selectedAMIds.Distinct().ToList();
            model.PrimaryAccountManagerId = primaryAMId;
        }
        catch { }

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public virtual async Task<IActionResult> Edit(CustomerModel model, bool continueEditing, IFormCollection form)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var currentCustomerRoleIds = await _customerService.GetCustomerRoleIdsAsync(currentCustomer);

        //wafaa - POST: Admin (role 1) can edit any customer
        //wafaa - Account Manager (role 6) can only edit customers assigned to them in AccountManager_CustomerMapping
        if (!currentCustomerRoleIds.Contains(1))
        {
            if (currentCustomerRoleIds.Contains(6))
            {
                MsSqlNopDataProvider dbPermCheck = new MsSqlNopDataProvider();
                var isAssignedPost = await dbPermCheck.QueryAsync<int>(@"
                    SELECT COUNT(1)
                    FROM AccountManager_CustomerMapping m
                    INNER JOIN Account_Manager am ON am.Id = m.AccountManagerId
                    WHERE m.Customer_Id = @CustomerId
                      AND am.Customer_Id = @CurrentCustomerId
                      AND am.Active = 1
                      AND am.Deleted = 0",
                    new DataParameter("@CustomerId", model.Id),
                    new DataParameter("@CurrentCustomerId", currentCustomer.Id));

                if (isAssignedPost.FirstOrDefault() == 0)
                    return AccessDeniedView();
            }
            else
            {
                return AccessDeniedView();
            }
        }

        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.Id);
        if (customer == null || customer.Deleted)
            return RedirectToAction("List");

        //validate customer roles
        var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
        var newCustomerRoles = new List<CustomerRole>();
        foreach (var customerRole in allCustomerRoles)
            if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                newCustomerRoles.Add(customerRole);

        var customerRolesError = await ValidateCustomerRolesAsync(newCustomerRoles, await _customerService.GetCustomerRolesAsync(customer));

        if (!string.IsNullOrEmpty(customerRolesError))
        {
            ModelState.AddModelError(string.Empty, customerRolesError);
            _notificationService.ErrorNotification(customerRolesError);
        }

        // Ensure that valid email address is entered if Registered role is checked to avoid registered customers with empty email address
        if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == NopCustomerDefaults.RegisteredRoleName) != null &&
            !CommonHelper.IsValidEmail(model.Email))
        {
            ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"));
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"));
        }

        //custom customer attributes
        var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);
        if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == NopCustomerDefaults.RegisteredRoleName) != null)
        {
            var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Upsert account manager mapping
                MsSqlNopDataProvider dbMap = new MsSqlNopDataProvider();
                await dbMap.ExecuteNonQueryAsync(
                    "EXEC dbo.UpsertAccountManagerCustomerMappingByCurrentCustomer @CustomerId, @CurrentCustomerId",
                    new DataParameter("@CustomerId", customer.Id),
                    new DataParameter("@CurrentCustomerId", currentCustomer.Id));

                // Save selected account managers to AccountManager_CustomerMapping with IsPrimary flag
                if (model.SelectedAccountManagerIds != null && model.SelectedAccountManagerIds.Any())
                {
                    try
                    {
                        MsSqlNopDataProvider dbAmSave = new MsSqlNopDataProvider();
                        await dbAmSave.ExecuteNonQueryAsync(
                            "DELETE FROM AccountManager_CustomerMapping WHERE Customer_Id = @CId",
                            new DataParameter("@CId", customer.Id));
                        foreach (var amId in model.SelectedAccountManagerIds.Distinct())
                        {
                            bool isPrimary = amId == model.PrimaryAccountManagerId;
                            await dbAmSave.ExecuteNonQueryAsync(
                                "INSERT INTO AccountManager_CustomerMapping (Customer_Id, AccountManagerId, IsPrimary) VALUES (@CId, @AmId, @IsPrimary)",
                                new DataParameter("@CId", customer.Id),
                                new DataParameter("@AmId", amId),
                                new DataParameter("@IsPrimary", isPrimary));
                        }
                    }
                    catch { }
                }

                // ERP: create new customer
                if (model.Active && (model.ERPCustomerId == null || model.ERPCustomerId == 0) && model.ERPAction == 1)
                {
                    customer.SelectedCODCountryId = model.SelectedCODCountryId;
                    customer.SelectedCustomerTypeId = model.SelectedCustomerTypeId;

                    var countryName = "";
                    var countryIso2 = "";
                    var countryIsoCode = model.CountryId;
                    if (model.CountryId > 0)
                    {
                        var country = await _countryService.GetCountryByIdAsync(model.CountryId);
                        countryName = country?.Name ?? "";
                        countryIso2 = country?.TwoLetterIsoCode ?? "";
                        countryIsoCode = country?.NumericIsoCode ?? 0;
                    }
                    var stateName = model.StateProvinceId > 0
                        ? (await _stateProvinceService.GetStateProvinceByIdAsync(model.StateProvinceId))?.Name ?? ""
                        : "";

                    // Build sales_rep array from selected account managers
                    var salesRepArray = await BuildSalesRepArrayAsync(model, 17375);
                    var salesRepJson = JsonConvert.SerializeObject(salesRepArray);

                    // Read alt_phone from CustomerAttribute id=2
                    var altPhone = _customerAttributeParser.ParseValues(customer.CustomCustomerAttributesXML, 2)
                                       .FirstOrDefault() ?? "";

                    var url = _config.ApiRoot + "&objType=23&objId=0";
                    var httpRequest = new RestRequest(url, Method.POST);
                    var content = $@"{{
    ""objType"": 23, ""objId"": """", ""ns_id"": """", ""type"": ""F"", ""customer_type"": """",
    ""first_name"": ""{model.FirstName}"", ""last_name"": ""{model.LastName}"", ""company_name"": ""{model.Company}"",
    ""leadsource"": ""7435"",
    ""sales_rep"": {salesRepJson},
    ""currency"": """", ""potential"": ""3"",
    ""location"": 3, ""terms"": ""5"", ""region"": ""4"", ""cust_country"": ""11"",
    ""tax_item"": ""10"", ""category"": ""7"", ""sub_category"": ""1"",
    ""whatsapp_number"": """", ""branch"": """",
    ""email"": ""{model.Email}"", ""phone"": ""{model.Username}"",
    ""alt_phone"": ""{altPhone}"", ""web_address"": """", ""emirate"": """", ""language"": """", ""religion"": """",
    ""country"": ""{countryName}"", ""city"": ""{model.City}"", ""zip"": """", ""address_1"": """", ""subsidiary"": ""1"",
    ""addresses"": [{{
        ""ns_id"": """", ""address_label"": """", ""default_billing"": true, ""default_shipping"": true,
        ""country"": ""{countryIso2}"", ""addressee"": ""{model.Company}"", ""addrphone"": ""{model.Username}"",
        ""address_1"": ""{countryName}"", ""address_2"": ""{stateName}"", ""city"": ""{model.City}"",
        ""state"": ""{stateName}"", ""zip"": ""00000""
    }}]
}}";
               
                    httpRequest.AddJsonBody(content);
                    var httpResponse = await _restClient.ExecuteAsync(httpRequest);
                    if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var settings = new JsonSerializerSettings { MetadataPropertyHandling = MetadataPropertyHandling.Ignore };
                        var userData = JsonConvert.DeserializeObject<CustomerRegisterRModel>(httpResponse.Content, settings);
                        model.ERPCustomerId = userData.ns_id;
                        MsSqlNopDataProvider dbErp = new MsSqlNopDataProvider();
                        await dbErp.QueryProcAsync<int>("CustomerSaveERPRegistered",
                            new DataParameter("@CID", customer.Id),
                            new DataParameter("@ERPRegisteredCIdsByPhone", model.ERPRegisteredCIdsByPhone),
                            new DataParameter("@ERPRegisteredCIdsByEmail", model.ERPRegisteredCIdsByEmail),
                            new DataParameter("@ERPCustomerId", model.ERPCustomerId));
                        _ = _logger.InformationAsync($"ERPCustomer ({customer.Id}) registered. ns_id: {userData.ns_id}, message: {userData.message}");
                    }
                    else
                        _ = _logger.ErrorAsync("ERPCustomer register failed: " + httpResponse.Content);
                }

                // ERP: update existing customer — use entity value from DB (reliable)
                int erpCustIdForUpdate = customer.ERPCustomerId ?? 0;
                if (erpCustIdForUpdate <= 0 && model.ERPCustomerId > 0)
                    erpCustIdForUpdate = model.ERPCustomerId;

                if (erpCustIdForUpdate > 0)
                {
                    try
                    {
                        // Build full sales_rep array from selected account managers
                        var updateSalesRepArray = await BuildSalesRepArrayAsync(model, 17375);

                        // Read alt_phone from CustomerAttribute id=2
                        var altPhoneUpd = _customerAttributeParser.ParseValues(customer.CustomCustomerAttributesXML, 2)
                                              .FirstOrDefault() ?? "";

                        // Get country name for update
                        var updCountryName = "";
                        if (model.CountryId > 0)
                        {
                            var updCountry = await _countryService.GetCountryByIdAsync(model.CountryId);
                            updCountryName = updCountry?.Name ?? "";
                        }

                        var updateUrl = $"{_config.ApiRoot}&objType=26&objId=0";
                        var updateRequest = new RestRequest(updateUrl, Method.POST);
                        var updateBody = $@"{{
    ""objType"": 23,
    ""objId"": ""{erpCustIdForUpdate}"",
    ""ns_id"": """",
    ""type"": ""F"",
    ""customer_type"": """",
    ""first_name"": ""{model.FirstName ?? ""}"",
    ""last_name"": ""{model.LastName ?? ""}"",
    ""company_name"": ""{model.Company ?? ""}"",
    ""leadsource"": ""7435"",
    ""sales_rep"": {JsonConvert.SerializeObject(updateSalesRepArray)},
    ""currency"": """",
    ""potential"": ""3"",
    ""location"": 3,
    ""terms"": ""5"",
    ""region"": ""4"",
    ""cust_country"": ""11"",
    ""tax_item"": ""10"",
    ""category"": ""7"",
    ""sub_category"": ""1"",
    ""whatsapp_number"": """",
    ""branch"": """",
    ""email"": ""{model.Email ?? ""}"",
    ""phone"": ""{model.Username ?? ""}"",
    ""alt_phone"": ""{altPhoneUpd}"",
    ""web_address"": """",
    ""emirate"": """",
    ""language"": """",
    ""religion"": """",
    ""country"": ""{updCountryName}"",
    ""city"": ""{model.City ?? ""}"",
    ""zip"": """",
    ""address_1"": """",
    ""subsidiary"": ""1""
}}";
                     
                        updateRequest.AddJsonBody(updateBody);
                        await _logger.InformationAsync($"ERP UPDATE REQUEST | URL: {updateUrl} | BODY: {updateBody}");
                        var updateResponse = await _restClient.ExecuteAsync(updateRequest);
                        if (updateResponse.IsSuccessful)
                        {
                            customer.ERPCustomerIdToUpdate = erpCustIdForUpdate;
                            await _customerService.UpdateCustomerAsync(customer);
                            await _logger.InformationAsync($"ERPCustomer UPDATE OK. CID={customer.Id}, ERP_ID={erpCustIdForUpdate}, Response={updateResponse.Content}");
                        }
                        else
                            await _logger.ErrorAsync($"ERPCustomer UPDATE FAILED. CID={customer.Id}, ERP_ID={erpCustIdForUpdate}, Status={updateResponse.StatusCode}, Response={updateResponse.Content}");
                    }
                    catch (Exception erpEx)
                    {
                        await _logger.ErrorAsync($"ERPCustomer UPDATE EXCEPTION. CID={customer.Id}, ERP_ID={erpCustIdForUpdate}", erpEx);
                    }
                }

                customer.SelectedCODCountryId = model.SelectedCODCountryId;
                customer.SelectedCustomerTypeId = model.SelectedCustomerTypeId;

                customer.AdminComment = model.AdminComment;
                customer.IsTaxExempt = model.IsTaxExempt;
                customer.MustChangePassword = model.MustChangePassword;

                //prevent deactivation of the last active administrator
                if (!await _customerService.IsAdminAsync(customer) || model.Active || await SecondAdminAccountExistsAsync(customer))
                    customer.Active = model.Active;
                else
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.AdminAccountShouldExists.Deactivate"));

                //email
                if (!string.IsNullOrWhiteSpace(model.Email))
                    await _customerRegistrationService.SetEmailAsync(customer, model.Email, false);
                else
                    customer.Email = model.Email;

                //username
                if (_customerSettings.UsernamesEnabled)
                {
                    if (!string.IsNullOrWhiteSpace(model.Username))
                        await _customerRegistrationService.SetUsernameAsync(customer, model.Username);
                    else
                        customer.Username = model.Username;
                }

                //VAT number
                if (_taxSettings.EuVatEnabled)
                {
                    var prevVatNumber = customer.VatNumber;

                    customer.VatNumber = model.VatNumber;
                    //set VAT number status
                    if (!string.IsNullOrEmpty(model.VatNumber))
                    {
                        if (!model.VatNumber.Equals(prevVatNumber, StringComparison.InvariantCultureIgnoreCase))
                        {
                            customer.VatNumberStatusId = (int)(await _taxService.GetVatNumberStatusAsync(model.VatNumber)).vatNumberStatus;
                        }
                    }
                    else
                        customer.VatNumberStatusId = (int)VatNumberStatus.Empty;
                }

                //vendor
                customer.VendorId = model.VendorId;

                //form fields
                if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    customer.TimeZoneId = model.TimeZoneId;
                if (_customerSettings.GenderEnabled)
                    customer.Gender = model.Gender;
                if (_customerSettings.FirstNameEnabled)
                    customer.FirstName = model.FirstName;
                if (_customerSettings.LastNameEnabled)
                    customer.LastName = model.LastName;
                if (_customerSettings.DateOfBirthEnabled)
                    customer.DateOfBirth = model.DateOfBirth;
                if (_customerSettings.CompanyEnabled)
                    customer.Company = model.Company;
                if (_customerSettings.StreetAddressEnabled)
                    customer.StreetAddress = model.StreetAddress;
                if (_customerSettings.StreetAddress2Enabled)
                    customer.StreetAddress2 = model.StreetAddress2;
                if (_customerSettings.ZipPostalCodeEnabled)
                    customer.ZipPostalCode = model.ZipPostalCode;
                if (_customerSettings.CityEnabled)
                    customer.City = model.City;
                if (_customerSettings.CountyEnabled)
                    customer.County = model.County;
                if (_customerSettings.CountryEnabled)
                    customer.CountryId = model.CountryId;
                if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                    customer.StateProvinceId = model.StateProvinceId;
                if (_customerSettings.PhoneEnabled)
                    customer.Phone = model.Phone;
                if (_customerSettings.FaxEnabled)
                    customer.Fax = model.Fax;

                //custom customer attributes
                customer.CustomCustomerAttributesXML = customerAttributesXml;

                var existingCustomerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer, true);

                //customer roles
                foreach (var customerRole in allCustomerRoles)
                {
                    //ensure that the current customer cannot add/remove to/from "Administrators" system role
                    //if he's not an admin himself
                    if (customerRole.SystemName == NopCustomerDefaults.AdministratorsRoleName &&
                        !await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
                        continue;

                    if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                    {
                        //new role
                        if (existingCustomerRoleIds.All(roleId => roleId != customerRole.Id))
                            await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = customerRole.Id });
                    }
                    else
                    {
                        //prevent attempts to delete the administrator role from the user, if the user is the last active administrator
                        if (customerRole.SystemName == NopCustomerDefaults.AdministratorsRoleName && !await SecondAdminAccountExistsAsync(customer))
                        {
                            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.AdminAccountShouldExists.DeleteRole"));
                            continue;
                        }

                        //remove role
                        if (existingCustomerRoleIds.Any(roleId => roleId == customerRole.Id))
                            await _customerService.RemoveCustomerRoleMappingAsync(customer, customerRole);
                    }
                }

                try
                {
                    MsSqlNopDataProvider dbFactor = new MsSqlNopDataProvider();
                    await dbFactor.QueryAsync<dynamic>(
                        "EXEC [dbo].[UpdateFactorRoleMappingsToCustomer] @CuId, @SelectedCODCountryId, @SelectedCustomerTypeId",
                        new DataParameter("@CuId", customer.Id),
                        new DataParameter("@SelectedCODCountryId", model.SelectedCODCountryId),
                        new DataParameter("@SelectedCustomerTypeId", model.SelectedCustomerTypeId));
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync($"UpdateCustomerFactorRoleMappings Error for CustomerId: {customer.Id}. {ex.Message}");
                }

                await _customerService.UpdateCustomerAsync(customer);

                //ensure that a customer with a vendor associated is not in "Administrators" role
                //otherwise, he won't have access to the other functionality in admin area
                if (await _customerService.IsAdminAsync(customer) && customer.VendorId > 0)
                {
                    customer.VendorId = 0;
                    await _customerService.UpdateCustomerAsync(customer);
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.AdminCouldNotbeVendor"));
                }

                //ensure that a customer in the Vendors role has a vendor account associated.
                //otherwise, he will have access to ALL products
                if (await _customerService.IsVendorAsync(customer) && customer.VendorId == 0)
                {
                    var vendorRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.VendorsRoleName);
                    await _customerService.RemoveCustomerRoleMappingAsync(customer, vendorRole);

                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.CannotBeInVendoRoleWithoutVendorAssociated"));
                }

                //activity log
                await _customerActivityService.InsertActivityAsync("EditCustomer",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditCustomer"), customer.Id), customer);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = customer.Id });
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
            }
        }

        //prepare model
        model = await _customerModelFactory.PrepareCustomerModelAsync(model, customer, true);

        var codFactorsEdit = await _customerService.GetAllCODFactorsAsync();
        model.AvailableCODCountries = codFactorsEdit.Select(c => new SelectListItem
        {
            Value = (c.CountryID + "," + c.FactorID).ToString(),
            Text = c.Name,
            Selected = model.SelectedCODCountryId == (c.CountryID + "," + c.FactorID).ToString()
        }).ToList();
        model.AvailableCODCountries.Insert(0, new SelectListItem { Text = "Select COD Country", Value = "" });

        var customerTypesEdit = await _customerService.GetAllCustomerTypeAsync();
        model.AvailableCustomerType = customerTypesEdit?.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.TypeName,
            Selected = model.SelectedCustomerTypeId == c.Id
        }).ToList() ?? new List<SelectListItem>();
        model.AvailableCustomerType.Insert(0, new SelectListItem { Text = "Select Customer Type", Value = "" });

        ModelState.Remove(nameof(model.SelectedCODCountryId));
        model.AvailableERPActions = GetAvailableERPActions();

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpPost, ActionName("Edit")]
    [FormValueRequired("changepassword")]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> ChangePassword(CustomerModel model)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.Id);
        if (customer == null)
            return RedirectToAction("List");

        //ensure that the current customer cannot change passwords of "Administrators" if he's not an admin himself
        if (await _customerService.IsAdminAsync(customer) && !await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.OnlyAdminCanChangePassword"));
            return RedirectToAction("Edit", new { id = customer.Id });
        }

        var changePassRequest = new ChangePasswordRequest(customer.Email,
            false, _customerSettings.DefaultPasswordFormat, model.Password);
        var changePassResult = await _customerRegistrationService.ChangePasswordAsync(changePassRequest);
        if (changePassResult.Success)
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.PasswordChanged"));
        else
            foreach (var error in changePassResult.Errors)
                _notificationService.ErrorNotification(error);

        return RedirectToAction("Edit", new { id = customer.Id });
    }

    [HttpPost, ActionName("Edit")]
    [FormValueRequired("markVatNumberAsValid")]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> MarkVatNumberAsValid(CustomerModel model)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.Id);
        if (customer == null)
            return RedirectToAction("List");

        customer.VatNumberStatusId = (int)VatNumberStatus.Valid;
        await _customerService.UpdateCustomerAsync(customer);

        return RedirectToAction("Edit", new { id = customer.Id });
    }

    [HttpPost, ActionName("Edit")]
    [FormValueRequired("markVatNumberAsInvalid")]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> MarkVatNumberAsInvalid(CustomerModel model)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.Id);
        if (customer == null)
            return RedirectToAction("List");

        customer.VatNumberStatusId = (int)VatNumberStatus.Invalid;
        await _customerService.UpdateCustomerAsync(customer);

        return RedirectToAction("Edit", new { id = customer.Id });
    }

    [HttpPost, ActionName("Edit")]
    [FormValueRequired("remove-affiliate")]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> RemoveAffiliate(CustomerModel model)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.Id);
        if (customer == null)
            return RedirectToAction("List");

        customer.AffiliateId = 0;
        await _customerService.UpdateCustomerAsync(customer);

        return RedirectToAction("Edit", new { id = customer.Id });
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> RemoveBindMFA(int id)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
            return RedirectToAction("List");

        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedMultiFactorAuthenticationProviderAttribute, string.Empty);

        //raise event       
        await _eventPublisher.PublishAsync(new CustomerChangeMultiFactorAuthenticationProviderEvent(customer));

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.UnbindMFAProvider"));

        return RedirectToAction("Edit", new { id = customer.Id });
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Delete(int id)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
            return RedirectToAction("List");

        try
        {
            //prevent attempts to delete the user, if it is the last active administrator
            if (await _customerService.IsAdminAsync(customer) && !await SecondAdminAccountExistsAsync(customer))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.AdminAccountShouldExists.DeleteAdministrator"));
                return RedirectToAction("Edit", new { id = customer.Id });
            }

            //ensure that the current customer cannot delete "Administrators" if he's not an admin himself
            if (await _customerService.IsAdminAsync(customer) && !await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.OnlyAdminCanDeleteAdmin"));
                return RedirectToAction("Edit", new { id = customer.Id });
            }

            //get customer email before deleting customer entity to avoid problems with the changed email after deleting see CustomerSettings.SuffixDeletedCustomers settings
            var customerEmail = customer.Email;

            //delete
            await _customerService.DeleteCustomerAsync(customer);

            //remove newsletter subscriptions (if exist)
            var subscriptions = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionsByEmailAsync(customerEmail);
            foreach (var subscription in subscriptions)
                await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(subscription);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteCustomer",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCustomer"), customer.Id), customer);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.Deleted"));

            return RedirectToAction("List");
        }
        catch (Exception exc)
        {
            _notificationService.ErrorNotification(exc.Message);
            return RedirectToAction("Edit", new { id = customer.Id });
        }
    }

    [HttpPost, ActionName("Edit")]
    [FormValueRequired("impersonate")]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_IMPERSONATION)]
    public virtual async Task<IActionResult> Impersonate(int id)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
            return RedirectToAction("List");

        if (!customer.Active)
        {
            _notificationService.WarningNotification(
                await _localizationService.GetResourceAsync("Admin.Customers.Customers.Impersonate.Inactive"));
            return RedirectToAction("Edit", customer.Id);
        }

        //ensure that a non-admin user cannot impersonate as an administrator
        //otherwise, that user can simply impersonate as an administrator and gain additional administrative privileges
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsAdminAsync(currentCustomer) && await _customerService.IsAdminAsync(customer))
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.NonAdminNotImpersonateAsAdminError"));
            return RedirectToAction("Edit", customer.Id);
        }

        //activity log
        await _customerActivityService.InsertActivityAsync("Impersonation.Started",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Started.StoreOwner"), customer.Email, customer.Id), customer);
        await _customerActivityService.InsertActivityAsync(customer, "Impersonation.Started",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Started.Customer"), currentCustomer.Email, currentCustomer.Id), currentCustomer);

        //ensure login is not required
        customer.RequireReLogin = false;
        await _customerService.UpdateCustomerAsync(customer);
        await _genericAttributeService.SaveAttributeAsync<int?>(currentCustomer, NopCustomerDefaults.ImpersonatedCustomerIdAttribute, customer.Id);

        return RedirectToAction("Index", "Home", new { area = string.Empty });
    }

    [HttpPost, ActionName("Edit")]
    [FormValueRequired("send-welcome-message")]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> SendWelcomeMessage(CustomerModel model)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.Id);
        if (customer == null)
            return RedirectToAction("List");

        await _workflowMessageService.SendCustomerWelcomeMessageAsync(customer, (await _workContext.GetWorkingLanguageAsync()).Id);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.SendWelcomeMessage.Success"));

        return RedirectToAction("Edit", new { id = customer.Id });
    }

    [HttpPost, ActionName("Edit")]
    [FormValueRequired("resend-activation-message")]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> ReSendActivationMessage(CustomerModel model)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.Id);
        if (customer == null)
            return RedirectToAction("List");

        //email validation message
        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
        await _workflowMessageService.SendCustomerEmailValidationMessageAsync(customer, (await _workContext.GetWorkingLanguageAsync()).Id);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.ReSendActivationMessage.Success"));

        return RedirectToAction("Edit", new { id = customer.Id });
    }

    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> SendEmail(CustomerModel model)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.Id);
        if (customer == null)
            return RedirectToAction("List");

        try
        {
            if (string.IsNullOrWhiteSpace(customer.Email))
                throw new NopException("Customer email is empty");
            if (!CommonHelper.IsValidEmail(customer.Email))
                throw new NopException("Customer email is not valid");
            if (string.IsNullOrWhiteSpace(model.SendEmail.Subject))
                throw new NopException("Email subject is empty");
            if (string.IsNullOrWhiteSpace(model.SendEmail.Body))
                throw new NopException("Email body is empty");

            var emailAccount = (await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId)
                ?? (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault())
                ?? throw new NopException("Email account can't be loaded");
            var email = new QueuedEmail
            {
                Priority = QueuedEmailPriority.High,
                EmailAccountId = emailAccount.Id,
                FromName = emailAccount.DisplayName,
                From = emailAccount.Email,
                ToName = await _customerService.GetCustomerFullNameAsync(customer),
                To = customer.Email,
                Subject = model.SendEmail.Subject,
                Body = model.SendEmail.Body,
                CreatedOnUtc = DateTime.UtcNow,
                DontSendBeforeDateUtc = model.SendEmail.SendImmediately || !model.SendEmail.DontSendBeforeDate.HasValue ?
                    null : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.SendEmail.DontSendBeforeDate.Value)
            };
            await _queuedEmailService.InsertQueuedEmailAsync(email);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.SendEmail.Queued"));
        }
        catch (Exception exc)
        {
            _notificationService.ErrorNotification(exc.Message);
        }

        return RedirectToAction("Edit", new { id = customer.Id });
    }

    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> SendPm(CustomerModel model)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.Id);
        if (customer == null)
            return RedirectToAction("List");

        try
        {
            if (!_forumSettings.AllowPrivateMessages)
                throw new NopException("Private messages are disabled");
            if (await _customerService.IsGuestAsync(customer))
                throw new NopException("Customer should be registered");
            if (string.IsNullOrWhiteSpace(model.SendPm.Subject))
                throw new NopException(await _localizationService.GetResourceAsync("PrivateMessages.SubjectCannotBeEmpty"));
            if (string.IsNullOrWhiteSpace(model.SendPm.Message))
                throw new NopException(await _localizationService.GetResourceAsync("PrivateMessages.MessageCannotBeEmpty"));

            var store = await _storeContext.GetCurrentStoreAsync();
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            var privateMessage = new PrivateMessage
            {
                StoreId = store.Id,
                ToCustomerId = customer.Id,
                FromCustomerId = currentCustomer.Id,
                Subject = model.SendPm.Subject,
                Text = model.SendPm.Message,
                IsDeletedByAuthor = false,
                IsDeletedByRecipient = false,
                IsRead = false,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _forumService.InsertPrivateMessageAsync(privateMessage);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.SendPM.Sent"));
        }
        catch (Exception exc)
        {
            _notificationService.ErrorNotification(exc.Message);
        }

        return RedirectToAction("Edit", new { id = customer.Id });
    }

    #endregion

    #region Reward points history

    [HttpPost]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    public virtual async Task<IActionResult> RewardPointsHistorySelect(CustomerRewardPointsSearchModel searchModel)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(searchModel.CustomerId)
            ?? throw new ArgumentException("No customer found with the specified id");

        //prepare model
        var model = await _customerModelFactory.PrepareRewardPointsListModelAsync(searchModel, customer);

        return Json(model);
    }

    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> RewardPointsHistoryAdd(AddRewardPointsToCustomerModel model)
    {
        //prevent adding a new row with zero value
        if (model.Points == 0)
            return ErrorJson(await _localizationService.GetResourceAsync("Admin.Customers.Customers.RewardPoints.AddingZeroValueNotAllowed"));

        //prevent adding negative point validity for point reduction
        if (model.Points < 0 && model.PointsValidity.HasValue)
            return ErrorJson(await _localizationService.GetResourceAsync("Admin.Customers.Customers.RewardPoints.Fields.AddNegativePointsValidity"));

        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.CustomerId);
        if (customer == null)
            return ErrorJson("Customer cannot be loaded");

        //check whether delay is set
        DateTime? activatingDate = null;
        if (!model.ActivatePointsImmediately && model.ActivationDelay > 0)
        {
            var delayPeriod = (RewardPointsActivatingDelayPeriod)model.ActivationDelayPeriodId;
            var delayInHours = delayPeriod.ToHours(model.ActivationDelay);
            activatingDate = DateTime.UtcNow.AddHours(delayInHours);
        }

        //whether points validity is set
        DateTime? endDate = null;
        if (model.PointsValidity > 0)
            endDate = (activatingDate ?? DateTime.UtcNow).AddDays(model.PointsValidity.Value);

        //add reward points
        await _rewardPointService.AddRewardPointsHistoryEntryAsync(customer, model.Points, model.StoreId, model.Message,
            activatingDate: activatingDate, endDate: endDate);

        return Json(new { Result = true });
    }

    #endregion

    #region Addresses

    [HttpPost]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    public virtual async Task<IActionResult> AddressesSelect(CustomerAddressSearchModel searchModel)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(searchModel.CustomerId)
            ?? throw new ArgumentException("No customer found with the specified id");

        //prepare model
        var model = await _customerModelFactory.PrepareCustomerAddressListModelAsync(searchModel, customer);

        return Json(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> AddressDelete(int id, int customerId)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(customerId)
            ?? throw new ArgumentException("No customer found with the specified id", nameof(customerId));

        //try to get an address with the specified id
        var address = await _customerService.GetCustomerAddressAsync(customer.Id, id);

        if (address == null)
            return Content("No address found with the specified id");

        await _customerService.RemoveCustomerAddressAsync(customer, address);
        await _customerService.UpdateCustomerAsync(customer);

        //now delete the address record
        await _addressService.DeleteAddressAsync(address);

        return new NullJsonResult();
    }

    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> AddressCreate(int customerId)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(customerId);
        if (customer == null)
            return RedirectToAction("List");

        //prepare model
        var model = await _customerModelFactory.PrepareCustomerAddressModelAsync(new CustomerAddressModel(), customer, null);

        return View(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> AddressCreate(CustomerAddressModel model, IFormCollection form)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.CustomerId);
        if (customer == null)
            return RedirectToAction("List");

        //custom address attributes
        var customAttributes = await _addressAttributeParser.ParseCustomAttributesAsync(form, NopCommonDefaults.AddressAttributeControlName);
        var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
        foreach (var error in customAttributeWarnings)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        if (ModelState.IsValid)
        {
            var address = model.Address.ToEntity<Address>();
            address.CustomAttributes = customAttributes;
            address.CreatedOnUtc = DateTime.UtcNow;

            //some validation
            if (address.CountryId == 0)
                address.CountryId = null;
            if (address.StateProvinceId == 0)
                address.StateProvinceId = null;

            await _addressService.InsertAddressAsync(address);

            await _customerService.InsertCustomerAddressAsync(customer, address);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.Addresses.Added"));

            return RedirectToAction("AddressEdit", new { addressId = address.Id, customerId = model.CustomerId });
        }

        //prepare model
        model = await _customerModelFactory.PrepareCustomerAddressModelAsync(model, customer, null, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> AddressEdit(int addressId, int customerId)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(customerId);
        if (customer == null)
            return RedirectToAction("List");

        //try to get an address with the specified id
        var address = await _addressService.GetAddressByIdAsync(addressId);
        if (address == null)
            return RedirectToAction("Edit", new { id = customer.Id });

        //prepare model
        var model = await _customerModelFactory.PrepareCustomerAddressModelAsync(null, customer, address);

        return View(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> AddressEdit(CustomerAddressModel model, IFormCollection form)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.CustomerId);
        if (customer == null)
            return RedirectToAction("List");

        //try to get an address with the specified id
        var address = await _addressService.GetAddressByIdAsync(model.Address.Id);
        if (address == null)
            return RedirectToAction("Edit", new { id = customer.Id });

        //custom address attributes
        var customAttributes = await _addressAttributeParser.ParseCustomAttributesAsync(form, NopCommonDefaults.AddressAttributeControlName);
        var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
        foreach (var error in customAttributeWarnings)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        if (ModelState.IsValid)
        {
            address = model.Address.ToEntity(address);
            address.CustomAttributes = customAttributes;
            await _addressService.UpdateAddressAsync(address);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.Addresses.Updated"));

            return RedirectToAction("AddressEdit", new { addressId = model.Address.Id, customerId = model.CustomerId });
        }

        //prepare model
        model = await _customerModelFactory.PrepareCustomerAddressModelAsync(model, customer, address, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    #endregion

    #region Orders

    [HttpPost]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    public virtual async Task<IActionResult> OrderList(CustomerOrderSearchModel searchModel)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(searchModel.CustomerId)
            ?? throw new ArgumentException("No customer found with the specified id");

        //prepare model
        var model = await _customerModelFactory.PrepareCustomerOrderListModelAsync(searchModel, customer);

        return Json(model);
    }

    #endregion

    #region Customer

    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    public virtual async Task<IActionResult> LoadCustomerStatistics(string period)
    {
        var result = new List<object>();

        var nowDt = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
        var timeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();
        var searchCustomerRoleIds = new[] { (await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName)).Id };

        var culture = new CultureInfo((await _workContext.GetWorkingLanguageAsync()).LanguageCulture);

        switch (period)
        {
            case "year":
                //year statistics
                var yearAgoDt = nowDt.AddYears(-1).AddMonths(1);
                var searchYearDateUser = new DateTime(yearAgoDt.Year, yearAgoDt.Month, 1);
                for (var i = 0; i <= 12; i++)
                {
                    result.Add(new
                    {
                        date = searchYearDateUser.Date.ToString("Y", culture),
                        value = (await _customerService.GetAllCustomersAsync(
                            createdFromUtc: _dateTimeHelper.ConvertToUtcTime(searchYearDateUser, timeZone),
                            createdToUtc: _dateTimeHelper.ConvertToUtcTime(searchYearDateUser.AddMonths(1), timeZone),
                            customerRoleIds: searchCustomerRoleIds,
                            pageIndex: 0,
                            pageSize: 1, getOnlyTotalCount: true)).TotalCount.ToString()
                    });

                    searchYearDateUser = searchYearDateUser.AddMonths(1);
                }

                break;
            case "month":
                //month statistics
                var monthAgoDt = nowDt.AddDays(-30);
                var searchMonthDateUser = new DateTime(monthAgoDt.Year, monthAgoDt.Month, monthAgoDt.Day);
                for (var i = 0; i <= 30; i++)
                {
                    result.Add(new
                    {
                        date = searchMonthDateUser.Date.ToString("M", culture),
                        value = (await _customerService.GetAllCustomersAsync(
                            createdFromUtc: _dateTimeHelper.ConvertToUtcTime(searchMonthDateUser, timeZone),
                            createdToUtc: _dateTimeHelper.ConvertToUtcTime(searchMonthDateUser.AddDays(1), timeZone),
                            customerRoleIds: searchCustomerRoleIds,
                            pageIndex: 0,
                            pageSize: 1, getOnlyTotalCount: true)).TotalCount.ToString()
                    });

                    searchMonthDateUser = searchMonthDateUser.AddDays(1);
                }

                break;
            case "week":
            default:
                //week statistics
                var weekAgoDt = nowDt.AddDays(-7);
                var searchWeekDateUser = new DateTime(weekAgoDt.Year, weekAgoDt.Month, weekAgoDt.Day);
                for (var i = 0; i <= 7; i++)
                {
                    result.Add(new
                    {
                        date = searchWeekDateUser.Date.ToString("d dddd", culture),
                        value = (await _customerService.GetAllCustomersAsync(
                            createdFromUtc: _dateTimeHelper.ConvertToUtcTime(searchWeekDateUser, timeZone),
                            createdToUtc: _dateTimeHelper.ConvertToUtcTime(searchWeekDateUser.AddDays(1), timeZone),
                            customerRoleIds: searchCustomerRoleIds,
                            pageIndex: 0,
                            pageSize: 1, getOnlyTotalCount: true)).TotalCount.ToString()
                    });

                    searchWeekDateUser = searchWeekDateUser.AddDays(1);
                }

                break;
        }

        return Json(result);
    }

    #endregion

    #region Current shopping cart/ wishlist

    [HttpPost]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    public virtual async Task<IActionResult> GetCartList(CustomerShoppingCartSearchModel searchModel)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(searchModel.CustomerId)
            ?? throw new ArgumentException("No customer found with the specified id");

        //prepare model
        var model = await _customerModelFactory.PrepareCustomerShoppingCartListModelAsync(searchModel, customer);

        return Json(model);
    }

    #endregion

    #region Activity log

    [HttpPost]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    [CheckPermission(StandardPermission.Customers.ACTIVITY_LOG_VIEW)]
    public virtual async Task<IActionResult> ListActivityLog(CustomerActivityLogSearchModel searchModel)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(searchModel.CustomerId)
            ?? throw new ArgumentException("No customer found with the specified id");

        //prepare model
        var model = await _customerModelFactory.PrepareCustomerActivityLogListModelAsync(searchModel, customer);

        return Json(model);
    }

    #endregion

    #region Back in stock subscriptions

    [HttpPost]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    public virtual async Task<IActionResult> BackInStockSubscriptionList(CustomerBackInStockSubscriptionSearchModel searchModel)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(searchModel.CustomerId)
            ?? throw new ArgumentException("No customer found with the specified id");

        //prepare model
        var model = await _customerModelFactory.PrepareCustomerBackInStockSubscriptionListModelAsync(searchModel, customer);

        return Json(model);
    }

    #endregion

    #region GDPR

    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    [CheckPermission(StandardPermission.Customers.GDPR_MANAGE)]
    public virtual async Task<IActionResult> GdprLog()
    {
        //prepare model
        var model = await _customerModelFactory.PrepareGdprLogSearchModelAsync(new GdprLogSearchModel());

        return View(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    [CheckPermission(StandardPermission.Customers.GDPR_MANAGE)]
    public virtual async Task<IActionResult> GdprLogList(GdprLogSearchModel searchModel)
    {
        //prepare model
        var model = await _customerModelFactory.PrepareGdprLogListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    [CheckPermission(StandardPermission.Customers.GDPR_MANAGE)]
    public virtual async Task<IActionResult> GdprDelete(int id)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
            return RedirectToAction("List");

        if (!_gdprSettings.GdprEnabled)
            return RedirectToAction("List");

        try
        {
            //prevent attempts to delete the user, if it is the last active administrator
            if (await _customerService.IsAdminAsync(customer) && !await SecondAdminAccountExistsAsync(customer))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.AdminAccountShouldExists.DeleteAdministrator"));
                return RedirectToAction("Edit", new { id = customer.Id });
            }

            //ensure that the current customer cannot delete "Administrators" if he's not an admin himself
            if (await _customerService.IsAdminAsync(customer) && !await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.OnlyAdminCanDeleteAdmin"));
                return RedirectToAction("Edit", new { id = customer.Id });
            }

            //delete
            await _gdprService.PermanentDeleteCustomerAsync(customer);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteCustomer",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCustomer"), customer.Id), customer);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.Deleted"));

            return RedirectToAction("List");
        }
        catch (Exception exc)
        {
            _notificationService.ErrorNotification(exc.Message);
            return RedirectToAction("Edit", new { id = customer.Id });
        }
    }

    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    [CheckPermission(StandardPermission.Customers.GDPR_MANAGE)]
    public virtual async Task<IActionResult> GdprExport(int id)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
            return RedirectToAction("List");

        try
        {
            //log
            //_gdprService.InsertLog(customer, 0, GdprRequestType.ExportData, await _localizationService.GetResource("Gdpr.Exported"));
            
            //export
            var store = await _storeContext.GetCurrentStoreAsync();
            var bytes = await _exportManager.ExportCustomerGdprInfoToXlsxAsync(customer, store.Id);

            return File(bytes, MimeTypes.TextXlsx, $"customerdata-{customer.Id}.xlsx");
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("Edit", new { id = customer.Id });
        }
    }
    #endregion

    #region Export / Import

    [HttpPost, ActionName("ExportExcel")]
    [FormValueRequired("exportexcel-all")]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_IMPORT_EXPORT)]
    public virtual async Task<IActionResult> ExportExcelAll(CustomerSearchModel model)
    {
        var customers = await _customerService.GetAllCustomersAsync(customerRoleIds: model.SelectedCustomerRoleIds.ToArray(),
            email: model.SearchEmail,
            username: model.SearchUsername,
            firstName: model.SearchFirstName,
            lastName: model.SearchLastName,
            dayOfBirth: int.TryParse(model.SearchDayOfBirth, out var dayOfBirth) ? dayOfBirth : 0,
            monthOfBirth: int.TryParse(model.SearchMonthOfBirth, out var monthOfBirth) ? monthOfBirth : 0,
            company: model.SearchCompany,
            isActive: model.SearchIsActive,
            phone: model.SearchPhone,
            zipPostalCode: model.SearchZipPostalCode);

        try
        {
            var bytes = await _exportManager.ExportCustomersToXlsxAsync(customers);
            return File(bytes, MimeTypes.TextXlsx, "customers.xlsx");
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("List");
        }
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_IMPORT_EXPORT)]
    public virtual async Task<IActionResult> ExportExcelSelected(string selectedIds)
    {
        var customers = new List<Customer>();
        if (selectedIds != null)
        {
            var ids = selectedIds
                .Split(_separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();
            customers.AddRange(await _customerService.GetCustomersByIdsAsync(ids));
        }

        try
        {
            var bytes = await _exportManager.ExportCustomersToXlsxAsync(customers);
            return File(bytes, MimeTypes.TextXlsx, "customers.xlsx");
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("List");
        }
    }

    [HttpPost, ActionName("ExportXML")]
    [FormValueRequired("exportxml-all")]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_IMPORT_EXPORT)]
    public virtual async Task<IActionResult> ExportXmlAll(CustomerSearchModel model)
    {
        var customers = await _customerService.GetAllCustomersAsync(customerRoleIds: model.SelectedCustomerRoleIds.ToArray(),
            email: model.SearchEmail,
            username: model.SearchUsername,
            firstName: model.SearchFirstName,
            lastName: model.SearchLastName,
            dayOfBirth: int.TryParse(model.SearchDayOfBirth, out var dayOfBirth) ? dayOfBirth : 0,
            monthOfBirth: int.TryParse(model.SearchMonthOfBirth, out var monthOfBirth) ? monthOfBirth : 0,
            company: model.SearchCompany,
            isActive: model.SearchIsActive,
            phone: model.SearchPhone,
            zipPostalCode: model.SearchZipPostalCode);

        try
        {
            var xml = await _exportManager.ExportCustomersToXmlAsync(customers);
            return File(Encoding.UTF8.GetBytes(xml), "application/xml", "customers.xml");
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("List");
        }
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_IMPORT_EXPORT)]
    public virtual async Task<IActionResult> ExportXmlSelected(string selectedIds)
    {
        var customers = new List<Customer>();
        if (selectedIds != null)
        {
            var ids = selectedIds
                .Split(_separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();
            customers.AddRange(await _customerService.GetCustomersByIdsAsync(ids));
        }

        try
        {
            var xml = await _exportManager.ExportCustomersToXmlAsync(customers);
            return File(Encoding.UTF8.GetBytes(xml), "application/xml", "customers.xml");
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("List");
        }
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_IMPORT_EXPORT)]
    public virtual async Task<IActionResult> ImportExcel(IFormFile importexcelfile)
    {
        if (await _workContext.GetCurrentVendorAsync() != null)
            //a vendor can not import customer
            return AccessDeniedView();

        try
        {
            if ((importexcelfile?.Length ?? 0) > 0)
                await _importManager.ImportCustomersFromXlsxAsync(importexcelfile.OpenReadStream());
            else
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));

                return RedirectToAction("List");
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.Imported"));

            return RedirectToAction("List");
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);

            return RedirectToAction("List");
        }
    }

    #endregion
}