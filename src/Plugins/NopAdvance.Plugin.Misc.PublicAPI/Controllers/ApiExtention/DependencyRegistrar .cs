// File: Plugins/NopAdvance.Plugin.Misc.PublicAPI/Infrastructure/DependencyRegistrar.cs
using System;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;

using static NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention.ApiExtentionController;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention
{

  


    public class DependencyRegistrar : INopStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
          
            
            services.AddScoped<IAppleAuthenticationService, AppleAuthenticationService>();
          
        }

      



        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application)
        {
        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order => 3000;
    }





}
