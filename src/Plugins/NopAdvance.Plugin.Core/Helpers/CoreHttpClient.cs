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
using System.Net;
using System.Text;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Nop.Core;
using NopAdvance.Plugin.Core.Infrastructure;
using NopAdvance.Plugin.Core.Models;

namespace NopAdvance.Plugin.Core.Helpers;

public class CoreHttpClient
{
    #region Fields

    private readonly HttpClient _httpClient;
    private readonly IWebHelper _webHelper;
    private readonly IGenericHelper _genericHelper;

    #endregion

    #region Ctor

    public CoreHttpClient(HttpClient httpClient,
    IWebHelper webHelper,
        IGenericHelper genericHelper)
    {
        //configure client
        httpClient.BaseAddress = new Uri("https://store.nopadvance.com/");
        httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, $"NopAdvance-Core-Plugin-{CoreDefaults.CORE_PLUGIN_VERSION}");
        httpClient.DefaultRequestHeaders.Add("x-api-key", "w/pzH4tLlXHk2SmxRNJA5ZTyb6sOlzE9P7AZlBDS+dA=");

        _httpClient = httpClient;
        _webHelper = webHelper;
        _genericHelper = genericHelper;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Check whether the site is available
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the asynchronous task whose result determines that request is completed
    /// </returns>
    public async Task PingAsync()
    {
        await _httpClient.GetStringAsync("/");
    }

    public async Task<(bool, string)> RegisterLicenseAsync(string licenseKey)
    {
        try
        {
            var model = new RegisterLicenseDto
            {
                LicenseKey = new Guid(licenseKey),
                StoreUrl = _webHelper.GetStoreLocation()
            };

            var url = "api/registerlicense";

            var requestContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, requestContent);
            var content = await response.Content.ReadAsStringAsync();
            return (response.StatusCode == HttpStatusCode.OK, content);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool, string)> DeRegisterLicenseAsync(string licenseKey)
    {
        try
        {
            var url = "api/deregisterlicense/";

            var licenseInfo = new
            {
                licenseKey
            };

            var data = JsonConvert.SerializeObject(licenseInfo);

            var requestContent = new StringContent(data, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, requestContent);

            var content = await response.Content.ReadAsStringAsync();
            return (response.StatusCode == HttpStatusCode.OK, content);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool, string)> InstallPluginAsync(string systemName)
    {
        try
        {
            var pluginDescriptor = _genericHelper.GetPluginDescriptor(systemName);

            var model = new InstallPluginDto
            {
                SystemName = systemName,
                PluginVersion = pluginDescriptor.Version,
                StoreUrl = _webHelper.GetStoreLocation(),
                NopVersion = NopVersion.FULL_VERSION,
                IpAddress = _webHelper.GetCurrentIpAddress()
            };

            var url = "api/installplugin";

            var requestContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, requestContent);
            var content = await response.Content.ReadAsStringAsync();
            return (response.StatusCode == HttpStatusCode.OK, content);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    #endregion
}
