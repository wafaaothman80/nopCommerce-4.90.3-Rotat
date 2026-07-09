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
using NopAdvance.Plugin.Core.Domain;
using NopAdvance.Core;
using Newtonsoft.Json;
using NopAdvance.Plugin.Core.Models;
using Nop.Services.Configuration;
using NopAdvance.Plugin.Core.Infrastructure;
using Nop.Core;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Services.Logging;
using Nop.Core.Domain.Logging;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Core.Infrastructure;
using Nop.Services.Security;
using Nop.Core.Configuration;
using NopAdvance.Plugin.Core.Services;

namespace NopAdvance.Plugin.Core.Helpers;

public sealed class LicenseHelper : ILicenseHelper
{
    #region Fields

    private readonly CoreHttpClient _coreHttpClient;
    private readonly ISettingService _settingService;
    private readonly IWebHelper _webHelper;
    private readonly IStoreService _storeService;
    private readonly ILogger _logger;
    private readonly IWorkContext _workContext;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly INopAdvanceCoreService _nopAdvanceCoreService;

    #endregion

    #region Ctor

    public LicenseHelper(CoreHttpClient coreHttpClient,
        ISettingService settingService,
        IWebHelper webHelper,
        IStoreService storeService,
        ILogger logger,
        IWorkContext workContext,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        INopAdvanceCoreService nopAdvanceCoreService)
    {
        _coreHttpClient = coreHttpClient;
        _settingService = settingService;
        _webHelper = webHelper;
        _storeService = storeService;
        _logger = logger;
        _workContext = workContext;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _nopAdvanceCoreService = nopAdvanceCoreService;
    }

    #endregion

    #region Methods

    public async Task<(LicenseStatusType licenseStatus, string message)> ValidateLicenseAsync(string systemName)
    {
        try
        {
            var storeUri = new Uri(_webHelper.GetStoreLocation());
            var storeHost = storeUri.Host.ToLowerInvariant();
            if (storeHost.Contains("localhost") || storeHost.Contains("nopadvance.com"))
                return (LicenseStatusType.Paid, "Full featured development license.");

            var licenseInfo = await _settingService.GetSettingByKeyAsync<string>($"{systemName.ToLowerInvariant()}.{CorePluginDefaults.LICENSE_INFO_SETTING}");
            if (string.IsNullOrEmpty(licenseInfo))
                return (LicenseStatusType.NotFound, "No license found.");

            var info = Licensing.DecryptLicenseInfo(licenseInfo);
            var model = JsonConvert.DeserializeObject<LicenseInfoModel>(info);
            if (model.LicenseType == LicenseType.Trial)
            {
                var nowUtc = DateTime.UtcNow;
                //by wafaa 10-2
                // var expiryDate = model.ActivatedOnUtc.AddDays(10);
                var expiryDate = model.ActivatedOnUtc.AddYears(100);
                if (nowUtc > expiryDate)
                    return (LicenseStatusType.Expired, "License expired");
                else
                {
                    var diff = expiryDate.Subtract(nowUtc);
                    return (LicenseStatusType.Trial, $"{diff.Days} days & {diff.Hours} hours");
                }
            }
            else if (model.LicenseType == LicenseType.Free)
                return (LicenseStatusType.Free, "Full featured free license.");
        }
        catch (Exception ex)
        {
            return (LicenseStatusType.Error, ex.Message);
        }
        return (LicenseStatusType.Paid, "Full featured paid license.");
    }

    /// <summary>
    /// Does default functionalities like AddOrUpdateLocaleResources, SaveSetting, InstallPermission etc.
    /// </summary>
    /// <typeparam name="T">Setting type</typeparam>
    /// <param name="systemName">Plugin system name</param>
    /// <param name="settings">Settings</param>
    /// <param name="permissionProvider">Plugin PermissionProvider</param>
    /// <returns>Error message</returns>
    public async Task<string> InstallPluginAsync<T>(string systemName, T settings) where T : BaseNopAdvanceSettings, new()
    {
        var storeUri = new Uri(_webHelper.GetStoreLocation());
        
        var storeHost = storeUri.Host.ToLowerInvariant();
        if (!storeHost.Contains("localhost") && !storeHost.Contains("nopadvance.com"))
        {
            var response = await _coreHttpClient.InstallPluginAsync(systemName);
            if (!response.Item1)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "NopAdvance plugin installation failed.", $"System name: {systemName}, Error: {response.Item2}", await _workContext.GetCurrentCustomerAsync());
                return response.Item2;
            }

