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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;
using NopAdvance.Plugin.Core.Helpers;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Admin;

namespace NopAdvance.Plugin.Misc.PublicAPI.Areas.Admin.Components;

public class NopAdvanceTokenManagementViewComponent : NopViewComponent
{
    #region Fields

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
    private readonly ILicenseHelper _licenseHelper;

    #endregion

    #region Ctor

    public NopAdvanceTokenManagementViewComponent(IHttpContextAccessor httpContextAccessor,
        ITempDataDictionaryFactory tempDataDictionaryFactory,
        ILicenseHelper licenseHelper)
    {
        _httpContextAccessor = httpContextAccessor;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
        _licenseHelper = licenseHelper;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var licenseCheckResult = await _licenseHelper.CheckLicense(PluginDefaults.SYSTEM_NAME, nameof(NopAdvanceAPISettings));
        if (licenseCheckResult == Core.Domain.LicenseCheckResult.Valid && widgetZone == AdminWidgetZones.MaintenanceDetailsBlock)
        {
            var model = new TokenMaintenanceModel();
            if (additionalData is MaintenanceModel maintenanceModel)
            {
                var key = $"{PluginDefaults.SYSTEM_NAME}.NumberOfDeletedTokens";
                var context = _httpContextAccessor.HttpContext;
                var tempData = _tempDataDictionaryFactory.GetTempData(context);
                if (tempData.ContainsKey(key))
                    model.NumberOfDeletedItems = (int)tempData[key];
            }
            return View($"~/Plugins/{PluginDefaults.SYSTEM_NAME}/Areas/Admin/Views/TokenMaintenance.cshtml", model);
        }

        return Content(string.Empty);
    }

    #endregion
}
