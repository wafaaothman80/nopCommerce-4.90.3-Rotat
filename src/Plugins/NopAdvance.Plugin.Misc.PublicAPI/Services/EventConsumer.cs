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
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;
using NopAdvance.Plugin.Core.Helpers;
using NopAdvance.Plugin.Core.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;

namespace NopAdvance.Plugin.Misc.PublicAPI.Services;

public class EventConsumer : IConsumer<AdminMenuCreatedEvent>, IConsumer<SystemWarningCreatedEvent>
{
    #region Fields

    private readonly ILicenseHelper _licenseHelper;
    private readonly IAdminMenu _adminMenu;
    private readonly ILocalizationService _localizationService;
    private readonly IGenericHelper _genericHelper;

    #endregion

    #region Ctor

    public EventConsumer(IAdminMenu adminMenu,
        ILocalizationService localizationService,
        ILicenseHelper licenseHelper,
        IGenericHelper genericHelper)
    {
        _adminMenu = adminMenu;
        _localizationService = localizationService;
        _licenseHelper = licenseHelper;
        _genericHelper = genericHelper;
    }

    #endregion

    #region Methods

    public async Task HandleEventAsync(SystemWarningCreatedEvent eventMessage)
    {
        await _licenseHelper.ShowLicenseWarningAsync(eventMessage, PluginDefaults.SYSTEM_NAME);
    }

    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        var nopAdvanceMenu = eventMessage.RootMenuItem.ChildNodes.FirstOrDefault(x => x.SystemName == CoreDefaults.ROOT_MENU_SYSTEM_NAME);
        if (nopAdvanceMenu != null)
        {
            var pluginMenu = nopAdvanceMenu.ChildNodes.FirstOrDefault(x => x.SystemName == CoreDefaults.PLUGINS_MENU_SYSTEM_NAME);
            if (pluginMenu != null)
            {
                var adminMenuItem = new AdminMenuItem()
                {
                    SystemName = SiteMapDefaults.MAIN_MENU_SYSTEM_NAME,
                    Title = await _localizationService.GetResourceAsync(PluginDefaults.SYSTEM_NAME),
                    IconClass = CoreIconClassDefaults.DOT_CIRCLE,
                    PermissionNames = new List<string> { PluginDefaults.SYSTEM_NAME }
                };
                pluginMenu.ChildNodes.Add(adminMenuItem);

                var configureMenu = new AdminMenuItem()
                {
                    SystemName = SiteMapDefaults.CONFIGURE_MENU_SYSTEM_NAME,
                    Title = await _localizationService.GetResourceAsync(CoreLocaleResourceDefaults.CONFIGURATION),
                    IconClass = CoreIconClassDefaults.CIRCLE,
                    Url = _adminMenu.GetMenuItemUrl(SiteMapDefaults.ADMIN_CONFIGURE_CONTROLLER_NAME, SiteMapDefaults.CONFIGURE_ACTION_NAME)
                };
                adminMenuItem.ChildNodes.Add(configureMenu);

                var applicationsMenu = new AdminMenuItem()
                {
                    SystemName = SiteMapDefaults.APPLICATIONS_MENU_SYSTEM_NAME,
                    Title = await _localizationService.GetResourceAsync(LocaleResourceDefaults.APPLICATIONS_MENU),
                    Url = _adminMenu.GetMenuItemUrl(SiteMapDefaults.ADMIN_PUBLIC_API_CONTROLLER_NAME, SiteMapDefaults.APPLICATION_LIST_ACTION_NAME),
                    Visible = true,
                    IconClass = CoreIconClassDefaults.CIRCLE
                };
                adminMenuItem.ChildNodes.Add(applicationsMenu);

                var tokensMenu = new AdminMenuItem()
                {
                    SystemName = SiteMapDefaults.TOKENS_MENU_SYSTEM_NAME,
                    Title = await _localizationService.GetResourceAsync(LocaleResourceDefaults.TOKENS_MENU),
                    Url = _adminMenu.GetMenuItemUrl(SiteMapDefaults.ADMIN_PUBLIC_API_CONTROLLER_NAME, SiteMapDefaults.TOKEN_LIST_ACTION_NAME),
                    Visible = true,
                    IconClass = CoreIconClassDefaults.CIRCLE
                };
                adminMenuItem.ChildNodes.Add(tokensMenu);

                var debugLogMenu = new AdminMenuItem()
                {
                    SystemName = SiteMapDefaults.DEBUG_LOG_MENU_SYSTEM_NAME,
                    Title = await _localizationService.GetResourceAsync(LocaleResourceDefaults.DEBUG_LOG_MENU),
                    Url = _adminMenu.GetMenuItemUrl(SiteMapDefaults.ADMIN_PUBLIC_API_CONTROLLER_NAME, SiteMapDefaults.DEBUG_LOG_LIST_ACTION_NAME),
                    Visible = true,
                    IconClass = CoreIconClassDefaults.CIRCLE
                };
                adminMenuItem.ChildNodes.Add(debugLogMenu);

                //help menu
                adminMenuItem.ChildNodes.Add(await _genericHelper.GetHelpMenuItemAsync(PluginDefaults.SYSTEM_NAME));
            }
        }
    }
    
    #endregion
}
