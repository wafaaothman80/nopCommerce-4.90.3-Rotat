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
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Services.Stores;
using NopAdvance.Plugin.Core.Helpers;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;
using NopAdvance.Plugin.Misc.PublicAPI.Services;
using Nop.Services.Localization;

namespace NopAdvance.Plugin.Misc.PublicAPI.Filters;

public sealed class AuthorizeAttribute : TypeFilterAttribute
{
    #region Ctor

    /// <summary>
    /// Create instance of the filter attribute
    /// </summary>
    public AuthorizeAttribute(bool ignore = false) : base(typeof(AuthorizeFilter))
    {
        IgnoreFilter = ignore;
        Arguments = new object[] { ignore };
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether to ignore the execution of filter actions
    /// </summary>
    public bool IgnoreFilter { get; }

    #endregion

    #region Nested filter

    /// <summary>
    /// Represents a filter that checks for the authorization
    /// </summary>
    private class AuthorizeFilter : IAsyncAuthorizationFilter
    {
        #region Fields

        private readonly bool _ignoreFilter;
        private readonly NopAdvanceAPISettings _pluginSettings;
        private readonly IAPIService _apiService;
        private readonly IStoreService _storeService;
        private readonly ICustomerService _customerService;
        private readonly IPermissionService _permissionService;
        private readonly ILicenseHelper _licenseHelper;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public AuthorizeFilter(bool ignoreFilter,
            NopAdvanceAPISettings pluginSettings,
            IAPIService apiService,
            IStoreService storeService,
            ICustomerService customerService,
            IPermissionService permissionService,
            ILicenseHelper licenseHelper,
            ILocalizationService localizationService)
        {
            _ignoreFilter = ignoreFilter;
            _pluginSettings = pluginSettings;
            _apiService = apiService;
            _storeService = storeService;
            _customerService = customerService;
            _permissionService = permissionService;
            _licenseHelper = licenseHelper;
            _localizationService = localizationService;
        }

        #endregion

        #region Utilities

        private async Task<bool> AuthorizeAPIKeyAsync(AuthorizationFilterContext context)
        {
            var isApiKeyValid = false;
            if (context.HttpContext.Request.Headers.ContainsKey(AuthenticationDefaults.API_KEY_NAME))
            {
                var apiKey = context.HttpContext.Request.Headers[AuthenticationDefaults.API_KEY_NAME].FirstOrDefault();
                if (!string.IsNullOrEmpty(apiKey))
                {
                    var application = await _apiService.GetAPIApplicationByAPIKeyAsync(apiKey);
                    if (application != null)
                    {
                        string host = context.HttpContext.Request.Headers[HeaderNames.Host];
                        var allStores = await _storeService.GetAllStoresAsync();
                        var store = allStores.FirstOrDefault(s => _storeService.ContainsHostValue(s, host)) ?? allStores.FirstOrDefault();
                        if (store != null && store.Id == application.StoreId)
                            isApiKeyValid = true;
                    }
                }
            }

            if (!isApiKeyValid)
            {
                context.Result = new JsonResult(new ErrorResponse(MessageDefaults.INVALID_API_KEY))
                {
                    ContentType = Nop.Core.MimeTypes.ApplicationJson,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }

            return isApiKeyValid;
        }

        private async Task AuthorizeTokenAsync(AuthorizationFilterContext context)
        {
            //check whether this filter has been overridden for the Action
            var actionFilter = context.ActionDescriptor.FilterDescriptors
                .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Action)
                .Select(filterDescriptor => filterDescriptor.Filter)
                .OfType<AuthorizeAttribute>()
                .FirstOrDefault();

            //ignore filter (the action is available even if navigation is not allowed)
            if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                return;

            if (context.HttpContext.Items[AuthenticationDefaults.CURRENT_API_CUSTOMER] is not Customer)
            {
                var errorMessage = Convert.ToString(context.HttpContext.Items[AuthenticationDefaults.TOKEN_ERROR_MESSAGE]);
                if (context.HttpContext.Request.Path.StartsWithSegments(new PathString("/api/publiccustomer/getlogin")) ||
                context.HttpContext.Request.Path.StartsWithSegments(new PathString("/api/publiccustomer/login")))
                {
                    var guestRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.GuestsRoleName);
                    var guestUserAllowed = await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.PUBLIC_STORE_ALLOW_NAVIGATION,
                        guestRole.Id);
                    if (guestUserAllowed)
                    {
                        context.Result = new JsonResult(new ErrorResponse(errorMessage))
                        {
                            ContentType = Nop.Core.MimeTypes.ApplicationJson,
                            StatusCode = StatusCodes.Status401Unauthorized
                        };
                    }
                    else
                        return;
                }
                else
                {
                    context.Result = context.Result = new JsonResult(new ErrorResponse(errorMessage))
                    {
                        ContentType = Nop.Core.MimeTypes.ApplicationJson,
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called early in the filter pipeline to confirm request is authorized
        /// </summary>
        /// <param name="context">Authorization filter context</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.HttpContext.Request == null)
                return;

            var licenseCheckResult = await _licenseHelper.CheckLicense(PluginDefaults.SYSTEM_NAME, nameof(NopAdvanceAPISettings));
            if (licenseCheckResult != Core.Domain.LicenseCheckResult.Valid)
            {
                context.Result = context.Result = new JsonResult(new ErrorResponse(await _localizationService.GetResourceAsync(LocaleResourceDefaults.INVALID_LICENSE)))
                {
                    ContentType = Nop.Core.MimeTypes.ApplicationJson,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            if (_pluginSettings.IsDevelopment)
                return;

            if (await AuthorizeAPIKeyAsync(context))
                await AuthorizeTokenAsync(context);
        }

        #endregion
    }

    #endregion
}
