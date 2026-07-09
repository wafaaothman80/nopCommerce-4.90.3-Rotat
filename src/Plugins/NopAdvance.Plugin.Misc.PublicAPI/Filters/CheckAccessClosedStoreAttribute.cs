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
using Nop.Core.Domain;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Topics;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;

namespace NopAdvance.Plugin.Misc.PublicAPI.Filters;

/// <summary>
/// Represents a filter attribute that confirms access to a closed store
/// </summary>
public sealed class CheckAccessClosedStoreAttribute : TypeFilterAttribute
{
    #region Ctor

    /// <summary>
    /// Create instance of the filter attribute
    /// </summary>
    /// <param name="ignore">Whether to ignore the execution of filter actions</param>
    public CheckAccessClosedStoreAttribute(bool ignore = false) : base(typeof(CheckAccessClosedStoreFilter))
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
    /// Represents a filter that confirms access to closed store
    /// </summary>
    private class CheckAccessClosedStoreFilter : IAsyncActionFilter
    {
        #region Fields

        private readonly bool _ignoreFilter;
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly ITopicService _topicService;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public CheckAccessClosedStoreFilter(bool ignoreFilter,
            IPermissionService permissionService,
            IStoreContext storeContext,
            ITopicService topicService,
            StoreInformationSettings storeInformationSettings,
            ILocalizationService localizationService)
        {
            _ignoreFilter = ignoreFilter;
            _permissionService = permissionService;
            _storeContext = storeContext;
            _topicService = topicService;
            _storeInformationSettings = storeInformationSettings;
            _localizationService = localizationService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Called asynchronously before the action, after model binding is complete.
        /// </summary>
        /// <param name="context">A context for action filters</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task CheckAccessClosedStoreAsync(ActionExecutingContext context)
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
                .OfType<CheckAccessClosedStoreAttribute>()
                .FirstOrDefault();

            //ignore filter (the action is available even if a store is closed)
            if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                return;

            //store isn't closed
            if (!_storeInformationSettings.StoreClosed)
                return;

            //get action and controller names
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var actionName = actionDescriptor?.ActionName;
            var controllerName = actionDescriptor?.ControllerName;

            if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(controllerName))
                return;

            //topics accessible when a store is closed
            if (controllerName.Equals("General", StringComparison.InvariantCultureIgnoreCase) &&
                actionName.Equals("GetTopicBlock", StringComparison.InvariantCultureIgnoreCase))
            {
                //get identifiers of topics are accessible when a store is closed

                var store = await _storeContext.GetCurrentStoreAsync();
                var allowedTopicSystemNames = (await _topicService.GetAllTopicsAsync(store.Id))
                    .Where(topic => topic.AccessibleWhenStoreClosed)
                    .Select(topic => topic.SystemName);

                //check whether requested topic is allowed
                var requestedTopicSystemName = context.RouteData.Values["systemName"] as string;
                if (!string.IsNullOrEmpty(requestedTopicSystemName) && allowedTopicSystemNames.Contains(requestedTopicSystemName))
                    return;
            }

            //check whether current customer has access to a closed store
            if (await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ACCESS_CLOSED_STORE))
                return;

            //store is closed and no access, so redirect to 'StoreClosed' page
            context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            var response = new ErrorResponse();
            response.AddError(await _localizationService.GetResourceAsync("StoreClosed"));

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
            await CheckAccessClosedStoreAsync(context);
            if (context.Result == null)
                await next();
        }

        #endregion
    }

    #endregion
}