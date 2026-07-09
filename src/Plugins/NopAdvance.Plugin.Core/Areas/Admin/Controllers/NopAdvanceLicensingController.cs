// ***	 ** ****** ****** ****** ******* **     ** ****** ***   ** **** ****
// ****  ** **  ** **  ** **  **  **  **  **   **  **  ** ****  ** *    *  
// ** ** ** **  ** ****** ******  **  **   ** **   ****** ** ** ** *    ***
// **  **** **  ** **	  **  **  **  **    ***    **  ** **  **** *    *  
// **   *** ****** **	  **  ** *******     *     **  ** **   *** **** ****
// ***************************************************************************
// *                                                                         *
// *    NopAdvance Core Plugin by NopAdvance team                            *
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
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopAdvance.Core;
using NopAdvance.Plugin.Core.Domain;
using NopAdvance.Plugin.Core.Factories;
using NopAdvance.Plugin.Core.Helpers;
using NopAdvance.Plugin.Core.Infrastructure;
using NopAdvance.Plugin.Core.Models;

namespace NopAdvance.Plugin.Core.Areas.Admin.Controllers;

public class NopAdvanceLicensingController : NopAdvanceBaseAdminController
{
    #region Fields

    private readonly CoreHttpClient _coreHttpClient;
    private readonly ILicenseModelFactory _licenseModelFactory;
    private readonly ISettingService _settingService;
    private readonly ILogger _logger;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public NopAdvanceLicensingController(CoreHttpClient coreHttpClient,
        ILicenseModelFactory licenseModelFactory,
        ISettingService settingService,
        ILogger logger,
        IWorkContext workContext,
        IStoreContext storeContext)
    {
        _coreHttpClient = coreHttpClient;
        _licenseModelFactory = licenseModelFactory;
        _settingService = settingService;
        _logger = logger;
        _workContext = workContext;
        _storeContext = storeContext;
    }

    #endregion

    #region Methods

    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    public async Task<IActionResult> List()
    {
        var model = await _licenseModelFactory.PrepareLicenseSearchModelAsync(new LicenseSearchModel());

        return View(model);
    }

    [HttpPost]
    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    public async Task<IActionResult> List(LicenseSearchModel searchModel)
    {
        //prepare model
        var model = await _licenseModelFactory.PrepareLicenseListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    public async Task<IActionResult> Register(LicenseModel model)
    {
        var (issuccess, licenseInfo) = await _coreHttpClient.RegisterLicenseAsync(model.LicenseKey);
        if (issuccess && !string.IsNullOrEmpty(licenseInfo))
        {
            var infoModel = JsonConvert.DeserializeObject<LicenseInfoModel>(licenseInfo);
            if (infoModel.SystemName == model.SystemName)
            {
                var pluginInfo = Licensing.EncryptLicenseInfo(licenseInfo);

                await _settingService.SetSettingAsync($"{model.SystemName.ToLowerInvariant()}.{CorePluginDefaults.LICENSE_KEY_SETTING}", model.LicenseKey, clearCache: false);
                await _settingService.SetSettingAsync($"{model.SystemName.ToLowerInvariant()}.{CorePluginDefaults.LICENSE_INFO_SETTING}", pluginInfo);

                return new NullJsonResult();
            }
        }
        await _logger.InsertLogAsync(LogLevel.Error, "NopAdvance license registration failed.", $"System name: {model.SystemName}, Error: {licenseInfo}", await _workContext.GetCurrentCustomerAsync());
        return Json(new { error = "License registration failed. Please check log for more details." });
    }

    [HttpPost]
    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    public async Task<IActionResult> DeRegister(string licenseKey)
    {
        var (issuccess, message) = await _coreHttpClient.DeRegisterLicenseAsync(licenseKey);
        if (issuccess && !string.IsNullOrEmpty(message))
        {
            var licenseInfo = JsonConvert.DeserializeObject<LicenseInfoModel>(message);

            var setting = await _settingService.GetSettingAsync($"{licenseInfo.SystemName.ToLowerInvariant()}.{CorePluginDefaults.LICENSE_KEY_SETTING}");
            if (setting != null)
                await _settingService.DeleteSettingAsync(setting);

            await _settingService.SetSettingAsync($"{licenseInfo.SystemName.ToLowerInvariant()}.{CorePluginDefaults.LICENSE_INFO_SETTING}",
                Licensing.EncryptLicenseInfo(JsonConvert.SerializeObject(licenseInfo)));

            return new NullJsonResult();
        }
        await _logger.InsertLogAsync(LogLevel.Error, "NopAdvance license deregistration failed.", message, await _workContext.GetCurrentCustomerAsync());
        return Json(new { error = "License deregistration failed. Please check log for more details." });
    }

    public async Task<IActionResult> LicenseMessage(string systemName, int licenseStatusId)
    {
        var model = new LicenseMessageModel
        {
            SystemName = systemName,
            LicenseStatus = (LicenseStatusType)licenseStatusId,
            StoreUrl = (await _storeContext.GetCurrentStoreAsync()).Url.TrimEnd('/')
        };
        return View($"{NopPluginDefaults.Path}/{CoreDefaults.SYSTEM_NAME}/Areas/{AreaNames.ADMIN}/Views/LicenseMessage.cshtml", model);
    }

    #endregion
}
