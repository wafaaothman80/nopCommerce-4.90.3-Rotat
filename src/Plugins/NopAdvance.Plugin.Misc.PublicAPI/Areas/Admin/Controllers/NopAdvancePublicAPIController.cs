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
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopAdvance.Plugin.Core.Areas.Admin.Controllers;
using NopAdvance.Plugin.Core.Filters;
using NopAdvance.Plugin.Misc.PublicAPI.Areas.Admin.Factories;
using NopAdvance.Plugin.Misc.PublicAPI.Domain;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Admin;
using NopAdvance.Plugin.Misc.PublicAPI.Services;
using NopAdvance.Plugin.Misc.PublicAPI.Services.Debugging;

namespace NopAdvance.Plugin.Misc.PublicAPI.Areas.Admin.Controllers;

[NopAdvanceCheckLicense(PluginDefaults.SYSTEM_NAME, nameof(NopAdvanceAPISettings))]
public class NopAdvancePublicAPIController : NopAdvanceBaseAdminController
{
    #region Fields

    private readonly IPermissionService _permissionService;
    private readonly IAPIModelFactory _apiModelFactory;
    private readonly IAPIService _apiService;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly INotificationService _notificationService;
    private readonly IStoreContext _storeContext;
    private readonly IAPIDebugLogModelFactory _apiDebugLogModelFactory;
    private readonly IAPIDebugService _apiDebugService;

    #endregion

    #region Ctor

    public NopAdvancePublicAPIController(IPermissionService permissionService,
        IAPIModelFactory apiModelFactory,
        IAPIService apiService,
        ILocalizationService localizationService,
        ISettingService settingService,
        INotificationService notificationService,
        IStoreContext storeContext,
        IAPIDebugLogModelFactory apiDebugLogModelFactory,
        IAPIDebugService apiDebugService)
    {
        _permissionService = permissionService;
        _apiModelFactory = apiModelFactory;
        _apiService = apiService;
        _localizationService = localizationService;
        _settingService = settingService;
        _notificationService = notificationService;
        _storeContext = storeContext;
        _apiDebugLogModelFactory = apiDebugLogModelFactory;
        _apiDebugService = apiDebugService;
    }

    #endregion

    #region Methods

    #region Token

    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    [CheckPermission(new[] { PluginDefaults.SYSTEM_NAME })]
    public async Task<IActionResult> Tokens()
    {
        //prepare model
        var model = await _apiModelFactory.PrepareRefreshTokenSearchModelAsync(new RefreshTokenSearchModel());

        return View(model);
    }

    [HttpPost]
    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    [CheckPermission(new[] { PluginDefaults.SYSTEM_NAME })]
    public async Task<IActionResult> TokenList(RefreshTokenSearchModel searchModel)
    {
        //prepare model
        var model = await _apiModelFactory.PrepareRefreshTokenListModelAsync(searchModel);

        return Json(model);
    }

    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    [CheckPermission(new[] { PluginDefaults.SYSTEM_NAME })]
    public async Task<IActionResult> RevokeToken(int id)
    {
        var refreshToken = await _apiService.GetAPIRefreshTokenByIdAsync(id);
        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            await _apiService.UpdateAPIRefreshTokenAsync(refreshToken);
        }

