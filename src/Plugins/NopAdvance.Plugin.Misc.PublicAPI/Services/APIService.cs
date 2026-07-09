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
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Plugins;
using Nop.Services.Security;
using NopAdvance.Plugin.Misc.PublicAPI.Domain;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure.Caching;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;

namespace NopAdvance.Plugin.Misc.PublicAPI.Services;

public class APIService : IAPIService
{
    #region Fields

    private readonly CustomerSettings _customerSettings;
    private readonly IRepository<APIRefreshToken> _apiRefreshTokenRepository;
    private readonly IRepository<APIApplication> _apiApplicationRepository;
    private readonly IRepository<APIDebugLog> _apiDebugRepository;
    private readonly IRepository<Store> _storeRepository;
    private readonly NopAdvanceAPISettings _nopAdvancePublicRestAPISettings;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IEncryptionService _encryptionService;
    private readonly INopFileProvider _fileProvider;
    private readonly ISettingService _settingService;
    private readonly IRepository<CustomerCustomerRoleMapping> _customerCustomerRoleMappingRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    #endregion

    #region Ctor

    public APIService(CustomerSettings customerSettings,
        IRepository<APIRefreshToken> apiRefreshTokenRepository,
        IRepository<APIApplication> apiApplicationRepository,
        IStaticCacheManager staticCacheManager,
        NopAdvanceAPISettings nopAdvancePublicRestAPISettings,
        IEncryptionService encryptionService,
        INopFileProvider fileProvider,
        ISettingService settingService,
        IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository,
        IRepository<Customer> customerRepository,
        IRepository<APIDebugLog> apiDebugRepository,
        IHttpContextAccessor httpContextAccessor,
        IRepository<Store> storeRepository)
    {
        _customerSettings = customerSettings;
        _apiRefreshTokenRepository = apiRefreshTokenRepository;
        _apiApplicationRepository = apiApplicationRepository;
        _staticCacheManager = staticCacheManager;
        _nopAdvancePublicRestAPISettings = nopAdvancePublicRestAPISettings;
        _encryptionService = encryptionService;
        _fileProvider = fileProvider;
        _settingService = settingService;
        _customerCustomerRoleMappingRepository = customerCustomerRoleMappingRepository;
        _customerRepository = customerRepository;
        _apiDebugRepository = apiDebugRepository;
        _httpContextAccessor = httpContextAccessor;
        _storeRepository = storeRepository;

    }

    #endregion

    #region Utilities

