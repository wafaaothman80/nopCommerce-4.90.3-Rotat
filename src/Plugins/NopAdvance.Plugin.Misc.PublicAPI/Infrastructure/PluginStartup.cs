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

using System;
using System.IO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Plugins;
using NopAdvance.Plugin.Misc.PublicAPI.Areas.Admin.Factories;
using NopAdvance.Plugin.Misc.PublicAPI.Factories;
using NopAdvance.Plugin.Misc.PublicAPI.Filters;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure.Extensions;
using NopAdvance.Plugin.Misc.PublicAPI.Services;
using NopAdvance.Plugin.Misc.PublicAPI.Services.Debugging;

namespace NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;

public class PluginStartup : INopStartup
{
    public void Configure(IApplicationBuilder application)
    {
        application.UseCors(x => x
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

        application.UseMiddleware<JwtMiddleware>();

        application.UseDebugging();

        //exception handling
        application.UseApiExceptionHandler();

        //handle 404 errors (not found)
        application.UseMethodNotFound();

        application.UseAPISwagger();
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Enable Cors
        services.AddCors();

        // ✅ IMPORTANT: Ensure plugin controllers are discovered by MVC
        // This prevents swagger.json from being generated with empty "paths": {}
        services
            .AddControllers()
            .AddApplicationPart(typeof(PluginStartup).Assembly) // plugin assembly
            .AddControllersAsServices();

        services.AddSwaggerGen(c =>
        {
            var filePath = $"{NopPluginDefaults.PathName}/{PluginDefaults.SYSTEM_NAME}/{NopPluginDefaults.DescriptionFileName}";
            var pluginVersion = "";
            if (File.Exists(filePath))
            {
                var text = File.ReadAllText(filePath);
                pluginVersion = PluginDescriptor.GetPluginDescriptorFromText(text).Version;
            }

            c.SwaggerDoc(PluginDefaults.SWAGGER_BASE_PATH, new OpenApiInfo()
            {
                Title = $"{PluginDefaults.SWAGGER_TITLE} v{pluginVersion}",
                Version = $"{pluginVersion}",
                Description = "nopCommerce's public api by NopAdvance.",
                Contact = new OpenApiContact
                {
                    Name = "NopAdvance LLP",
                    Url = new Uri("https://nopadvance.com/contactus?utm_source=swagger&utm_medium=api&utm_campaign=public"),
                    Email = "sales@nopadvance.com"
                }
            });

            c.CustomSchemaIds(x => x.FullName?.Replace("+", "."));

           
            c.DocInclusionPredicate((docName, apiDesc) => true);

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = AuthenticationDefaults.API_KEY_NAME,
                Description = "Enter the store API key",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = AuthenticationDefaults.API_KEY_NAME.ToLower(), 
                BearerFormat = "APIkey",
                Reference = new OpenApiReference
                {
                    Id = AuthenticationDefaults.API_KEY_NAME,
                    Type = ReferenceType.SecurityScheme
                }
            };
            c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, Array.Empty<string>() }
            });

            securityScheme = new OpenApiSecurityScheme
            {
                Name = "JWT Authentication",
                Description = "Enter JWT Bearer token **_only_**",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme.ToLower(), // must be lower case
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };
            c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, Array.Empty<string>() }
            });

            // Tells swagger to pick up the output XML document file
            var fileProvider = CommonHelper.DefaultFileProvider;
            var xmlFilePath = fileProvider.Combine(
                fileProvider.MapPath($"{NopPluginDefaults.Path}/{PluginDefaults.SYSTEM_NAME}"),
                $"{PluginDefaults.SYSTEM_NAME}.xml"
            );
            c.IncludeXmlComments(xmlFilePath);
        });

        services.Configure<RazorViewEngineOptions>(option =>
        {
            option.ViewLocationExpanders.Add(new ViewLocationExpander());
        });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        // Keep existing MVC + Newtonsoft behavior
        services.AddMvc()
            .AddNewtonsoftJson(opts =>
            {
                opts.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

        services.AddSwaggerGenNewtonsoftSupport();

        //Register Services
        services.AddScoped<IWorkContext, APIWebWorkContext>();
        services.AddScoped<IAPIService, APIService>();
        services.AddScoped<IAPIDebugService, APIDebugService>();
        services.AddScoped<IPluginPaymentService, PluginPaymentService>();
        services.AddScoped<PaymentSkrillServiceManager>();
        services.AddScoped<IPublicSettingService, PublicSettingService>();

        //Factories
        services.AddScoped<IAPIModelFactory, APIModelFactory>();
        services.AddScoped<IAPICommonModelFactory, APICommonModelFactory>();
        services.AddScoped<IAPIDebugLogModelFactory, APIDebugLogModelFactory>();
        services.AddScoped<IProductReviewTotalsService, ProductReviewTotalsService>();
        services.AddScoped<IProductReviewApiService, ProductReviewApiService>();
    }

    public int Order => 1;
}