        return Json(new { Result = true });
    }

    [HttpPost]
    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    [CheckPermission(new[] { PluginDefaults.SYSTEM_NAME })]
    public async Task<IActionResult> RevokeSelected(ICollection<int> selectedIds)
    {
        if (selectedIds != null)
            foreach (var selectedId in selectedIds)
            {
                var refreshToken = await _apiService.GetAPIRefreshTokenByIdAsync(selectedId);
                if (refreshToken != null)
                {
                    refreshToken.IsRevoked = true;
                    await _apiService.UpdateAPIRefreshTokenAsync(refreshToken);
                }
            }
        return Json(new { Result = true });
    }

    #endregion

    #region Application

    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    [CheckPermission(new[] { PluginDefaults.SYSTEM_NAME })]
    public async Task<IActionResult> Applications()
    {
        //prepare model
        var model = await _apiModelFactory.PrepareAPIApplicationSearchModelAsync(new APIApplicationSearchModel());

        return View(model);
    }

    [HttpPost]
    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    [CheckPermission(new[] { PluginDefaults.SYSTEM_NAME })]
    public async Task<IActionResult> ApplicationList(APIApplicationSearchModel searchModel)
    {
        //prepare model
        var model = await _apiModelFactory.PrepareAPIApplicationListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    [CheckPermission(new[] { PluginDefaults.SYSTEM_NAME })]
    public async Task<IActionResult> ApplicationUpdate(APIApplicationModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError("Name", "Application Name cannot be empty");
            return ErrorJson(ModelState.SerializeErrors());
        }

        if (model.Name != null)
            model.Name = model.Name.Trim();

        if (!ModelState.IsValid)
            return ErrorJson(ModelState.SerializeErrors());

        if (await _apiService.ApplicationNameIsExistOrNot(model.Name))
        {
            ModelState.AddModelError("Name", "Application Name Already Exits");
            return ErrorJson(ModelState.SerializeErrors());
        }

        //try to get an application with the specified id
        var application = await _apiService.GetAPIApplicationByIdAsync(model.Id)
            ?? throw new ArgumentException("No application found with the specified id");

        application.Name = model.Name;
        application.Active = model.Active;

        await _apiService.UpdateAPIApplicationAsync(application);

        return new NullJsonResult();
    }

    [HttpPost]
    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    [CheckPermission(new[] { PluginDefaults.SYSTEM_NAME })]
    public async Task<IActionResult> ApplicationAdd(APIApplicationModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError("Name", "Application Name cannot be empty");
            return ErrorJson(ModelState.SerializeErrors());
        }

        if (model.Name != null)
            model.Name = model.Name.Trim();

        if (!ModelState.IsValid)
            return ErrorJson(ModelState.SerializeErrors());

        if (await _apiService.ApplicationNameIsExistOrNot(model.Name))
        {
            ModelState.AddModelError("Name", "Application Name Already Exits");
            return ErrorJson(ModelState.SerializeErrors());
        }

        var application = new APIApplication
        {
            Name = model.Name,
            StoreId = model.StoreId,
            APIKey = PluginCommonHelper.GenerateSecretKey(),
            Active = model.Active
        };

        await _apiService.InsertAPIApplicationAsync(application);

        return Json(new { Result = true });
    }

    [HttpPost]
    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    [CheckPermission(new[] { PluginDefaults.SYSTEM_NAME })]
    public async Task<IActionResult> ApplicationDelete(int id)
    {
        //try to get an application with the specified id
        var application = await _apiService.GetAPIApplicationByIdAsync(id)
            ?? throw new ArgumentException("No application found with the specified id", nameof(id));

        await _apiService.DeleteAPIApplicationAsync(application);

        return new NullJsonResult();
    }

    #endregion

    #region Debug log

    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    [CheckPermission(new[] { PluginDefaults.SYSTEM_NAME })]
    public async Task<IActionResult> DebugLogs()
    {
        //prepare model
        var model = await _apiDebugLogModelFactory.PrepareDebugLogSearchModel(new APIDebugLogSearchModel());

        return View(model);
    }

    [HttpPost]
    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    [CheckPermission(new[] { PluginDefaults.SYSTEM_NAME })]
    public async Task<IActionResult> DebugLogList(APIDebugLogSearchModel searchModel)
    {
        //prepare model
        var model = await _apiDebugLogModelFactory.PrepareDebugLogListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost, ActionName("DebugLogList")]
    [FormValueRequired("clearall")]
    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    [CheckPermission(new[] { PluginDefaults.SYSTEM_NAME })]
    public async Task<IActionResult> ClearAll()
    {
        await _apiDebugService.ClearDebugLogAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync(LocaleResourceDefaults.DEBUG_LOG_CLEARED));

        return RedirectToAction("DebugLogs");
    }

    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    [CheckPermission(new[] { PluginDefaults.SYSTEM_NAME })]
    public async Task<IActionResult> DebugLogView(int id)
    {
        //try to get a debug log with the specified id
        var debugLog = await _apiDebugService.GetAPIDebugLogByDebugIdAsync(id);
        if (debugLog == null)
            return RedirectToAction("DebugLogs");

        //prepare model
        var model = await _apiDebugLogModelFactory.PrepareDebugLogModelAsync(null, debugLog);

        return View(model);
    }

    [HttpPost]
    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    [CheckPermission(new[] { PluginDefaults.SYSTEM_NAME })]
    public async Task<IActionResult> Delete(int id)
    {
        //try to get a debug log with the specified id
        var debugLog = await _apiDebugService.GetAPIDebugLogByDebugIdAsync(id);
        if (debugLog == null)
            return RedirectToAction("DebugLogs");

        await _apiDebugService.DeleteAPIDebugLogAsync(debugLog);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync(LocaleResourceDefaults.DEBUG_LOG_DELETED));

        return RedirectToAction("DebugLogs");
    }

    [HttpPost]
    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    [CheckPermission(new[] { PluginDefaults.SYSTEM_NAME })]
    public async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
    {
        if (selectedIds != null)
            await _apiDebugService.DeleteAPIDebugLogsAsync((await _apiDebugService.GetAPIDebugLogByIdsAsync(selectedIds.ToArray())).ToList());

        return Json(new { Result = true });
    }

    #endregion

    #endregion
}
