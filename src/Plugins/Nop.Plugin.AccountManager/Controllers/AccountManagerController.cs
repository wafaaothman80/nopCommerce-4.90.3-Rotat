using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Plugin.AccountManager.Domain;
using Nop.Plugin.AccountManager.Factories;
using Nop.Plugin.AccountManager.Models;
using Nop.Plugin.AccountManager.Services;
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
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;

namespace Nop.Plugin.AccountManager.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public partial class AccountManagerController : BasePluginController
{
    #region Fields

    protected readonly CustomerSettings _customerSettings;
    protected readonly DateTimeSettings _dateTimeSettings;
    protected readonly EmailAccountSettings _emailAccountSettings;
    protected readonly ForumSettings _forumSettings;
    protected readonly GdprSettings _gdprSettings;
    protected readonly IAddressService _addressService;
    protected readonly IAccountManagerService _accountManagerService;
    protected readonly IAccountManagerModelFactory _accountManagerModelFactory;
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
    protected readonly IStoreContext _storeContext;
    protected readonly IStoreService _storeService;
    protected readonly ITaxService _taxService;
    protected readonly IWorkContext _workContext;
    protected readonly IWorkflowMessageService _workflowMessageService;
    protected readonly TaxSettings _taxSettings;
    private static readonly char[] _separator = [','];
    private static readonly NetSuiteApiConfig _erpConfig = new NetSuiteApiConfig();
    private static readonly RestClient _erpClient = CreateErpRestClient();

    #endregion

    #region Ctor

    public AccountManagerController(CustomerSettings customerSettings,
        DateTimeSettings dateTimeSettings,
        EmailAccountSettings emailAccountSettings,
        ForumSettings forumSettings,
        GdprSettings gdprSettings,
        IAddressService addressService,
        IAccountManagerService accountManagerService,
        IAccountManagerModelFactory accountManagerModelFactory,
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
        IStoreContext storeContext,
        IStoreService storeService,
        ITaxService taxService,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService,
        TaxSettings taxSettings)
    {
        _customerSettings = customerSettings;
        _dateTimeSettings = dateTimeSettings;
        _emailAccountSettings = emailAccountSettings;
        _forumSettings = forumSettings;
        _gdprSettings = gdprSettings;
        _addressService = addressService;
        _accountManagerModelFactory = accountManagerModelFactory;
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
        _accountManagerService = accountManagerService;
        _storeContext = storeContext;
        _storeService = storeService;
        _taxService = taxService;
        _workContext = workContext;
        _workflowMessageService = workflowMessageService;
        _taxSettings = taxSettings;
    }

    #endregion

    #region ERP helpers

    private static RestClient CreateErpRestClient()
    {
        var cfg = new NetSuiteApiConfig();
        var client = new RestClient();
        var oAuth1 = OAuth1Authenticator.ForAccessToken(
            consumerKey: cfg.ClientId,
            consumerSecret: cfg.ClientSecret,
            token: cfg.TokenId,
            tokenSecret: cfg.TokenSecret,
            OAuthSignatureMethod.HmacSha256);
        oAuth1.Realm = cfg.AccountId;
        client.Authenticator = oAuth1;
        return client;
    }

