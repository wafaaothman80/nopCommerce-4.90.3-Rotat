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
using Nop.Data;
using Nop.Services.Security;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;

namespace NopAdvance.Plugin.Misc.PublicAPI.Filters;

/// <summary>
/// Represents a filter attribute that confirms access to public store
/// </summary>
public sealed class CheckAccessPublicStoreAttribute : TypeFilterAttribute
{
    #region Ctor

    /// <summary>
    /// Create instance of the filter attribute
    /// </summary>
    /// <param name="ignore">Whether to ignore the execution of filter actions</param>
    public CheckAccessPublicStoreAttribute(bool ignore = false) : base(typeof(CheckAccessPublicStoreFilter))
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
    /// Represents a filter that confirms access to public store
    /// </summary>
    private class CheckAccessPublicStoreFilter : IAsyncAuthorizationFilter
    {
        #region Fields

        private readonly bool _ignoreFilter;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public CheckAccessPublicStoreFilter(bool ignoreFilter, IPermissionService permissionService)
        {
            _ignoreFilter = ignoreFilter;
            _permissionService = permissionService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Called early in the filter pipeline to confirm request is authorized
        /// </summary>
        /// <param name="context">Authorization filter context</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task CheckAccessPublicStoreAsync(AuthorizationFilterContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.HttpContext.Request == null)
                return;

            if (!context.HttpContext.Request.Path.StartsWithSegments(new PathString("/api")))
                return;

            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            //check whether this filter has been overridden for the Action
            var actionFilter = context.ActionDescriptor.FilterDescriptors
                .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Action)
                .Select(filterDescriptor => filterDescriptor.Filter)
                .OfType<CheckAccessPublicStoreAttribute>()
                .FirstOrDefault();

            //ignore filter (the action is available even if navigation is not allowed)
            if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                return;

            //check whether current customer has access to a public store
            if (await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.PUBLIC_STORE_ALLOW_NAVIGATION))
                return;

            //customer hasn't access to a public store
            context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            var response = new ErrorResponse();
            response.AddError("Public store navigation is disabled");

            context.Result = new JsonResult(response)
            {
                ContentType = Nop.Core.MimeTypes.ApplicationJson,
                StatusCode = StatusCodes.Status401Unauthorized
            };
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
            await CheckAccessPublicStoreAsync(context);
        }

        #endregion
    }

    #endregion
}