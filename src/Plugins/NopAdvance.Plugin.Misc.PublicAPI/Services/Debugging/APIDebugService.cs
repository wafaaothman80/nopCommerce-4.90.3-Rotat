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
using Nop.Core;
using Nop.Data;
using NopAdvance.Plugin.Misc.PublicAPI.Domain;

namespace NopAdvance.Plugin.Misc.PublicAPI.Services.Debugging;

public class APIDebugService : IAPIDebugService
{
    #region Fields

    private readonly IRepository<APIDebugLog> _apiDebugRepository;
    private readonly IRepository<APIApplication> _apiApplicationRepository;
    private readonly IRepository<APIRefreshToken> _apiRefreshTokenRepository;

    #endregion

    #region Ctor

    public APIDebugService(IRepository<APIDebugLog> apiDebugRepository,
        IRepository<APIApplication> apiApplicationRepository,
        IRepository<APIRefreshToken> apiRefreshTokenRepository)
    {
        _apiDebugRepository = apiDebugRepository;
        _apiApplicationRepository = apiApplicationRepository;
        _apiRefreshTokenRepository = apiRefreshTokenRepository;
    }

    #endregion

    #region Methods

    public virtual async Task InsertDebug(APIDebugLog apiDebug)
    {
        await _apiDebugRepository.InsertAsync(apiDebug, false);
    }

    /// <summary>
    /// Get all api debug log
    /// </summary>
    /// <param name="storeId">Store identifier; pass 0 to load all records</param>
    /// <param name="dateFrom">Filter by created date; null if you want to get all records</param>
    /// <param name="dateTo">Filter by created date; null if you want to get all records</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the API debug logs
    /// </returns>
    public virtual async Task<IPagedList<APIDebugLog>> GetAllAPIDebugLogsAsync(int storeId = 0, int applicationId = 0, DateTime? dateFrom = null, DateTime? dateTo = null,
        int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _apiDebugRepository.GetAllPagedAsync(query =>
        {
            if (dateFrom.HasValue)
                query = query.Where(d => dateFrom.Value <= d.CreatedOnUtc);

            if (dateTo.HasValue)
                query = query.Where(d => dateTo.Value >= d.CreatedOnUtc);

            if (storeId > 0)
                query = query.Where(d => storeId == d.StoreId);

            if (applicationId > 0)
            {
                query = from debugLog in _apiDebugRepository.Table
                        join application in _apiApplicationRepository.Table on debugLog.StoreId equals application.StoreId
                        join reference in _apiRefreshTokenRepository.Table on applicationId equals reference.ApplicationId
                        where application.Id == applicationId
                        select debugLog;
            }

            query = query.OrderByDescending(d => d.CreatedOnUtc);

            return Task.FromResult(query);
        }, pageIndex, pageSize);
    }

    /// <summary>
    /// Gets a api debug log
    /// </summary>
    /// <param name="debugLogId">Debug log identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the Debug log
    /// </returns>
    public virtual async Task<APIDebugLog> GetAPIDebugLogByDebugIdAsync(int debugLogId)
    {
        return await _apiDebugRepository.GetByIdAsync(debugLogId, cache => default);
    }

    /// <summary>
    /// Clears a debug log
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task ClearDebugLogAsync()
    {
        await _apiDebugRepository.TruncateAsync();
    }

    /// <summary>
    /// Get debug log items by identifiers
    /// </summary>
    /// <param name="debugLogIds">Debug log item identifiers</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the debug log items
    /// </returns>
    public virtual async Task<IList<APIDebugLog>> GetAPIDebugLogByIdsAsync(int[] debugLogIds)
    {
        return await _apiDebugRepository.GetByIdsAsync(debugLogIds);
    }

    /// <summary>
    /// Deletes a debug log item
    /// </summary>
    /// <param name="debugLog">Debug log item</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task DeleteAPIDebugLogAsync(APIDebugLog debugLog)
    {
        if (debugLog == null)
            throw new ArgumentNullException(nameof(debugLog));

        await _apiDebugRepository.DeleteAsync(debugLog, false);
    }

    /// <summary>
    /// Deletes a debug log items
    /// </summary>
    /// <param name="debugLogs">Debug log items</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task DeleteAPIDebugLogsAsync(IList<APIDebugLog> debugLogs)
    {
        await _apiDebugRepository.DeleteAsync(debugLogs, false);
    }

    #endregion
}
