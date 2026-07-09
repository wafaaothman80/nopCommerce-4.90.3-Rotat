using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
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
using Nop.Services.Directory;
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

namespace Nop.Plugin.AccountManager.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public partial class RigionController : BasePluginController
{
    #region Fields

    protected readonly CustomerSettings _customerSettings;
    protected readonly DateTimeSettings _dateTimeSettings;
    protected readonly EmailAccountSettings _emailAccountSettings;
    protected readonly ForumSettings _forumSettings;
    protected readonly GdprSettings _gdprSettings;
    protected readonly IAddressService _addressService;
    protected readonly IRigionService _rigionService;
    protected readonly IRigionModelFactory _rigionModelFactory;
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
    protected readonly ICountryService _countryService;

    #endregion

    #region Ctor

    public RigionController(CustomerSettings customerSettings,
        DateTimeSettings dateTimeSettings,
        EmailAccountSettings emailAccountSettings,
        ForumSettings forumSettings,
        GdprSettings gdprSettings,
        IAddressService addressService,
        IRigionService rigionService,
        IRigionModelFactory rigionModelFactory,
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
        TaxSettings taxSettings,
        ICountryService countryService)
    {
        _customerSettings = customerSettings;
        _dateTimeSettings = dateTimeSettings;
        _emailAccountSettings = emailAccountSettings;
        _forumSettings = forumSettings;
        _gdprSettings = gdprSettings;
        _addressService = addressService;
        _rigionModelFactory = rigionModelFactory;
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
        _rigionService = rigionService;
        _storeContext = storeContext;
        _storeService = storeService;
        _taxService = taxService;
        _workContext = workContext;
        _workflowMessageService = workflowMessageService;
        _taxSettings = taxSettings;
        _countryService = countryService;
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

    #region Rigion

    public virtual IActionResult Index()
    {
        return RedirectToAction("List");
    }

    public virtual async Task<IActionResult> List()
    {
        if (!await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
            return AccessDeniedView();

        //prepare model
        var model = await _rigionModelFactory.PrepareRigionSearchModelAsync(new RigionSearchModel());
        return View("~/Plugins/Nop.Plugin.AccountManager/Views/ListRigion.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> RigionList(RigionSearchModel searchModel)
    {
        //if (!await _permissionService.AuthorizeAsync("ManageCustomers"))
        //    return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _rigionModelFactory.PrepareRigionListModelAsync(searchModel);
        return Json(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
            return AccessDeniedView();

        //prepare model
        var model = await _rigionModelFactory.PrepareRigionModelAsync(new RigionModel(), null);
        return View("~/Plugins/Nop.Plugin.AccountManager/Views/CreateRigion.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public virtual async Task<IActionResult> Create(RigionModel model, bool continueEditing, IFormCollection form)
    {
        if (!await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
            return AccessDeniedView();

        if (!string.IsNullOrWhiteSpace(model.RigionName) && await _rigionService.GetRigionByNameAsync(model.RigionName) != null)
            ModelState.AddModelError(string.Empty, "RigionName is already registered");

        var allRigionCountries = _rigionService.GetAllCountriessAsync(true);
        var newRigionCountries = new List<Country>();
        foreach (var RigionCountriy in allRigionCountries)
            if (model.SelectedCountryIds.Contains(RigionCountriy.Id))
                newRigionCountries.Add(RigionCountriy);

        if (ModelState.IsValid)
        {
            //fill entity from model
            var rigion = model.ToEntity<Nop.Plugin.AccountManager.Domain.Rigion>();
            var currentStore = await _storeContext.GetCurrentStoreAsync();
            rigion.RigionName = model.RigionName;
            rigion.RigionAddedDate = DateTime.UtcNow;
            rigion.Active = model.Active;
            await _rigionService.InsertRigionAsync(rigion);

            //countries
            foreach (var RigionCountry in newRigionCountries)
            {
                await _rigionService.AddrigionMappingAsync(new CountryRigionMapping { CountryId = RigionCountry.Id, RigionId = rigion.Id });
            }
            await _rigionService.UpdateRigionAsync(rigion);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.AccountManager.Added"));
            if (!continueEditing)
                return RedirectToAction("List");
            return RedirectToAction("Edit", new { id = rigion.Id });
        }

        //prepare model
        model = await _rigionModelFactory.PrepareRigionModelAsync(model, null, true);
        //if we got this far, something failed, redisplay form
        return View("~/Plugins/Nop.Plugin.AccountManager/Views/CreateRigion.cshtml", model);
    }

    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
            return AccessDeniedView();

        //try to get a rigion with the specified id
        var rigion = await _rigionService.GetRigionByIdAsync(id);
        if (rigion == null)
            return RedirectToAction("List");

        //prepare model
        var model = await _rigionModelFactory.PrepareRigionModelAsync(null, rigion, true);
        return View("~/Plugins/Nop.Plugin.AccountManager/Views/EditRigion.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public virtual async Task<IActionResult> Edit(RigionModel model, bool continueEditing, IFormCollection form)
    {
        if (!await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
            return AccessDeniedView();

        //try to get a rigion with the specified id
        var rigion = await _rigionService.GetRigionByIdAsync(model.Id);
        if (rigion == null)
            return RedirectToAction("List");

        var allRigionCountries = _rigionService.GetAllCountriessAsync(true);
        var newRigionCountries = new List<Country>();
        foreach (var RigionCountriy in allRigionCountries)
            if (model.SelectedCountryIds.Contains(RigionCountriy.Id))
                newRigionCountries.Add(RigionCountriy);

        if (ModelState.IsValid)
        {
            rigion.RigionName = model.RigionName;
            rigion.Active = model.Active;
            rigion.DisplayOrder = model.DisplayOrder;

            var currentigionCountryIds = await _rigionService.GetCountryIdsAsync(rigion);

            //countries
            foreach (var RigionCountry in newRigionCountries)
            {
                if (model.SelectedCountryIds.Contains(RigionCountry.Id))
                {
                    //new mapping
                    if (currentigionCountryIds.All(country => country != RigionCountry.Id))
                        await _rigionService.AddrigionMappingAsync(new CountryRigionMapping { CountryId = RigionCountry.Id, RigionId = rigion.Id });
                }
                else
                {
                    //remove mapping
                    if (currentigionCountryIds.Any(country => country == RigionCountry.Id))
                        await _rigionService.RemoveCountryRigionMappingAsync(rigion, RigionCountry);
                }
            }

            var currentigionCountry = await _countryService.GetCountriesByIdsAsync(currentigionCountryIds.ToArray());
            foreach (var RigionCountry in currentigionCountry)
            {
                if (model.SelectedCountryIds.Contains(RigionCountry.Id))
                {
                    //new mapping
                    if (currentigionCountryIds.All(country => country != RigionCountry.Id))
                        await _rigionService.AddrigionMappingAsync(new CountryRigionMapping { CountryId = RigionCountry.Id, RigionId = rigion.Id });
                }
                else
                {
                    //remove mapping
                    if (currentigionCountryIds.Any(country => country == RigionCountry.Id))
                        await _rigionService.RemoveCountryRigionMappingAsync(rigion, RigionCountry);
                }
            }

            await _rigionService.UpdateRigionAsync(rigion);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.rigion.Updated"));

            if (!continueEditing)
                return RedirectToAction("List");
            return RedirectToAction("Edit", new { id = rigion.Id });
        }

        //prepare model
        model = await _rigionModelFactory.PrepareRigionModelAsync(model, rigion, true);
        //if we got this far, something failed, redisplay form
        return View("~/Plugins/Nop.Plugin.AccountManager/Views/EditRigion.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
            return AccessDeniedView();

        //try to get a rigion with the specified id
        var rigion = await _rigionService.GetRigionByIdAsync(id);
        if (rigion == null)
            return RedirectToAction("List");

        try
        {
            //delete
            await _rigionService.DeleteRigionAsync(rigion);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.rigion.Deleted"));
            return RedirectToAction("List");
        }
        catch (Exception exc)
        {
            _notificationService.ErrorNotification(exc.Message);
            return RedirectToAction("Edit", new { id = rigion.Id });
        }
    }

    #endregion
}