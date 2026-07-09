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
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopAdvance.Plugin.Misc.PublicAPI.Domain;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Admin;
using NopAdvance.Plugin.Misc.PublicAPI.Services;
using NopAdvance.Plugin.Misc.PublicAPI.Services.Debugging;

namespace NopAdvance.Plugin.Misc.PublicAPI.Areas.Admin.Factories;

public partial class APIDebugLogModelFactory : IAPIDebugLogModelFactory
{
    #region Fields

    private readonly IAPIDebugService _apiDebugService;
    private readonly IStoreService _storeService;
    private readonly ICustomerService _customerService;
    private readonly IBaseAdminModelFactory _baseAdminModelFactory;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IHtmlFormatter _htmlFormatter;
    private readonly ILocalizationService _localizationService;
    private readonly IAPIService _apiService;

    #endregion

    #region Ctor

    public APIDebugLogModelFactory(IAPIDebugService apiDebugService,
        IStoreService storeService,
        ICustomerService customerService,
        IBaseAdminModelFactory baseAdminModelFactory,
        IDateTimeHelper dateTimeHelper,
        IHtmlFormatter htmlFormatter,
        ILocalizationService localizationService,
        IAPIService aPIService)
    {
        _apiDebugService = apiDebugService;
        _storeService = storeService;
        _customerService = customerService;
        _baseAdminModelFactory = baseAdminModelFactory;
        _dateTimeHelper = dateTimeHelper;
        _htmlFormatter = htmlFormatter;
        _localizationService = localizationService; 
        _apiService = aPIService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare paged debug log list model
    /// </summary>
    /// <param name="searchModel">Debug log search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the debug log list model
    /// </returns>
    public async Task<APIDebugLogListModel> PrepareDebugLogListModelAsync(APIDebugLogSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var createdOnFromValue = !searchModel.CreatedOnFrom.HasValue ? null
        : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnFrom.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
        var createdToFromValue = !searchModel.CreatedOnTo.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnTo.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

        //get debug logs
        var debugLogs = await _apiDebugService.GetAllAPIDebugLogsAsync(
            storeId: searchModel.StoreId,
            applicationId: searchModel.SearchApplicationId,
            dateFrom: createdOnFromValue,
            dateTo: createdToFromValue,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        //prepare grid model
        var model = await new APIDebugLogListModel().PrepareToGridAsync(searchModel, debugLogs, () =>
        {
            return debugLogs.SelectAwait(async debugLog =>
            {
                var customer = await _customerService.GetCustomerByIdAsync(debugLog.CustomerId.HasValue ? debugLog.CustomerId.Value : 0);
                var store = await _storeService.GetStoreByIdAsync(debugLog.StoreId);
                var customerName = customer != null ? await _customerService.GetCustomerFullNameAsync(customer) : string.Empty;

                var debugLogModel = new APIDebugLogModel
                {
                    Id = debugLog.Id,
                    Customer = !string.IsNullOrEmpty(customerName) ? customerName : await _localizationService.GetResourceAsync("Admin.Customers.Guest"),
                    Store = store != null ? store.Name : string.Empty,
                    Path = debugLog.Path,
                    CreatedOnUtc = debugLog.CreatedOnUtc
                };

                return debugLogModel;
            });
        });

        return model;
    }

    /// <summary>
    /// Prepare debug log search model
    /// </summary>
    /// <param name="searchModel">Debug log search model</param>
    /// <returns>Debug log  search model</returns>
    public async Task<APIDebugLogSearchModel> PrepareDebugLogSearchModel(APIDebugLogSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //available stores
        await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores, true);

        var applications = await _apiService.GetAllAPIApplicationsAsync();

        searchModel.AvailableApplications.Add(new SelectListItem
        {
            Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
            Value = "0"
        });

        foreach (var application in applications)
            searchModel.AvailableApplications.Add(new SelectListItem
            {
                Text = application.Name,
                Value = application.Id.ToString(),
                Selected = searchModel.SearchApplicationId == application.Id
            });

        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare debug log model
    /// </summary>
    /// <param name="model">Debug log</param>
    /// <param name="apiDebug">API debug</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the debug log model
    /// </returns>
    public async Task<APIDebugLogModel> PrepareDebugLogModelAsync(APIDebugLogModel model, APIDebugLog apiDebug)
    {
        if (apiDebug != null)
            if (model == null)
            {
                var customer = await _customerService.GetCustomerByIdAsync(apiDebug.CustomerId.HasValue ? apiDebug.CustomerId.Value : 0);
                var store = await _storeService.GetStoreByIdAsync(apiDebug.StoreId);
                model = new APIDebugLogModel
                {
                    Id = apiDebug.Id,
                    Customer = customer != null ? await _customerService.GetCustomerFullNameAsync(customer) : string.Empty,
                    Store = store != null ? store.Name : string.Empty,
                    Path = apiDebug.Path,
                    CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(apiDebug.CreatedOnUtc, DateTimeKind.Utc),
                    StatusCode = apiDebug.StatusCode,
                    Method = apiDebug.Method,
                    Headers = apiDebug.Headers,
                    RequestBody = _htmlFormatter.FormatText(apiDebug.RequestBody, false, true, false, false, false, false),
                    QueryString = apiDebug.QueryString,
                    ResponseBody = _htmlFormatter.FormatText(apiDebug.ResponseBody, false, true, false, false, false, false),
                    ResponseTime = apiDebug.ResponseTime,
                    ApplicationName = (await _apiService.GetAPIApplicationByAPIKeyAsync(ExtractApiKeyFromHeaders(apiDebug.Headers))).Name
                };
            }
        return model;
    }

    private string ExtractApiKeyFromHeaders(string headers)
    {
        const string apiKeyHeader = "x-api-key:";
        int index = headers.IndexOf(apiKeyHeader, 44, StringComparison.OrdinalIgnoreCase);
        if (index != -1)
        {
            int startIndex = index + apiKeyHeader.Length;
            int endIndex = headers.IndexOf("\r\n", startIndex);
            if (endIndex == -1)
                endIndex = headers.Length;

            // Extract the API key substring
            string apiKeySubstring = headers.Substring(startIndex, endIndex - startIndex);

            // Trim any leading or trailing whitespace characters
            return apiKeySubstring.Trim();
        }

        return null; // API key not found
    }

    #endregion
}
