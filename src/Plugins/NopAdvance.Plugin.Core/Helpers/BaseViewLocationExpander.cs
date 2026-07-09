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
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Plugins;
using Nop.Web.Framework;
using NopAdvance.Plugin.Core.Areas.Admin.Controllers;
using NopAdvance.Plugin.Core.Controllers;
using NopAdvance.Plugin.Core.Infrastructure;

namespace NopAdvance.Plugin.Core.Helpers;

public abstract class BaseViewLocationExpander
{
    public IEnumerable<string> AddAdminLocation(ViewLocationExpanderContext context, IEnumerable<string> viewLocations, string systemName)
    {
        if (context.AreaName == AreaNames.ADMIN)
        {
            if (context.ActionContext.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                if (controllerActionDescriptor.ControllerTypeInfo.BaseType.Name == nameof(NopAdvanceBaseAdminController))
                    viewLocations = new[] { 
                        $"{NopPluginDefaults.Path}/{systemName}/Areas/{AreaNames.ADMIN}/Views/{{1}}/{{0}}.cshtml",
                        $"{NopPluginDefaults.Path}/{systemName}/Areas/{AreaNames.ADMIN}/Views/Shared/{{0}}.cshtml"
                    }.Concat(viewLocations);
            }
        }

        return viewLocations;
    }

    public IEnumerable<string> AddThemeLocation(ViewLocationExpanderContext context, IEnumerable<string> viewLocations, 
        string systemName, string settingPrefix)
    {
        if (context.AreaName == null && context.ActionContext.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor &&
            controllerActionDescriptor.ControllerTypeInfo.BaseType.Name == nameof(NopAdvanceBasePublicController))
        {
            var key = settingPrefix.ToLowerInvariant() + "." + CorePluginDefaults.ENABLED_KEY_SETTING;
            var settingService = EngineContext.Current.Resolve<ISettingService>();
            var setting = settingService.GetSettingAsync(key).Result;
            if (setting != null && setting.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                var viewPath = $"/{NopPluginDefaults.PathName}/{systemName}/Views";
                viewLocations = new[] {
                        $"{viewPath}/{{1}}/{{0}}.cshtml",
                        $"{viewPath}/Shared/{{0}}.cshtml"
                    }.Concat(viewLocations);

                if (context.Values.TryGetValue("nop.themename", out var themeName))
                {
                    var themeViewPath = $"/{NopPluginDefaults.PathName}/{systemName}/Themes/{themeName}/Views";
                    viewLocations = new[] {
                            $"{themeViewPath}/{{1}}/{{0}}.cshtml",
                            $"{themeViewPath}/Shared/{{0}}.cshtml"
                        }.Concat(viewLocations);
                }
            }
        }

        return viewLocations;
    }
}