    protected virtual bool IsTokenHeaderValid(string alg)
    {
        return alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase)
            || alg.Equals(SecurityAlgorithms.HmacSha384Signature, StringComparison.InvariantCultureIgnoreCase)
            || alg.Equals(SecurityAlgorithms.HmacSha512Signature, StringComparison.InvariantCultureIgnoreCase);
    }

    protected virtual string[] GetFileInfo()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("NopAdvance.Plugin.Misc.PublicAPI.p.txt");
        using var reader = new StreamReader(stream);
        return EnumerateLines(reader).ToArray();
    }

    protected virtual IEnumerable<string> EnumerateLines(TextReader reader)
    {
        string line;

        while ((line = reader.ReadLine()) != null)
            yield return line;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets a principal from token
    /// </summary>
    /// <param name="accessToken">access token</param>
    /// <param name="validateLifeTime">validate lifetime(true/false)</param>
    /// <returns>
    /// The task result contains the ClaimsPrincipal
    /// </returns>
    public virtual (ClaimsPrincipal, string) GetPrincipalFromToken(string accessToken, bool validateLifeTime = true)
    {
        try
        {
            var host = _httpContextAccessor.HttpContext.Request.Host.Value;
            var storeId = _storeRepository.Table.Where(s => s.Hosts == host).Select(s => s.Id).FirstOrDefault();
            var pluginSettings = _settingService.LoadSettingAsync<NopAdvanceAPISettings>(storeId).Result;
            var key = Encoding.ASCII.GetBytes(pluginSettings.SecretKey);

            var tokenHandler = new JwtSecurityTokenHandler();
            var claimsPrincipal = tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = validateLifeTime,
                ClockSkew = TimeSpan.Zero

            }, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtSecurityToken)
                return (null, "Invalid jwt token.");
            if (!IsTokenHeaderValid(jwtSecurityToken.Header.Alg))
                return (null, "Invalid token header.");

            return (claimsPrincipal, string.Empty);
        }
        catch (Exception ex)
        {
            string errorMessage;
            if (ex.Message.StartsWith("IDX10223"))
                errorMessage = "Access token has been expired.";
            else if (ex.Message.StartsWith("IDX10222"))
                errorMessage = "Access token is not valid yet.";
            else
                errorMessage = ex.Message;
            return (null, errorMessage);
        }
    }

    /// <summary>
    /// Gets a token id
    /// </summary>
    /// <param name="authorization">authorization</param>
    /// <returns>
    /// The task result contains the guid
    /// </returns>
    public virtual Guid? GetTokenId(string authorization)
    {
        var tokens = authorization?.Split(" ");
        if (tokens != null && tokens.Length > 1 && tokens[0] == JwtBearerDefaults.AuthenticationScheme &&
               !string.IsNullOrEmpty(tokens[1]))
        {
            var accessToken = tokens[1];
            var (principal, _) = GetPrincipalFromToken(accessToken, false);
            if (principal != null)
            {
                var tokenIdClaim = principal.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Jti);
                if (Guid.TryParse(tokenIdClaim.Value, out var tokenId))
                    return tokenId;
            }
        }
        return null;
    }

    #region API Application

    /// <summary>
    /// Gets an application by identifier
    /// </summary>
    /// <param name="applicationId">Application identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the application
    /// </returns>
    public virtual async Task<APIApplication> GetAPIApplicationByIdAsync(int applicationId)
    {
        return await _apiApplicationRepository.GetByIdAsync(applicationId, cache => default);
    }

    /// <summary>
    /// Application name is exist or not
    /// </summary>
    /// <param name="name">Name</param>
    /// <returns></returns>
    public virtual async Task<bool> ApplicationNameIsExistOrNot(string name)
    {
        return await _apiApplicationRepository.Table
                            .AnyAsync(a => a.Name == name);
    }

    /// <summary>
    /// Inserts APIApplication
    /// </summary>
    /// <param name="apiApplication">APIApplication</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task InsertAPIApplicationAsync(APIApplication apiApplication)
    {
        await _apiApplicationRepository.InsertAsync(apiApplication);
    }

    /// <summary>
    /// Updates the APIApplication
    /// </summary>
    /// <param name="apiApplication">APIApplication</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task UpdateAPIApplicationAsync(APIApplication apiApplication)
    {
        if (apiApplication == null)
            throw new ArgumentNullException(nameof(apiApplication));

        await _apiApplicationRepository.UpdateAsync(apiApplication);
    }

    /// <summary>
    /// Delete APIApplication
    /// </summary>
    /// <param name="apiApplication">APIApplication</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task DeleteAPIApplicationAsync(APIApplication apiApplication)
    {
        await _apiApplicationRepository.DeleteAsync(apiApplication);
    }

    /// <summary>
    /// Gets a APIApplication by APIKey 
    /// </summary>
    /// <param name="apiKey">apiKey</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the APIApplication
    /// </returns>
    public virtual async Task<APIApplication> GetAPIApplicationByAPIKeyAsync(string apiKey)
    {
        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CachingDefaults.APIApplicationByAPIKeyCacheKey, apiKey);
        var application = await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            var query = from a in _apiApplicationRepository.Table
                        where a.APIKey == apiKey && !a.Deleted && a.Active
                        select a;

            return await query.FirstOrDefaultAsync();
        });

        return application;
    }

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
    public virtual async Task<IPagedList<APIApplication>> GetAllAPIApplicationsAsync(string apiApplicationName = null,
        bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = _apiApplicationRepository.Table;

        if (!string.IsNullOrWhiteSpace(apiApplicationName))
            query = query.Where(c => c.Name.Contains(apiApplicationName));

        if (!showHidden)
            query = query.Where(c => !c.Deleted);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    /// <summary>
    /// Gets an refresh by identifier
    /// </summary>
    /// <param name="tokenId">Token identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the refresh token
    /// </returns>
    public virtual async Task<APIRefreshToken> GetAPIRefreshTokenByIdAsync(int tokenId)
    {
        return await _apiRefreshTokenRepository.GetByIdAsync(tokenId, cache => default);
    }

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
    public virtual async Task<AuthenticationResponse> GenerateTokensAsync(Customer customer, int applicationId)
    {
        var apiApplication = await GetAPIApplicationByIdAsync(applicationId);
        var pluginSettings = await _settingService.LoadSettingAsync<NopAdvanceAPISettings>(apiApplication.StoreId);
        var key = Encoding.ASCII.GetBytes(pluginSettings.SecretKey);
        SigningCredentials signingCredentials = null;
        switch (_nopAdvancePublicRestAPISettings.SecurityAlgorithmType)
        {
            case AlgorithmType.HmacSha256:
                signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
                break;
            case AlgorithmType.HmacSha384:
                signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha384Signature);
                break;
            case AlgorithmType.HmacSha512:
                signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature);
                break;
        }

        var accessTokenExpirationTimeSpan = new TimeSpan();
        switch (_nopAdvancePublicRestAPISettings.AccessTokenExpirationDuration)
        {
            case DurationType.Seconds:
                accessTokenExpirationTimeSpan = new TimeSpan(0, 0, 0, _nopAdvancePublicRestAPISettings.AccessTokenExpiration);
                break;
            case DurationType.Minutes:
                accessTokenExpirationTimeSpan = new TimeSpan(0, 0, _nopAdvancePublicRestAPISettings.AccessTokenExpiration, 0);
                break;
            case DurationType.Hours:
                accessTokenExpirationTimeSpan = new TimeSpan(0, _nopAdvancePublicRestAPISettings.AccessTokenExpiration, 0, 0);
                break;
            case DurationType.Days:
                accessTokenExpirationTimeSpan = new TimeSpan(_nopAdvancePublicRestAPISettings.AccessTokenExpiration, 0, 0, 0);
                break;
        }

        var createdDate = DateTime.UtcNow;
        var centuryBegin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        var exp = new TimeSpan(createdDate.Add(accessTokenExpirationTimeSpan).Ticks - centuryBegin.Ticks).TotalSeconds;
        var now = new TimeSpan(createdDate.Ticks - centuryBegin.Ticks).TotalSeconds;
        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Nbf, now.ToString(CultureInfo.InvariantCulture)),
            new (JwtRegisteredClaimNames.Exp, exp.ToString(CultureInfo.InvariantCulture)),
            //API authentication
            new (AuthenticationDefaults.CLAIMS_CUSTOMER_ID, customer.Id.ToString()),
            //Token Id
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            //External authentication
            new (ClaimTypes.NameIdentifier, customer.CustomerGuid.ToString())
        };

        //Standard
        if (_customerSettings.UsernamesEnabled)
            if (!string.IsNullOrEmpty(customer.Username))
                claims.Add(new Claim(ClaimTypes.Name, customer.Username));
            else
            if (!string.IsNullOrEmpty(customer.Email))
                claims.Add(new Claim(ClaimTypes.Email, customer.Email));

        var refreshTokenExpirationTimeSpan = new TimeSpan();
        switch (_nopAdvancePublicRestAPISettings.RefreshTokenExpirationDuration)
        {
            case DurationType.Seconds:
                refreshTokenExpirationTimeSpan = new TimeSpan(0, 0, 0, _nopAdvancePublicRestAPISettings.RefreshTokenExpiration);
                break;
            case DurationType.Minutes:
                refreshTokenExpirationTimeSpan = new TimeSpan(0, 0, _nopAdvancePublicRestAPISettings.RefreshTokenExpiration, 0);
                break;
            case DurationType.Hours:
                refreshTokenExpirationTimeSpan = new TimeSpan(0, _nopAdvancePublicRestAPISettings.RefreshTokenExpiration, 0, 0);
                break;
            case DurationType.Days:
                refreshTokenExpirationTimeSpan = new TimeSpan(_nopAdvancePublicRestAPISettings.RefreshTokenExpiration, 0, 0, 0);
                break;
        }

        var securityToken = new JwtSecurityToken(new JwtHeader(signingCredentials), new JwtPayload(claims));
        var refreshToken = PluginCommonHelper.GenerateSecretKey();

        await InsertAPIRefreshTokenAsync(new APIRefreshToken
        {
            ApplicationId = applicationId,
            CustomerId = customer.Id,
            AccessTokenId = new Guid(securityToken.Id),
            Token = refreshToken,
            IsUsed = false,
            IsRevoked = false,
            CreatedOnUtc = createdDate,
            ExpiryInUtc = createdDate.Add(refreshTokenExpirationTimeSpan)
        });

        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var result = new AuthenticationResponse
        {
            CustomerId = customer.Id,
            AccessToken = jwtTokenHandler.WriteToken(securityToken),
            RefreshToken = refreshToken
        };

        return result;
    }

    /// <summary>
    /// Insert a APIRefreshToken
    /// </summary>
    /// <param name="apiRefreshToken">APIRefreshToken</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task InsertAPIRefreshTokenAsync(APIRefreshToken apiRefreshToken)
    {
        await _apiRefreshTokenRepository.InsertAsync(apiRefreshToken);
    }

    /// <summary>
    /// Delete API refresh token
    /// </summary>
    /// <param name="apiRefreshToken">apiRefreshToken</param>
    public virtual async Task DeleteAPIRefreshTokenAsync(APIRefreshToken apiRefreshToken)
    {
        await _apiRefreshTokenRepository.DeleteAsync(apiRefreshToken);
    }

    /// <summary>
    /// Delete expired refresh tokens
    /// </summary>
    /// <param name="includeRevoked">includeRevoked</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the expired refresh token id
    /// </returns>
    public virtual async Task<int> DeleteExpiredRefreshTokensAsync(bool includeRevoked)
    {
        var tokens = await _apiRefreshTokenRepository.GetAllAsync(query => query, cache => default);
        tokens = tokens.Where(t => t.ExpiryInUtc < DateTime.UtcNow).ToList();

        await _apiRefreshTokenRepository.DeleteAsync(tokens);
        return tokens.Count;
    }

    /// <summary>
    /// Update API refresh token
    /// </summary>
    /// <param name="apiRefreshToken">apiRefreshToken</param>
    public virtual async Task UpdateAPIRefreshTokenAsync(APIRefreshToken apiRefreshToken)
    {
        await _apiRefreshTokenRepository.UpdateAsync(apiRefreshToken);
    }

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
    public virtual async Task<APIRefreshToken> GetAPIRefreshTokenAsync(int applicationId, int customerId, Guid tokenId)
    {
        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CachingDefaults.APIRefreshTokenCacheKey, applicationId,
            customerId, tokenId);
        var apiRefreshToken = await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            var query = from rt in _apiRefreshTokenRepository.Table
                        join a in _apiApplicationRepository.Table
                        on rt.ApplicationId equals a.Id
                        where rt.CustomerId == customerId && rt.AccessTokenId == tokenId
                        && !a.Deleted && a.Active
                        select rt;
            return await query.FirstOrDefaultAsync();
        });

        return apiRefreshToken;
    }

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
    public virtual async Task<IPagedList<APIRefreshToken>> GetActiveTokensAsync(int[] customerRoleIds = null, string email = null,
        string firstName = null, string lastName = null, int applicationId = 0, int storeId = 0,
        int pageIndex = 0, int pageSize = int.MaxValue)
    {
        //some databases don't support int.MaxValue
        if (pageSize == int.MaxValue)
            pageSize = int.MaxValue - 1;

        var query = _apiRefreshTokenRepository.Table;

        query = query.Where(t => !t.IsUsed && !t.IsRevoked);

        query = query
                .Join(_customerRepository.Table, x => x.CustomerId, y => y.Id,
                    (x, y) => new { Token = x, Customer = y })
                .Where(z => z.Customer.Active && !z.Customer.Deleted)
                .Select(z => z.Token);

        if (customerRoleIds != null && customerRoleIds.Length > 0)
            query = query.Join(_customerCustomerRoleMappingRepository.Table, x => x.CustomerId, y => y.CustomerId,
                    (x, y) => new { Token = x, Mapping = y })
                .Where(z => customerRoleIds.Contains(z.Mapping.CustomerRoleId))
                .Select(z => z.Token)
                .Distinct();

        if (applicationId > 0)
            query = query.Where(t => t.ApplicationId == applicationId);

        if (!string.IsNullOrWhiteSpace(email))
            query = query
                .Join(_customerRepository.Table, x => x.CustomerId, y => y.Id,
                    (x, y) => new { Token = x, Customer = y })
                .Where(z => z.Customer.Email.Contains(email))
                .Select(z => z.Token);

        if (!string.IsNullOrWhiteSpace(firstName))
            query = query
                .Join(_customerRepository.Table, x => x.CustomerId, y => y.Id,
                    (x, y) => new { Token = x, Customer = y })
                .Where(z => z.Customer.FirstName.Contains(firstName))
                .Select(z => z.Token);

        if (!string.IsNullOrWhiteSpace(lastName))
            query = query
                .Join(_customerRepository.Table, x => x.CustomerId, y => y.Id,
                    (x, y) => new { Token = x, Customer = y })
                .Where(z => z.Customer.LastName.Contains(lastName))
                .Select(z => z.Token);

        if (storeId > 0)
        {
            // Join with _apiApplicationRepository to get StoreId
            query = query.Join(_apiApplicationRepository.Table, t => t.ApplicationId, app => app.Id,
                    (t, app) => new { Token = t, StoreId = app.StoreId })
                .Where(joined => joined.StoreId == storeId)
                .Select(joined => joined.Token);
        }

        var activeTokens = query.ToList();

        query = activeTokens.Where(t => t.ExpiryInUtc > DateTime.UtcNow).AsQueryable();

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    #endregion

    /// <summary>
    /// Get calls count
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the numebers of calls
    /// </returns>
    public virtual async Task<int> GetCallsCount()
    {
        var filePath = _fileProvider.Combine(_fileProvider.MapPath($"{NopPluginDefaults.Path}/{PluginDefaults.SYSTEM_NAME}"), "Content", "pinfo.txt");
        if (_fileProvider.FileExists(filePath))
        {
            var pfinfo = GetFileInfo();
            var pinfos = _encryptionService.DecryptText(await _fileProvider.ReadAllTextAsync(filePath, Encoding.UTF8), pfinfo[0]).Split(' ');
            if (pinfos[0] == pfinfo[2] && pinfos[1] == pfinfo[3] && pinfos[2] == pfinfo[4] &&
                pinfos[4] == pfinfo[5] && int.TryParse(pinfos[3], out var cnt))
                return cnt;
        }
        return -1;
    }

    /// <summary>
    /// Set calls count
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the numebers of calls
    /// </returns>
    public virtual async Task<int> SetCallsCount()
    {
        var filePath = _fileProvider.Combine(_fileProvider.MapPath($"{NopPluginDefaults.Path}/{PluginDefaults.SYSTEM_NAME}"), "Content", "pinfo.txt");
        if (_fileProvider.FileExists(filePath))
        {
            var pfinfo = GetFileInfo();
            var pinfos = _encryptionService.DecryptText(await _fileProvider.ReadAllTextAsync(filePath, Encoding.UTF8), pfinfo[0]).Split(' ');
            if (pinfos[0] == pfinfo[2] && pinfos[1] == pfinfo[3] && pinfos[2] == pfinfo[4] &&
                pinfos[4] == pfinfo[5] && int.TryParse(pinfos[3], out var cnt))
            {
                cnt++;
                var pinfo = _encryptionService.EncryptText(pfinfo[2] + " " + pfinfo[3] + " " + pfinfo[4] + " " + cnt + " " + pfinfo[5], pfinfo[0]);
                await _fileProvider.WriteAllTextAsync(filePath, pinfo, Encoding.UTF8);
                return cnt;
            }
        }
        return -1;
    }

    /// <summary>
    /// Create calls count
    /// </summary>
    public virtual async Task CreateCallsCount()
    {
        var filePath = _fileProvider.Combine(_fileProvider.MapPath($"{NopPluginDefaults.Path}/{PluginDefaults.SYSTEM_NAME}"), "Content", "pinfo.txt");
        if (!_fileProvider.FileExists(filePath))
        {
            var pfinfo = GetFileInfo();
            var keyV = await _settingService.GetSettingByKeyAsync<bool?>(pfinfo[1], null);
            if (!keyV.HasValue)
            {
                var pinfo = _encryptionService.EncryptText(pfinfo[6], pfinfo[0]);
                await _fileProvider.WriteAllTextAsync(filePath, pinfo, Encoding.UTF8);
                await _settingService.SetSettingAsync(pfinfo[1], false);
            }
        }
    }

    #endregion
}
