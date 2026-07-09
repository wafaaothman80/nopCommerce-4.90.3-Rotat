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
using Nop.Services.Plugins;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;
using NopAdvance.Plugin.Core.Infrastructure;
using Nop.Services.Security;
using Nop.Services.Localization;
using Nop.Services.Events;

namespace NopAdvance.Plugin.Core.Services;
public class EventConsumer : IConsumer<AdminMenuCreatedEvent>

{
    #region Fields

    private readonly IPluginService _pluginService;
    private readonly ILocalizationService _localizationService;
    private readonly IAdminMenu _adminMenu;

    #endregion

    #region Ctor

    public EventConsumer(IPluginService pluginService,
        ILocalizationService localizationService,
        IAdminMenu adminMenu)
    {
        _pluginService = pluginService;
        _localizationService = localizationService;
        _adminMenu = adminMenu;
    }

    #endregion

    #region Methods

    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        var nopAdvanceMenu = new AdminMenuItem()
        {
            Title = CoreDefaults.ROOT_MENU_SYSTEM_NAME,
            IconClass = CoreIconClassDefaults.NOPADVANCE,
            SystemName = CoreDefaults.ROOT_MENU_SYSTEM_NAME,
            PermissionNames = new List<string> { StandardPermission.Configuration.MANAGE_PLUGINS }
        };

        var plugins = await _pluginService.GetPluginDescriptorsAsync<IPlugin>(dependsOnSystemName: CoreDefaults.SYSTEM_NAME);
        var nopAdvancePlugins = plugins.Where(x => x.SystemName.StartsWith(CorePluginDefaults.PLUGINS_SYSTEM_NAME_PREFIX, StringComparison.InvariantCultureIgnoreCase));

        if (nopAdvancePlugins.Any())
        {
            var pluginMenu = new AdminMenuItem()
            {
                Title = await _localizationService.GetResourceAsync(CoreLocaleResourceDefaults.PLUGIN_MENU),
                IconClass = CoreIconClassDefaults.PLUG,
                SystemName = CoreDefaults.PLUGINS_MENU_SYSTEM_NAME
            };
            nopAdvanceMenu.ChildNodes.Add(pluginMenu);
        }

        var nopAdvanceThemePlugins = plugins.Where(x => x.SystemName.StartsWith(CorePluginDefaults.THEME_PLUGINS_SYSTEM_NAME_PREFIX, StringComparison.InvariantCultureIgnoreCase));
        if (nopAdvanceThemePlugins.Any())
        {
            var themeMenu = new AdminMenuItem()
            {
                Title = await _localizationService.GetResourceAsync(CoreLocaleResourceDefaults.THEME_MENU),
                Visible = true,
                IconClass = CoreIconClassDefaults.THEME,
                SystemName = CoreDefaults.THEME_PLUGINS_MENU_SYSTEM_NAME
            };
            nopAdvanceMenu.ChildNodes.Add(themeMenu);
        }

        var nopAdvanceDicountRulesPlugins = plugins.Where(x => x.SystemName.StartsWith(CorePluginDefaults.DISCOUNT_RULES_PLUGINS_SYSTEM_NAME_PREFIX, StringComparison.InvariantCultureIgnoreCase));
        if (nopAdvanceDicountRulesPlugins.Any())
        {
            var discountRuleMenu = new AdminMenuItem()
            {
                Title = await _localizationService.GetResourceAsync(CoreLocaleResourceDefaults.DISCOUNT_RULES_MENU),
                Visible = true,
                IconClass = CoreIconClassDefaults.DISCOUNT_RULE,
                SystemName = CoreDefaults.DISCOUNT_RULES_PLUGINS_MENU_SYSTEM_NAME
            };
            nopAdvanceMenu.ChildNodes.Add(discountRuleMenu);
        }

        var widgetZoneMenu = new AdminMenuItem()
        {
            Title = await _localizationService.GetResourceAsync(CoreLocaleResourceDefaults.WIDGET_ZONES),
            Visible = true,
            IconClass = "fas fa-th-large",
            SystemName = CorePluginDefaults.WIDGET_ZONE_SYSTEM_NAME,
            Url = _adminMenu.GetMenuItemUrl(CorePluginDefaults.WIDGET_ZONE_CONTROLLER_NAME, CorePluginDefaults.WIDGET_ZONE_MANAGE_ACTION_NAME)
        };
        nopAdvanceMenu.ChildNodes.Insert(nopAdvanceMenu.ChildNodes.Count, widgetZoneMenu);

        var licenseMenu = new AdminMenuItem()
        {
            Title = await _localizationService.GetResourceAsync(CoreLocaleResourceDefaults.LICENSE),
            Visible = true,
            IconClass = "fas fa-key",
            SystemName = CorePluginDefaults.LICENSE_SYSTEM_NAME,
            Url = _adminMenu.GetMenuItemUrl(CorePluginDefaults.LICENSE_CONTROLLER_NAME, "List"),
        };
        nopAdvanceMenu.ChildNodes.Insert(nopAdvanceMenu.ChildNodes.Count, licenseMenu);

        var morePluginsMenu = new AdminMenuItem()
        {
            Title = await _localizationService.GetResourceAsync(CoreLocaleResourceDefaults.MORE_PLUGINS_MENU),
            Visible = true,
            IconClass = CoreIconClassDefaults.BINOCULARS,
            SystemName = CorePluginDefaults.MORE_PLUGINS_MENU_SYSTEM_NAME,
            Url = CorePluginDefaults.MORE_PLUGINS_URL,
            OpenUrlInNewTab = true
        };
        nopAdvanceMenu.ChildNodes.Insert(nopAdvanceMenu.ChildNodes.Count, morePluginsMenu);

        eventMessage.RootMenuItem.ChildNodes.Add(nopAdvanceMenu);
    }

    #endregion
}