            var model = JsonConvert.DeserializeObject<InstallPluginResponse>(response.Item2);

            var licenseInfoModel = new LicenseInfoModel
            {
                SystemName = systemName,
                LicenseType = LicenseType.Trial,
                ActivatedOnUtc = model.CreatedOnUtc,
            };

            var licenseInfo = JsonConvert.SerializeObject(licenseInfoModel);

            await _settingService.SetSettingAsync($"{systemName.ToLowerInvariant()}.{CorePluginDefaults.LICENSE_INFO_SETTING}",
                Licensing.EncryptLicenseInfo(licenseInfo));
        }

        //Install locale resources
        if (DataSettingsManager.IsDatabaseInstalled())
        {
            //do not use DI, because it produces exception on the installation process
            var localeResourceHelper = EngineContext.Current.Resolve<ILocaleResourceHelper>();
            await localeResourceHelper.AddOrUpdateLocaleResourcesAsync(systemName);
        }

        //Install default settings
        settings.CoreVersion = CoreDefaults.CORE_PLUGIN_VERSION;
        await _settingService.SaveSettingAsync(settings);

        //permission record
        await _permissionService.InsertPermissionsAsync();

        return string.Empty;
    }

    /// <summary>
    /// Does default functionalities like AddOrUpdateLocaleResources, SaveSetting, InstallPermission etc.
    /// </summary>
    /// <param name="systemName">Plugin system name</param>
    /// <param name="settingPrefix">Setting prefix</param>
    /// <param name="permissionProvider">Plugin PermissionProvider</param>
    /// <returns>Error message</returns>
    public async Task<string> UpdatePluginAsync(string systemName, string settingPrefix)
    {
        var storeUri = new Uri(_webHelper.GetStoreLocation());
        var storeHost = storeUri.Host.ToLowerInvariant();
        if (!storeHost.Contains("localhost") && !storeHost.Contains("nopadvance.com"))
        {
            var licenseInfoSettingKey = $"{systemName.ToLowerInvariant()}.{CorePluginDefaults.LICENSE_INFO_SETTING}";
            var licenseKey = await _settingService.GetSettingByKeyAsync<string>(licenseInfoSettingKey);
            if (string.IsNullOrEmpty(licenseKey))
            {
                var response = _coreHttpClient.InstallPluginAsync(systemName).Result;
                if (!response.Item1)
                {
                    await _logger.InsertLogAsync(LogLevel.Error, "NopAdvance plugin updation failed.", $"System name: {systemName}, Error: {response.Item2}", await _workContext.GetCurrentCustomerAsync());
                    return response.Item2;
                }

                var model = JsonConvert.DeserializeObject<InstallPluginResponse>(response.Item2);

                var licenseInfoModel = new LicenseInfoModel
                {
                    SystemName = systemName,
                    LicenseType = LicenseType.Trial,
                    ActivatedOnUtc = model.CreatedOnUtc,
                };

                var licenseInfo = JsonConvert.SerializeObject(licenseInfoModel);

                await _settingService.SetSettingAsync(licenseInfoSettingKey, Licensing.EncryptLicenseInfo(licenseInfo), clearCache: false);
            }
        }

        //Install locale resources
        //do not use DI, because it produces exception on the installation process
        var localeResourceHelper = EngineContext.Current.Resolve<ILocaleResourceHelper>();
        await localeResourceHelper.AddOrUpdateLocaleResourcesAsync(systemName);
        
        //Install default settings
        await _settingService.SetSettingAsync($"{settingPrefix.ToLowerInvariant()}.coreversion", CoreDefaults.CORE_PLUGIN_VERSION, clearCache: true);

        //permission record
        await _permissionService.InsertPermissionsAsync();

        return string.Empty;
    }

    /// <summary>
    /// Does default functionalities like DeleteSetting, DeleteLocaleResources, UninstallPermission etc.
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="systemName">Plugin system name</param>
    /// <param name="allConfigs">Permission configs</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task UnInstallPluginAsync<T>(string systemName, IList<PermissionConfig> allConfigs)
         where T : ISettings, new()
    {
        //settings
        await _settingService.DeleteSettingAsync<T>();

        //locale resources
        await _localizationService.DeleteLocaleResourcesAsync(systemName);

        //permission
        await _nopAdvanceCoreService.UninstallPermissionsAsync(allConfigs);
    }

    public async Task<LicenseCheckResult> CheckLicense(string systemName, string settingPrefix)
    {
        var (licenseStatus, _) = await ValidateLicenseAsync(systemName);
        if (licenseStatus == LicenseStatusType.Expired)
        {
            var key = settingPrefix.ToLowerInvariant() + "." + CorePluginDefaults.ENABLED_KEY_SETTING;

            var setting = await _settingService.GetSettingAsync(key);
            if (setting != null && !setting.Value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                setting.Value = "False";
                await _settingService.UpdateSettingAsync(setting);
            }
            foreach (var store in await _storeService.GetAllStoresAsync())
            {
                var storeSetting = await _settingService.GetSettingAsync(key, store.Id);
                if (storeSetting != null)
                    await _settingService.DeleteSettingAsync(storeSetting);
            }
        }

        return licenseStatus switch
        {
            LicenseStatusType.Trial or LicenseStatusType.Free or LicenseStatusType.Paid => LicenseCheckResult.Valid,
            LicenseStatusType.Expired or LicenseStatusType.NotFound or LicenseStatusType.Error => LicenseCheckResult.Invalid,
            _ => LicenseCheckResult.Invalid,
        };
    }

    public async Task ShowLicenseWarningAsync(SystemWarningCreatedEvent eventMessage, string systemName)
    {
        var (licenseStatus, message) = await ValidateLicenseAsync(systemName);
        var warningText = string.Empty;
        var warningLevel = SystemWarningLevel.Pass;
        switch (licenseStatus)
        {
            case LicenseStatusType.Trial:
                warningText = $"{systemName}. Unregistered version, {message} left of 10 days.";
                warningLevel = SystemWarningLevel.Warning;
                break;
            case LicenseStatusType.Expired:
                warningText = $"{systemName}. Unregistered version, trial period of 10 days has been expired.";
                warningLevel = SystemWarningLevel.Fail;
                break;
            case LicenseStatusType.NotFound:
            case LicenseStatusType.Error:
                warningText = $"{systemName}. Invalid or no license found.";
                warningLevel = SystemWarningLevel.Fail;
                break;
        }

        if (!string.IsNullOrEmpty(warningText))
        {
            eventMessage.SystemWarnings.Add(new SystemWarningModel
            {
                Level = warningLevel,
                Text = warningText
            });
        }
    }

    #endregion
}

