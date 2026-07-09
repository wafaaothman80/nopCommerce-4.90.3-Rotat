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
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopAdvance.Plugin.Core.Areas.Admin.Controllers;
using NopAdvance.Plugin.Core.Filters;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Admin;
using NopAdvance.Plugin.Misc.PublicAPI.Services;

namespace NopAdvance.Plugin.Misc.PublicAPI.Areas.Admin.Controllers;

[NopAdvanceCheckLicense(PluginDefaults.SYSTEM_NAME, nameof(NopAdvanceAPISettings))]
public class CommonController : NopAdvanceBaseAdminController
{
    #region Fields

    private readonly IPermissionService _permissionService;
    private readonly ICommonModelFactory _commonModelFactory;
    private readonly IAPIService _apiService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;

    #endregion

    #region Ctor

    public CommonController(IPermissionService permissionService,
        ICommonModelFactory commonModelFactory,
        IAPIService apiService,
        IHttpContextAccessor httpContextAccessor,
        ITempDataDictionaryFactory tempDataDictionaryFactory)
    {
        _permissionService = permissionService;
        _commonModelFactory = commonModelFactory;
        _apiService = apiService;
        _httpContextAccessor = httpContextAccessor;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
    }

    #endregion

    #region Methods

    [HttpPost, ActionName("Maintenance")]
    [FormValueRequired("delete-tokens")]
    [CheckPermission(new[] { StandardPermission.Configuration.MANAGE_PLUGINS })]
    [CheckPermission(new[] { PluginDefaults.SYSTEM_NAME })]
    public async Task<IActionResult> MaintenanceDeleteTokens(TokenMaintenanceModel pluginModel)
    {
        var model = await _commonModelFactory.PrepareMaintenanceModelAsync(new MaintenanceModel());
        var key = $"{PluginDefaults.SYSTEM_NAME}.NumberOfDeletedTokens";
        var context = _httpContextAccessor.HttpContext;
        var tempData = _tempDataDictionaryFactory.GetTempData(context);
        tempData[key] = await _apiService.DeleteExpiredRefreshTokensAsync(pluginModel.IncludeRevoked);

        return View(model);
    }

    #endregion
}
