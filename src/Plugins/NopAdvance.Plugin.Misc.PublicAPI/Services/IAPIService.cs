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
using System.Security.Claims;
using Nop.Core;
using Nop.Core.Domain.Customers;
using NopAdvance.Plugin.Misc.PublicAPI.Domain;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;

namespace NopAdvance.Plugin.Misc.PublicAPI.Services;

public interface IAPIService
{
    /// <summary>
    /// Gets a principal from token
    /// </summary>
    /// <param name="accessToken">access token</param>
    /// <param name="validateLifeTime">validate lifetime(true/false)</param>
    /// <returns>
    /// The task result contains the ClaimsPrincipal
    /// </returns>
    (ClaimsPrincipal, string) GetPrincipalFromToken(string accessToken, bool validateLifeTime = true);

    /// <summary>
    /// Gets a token id
    /// </summary>
    /// <param name="authorization">authorization</param>
    /// <returns>
    /// The task result contains the guid
    /// </returns>
    Guid? GetTokenId(string authorization);

    #region API Application

    /// <summary>
    /// Gets an application by identifier
    /// </summary>
    /// <param name="applicationId">Application identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the application
    /// </returns>
    Task<APIApplication> GetAPIApplicationByIdAsync(int applicationId);

    /// <summary>
    /// Application name is exist or not
    /// </summary>
    /// <param name="name">Name</param>
    /// <returns></returns>
    Task<bool> ApplicationNameIsExistOrNot(string name);

    /// <summary>
    /// Inserts APIApplication
    /// </summary>
    /// <param name="apiApplication">APIApplication</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task InsertAPIApplicationAsync(APIApplication apiApplication);

    /// <summary>
    /// Updates the APIApplication
    /// </summary>
    /// <param name="apiApplication">APIApplication</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpdateAPIApplicationAsync(APIApplication apiApplication);

    /// <summary>
    /// Delete APIApplication
    /// </summary>
    /// <param name="apiApplication">APIApplication</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteAPIApplicationAsync(APIApplication apiApplication);

    /// <summary>
    /// Gets a APIApplication by APIKey 
    /// </summary>
    /// <param name="apiKey">apiKey</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the APIApplication
    /// </returns>
    Task<APIApplication> GetAPIApplicationByAPIKeyAsync(string apiKey);

    /// <summary>
    /// Gets all APIApplications
    /// </summary>
    /// <param name="apiApplicationName">apiApplicationName</param>
    /// <param name="showHidden">showHidden</param>
    /// <param name="pageIndex">pageIndex</param>
    /// <param name="pageSize">pageSize</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the APIApplications
    /// </returns>
    Task<IPagedList<APIApplication>> GetAllAPIApplicationsAsync(string apiApplicationName = null,
        bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue);

    #endregion

    #region API Refresh Token

    /// <summary>
    /// Generate tokens
    /// </summary>
    /// <param name="customer">customer</param>
    /// <param name="applicationId">applicationId</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the AuthenticationResponse
    /// </returns>
    Task<AuthenticationResponse> GenerateTokensAsync(Customer customer, int applicationId);

    /// <summary>
    /// Delete API refresh token
    /// </summary>
    /// <param name="apiRefreshToken">apiRefreshToken</param>
    Task DeleteAPIRefreshTokenAsync(APIRefreshToken apiRefreshToken);

    /// <summary>
    /// Delete expired refresh tokens
    /// </summary>
    /// <param name="includeRevoked">includeRevoked</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the expired refresh token id
    /// </returns>
    Task<int> DeleteExpiredRefreshTokensAsync(bool includeRevoked);

    /// <summary>
    /// Update API refresh token
    /// </summary>
    /// <param name="apiRefreshToken">apiRefreshToken</param>
    Task UpdateAPIRefreshTokenAsync(APIRefreshToken apiRefreshToken);

    /// <summary>
    /// Get API refresh token 
    /// </summary>
    /// <param name="applicationId">applicationId</param>
    /// <param name="customerId">customerId</param>
    /// <param name="tokenId">tokenId</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the APIRefreshToken
    /// </returns>
    Task<APIRefreshToken> GetAPIRefreshTokenAsync(int applicationId, int customerId, Guid tokenId);

    /// <summary>
    /// Get active tokens
    /// </summary>
    /// <param name="customerRoleIds">customerRoleIds</param>
    /// <param name="email">email</param>
    /// <param name="firstName">firstName</param>
    /// <param name="lastName">lastName</param>
    /// <param name="applicationId">applicationId</param>
    /// <param name="pageIndex">pageIndex</param>
    /// <param name="pageSize">pageSize</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the APIRefreshToken
    /// </returns>
    Task<IPagedList<APIRefreshToken>> GetActiveTokensAsync(int[] customerRoleIds = null, string email = null,
        string firstName = null, string lastName = null, int applicationId = 0, int storeId = 0,
        int pageIndex = 0, int pageSize = int.MaxValue);

    /// <summary>
    /// Gets an refresh by identifier
    /// </summary>
    /// <param name="tokenId">Token identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the refresh token
    /// </returns>
    Task<APIRefreshToken> GetAPIRefreshTokenByIdAsync(int tokenId);

    #endregion

    /// <summary>
    /// Get calls count
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the numebers of calls
    /// </returns>
    Task<int> GetCallsCount();

    /// <summary>
    /// Set calls count
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the numebers of calls
    /// </returns>
    Task<int> SetCallsCount();

    /// <summary>
    /// Create calls count
    /// </summary>
    Task CreateCallsCount();
}
