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
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using NopAdvance.Plugin.Core.Helpers;
using NopAdvance.Plugin.Core.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Areas.Admin.Components;
using NopAdvance.Plugin.Misc.PublicAPI.Domain;
using NopAdvance.Plugin.Misc.PublicAPI.Services;

namespace NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;

public class PublicAPIPlugin : BasePlugin, IWidgetPlugin
{
    #region Fields

    private readonly ISettingService _settingService;
    private readonly IWebHelper _webHelper;
    private readonly WidgetSettings _widgetSettings;
    private readonly IAPIService _apiService;
    private readonly ILicenseHelper _licenseHelper;
    private readonly IPermissionService _permissionService;
    private readonly ICustomerService _customerService;

    #endregion

    #region Ctor

    public PublicAPIPlugin(ISettingService settingService,
        IWebHelper webHelper,
        WidgetSettings widgetSettings,
        IAPIService apiService,
        ILicenseHelper licenseHelper,
        IPermissionService permissionService,
        ICustomerService customerService)
    {
        _settingService = settingService;
        _webHelper = webHelper;
        _widgetSettings = widgetSettings;
        _apiService = apiService;
        _licenseHelper = licenseHelper;
        _permissionService = permissionService;
        _customerService = customerService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets widget zones where this widget should be rendered
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the widget zones
    /// </returns>
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string> { AdminWidgetZones.MaintenanceDetailsBlock });
    }

    /// <summary>
    /// Gets a name of a view component for displaying widget
    /// </summary>
    /// <param name="widgetZone">Name of the widget zone</param>
    /// <returns>View component name</returns>
    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(NopAdvanceTokenManagementViewComponent);
    }

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + SiteMapDefaults.CONFIGURATION_PAGE_URL;
    }

    /// <summary>
    /// Install plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        var nopAdvanceAPISettings = new NopAdvanceAPISettings
        {
            Enabled = true,
            AccessTokenExpiration = 7,
            AccessTokenExpirationDuration = DurationType.Days,
            RefreshTokenExpiration = 30,
            RefreshTokenExpirationDuration = DurationType.Days,
            SecretKey = PluginCommonHelper.GenerateSecretKey(),
            SecurityAlgorithmType = AlgorithmType.HmacSha256,
        };

        //licencing
        var message = await _licenseHelper.InstallPluginAsync(PluginDefaults.SYSTEM_NAME,
            nopAdvanceAPISettings);


        if (!string.IsNullOrEmpty(message))
            throw new Exception(message);

        await base.InstallAsync();

        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDefaults.SYSTEM_NAME))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(PluginDefaults.SYSTEM_NAME);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await _apiService.CreateCallsCount();
    }

    /// <summary>
    /// Uninstall plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        await base.UninstallAsync();

        //uninstall plugin
        await _licenseHelper.UnInstallPluginAsync<NopAdvanceAPISettings>(PluginDefaults.SYSTEM_NAME, PluginDefaults.AllConfigs);

        // Remove the permission
        await _permissionService.DeletePermissionAsync(PluginDefaults.SYSTEM_NAME);

        if (_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDefaults.SYSTEM_NAME))
        {
            _widgetSettings.ActiveWidgetSystemNames.Remove(PluginDefaults.SYSTEM_NAME);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }
    }

    #endregion

    #region Properties

    public bool HideInWidgetList => false;

    #endregion
}
