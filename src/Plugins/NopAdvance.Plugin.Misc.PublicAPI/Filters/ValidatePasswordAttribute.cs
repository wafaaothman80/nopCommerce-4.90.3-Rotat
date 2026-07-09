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
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Localization;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;

namespace NopAdvance.Plugin.Misc.PublicAPI.Filters;

/// <summary>
/// Represents filter attribute that validates customer password expiration
/// </summary>
public sealed class ValidatePasswordAttribute : TypeFilterAttribute
{
    #region Ctor

    /// <summary>
    /// Create instance of the filter attribute
    /// </summary>
    public ValidatePasswordAttribute() : base(typeof(ValidatePasswordFilter))
    {
    }

    #endregion

    #region Nested filter

    /// <summary>
    /// Represents a filter that validates customer password expiration
    /// </summary>
    private class ValidatePasswordFilter : IAsyncActionFilter
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public ValidatePasswordFilter(ICustomerService customerService,
            IWorkContext workContext,
            ILocalizationService localizationService)
        {
            _customerService = customerService;
            _workContext = workContext;
            _localizationService = localizationService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Called asynchronously before the action, after model binding is complete.
        /// </summary>
        /// <param name="context">A context for action filters</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task ValidatePasswordAsync(ActionExecutingContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.HttpContext.Request == null)
                return;

            if (!context.HttpContext.Request.Path.StartsWithSegments(new PathString("/api")))
                return;

            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            //get action and controller names
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var actionName = actionDescriptor?.ActionName;
            var controllerName = actionDescriptor?.ControllerName;

            if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(controllerName))
                return;

            //don't validate on the 'Change Password' page
            if (controllerName.Equals("Customer", StringComparison.InvariantCultureIgnoreCase) &&
                actionName.Equals("ChangePassword", StringComparison.InvariantCultureIgnoreCase))
                return;

            //check password expiration
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsPasswordExpiredAsync(customer))
                return;

            //redirect to ChangePassword page if expires
            context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            var response = new ErrorResponse();
            response.AddError(await _localizationService.GetResourceAsync("Account.ChangePassword.PasswordIsExpired"));

            context.Result = new JsonResult(response)
            {
                ContentType = Nop.Core.MimeTypes.ApplicationJson,
                StatusCode = StatusCodes.Status401Unauthorized
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
            await ValidatePasswordAsync(context);
            if (context.Result == null)
                await next();
        }

        #endregion
    }

    #endregion
}