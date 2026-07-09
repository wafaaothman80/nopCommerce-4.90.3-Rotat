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
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Stores;
using Nop.Web.Framework;
using NopAdvance.Plugin.Core.Domain;
using NopAdvance.Plugin.Core.Helpers;
using NopAdvance.Plugin.Core.Infrastructure;

namespace NopAdvance.Plugin.Core.Filters;

public sealed class NopAdvanceCheckLicenseAttribute : TypeFilterAttribute
{
    #region Ctor

    /// <summary>
    /// Create instance of the filter attribute
    /// </summary>
    /// <param name="systemName">Plugin's system name</param>
    /// <param name="type">Type</param>
    /// <param name="ignore">Whether to ignore the execution of filter actions</param>
    public NopAdvanceCheckLicenseAttribute(string systemName, string settingPrefix, bool ignore = false) :
        base(typeof(NopAdvanceCheckLicenseFilter))
    {
        SystemName = systemName;
        SettingPrefix = settingPrefix;
        IgnoreFilter = ignore;
        Arguments = new object[] { systemName, settingPrefix, ignore };
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the plugin's system name
    /// </summary>
    public string SystemName { get; }

    /// <summary>
    /// Gets the plugin's setting type
    /// </summary>
    public string SettingPrefix { get; }

    /// <summary>
    /// Gets a value indicating whether to ignore the execution of filter actions
    /// </summary>
    public bool IgnoreFilter { get; }

    #endregion

    #region Nested filter

    /// <summary>
    /// Represents a filter that confirms access to closed store
    /// </summary>
    private class NopAdvanceCheckLicenseFilter : IAsyncActionFilter
    {
        #region Fields

        private readonly string _systemName;
        private readonly string _settingPrefix;
        private readonly bool _ignoreFilter;
        private readonly ILicenseHelper _licenseHelper;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public NopAdvanceCheckLicenseFilter(string systemName,
        string settingPrefix,
        bool ignoreFilter,
        ILicenseHelper licenseHelper,
            ISettingService settingService,
            IStoreService storeService)
        {
            _systemName = systemName;
            _settingPrefix = settingPrefix;
            _ignoreFilter = ignoreFilter;
            _licenseHelper = licenseHelper;
            _settingService = settingService;
            _storeService = storeService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Called asynchronously before the action, after model binding is complete.
        /// </summary>
        /// <param name="context">A context for action filters</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task CheckLicenseAsync(ActionExecutingContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            //check whether this filter has been overridden for the Action
            var actionFilter = context.ActionDescriptor.FilterDescriptors
                .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Action)
                .Select(filterDescriptor => filterDescriptor.Filter)
                .OfType<NopAdvanceCheckLicenseAttribute>()
                .FirstOrDefault();

            //ignore filter (the action is available even if a store is closed)
            if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                return;

            var controllerFilter = context.ActionDescriptor.FilterDescriptors
                .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Controller)
                .Select(filterDescriptor => filterDescriptor.Filter)
                .OfType<NopAdvanceCheckLicenseAttribute>()
                .FirstOrDefault();

            var area = string.Empty;
            if (context.RouteData.Values.ContainsKey("area"))
                area = Convert.ToString(context.RouteData.Values["area"]);

            var systemName = controllerFilter?.SystemName ?? _systemName;
            var (licenseStatus, _) = await _licenseHelper.ValidateLicenseAsync(systemName);

            if (licenseStatus == LicenseStatusType.NotFound || licenseStatus == LicenseStatusType.Error ||
                licenseStatus == LicenseStatusType.Expired)
            {
                var key = (controllerFilter?.SettingPrefix ?? _settingPrefix).ToLowerInvariant() + "." + CorePluginDefaults.ENABLED_KEY_SETTING;

                var setting = await _settingService.GetSettingAsync(key);
                if (setting != null && !setting.Value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                {
                    setting.Value = "False";
                    await _settingService.UpdateSettingAsync(setting);
                }
                foreach (var store in await _storeService.GetAllStoresAsync())
                {
                    var storeSetting = await _settingService.GetSettingAsync(key, store.Id);
                    if (storeSetting != null)
                        await _settingService.DeleteSettingAsync(storeSetting);
                }
                if (area == AreaNames.ADMIN)
                    context.Result = new RedirectToActionResult("LicenseMessage", CorePluginDefaults.LICENSE_CONTROLLER_NAME, new { systemName, licenseStatusId = (int)licenseStatus, area = AreaNames.ADMIN });
                else if (string.IsNullOrEmpty(area))
                    context.Result = new RedirectToRouteResult("PageNotFound", null);
            }
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
            await CheckLicenseAsync(context);
            if (context.Result == null)
                await next();
        }

        #endregion
    }

    #endregion
}
