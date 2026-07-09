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
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;

namespace NopAdvance.Plugin.Misc.PublicAPI.Filters;

public sealed class CheckAPIEnabledAttribute : TypeFilterAttribute
{
    #region Ctor

    /// <summary>
    /// Create instance of the filter attribute
    /// </summary>
    public CheckAPIEnabledAttribute() : base(typeof(CheckAPIEnabledFilter))
    {
    }

    #endregion

    #region Nested filter

    /// <summary>
    /// Represents a filter that confirms access to closed store
    /// </summary>
    private class CheckAPIEnabledFilter : IAsyncActionFilter
    {
        #region Fields

        private readonly NopAdvanceAPISettings _nopAdvancePublicAPISettings;

        #endregion

        #region Ctor

        public CheckAPIEnabledFilter(NopAdvanceAPISettings nopAdvancePublicAPISettings)
        {
            _nopAdvancePublicAPISettings = nopAdvancePublicAPISettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Called synchronously before the action, after model binding is complete.
        /// </summary>
        /// <param name="context">A context for action filters</param>
        private void CheckAPIEnabled(ActionExecutingContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.HttpContext.Request == null)
                return;

            if (!context.HttpContext.Request.Path.StartsWithSegments(new PathString("/api")))
                return;

            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            if (_nopAdvancePublicAPISettings.Enabled)
                return;

            context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            var response = new ErrorResponse();
            response.AddError("API's are being disabled");

            context.Result = new JsonResult(response)
            {
                ContentType = Nop.Core.MimeTypes.ApplicationJson,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called asynchronously before the action, after model binding is complete.
        /// </summary>
        /// <param name="context">A context for action filters</param>
        /// <param name="next">A delegate invoked to execute the next action filter or the action itself</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            CheckAPIEnabled(context);
            if (context.Result == null)
                await next();
        }

        #endregion
    }

    #endregion
}
