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
using NopAdvance.Plugin.Misc.PublicAPI.Domain;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Admin;

namespace NopAdvance.Plugin.Misc.PublicAPI.Areas.Admin.Factories;

public partial interface IAPIDebugLogModelFactory
{
    /// <summary>
    /// Prepare debug log search model
    /// </summary>
    /// <param name="searchModel">Debug log search model</param>
    /// <returns>Debug log  search model</returns>
    Task<APIDebugLogSearchModel> PrepareDebugLogSearchModel(APIDebugLogSearchModel searchModel);

    /// <summary>
    /// Prepare paged debug log list model
    /// </summary>
    /// <param name="searchModel">Debug log search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the debug log list model
    /// </returns>
    Task<APIDebugLogListModel> PrepareDebugLogListModelAsync(APIDebugLogSearchModel searchModel);

    /// <summary>
    /// Prepare debug log model
    /// </summary>
    /// <param name="model">Debug log</param>
    /// <param name="apiDebug">API debug</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the debug log model
    /// </returns>
    Task<APIDebugLogModel> PrepareDebugLogModelAsync(APIDebugLogModel model, APIDebugLog apiDebug);
}
