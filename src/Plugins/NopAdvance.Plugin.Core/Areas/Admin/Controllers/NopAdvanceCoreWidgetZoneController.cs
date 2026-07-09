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
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Web.Framework.Mvc.Filters;
using NopAdvance.Plugin.Core.Infrastructure;
using NopAdvance.Plugin.Core.Models;
using Nop.Services.Security;

namespace NopAdvance.Plugin.Core.Areas.Admin.Controllers;

public class NopAdvanceCoreWidgetZoneController : NopAdvanceBaseAdminController
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public NopAdvanceCoreWidgetZoneController(ILocalizationService localizationService,
        INotificationService notificationService,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
        _storeContext = storeContext;
    }

    #endregion

    #region Methods

    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    public async Task<IActionResult> Manage()
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var widgetZoneSettings = await _settingService.LoadSettingAsync<NopAdvanceCorePluginSettings>(storeScope);

        var model = new CustomWidgetZoneModel
        {
            CustomWidgetZones = widgetZoneSettings.CustomWidgetZones,
            SystemName = CorePluginDefaults.WIDGET_ZONE_SYSTEM_NAME,
            ControllerName = CorePluginDefaults.WIDGET_ZONE_CONTROLLER_NAME,
            ActionName = CorePluginDefaults.WIDGET_ZONE_MANAGE_ACTION_NAME
        };

        if (storeScope > 0)
            model.CustomWidgetZones_OverrideForStore = await _settingService.SettingExistsAsync(widgetZoneSettings, x => x.CustomWidgetZones, storeScope);

        return View(model);
    }

    [HttpPost]
    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    public async Task<IActionResult> Manage(CustomWidgetZoneModel model)
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var widgetZoneSettings = await _settingService.LoadSettingAsync<NopAdvanceCorePluginSettings>(storeScope);

        widgetZoneSettings.CustomWidgetZones = model.CustomWidgetZones;

        /* We do not clear cache after each setting update.
         * This behavior can increase performance because cached settings will not be cleared 
         * and loaded from database after each update */
        await _settingService.SaveSettingOverridablePerStoreAsync(widgetZoneSettings, x => x.CustomWidgetZones, model.CustomWidgetZones_OverrideForStore, storeScope, false);

        //now clear settings cache
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync(CoreLocaleResourceDefaults.WIDGET_ZONES_UPDATED));

        return await Manage();
    }

    #endregion
}
