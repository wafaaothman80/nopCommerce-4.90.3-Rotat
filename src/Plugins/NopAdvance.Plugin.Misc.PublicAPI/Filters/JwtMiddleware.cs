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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Services.Customers;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Services;
using System.IdentityModel.Tokens.Jwt;

namespace NopAdvance.Plugin.Misc.PublicAPI.Filters;

public class JwtMiddleware
{
    #region Fields

    private readonly RequestDelegate _next;
    private readonly IAPIService _apiService;
    private readonly ICustomerService _customerService;

    #endregion

    #region Ctor

    public JwtMiddleware(RequestDelegate next,
        IAPIService apiService,
        ICustomerService customerService)
    {
        _next = next;
        _apiService = apiService; ;
        _customerService = customerService;
    }

    #endregion

    #region Methods

    public async Task Invoke(HttpContext context, IWorkContext workContext)
    {
        if (context.Request.Path.StartsWithSegments(new PathString(PluginDefaults.PUBLIC_BASEURL_PREFIX)))
        {
            var tokens = context.Request.Headers[AuthenticationDefaults.AUTHORIZATION_KEY_NAME].FirstOrDefault()?.Split(" ");
            if (tokens != null && tokens.Length > 1 && tokens[0] == JwtBearerDefaults.AuthenticationScheme &&
               !string.IsNullOrEmpty(tokens[1]))
            {
                var accessToken = tokens[1];
                var (principal, error) = _apiService.GetPrincipalFromToken(accessToken);
                if (principal != null)
                {
                    var tokenIdClaim = principal.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Jti);
                    if (tokenIdClaim != null && Guid.TryParse(tokenIdClaim.Value, out var tokenId))
                    {
                        var customerId = int.Parse(principal.Claims.First(x => x.Type == AuthenticationDefaults.CLAIMS_CUSTOMER_ID).Value);
                        var customer = await _customerService.GetCustomerByIdAsync(customerId);
                        if (customer != null && !customer.Deleted && customer.Active && !customer.RequireReLogin)
                        {
                            var apiKey = context.Request.Headers[AuthenticationDefaults.API_KEY_NAME].FirstOrDefault();
                            var application = await _apiService.GetAPIApplicationByAPIKeyAsync(apiKey);
                            if (application != null)
                            {
                                var refreshToken = await _apiService.GetAPIRefreshTokenAsync(application.Id,
                                    workContext.OriginalCustomerIfImpersonated?.Id ?? customer.Id, tokenId);
                                if (refreshToken != null && !refreshToken.IsUsed && !refreshToken.IsRevoked)
                                {
                                    await workContext.SetCurrentCustomerAsync(customer);
                                    context.Items[AuthenticationDefaults.CURRENT_API_CUSTOMER] = customer;
                                }
                            }
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(error))
                    context.Items[AuthenticationDefaults.TOKEN_ERROR_MESSAGE] = error;
            }
        }
        await _next(context);
    }

    #endregion
}
