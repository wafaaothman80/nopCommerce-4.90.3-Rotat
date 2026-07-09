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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;
using NopAdvance.Plugin.Misc.PublicAPI.Services.Debugging;
using System.Runtime.ExceptionServices;

namespace NopAdvance.Plugin.Misc.PublicAPI.Infrastructure.Extensions;

public static class ApplicationBuilderExtensions
{
    public static void UseAPISwagger(this IApplicationBuilder application)
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        var pluginSettings = EngineContext.Current.Resolve<NopAdvanceAPISettings>();
        if (!pluginSettings.Enabled || !pluginSettings.IsSwaggerEnabled)
            return;

        application.UseSwagger(options =>
        {
            options.RouteTemplate = PluginDefaults.ROUTE_PREFIX + "/{documentName}/swagger.json";
        });

        application.UseSwaggerUI(options =>
        {
            options.DocumentTitle = PluginDefaults.SWAGGER_TITLE;
            var cssFilePath = $"/{NopPluginDefaults.PathName}/{PluginDefaults.SYSTEM_NAME}/Content/css/swagger-ui.css";
            options.InjectStylesheet(cssFilePath);

            var jsFilePath = $"/{NopPluginDefaults.PathName}/{PluginDefaults.SYSTEM_NAME}/Content/js/swagger-ui.js";
            options.InjectJavascript(jsFilePath);

            options.DefaultModelsExpandDepth(-1);

            options.RoutePrefix = PluginDefaults.ROUTE_PREFIX;
            options.SwaggerEndpoint($"{PluginDefaults.SWAGGER_BASE_PATH}/swagger.json", PluginDefaults.SWAGGER_TITLE);
        });
    }

    public static void UseDebugging(this IApplicationBuilder application)
    {
        var pluginSettings = EngineContext.Current.Resolve<NopAdvanceAPISettings>();
        if (!pluginSettings.Enabled)
            return;

        application.UseMiddleware<DebuggingMiddleware>();
    }

    /// <summary>
    /// Adds a special handler that checks for responses with the 404 status code that do not have a body
    /// </summary>
    /// <param name="application">Builder for configuring an application's request pipeline</param>
    public static void UseMethodNotFound(this IApplicationBuilder application)
    {
        var pluginSettings = EngineContext.Current.Resolve<NopAdvanceAPISettings>();
        if (!pluginSettings.Enabled)
            return;

        application.UseStatusCodePages(async context =>
        {
            //handle 404 Not Found
            if (context.HttpContext.Request.Path.StartsWithSegments(new PathString("/api")) &&
                context.HttpContext.Response.StatusCode == StatusCodes.Status404NotFound)
            {
                var webHelper = EngineContext.Current.Resolve<IWebHelper>();
                if (!webHelper.IsStaticResource())
                {
                    //get original path
                    var originalPath = context.HttpContext.Request.Path;

                    if (DataSettingsManager.IsDatabaseInstalled())
                    {
                        var commonSettings = EngineContext.Current.Resolve<CommonSettings>();

                        if (commonSettings.Log404Errors)
                        {
                            var logger = EngineContext.Current.Resolve<ILogger>();
                            var workContext = EngineContext.Current.Resolve<IWorkContext>();

                            await logger.ErrorAsync($"Error 404. The requested page ({originalPath}) was not found",
                                customer: await workContext.GetCurrentCustomerAsync());
                        }
                    }

                    var response = new ErrorResponse();
                    response.AddError($"The requested page ({originalPath}) was not found");
                    context.HttpContext.Response.ContentType = Nop.Core.MimeTypes.ApplicationJson;
                    await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(response));

                }
            }
        });
    }

    /// <summary>
    /// Add exception handling
    /// </summary>
    /// <param name="application">Builder for configuring an application's request pipeline</param>
    public static void UseApiExceptionHandler(this IApplicationBuilder application)
    {
        var pluginSettings = EngineContext.Current.Resolve<NopAdvanceAPISettings>();
        if (!pluginSettings.Enabled)
            return;

        application.UseExceptionHandler(handler =>
        {
            handler.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                if (exception == null)
                    return;

                if (!context.Request.Path.StartsWithSegments(new PathString("/api")))
                {
                    ExceptionDispatchInfo.Throw(exception);
                    return;
                }
                
                try
                {
                    //check whether database is installed
                    if (DataSettingsManager.IsDatabaseInstalled())
                    {
                        //get current customer
                        var currentCustomer = await EngineContext.Current.Resolve<IWorkContext>().GetCurrentCustomerAsync();

                        //log error
                        await EngineContext.Current.Resolve<ILogger>().ErrorAsync(exception.Message, exception, currentCustomer);
                    }
                }
                finally
                {
                    var response = new ErrorResponse();
                    response.AddError(exception.Message);
                    response.AddError(exception.ToString());
                    context.Response.ContentType = Nop.Core.MimeTypes.ApplicationJson;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
                }
            });
        });
    }
}
