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
using Microsoft.IO;
using Nop.Core;
using Nop.Services.Configuration;
using NopAdvance.Plugin.Misc.PublicAPI.Domain;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Admin;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;
using System.Diagnostics;
using System.Text;

namespace NopAdvance.Plugin.Misc.PublicAPI.Services.Debugging;

public class DebuggingMiddleware
{
    #region Fields

    private readonly RequestDelegate _next;
    private readonly IAPIDebugService _apiDebugService;
    private readonly IWorkContext _workContext;
    private readonly IAPIService _apiService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

    #endregion

    #region Ctor

    public DebuggingMiddleware(RequestDelegate next,
        IAPIDebugService apiDebugService,
        IWorkContext workContext,
        IAPIService apiService,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _next = next;
        _apiDebugService = apiDebugService;
        _workContext = workContext;
        _apiService = apiService;
        _settingService = settingService;
        _storeContext = storeContext;
        _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
    }

    #endregion

    #region Utilities

    private static string ReadStreamInChunks(Stream stream)
    {
        const int readChunkBufferLength = 4096;
        stream.Seek(0, SeekOrigin.Begin);
        using var textWriter = new StringWriter();
        using var reader = new StreamReader(stream);
        var readChunk = new char[readChunkBufferLength];
        int readChunkLength;
        do
        {
            readChunkLength = reader.ReadBlock(readChunk,
                                               0,
                                               readChunkBufferLength);
            textWriter.Write(readChunk, 0, readChunkLength);
        } while (readChunkLength > 0);
        return textWriter.ToString();
    }

    private async Task<RequestLog> GetRequestAsync(HttpContext context)
    {
        context.Request.EnableBuffering();
        await using var requestStream = _recyclableMemoryStreamManager.GetStream();
        await context.Request.Body.CopyToAsync(requestStream);

        var headers = new StringBuilder();
        foreach (var header in context.Request.Headers)
            headers.AppendLine($"{header.Key}:{header.Value}");

        context.Request.Body.Position = 0;

        return new RequestLog
        {
            Method = context.Request.Method,
            Headers = headers.ToString(),
            Body = ReadStreamInChunks(requestStream),
            QueryString = context.Request.QueryString.ToString(),
            Path = context.Request.Path
        };
    }

    private async Task<ResponseLog> GetResponseAsync(HttpContext context, Stopwatch stopWatch)
    {
        var originalBodyStream = context.Response.Body;
        await using var responseBody = _recyclableMemoryStreamManager.GetStream();
        context.Response.Body = responseBody;
        await _next(context);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var text = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        await responseBody.CopyToAsync(originalBodyStream);

        return new ResponseLog
        {
            StatusCode = context.Response.StatusCode,
            Body = text,
            ResponseTime = stopWatch.ElapsedMilliseconds
        };
    }

    #endregion

    #region Methods

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments(new PathString("/api")))
        {
            var apiKey = context.Request.Headers[AuthenticationDefaults.API_KEY_NAME].FirstOrDefault();
            if (apiKey != null)
            {
                var application = await _apiService.GetAPIApplicationByAPIKeyAsync(apiKey);
                if (application != null)
                {
                    var key = $"{nameof(NopAdvanceAPISettings)}.{nameof(NopAdvanceAPISettings.IsDebuggingEnabled)}";
                    var isDebugEnabled = await _settingService.GetSettingByKeyAsync<bool>(key, storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                        loadSharedValueIfNotFound: true);

                    if (isDebugEnabled)
                    {
                        var stopWatch = Stopwatch.StartNew();
                        var request = await GetRequestAsync(context);
                        var response = await GetResponseAsync(context, stopWatch);

                        await _apiDebugService.InsertDebug(new APIDebugLog
                        {
                            CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                            StatusCode = response.StatusCode,
                            Method = request.Method,
                            Headers = request.Headers,
                            RequestBody = request.Body,
                            QueryString = request.QueryString,
                            ResponseBody = response.Body,
                            ResponseTime = stopWatch.ElapsedMilliseconds,
                            Path = request.Path,
                            StoreId = application.StoreId,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                        return;
                    }
                }
            }
        }
        await _next(context);
    }

    #endregion
}
