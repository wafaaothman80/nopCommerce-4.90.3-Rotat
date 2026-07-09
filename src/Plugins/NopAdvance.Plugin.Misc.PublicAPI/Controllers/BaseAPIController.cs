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
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using NopAdvance.Plugin.Core.Filters;
using NopAdvance.Plugin.Misc.PublicAPI.Filters;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;
using NopAdvance.Plugin.Misc.PublicAPI.Services;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;

[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[Produces(PluginDefaults.CONTENT_TYPE_APPLICATION_JSON)]
[Authorize]
[Route(PluginDefaults.PUBLIC_BASEURL)]
[ApiExplorerSettings(GroupName = PluginDefaults.SWAGGER_BASE_PATH)]
[CheckAPIEnabled]
[TrackCalls]
[CheckAccessPublicStore]
[CheckAccessClosedStore]
[ValidatePassword]
[Nop.Web.Framework.Mvc.Filters.SaveLastActivity]
[ApiController]
[NopAdvanceCheckLicense(PluginDefaults.SYSTEM_NAME, nameof(NopAdvanceAPISettings))]
public abstract partial class BaseAPIController : ControllerBase
{
    protected virtual async Task<int> GetApplicationIdAsync(IAPIService apiService, IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor.HttpContext.Request.Headers.ContainsKey(AuthenticationDefaults.API_KEY_NAME))
        {
            var apiKey = httpContextAccessor.HttpContext.Request.Headers[AuthenticationDefaults.API_KEY_NAME].FirstOrDefault();
            if (!string.IsNullOrEmpty(apiKey))
            {
                var application = await apiService.GetAPIApplicationByAPIKeyAsync(apiKey);
                if(application != null)
                    return application.Id;
            }
        }

        return 0;
    }

    protected virtual IActionResult BadRequest(string message = "")
    {
        return BadRequest(new List<string> { message });
    }

    protected virtual IActionResult PrepareBadRequest(ModelStateDictionary modelState)
    {
        var errors = modelState.Values.Where(v => v.Errors.Count > 0)
                .SelectMany(v => v.Errors)
                .Select(v => v.ErrorMessage)
                .ToList();

        return BadRequest(errors);
    }

    protected virtual IActionResult BadRequest(IList<string> errors = null)
    {
        var errorResponse = new ErrorResponse();
        if (errors != null)
            errorResponse.AddErrors(errors);
        return BadRequest(errorResponse);
    }

    protected virtual Dictionary<string, StringValues> ConvertToFormCollection(IDictionary<string, string> request)
    {
        var formCollection = new Dictionary<string, StringValues>();
        if (request != null)
        {
            foreach (var item in request)
            {
                formCollection.Add(item.Key, new StringValues(item.Value.Split(',')));
            }
        }
        return formCollection;
    }
}
