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
using Nop.Core.Domain.Common;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopAdvance.Plugin.Core.Areas.Admin.Controllers;
using NopAdvance.Plugin.Core.Filters;
using NopAdvance.Plugin.Core.Helpers;
using NopAdvance.Plugin.Misc.PublicAPI.Domain;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Admin;
using NopAdvance.Plugin.Misc.PublicAPI.Services;

namespace NopAdvance.Plugin.Misc.PublicAPI.Areas.Admin.Controllers;

[NopAdvanceCheckLicense(PluginDefaults.SYSTEM_NAME, nameof(NopAdvanceAPISettings))]
public class NopAdvancePublicAPIConfigureController : NopAdvanceBaseAdminController
{
    #region Fields

    private readonly IPermissionService _permissionService;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly INotificationService _notificationService;
    private readonly IStoreContext _storeContext;
    private readonly ILicenseHelper _licenseHelper;

    #endregion

    #region Ctor

    public NopAdvancePublicAPIConfigureController(IPermissionService permissionService,
        ILocalizationService localizationService,
        ISettingService settingService,
        INotificationService notificationService,
        IStoreContext storeContext,
        ILicenseHelper licenseHelper)
    {
        _permissionService = permissionService;
        _localizationService = localizationService;
        _settingService = settingService;
        _notificationService = notificationService;
        _storeContext = storeContext;
        _licenseHelper = licenseHelper;
    }

    #endregion

    #region Methods

    public IActionResult Index()
    {
        return RedirectToAction("Configure");
    }

    #region Configure

    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    [CheckPermission(new[] { PluginDefaults.SYSTEM_NAME })]
    public async Task<IActionResult> Configure()
    {
        //load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var pluginSettings = await _settingService.LoadSettingAsync<NopAdvanceAPISettings>(storeScope);
        var (licenseStatus, _) = await _licenseHelper.ValidateLicenseAsync(PluginDefaults.SYSTEM_NAME);

        var model = new ConfigurationModel
        {
            Enabled = pluginSettings.Enabled,
            EnableSwagger = pluginSettings.IsSwaggerEnabled,
            IsDevelopment = pluginSettings.IsDevelopment,
            SecretKey = pluginSettings.SecretKey,
            SecurityAlgorithmTypeId = Convert.ToInt32(pluginSettings.SecurityAlgorithmType),
            AvailableSecurityAlgorithmTypes = await pluginSettings.SecurityAlgorithmType.ToSelectListAsync(),
            AccessTokenExpiration = pluginSettings.AccessTokenExpiration,
            AccessTokenExpirationDurationId = Convert.ToInt32(pluginSettings.AccessTokenExpirationDuration),
            AvailableAccessTokenExpirationDurations = await pluginSettings.AccessTokenExpirationDuration.ToSelectListAsync(),
            RefreshTokenExpiration = pluginSettings.RefreshTokenExpiration,
            RefreshTokenExpirationDurationId = Convert.ToInt32(pluginSettings.RefreshTokenExpirationDuration),
            AvailableRefreshTokenExpirationDurations = await pluginSettings.RefreshTokenExpirationDuration.ToSelectListAsync(),
            EnableDebugging = pluginSettings.IsDebuggingEnabled,
            LicenseStatus = licenseStatus,
            ActiveStoreScopeConfiguration = storeScope
        };

        if (storeScope > 0)
        {
            model.Enabled_OverrideForStore = await _settingService.SettingExistsAsync(pluginSettings, x => x.Enabled, storeScope);
            model.EnableSwagger_OverrideForStore = await _settingService.SettingExistsAsync(pluginSettings, x => x.IsSwaggerEnabled, storeScope);
            model.IsDevelopment_OverrideForStore = await _settingService.SettingExistsAsync(pluginSettings, x => x.IsDevelopment, storeScope);
            model.SecretKey_OverrideForStore = await _settingService.SettingExistsAsync(pluginSettings, x => x.SecretKey, storeScope);
            model.SecurityAlgorithmTypeId_OverrideForStore = await _settingService.SettingExistsAsync(pluginSettings, x => x.SecurityAlgorithmType, storeScope);
            model.AccessTokenExpiration_OverrideForStore = await _settingService.SettingExistsAsync(pluginSettings, x => x.AccessTokenExpiration, storeScope);
            model.RefreshTokenExpiration_OverrideForStore = await _settingService.SettingExistsAsync(pluginSettings, x => x.RefreshTokenExpiration, storeScope);
            model.EnableDebugging_OverrideForStore = await _settingService.SettingExistsAsync(pluginSettings, x => x.IsDebuggingEnabled, storeScope);
        }

        return ConfigureView(PluginDefaults.SYSTEM_NAME, model);
    }

