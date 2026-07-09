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
using Nop.Core.Domain.Cms;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using NopAdvance.Plugin.Core.Areas.Admin.Components;
using NopAdvance.Plugin.Core.Helpers;

namespace NopAdvance.Plugin.Core.Infrastructure;

public class NopAdvanceCorePlugin : BasePlugin, IWidgetPlugin
{
    #region Fields

    private readonly WidgetSettings _widgetSettings;
    private readonly IPluginService _pluginService;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly ILocaleResourceHelper _localeResourceHelper;

    #endregion

    #region Ctor

    public NopAdvanceCorePlugin(WidgetSettings widgetSettings,
        IPluginService pluginService,
        ILocalizationService localizationService,
        ISettingService settingService,
        ILocaleResourceHelper localeResourceHelper)
    {
        _widgetSettings = widgetSettings;
        _pluginService = pluginService;
        _localizationService = localizationService;
        _settingService = settingService;
        _localeResourceHelper = localeResourceHelper;
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
        return Task.FromResult<IList<string>>(new List<string> { AdminWidgetZones.HeaderBefore });
    }

    /// <summary>
    /// Gets a name of a view component for displaying widget
    /// </summary>
    /// <param name="widgetZone">Name of the widget zone</param>
    /// <returns>View component name</returns>
    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(NopAdvanceCoreViewComponent);
    }

    /// <summary>
    /// Install plugin
    /// </summary>
    public override async Task InstallAsync()
    {
        //locales
        await _localeResourceHelper.AddOrUpdateLocaleResourcesAsync(CoreDefaults.SYSTEM_NAME);

        await base.InstallAsync();

        _widgetSettings.ActiveWidgetSystemNames.Add(PluginDescriptor.SystemName);
        await _settingService.SaveSettingAsync(_widgetSettings);
    }

    /// <summary>
    /// Uninstall plugin
    /// </summary>
    public override async Task UninstallAsync()
    {
        await _localizationService.DeleteLocaleResourcesAsync(CoreDefaults.SYSTEM_NAME);

        await base.UninstallAsync();

        _widgetSettings.ActiveWidgetSystemNames.Remove(PluginDescriptor.SystemName);
        await _settingService.SaveSettingAsync(_widgetSettings);
    }

    public override async Task PreparePluginToUninstallAsync()
    {
        var allNopAdvanceInstalledPlugins =
            _pluginService.GetPluginDescriptorsAsync<IPlugin>(dependsOnSystemName: CoreDefaults.SYSTEM_NAME).Result;

        if (allNopAdvanceInstalledPlugins.Any())
            throw new Exception(CoreDefaults.DEPENDENT_ERROR);
        await base.PreparePluginToUninstallAsync();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
    /// </summary>
    public bool HideInWidgetList => true;

    #endregion
}