public interface ILicenseHelper
{
    Task<(LicenseStatusType licenseStatus, string message)> ValidateLicenseAsync(string systemName);

    /// <summary>
    /// Does default functionalities like AddOrUpdateLocaleResources, SaveSetting, InstallPermissions etc.
    /// </summary>
    /// <typeparam name="T">Setting type</typeparam>
    /// <param name="systemName">Plugin system name</param>
    /// <param name="settings">Settings</param>
    /// <returns>Error message</returns>
    Task<string> InstallPluginAsync<T>(string systemName, T settings) where T : BaseNopAdvanceSettings, new();

    /// <summary>
    /// Does default functionalities like DeleteSetting, DeleteLocaleResources, UninstallPermission etc.
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="systemName">Plugin system name</param>
    /// <param name="allConfigs">Permission configs</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UnInstallPluginAsync<T>(string systemName, IList<PermissionConfig> allConfigs)
         where T : ISettings, new();

    Task<LicenseCheckResult> CheckLicense(string systemName, string settingPrefix);

    Task ShowLicenseWarningAsync(SystemWarningCreatedEvent eventMessage, string systemName);

    /// <summary>
    /// Does default functionalities like AddOrUpdateLocaleResources, SaveSetting, InstallPermission etc.
    /// </summary>
    /// <param name="systemName">Plugin system name</param>
    /// <param name="settingPrefix">Setting prefix</param>
    /// <returns>Error message</returns>
    Task<string> UpdatePluginAsync(string systemName, string settingPrefix);
}
