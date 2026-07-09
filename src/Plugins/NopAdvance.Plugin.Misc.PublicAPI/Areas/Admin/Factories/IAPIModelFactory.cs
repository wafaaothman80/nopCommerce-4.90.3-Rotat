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
using NopAdvance.Plugin.Misc.PublicAPI.Models.Admin;

namespace NopAdvance.Plugin.Misc.PublicAPI.Areas.Admin.Factories;

public partial interface IAPIModelFactory
{
    /// <summary>
    /// Prepare application search model
    /// </summary>
    /// <param name="searchModel">Application search model</param>
    /// <returns>Application search model</returns>
    Task<APIApplicationSearchModel> PrepareAPIApplicationSearchModelAsync(APIApplicationSearchModel searchModel);

    /// <summary>
    /// Prepare paged application list model
    /// </summary>
    /// <param name="searchModel">Application search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the application list model
    /// </returns>
    Task<APIApplicationListModel> PrepareAPIApplicationListModelAsync(APIApplicationSearchModel searchModel);

    /// <summary>
    /// Prepare active tokens search model
    /// </summary>
    /// <param name="searchModel">RefreshTokenSearchModel</param>
    /// <returns>Refresh token search model</returns>
    Task<RefreshTokenSearchModel> PrepareRefreshTokenSearchModelAsync(RefreshTokenSearchModel searchModel);

    /// <summary>
    /// Prepare refresh token list model
    /// </summary>
    /// <param name="searchModel">Refresh token search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the refresh token list model
    /// </returns>
    Task<RefreshTokenListModel> PrepareRefreshTokenListModelAsync(RefreshTokenSearchModel searchModel);
}