    private class ErpAccountManagerListResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public int totalCount { get; set; }
        public List<ErpAccountManagerItem> data { get; set; }
    }

    private class ErpAccountManagerItem
    {
        public string AccountManagerERPID { get; set; }
        public string AccountManagerName { get; set; }
        public string ManagerStartDate { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    private class ErpCustomersByAccountManagerResponse
    {
        public bool success { get; set; }
        public string AccountManagerERPID { get; set; }
        public int totalCount { get; set; }
        public List<string> ERPCustomerIds { get; set; }
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
            if (!await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
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

    protected virtual async Task<bool> SecondAdminAccountExistsAsync(Customer customer)
    {
        var customers = await _customerService.GetAllCustomersAsync(customerRoleIds: [(await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName)).Id]);
        return customers.Any(c => c.Active && c.Id != customer.Id);
    }

    #endregion

    #region Customers

    public virtual IActionResult Index()
    {
        return RedirectToAction("List");
    }

    public virtual async Task<IActionResult> List()
    {
        if (!await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
            return AccessDeniedView();

        //prepare model
        var model = await _accountManagerModelFactory.PrepareAccountManagerSearchModelAsync(new AccountManagerSearchModel());
        return View("~/Plugins/Nop.Plugin.AccountManager/Views/List.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AccountManagerList(AccountManagerSearchModel searchModel)
    {
        //if (!await _permissionService.AuthorizeAsync("ManageCustomers"))
        //    return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _accountManagerModelFactory.PrepareAccountManagerListModelAsync(searchModel);
        return Json(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
            return AccessDeniedView();

        //prepare model
        var model = await _accountManagerModelFactory.PrepareAccountManagerModelAsync(new AccountManagerModel(), null);
        return View("~/Plugins/Nop.Plugin.AccountManager/Views/Create.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public virtual async Task<IActionResult> Create(AccountManagerModel model, bool continueEditing, IFormCollection form)
    {
        var CUSTOMER = await _customerService.GetCustomerByEmailAsync(model.Email);
        if (!await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
            return AccessDeniedView();

        if (!string.IsNullOrWhiteSpace(model.Email) && await _accountManagerService.GetAccountManagerByEmailAsync(model.Email) != null)
            ModelState.AddModelError(string.Empty, "Email is already registered");

        if (!string.IsNullOrWhiteSpace(model.Email) && CUSTOMER != null && await _accountManagerService.GetAccountManagerByEmailAsync(model.Email) == null)
        {
            model.Customer_Id = CUSTOMER.Id;
            ModelState.AddModelError(string.Empty, "customer email is already registered with UserNmae: " + CUSTOMER.Username.ToString());
        }
        else
        {
            if (ModelState.IsValid)
            {
                //fill entity from model
                Customer customer = new Customer();

                var currentStore = await _storeContext.GetCurrentStoreAsync();
                customer.CustomerGuid = Guid.NewGuid();
                customer.CreatedOnUtc = DateTime.UtcNow;
                customer.LastActivityDateUtc = DateTime.UtcNow;
                customer.RegisteredInStoreId = currentStore.Id;
                string[] customerName = model.AccountManagerName.Split(" ");
                customer.FirstName = customerName[0];
                if (customerName.Length > 1)
                { customer.LastName = customerName[1]; }
                customer.Phone = model.Phone;
                customer.Email = model.Email;
                customer.Active = true;
                await _customerService.InsertCustomerAsync(customer);

                //password
                var changePassRequest = new ChangePasswordRequest(model.Email, false, _customerSettings.DefaultPasswordFormat, "12345678");
                var changePassResult = await _customerRegistrationService.ChangePasswordAsync(changePassRequest);
                if (!changePassResult.Success)
                {
                    foreach (var changePassError in changePassResult.Errors)
                        _notificationService.ErrorNotification(changePassError);
                }

                await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = 3 });
                CUSTOMER = await _customerService.GetCustomerByEmailAsync(model.Email);
                model.Customer_Id = CUSTOMER.Id;
            }
        }

        var allAccountManagerRigions = _accountManagerService.GetAllRigionsAsync(true);
        var newAccountManagerRigions = new List<Nop.Plugin.AccountManager.Domain.Rigion>();
        foreach (var AccountManagerRigion in allAccountManagerRigions)
            if (model.SelectedRigionIds.Contains(AccountManagerRigion.Id))
                newAccountManagerRigions.Add(AccountManagerRigion);

        if (ModelState.IsValid)
        {
            //fill entity from model
            var account_Manager = model.ToEntity<Account_Manager>();
            var currentStore = await _storeContext.GetCurrentStoreAsync();
            account_Manager.AccountManagerName = model.AccountManagerName;
            account_Manager.ManagerStartDate = DateTime.UtcNow;
            account_Manager.Phone = model.Phone;
            account_Manager.Email = model.Email;
            account_Manager.Customer_Id = model.Customer_Id;
            account_Manager.Active = model.Active;
            account_Manager.ERPAccountManagerId = model.ERPAccountManagerId;
            await _accountManagerService.InsertAccountManagerAsync(account_Manager);

            //rigins
            foreach (var AccountManagerRigion in newAccountManagerRigions)
            {
                await _accountManagerService.AddrigionMappingAsync(new AccountManagerRigionMapping { AccountManagerId = account_Manager.Id, RigionId = AccountManagerRigion.Id });
            }
            await _accountManagerService.UpdateAccountManagerAsync(account_Manager);
            await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = CUSTOMER.Id, CustomerRoleId = 6 });
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.AccountManager.Added"));

            if (!continueEditing)
                return RedirectToAction("List");
            return RedirectToAction("Edit", new { id = account_Manager.Id });
        }

        //prepare model
        model = await _accountManagerModelFactory.PrepareAccountManagerModelAsync(model, null, true);
        //if we got this far, something failed, redisplay form
        return View("~/Plugins/Nop.Plugin.AccountManager/Views/Create.cshtml", model);
    }

    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
            return AccessDeniedView();

        //try to get a customer with the specified id
        var AccountManager = await _accountManagerService.GetAccountManagerByIdAsync(id);
        if (AccountManager == null || AccountManager.Deleted)
            return RedirectToAction("List");

        //prepare model
        var model = await _accountManagerModelFactory.PrepareAccountManagerModelAsync(null, AccountManager, true);
        return View("~/Plugins/Nop.Plugin.AccountManager/Views/Edit.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public virtual async Task<IActionResult> Edit(AccountManagerModel model, bool continueEditing, IFormCollection form)
    {
        if (!await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
            return AccessDeniedView();

        //try to get a customer with the specified id
        var accountManager = await _accountManagerService.GetAccountManagerByIdAsync(model.Id);
        if (accountManager == null || accountManager.Deleted)
            return RedirectToAction("List");

        var allAccountManagerRigions = _accountManagerService.GetAllRigionsAsync(true);
        var newAccountManagerRigions = new List<Nop.Plugin.AccountManager.Domain.Rigion>();
        foreach (var AccountManagerRigion in allAccountManagerRigions)
            if (model.SelectedRigionIds.Contains(AccountManagerRigion.Id))
                newAccountManagerRigions.Add(AccountManagerRigion);

        if (ModelState.IsValid)
        {
            accountManager.Email = model.Email;
            accountManager.Active = model.Active;
            accountManager.AccountManagerName = model.AccountManagerName;
            accountManager.Phone = model.Phone;
            accountManager.ERPAccountManagerId = model.ERPAccountManagerId;

            var currentAccountManagerRigionIds = await _accountManagerService.GetRigionIdsAsync(accountManager);

            //rigins
            foreach (var AccountManagerRigion in newAccountManagerRigions)
            {
                if (model.SelectedRigionIds.Contains(AccountManagerRigion.Id))
                {
                    //new role
                    if (currentAccountManagerRigionIds.All(rigion => rigion != AccountManagerRigion.Id))
                        await _accountManagerService.AddrigionMappingAsync(new AccountManagerRigionMapping { AccountManagerId = accountManager.Id, RigionId = AccountManagerRigion.Id });
                }
                else
                {
                    //remove role
                    if (currentAccountManagerRigionIds.Any(rigion => rigion == AccountManagerRigion.Id))
                        await _accountManagerService.RemoveRigionMappingAsync(accountManager, AccountManagerRigion);
                }
            }

            foreach (var i in currentAccountManagerRigionIds)
            {
                var AccountManagerRigion = await _accountManagerService.GetRigionByIdAsync(i);
                if (model.SelectedRigionIds.Contains(AccountManagerRigion.Id))
                {
                    //new role
                    if (currentAccountManagerRigionIds.All(rigion => rigion != AccountManagerRigion.Id))
                        await _accountManagerService.AddrigionMappingAsync(new AccountManagerRigionMapping { AccountManagerId = accountManager.Id, RigionId = AccountManagerRigion.Id });
                }
                else
                {
                    //remove role
                    if (currentAccountManagerRigionIds.Any(rigion => rigion == AccountManagerRigion.Id))
                        await _accountManagerService.RemoveRigionMappingAsync(accountManager, AccountManagerRigion);
                }
            }

            await _accountManagerService.UpdateAccountManagerAsync(accountManager);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.AccountManager.Updated"));

            if (!continueEditing)
                return RedirectToAction("List");
            return RedirectToAction("Edit", new { id = accountManager.Id });
        }

        //prepare model
        model = await _accountManagerModelFactory.PrepareAccountManagerModelAsync(model, accountManager, true);
        //if we got this far, something failed, redisplay form
        return View("~/Plugins/Nop.Plugin.AccountManager/Views/Edit.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
            return AccessDeniedView();

        //try to get a customer with the specified id
        var accountManager = await _accountManagerService.GetAccountManagerByIdAsync(id);
        if (accountManager == null)
            return RedirectToAction("List");

        try
        {
            var CUSTOMER = await _customerService.GetCustomerByIdAsync(accountManager.Customer_Id);
            var customerRole = await _customerService.GetCustomerRoleByIdAsync(6);

            //delete
            await _accountManagerService.DeleteAccountManagerAsync(accountManager);
            await _customerService.RemoveCustomerRoleMappingAsync(CUSTOMER, customerRole);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.AccountManager.Deleted"));
            return RedirectToAction("List");
        }
        catch (Exception exc)
        {
            _notificationService.ErrorNotification(exc.Message);
            return RedirectToAction("Edit", new { id = accountManager.Id });
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> SyncERPAccountManagers()
    {
        if (!await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
            return AccessDeniedView();

        int created = 0, updated = 0, errors = 0;
        var errorMessages = new List<string>();

        try
        {
            // 1. Fetch all account managers from ERP
            var req = new RestRequest($"{_erpConfig.ApiRoot}&objType=86&objId=0", Method.GET);
            var resp = await _erpClient.ExecuteAsync(req);
            if (!resp.IsSuccessful)
                return Json(new { success = false, message = "ERP API call failed: " + resp.Content });

            var erpResponse = JsonConvert.DeserializeObject<ErpAccountManagerListResponse>(resp.Content);
            if (erpResponse?.success != true || erpResponse.data == null)
                return Json(new { success = false, message = "ERP returned no data: " + resp.Content });

            var accountManagerRole = await _customerService.GetCustomerRoleByIdAsync(6);
            var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
            var currentStore = await _storeContext.GetCurrentStoreAsync();

            foreach (var erpAm in erpResponse.data)
            {
                try
                {
                    if (!int.TryParse(erpAm.AccountManagerERPID, out int erpId) || erpId <= 0)
                        continue;

                    // 2. Find or create nopCommerce Customer
                    Customer customer = null;
                    if (!string.IsNullOrEmpty(erpAm.Email))
                        customer = await _customerService.GetCustomerByEmailAsync(erpAm.Email);

                    if (customer == null)
                    {
                        // Create new customer
                        customer = new Customer
                        {
                            CustomerGuid = Guid.NewGuid(),
                            CreatedOnUtc = DateTime.UtcNow,
                            LastActivityDateUtc = DateTime.UtcNow,
                            RegisteredInStoreId = currentStore.Id,
                            Email = erpAm.Email ?? $"am_{erpId}@erp.local",
                            Active = true
                        };
                        var nameParts = (erpAm.AccountManagerName ?? "").Split(' ', 2);
                        customer.FirstName = nameParts[0];
                        customer.LastName = nameParts.Length > 1 ? nameParts[1] : "";
                        customer.Phone = erpAm.Phone;

                        await _customerService.InsertCustomerAsync(customer);

                        // Set password
                        var pwdReq = new ChangePasswordRequest(customer.Email, false, _customerSettings.DefaultPasswordFormat, "12345678");
                        await _customerRegistrationService.ChangePasswordAsync(pwdReq);

                        // Assign Registered + AccountManager roles
                        if (registeredRole != null)
                            await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = registeredRole.Id });
                        if (accountManagerRole != null)
                            await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = accountManagerRole.Id });

                        created++;
                    }
                    else
                    {
                        // Update phone if missing
                        if (!string.IsNullOrEmpty(erpAm.Phone) && string.IsNullOrEmpty(customer.Phone))
                        {
                            customer.Phone = erpAm.Phone;
                            await _customerService.UpdateCustomerAsync(customer);
                        }

                        // Ensure AccountManager role
                        var roleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
                        if (accountManagerRole != null && !roleIds.Contains(accountManagerRole.Id))
                            await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = accountManagerRole.Id });

                        updated++;
                    }

                    // 3. Find or create Account_Manager plugin record — capture its Id
                    var existingAm = await _accountManagerService.GetAccountManagerByEmailAsync(customer.Email);
                    int accountManagerPluginId;
                    if (existingAm == null)
                    {
                        var newAm = new Account_Manager
                        {
                            AccountManagerName = erpAm.AccountManagerName ?? "",
                            Email = customer.Email,
                            Phone = erpAm.Phone ?? "",
                            Customer_Id = customer.Id,
                            Active = true,
                            Deleted = false,
                            ManagerStartDate = DateTime.UtcNow,
                            ERPAccountManagerId = erpId
                        };
                        await _accountManagerService.InsertAccountManagerAsync(newAm);
                        accountManagerPluginId = newAm.Id;
                    }
                    else
                    {
                        existingAm.ERPAccountManagerId = erpId;
                        if (existingAm.Customer_Id == 0)
                            existingAm.Customer_Id = customer.Id;
                        await _accountManagerService.UpdateAccountManagerAsync(existingAm);
                        accountManagerPluginId = existingAm.Id;
                    }

                  
                    try
                    {
                        var custReq = new RestRequest(_erpConfig.ApiRoot + "&objType=87&objId=" + erpId, Method.GET);
                        var custResp = await _erpClient.ExecuteAsync(custReq);
                        if (custResp.IsSuccessful && !string.IsNullOrEmpty(custResp.Content))
                        {
                            var custData = JsonConvert.DeserializeObject<ErpCustomersByAccountManagerResponse>(custResp.Content);
                            if (custData?.success == true && custData.ERPCustomerIds != null)
                            {
                                foreach (var erpCustIdStr in custData.ERPCustomerIds)
                                {
                                    if (!int.TryParse(erpCustIdStr, out int erpCustId) || erpCustId <= 0)
                                        continue;

                                    await _accountManagerService.UpsertAccountManagerCustomerMappingByERPCustomerIdAsync(
                                        erpCustId, accountManagerPluginId);
                                }
                            }
                        }
                    }
                    catch { }
                }
                catch (Exception ex)
                {
                    errors++;
                    errorMessages.Add($"ERP ID {erpAm.AccountManagerERPID} ({erpAm.AccountManagerName}): {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }

        var summary = $"Sync complete. Created: {created}, Updated: {updated}, Errors: {errors}.";
        if (errorMessages.Any())
            summary += " " + string.Join("; ", errorMessages);

        _notificationService.SuccessNotification(summary);
        return Json(new { success = true, message = summary, created, updated, errors });
    }

    #endregion
}