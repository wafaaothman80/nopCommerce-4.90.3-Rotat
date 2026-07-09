using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.Core.Infrastructure;

public class PluginNopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNopStationServices(NopStationCoreDefaults.SystemName, true);

        services.AddScoped<INopStationContext, NopStationContext>();
        services.AddScoped<INopStationCustomerService, NopStationCustomerService>();
        services.AddHostedService<StartupEventHostedService>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 11;
}