    [HttpPost]
    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    [CheckPermission(new[] { PluginDefaults.SYSTEM_NAME })]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        //load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var pluginSettings = await _settingService.LoadSettingAsync<NopAdvanceAPISettings>(storeScope);

        pluginSettings.Enabled = model.Enabled;
        pluginSettings.IsSwaggerEnabled = model.EnableSwagger;
        pluginSettings.IsDevelopment = model.IsDevelopment;
        pluginSettings.SecretKey = model.SecretKey;
        pluginSettings.SecurityAlgorithmType = (AlgorithmType)model.SecurityAlgorithmTypeId;
        pluginSettings.AccessTokenExpiration = model.AccessTokenExpiration;
        pluginSettings.AccessTokenExpirationDuration = (DurationType)model.AccessTokenExpirationDurationId;
        pluginSettings.RefreshTokenExpiration = model.RefreshTokenExpiration;
        pluginSettings.RefreshTokenExpirationDuration = (DurationType)model.RefreshTokenExpirationDurationId;
        pluginSettings.IsDebuggingEnabled = model.EnableDebugging;

        /* We do not clear cache after each setting update.
         * This behavior can increase performance because cached settings will not be cleared 
         * and loaded from database after each update */
        await _settingService.SaveSettingOverridablePerStoreAsync(pluginSettings, x => x.Enabled, model.Enabled_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pluginSettings, x => x.IsSwaggerEnabled, model.EnableSwagger_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pluginSettings, x => x.IsDevelopment, model.IsDevelopment_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pluginSettings, x => x.SecretKey, model.SecretKey_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pluginSettings, x => x.SecurityAlgorithmType, model.SecurityAlgorithmTypeId_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pluginSettings, x => x.AccessTokenExpiration, model.AccessTokenExpiration_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pluginSettings, x => x.AccessTokenExpirationDuration, model.AccessTokenExpiration_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pluginSettings, x => x.RefreshTokenExpiration, model.RefreshTokenExpiration_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pluginSettings, x => x.RefreshTokenExpirationDuration, model.RefreshTokenExpiration_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pluginSettings, x => x.IsDebuggingEnabled, model.EnableDebugging_OverrideForStore, storeScope, false);

        if (pluginSettings.IsDebuggingEnabled)
        {
            var commonSettings = await _settingService.LoadSettingAsync<CommonSettings>(storeScope);
            commonSettings.UseResponseCompression = false;
            await _settingService.SaveSettingOverridablePerStoreAsync(commonSettings, x => x.UseResponseCompression, model.EnableDebugging_OverrideForStore, storeScope, false);
        }

        //now clear settings cache
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    [HttpPost, ActionName("Configure")]
    [FormValueRequired("changesecuritykey")]
    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    [CheckPermission(new[] { PluginDefaults.SYSTEM_NAME })]
    public async Task<IActionResult> ChangeSecurityKey(ConfigurationModel model)
    {
        //load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var pluginSettings = await _settingService.LoadSettingAsync<NopAdvanceAPISettings>(storeScope);

        pluginSettings.SecretKey = PluginCommonHelper.GenerateSecretKey();
        await _settingService.SaveSettingOverridablePerStoreAsync(pluginSettings, x => x.SecretKey, model.SecretKey_OverrideForStore, storeScope, false);

        //now clear settings cache
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync(LocaleResourceDefaults.CONFIGURE_SECURITY_KEY_CHANGED));

        return RedirectToAction("Configure");
    }

    #endregion

    #endregion
}
