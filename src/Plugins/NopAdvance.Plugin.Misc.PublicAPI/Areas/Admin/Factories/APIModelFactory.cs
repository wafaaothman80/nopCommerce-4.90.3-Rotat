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
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Admin;
using NopAdvance.Plugin.Misc.PublicAPI.Services;

namespace NopAdvance.Plugin.Misc.PublicAPI.Areas.Admin.Factories;

public partial class APIModelFactory : IAPIModelFactory
{
    #region Fields

    private readonly IAPIService _apiService;
    private readonly IStoreService _storeService;
    private readonly ICustomerService _customerService;
    private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
    private readonly ILocalizationService _localizationService;
    private readonly IBaseAdminModelFactory _baseAdminModelFactory;

    #endregion

    #region Ctor

    public APIModelFactory(IAPIService apiService,
        IStoreService storeService,
        ICustomerService customerService,
        IAclSupportedModelFactory aclSupportedModelFactory,
        IBaseAdminModelFactory baseAdminModelFactory,
        ILocalizationService localizationService)
    {
        _apiService = apiService;
        _storeService = storeService;
        _customerService = customerService;
        _aclSupportedModelFactory = aclSupportedModelFactory;
        _localizationService = localizationService;
        _baseAdminModelFactory = baseAdminModelFactory;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare application search model
    /// </summary>
    /// <param name="searchModel">Application search model</param>
    /// <returns>Application search model</returns>
    public async Task<APIApplicationSearchModel> PrepareAPIApplicationSearchModelAsync(APIApplicationSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        foreach (var store in await _storeService.GetAllStoresAsync())
            searchModel.AddApplication.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });

        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare paged application list model
    /// </summary>
    /// <param name="searchModel">Application search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the application list model
    /// </returns>
    public async Task<APIApplicationListModel> PrepareAPIApplicationListModelAsync(APIApplicationSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));
        //get applications
        var applications = await _apiService.GetAllAPIApplicationsAsync(searchModel.SearchApplicationName,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare grid model
        var model = await new APIApplicationListModel().PrepareToGridAsync(searchModel, applications, () =>
        {
            return applications.SelectAwait(async application =>
            {
                //fill in model values from the entity
                var apiApplicationModel = new APIApplicationModel
                {
                    Id = application.Id,
                    Name = application.Name,
                    APIKey = application.APIKey,
                    Store = (await _storeService.GetStoreByIdAsync(application.StoreId))?.Name ?? "Deleted",
                    Active = application.Active
                };

                return apiApplicationModel;
            });
        });

        return model;
    }

    /// <summary>
    /// Prepare active tokens search model
    /// </summary>
    /// <param name="searchModel">RefreshTokenSearchModel</param>
    /// <returns>Refresh token search model</returns>
    public async Task<RefreshTokenSearchModel> PrepareRefreshTokenSearchModelAsync(RefreshTokenSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //search registered customers by default
        var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
        if (registeredRole != null)
            searchModel.SelectedCustomerRoleIds.Add(registeredRole.Id);

        //prepare available customer roles
        await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(searchModel);

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

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare refresh token list model
    /// </summary>
    /// <param name="searchModel">Refresh token search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the refresh token list model
    /// </returns>
    public async Task<RefreshTokenListModel> PrepareRefreshTokenListModelAsync(RefreshTokenSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var activeTokens = await _apiService.GetActiveTokensAsync(searchModel.SelectedCustomerRoleIds.ToArray(),
            searchModel.SearchEmail, searchModel.SearchFirstName, searchModel.SearchLastName, searchModel.SearchApplicationId,searchModel.StoreId,
            searchModel.Page - 1, searchModel.PageSize);

        //prepare list model
        var model = await new RefreshTokenListModel().PrepareToGridAsync(searchModel, activeTokens, () =>
        {
            return activeTokens.SelectAwait(async token =>
            {
                var customer = await _customerService.GetCustomerByIdAsync(token.CustomerId);
                var application = await _apiService.GetAPIApplicationByIdAsync(token.ApplicationId);

                var refreshTokenModel = new RefreshTokenModel
                {
                    Id = token.Id,
                    Email = await _customerService.IsRegisteredAsync(customer)
                    ? customer.Email
                    : await _localizationService.GetResourceAsync("Admin.Customers.Guest"),
                    FullName = await _customerService.GetCustomerFullNameAsync(customer),
                    CustomerRoleNames = string.Join(", ",
                    (await _customerService.GetCustomerRolesAsync(customer)).Select(role => role.Name)),
                    Application = application?.Name ?? "Deleted",
                    Store = (await _storeService.GetStoreByIdAsync(application.StoreId)).Name
                };

                return refreshTokenModel;
            });
        });

        return model;
    }

    #endregion
}
