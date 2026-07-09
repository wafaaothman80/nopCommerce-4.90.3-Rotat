using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.CloudflareImages.Services;
using Nop.Services.Configuration;
using Nop.Services.Media;
using Nop.Services.ScheduleTasks;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace Nop.Plugin.Misc.CloudflareImages.Infrastructure;

/// <summary>
/// Represents the object for the configuring services on application startup
/// </summary>
public class PluginNopStartup : INopStartup
{
    /// <summary>
    /// Add and configure any of the middleware
    /// </summary>
    /// <param name="services">Collection of service descriptors</param>
    /// <param name="configuration">Configuration of the application</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<CloudflareImagesHttpClient>().WithProxy();
        services.AddTransient<CloudflareThumbService>();
        services.AddTransient<ThumbService>();
        services.AddTransient<IThumbService>(provider =>
        {
            var settings = provider.GetRequiredService<CloudflareImagesSettings>();

            if (settings.Enabled && !string.IsNullOrEmpty(settings.AccessToken))
                return provider.GetRequiredService<CloudflareThumbService>();

            return provider.GetRequiredService<ThumbService>();
        });
        services.AddTransient<CloudflareImagesSyncTask>();
    }

    /// <summary>
    /// Ensures the sync scheduled task exists in the database.
    /// Runs on every startup so the task is created even on stores where
    /// the plugin was already installed before this code was added.
    /// </summary>
    public void Configure(IApplicationBuilder application)
    {
        var scheduleTaskService = application.ApplicationServices
            .GetService(typeof(IScheduleTaskService)) as IScheduleTaskService;

        if (scheduleTaskService == null)
            return;

        // Use GetAwaiter().GetResult() — Configure() is synchronous in INopStartup
        var existing = scheduleTaskService
            .GetTaskByTypeAsync(CloudflareImagesSyncTask.TaskType)
            .GetAwaiter().GetResult();

        if (existing == null)
        {
            scheduleTaskService.InsertTaskAsync(new ScheduleTask
            {
                Name = CloudflareImagesSyncTask.TaskName,
                Seconds = 300,
                Type = CloudflareImagesSyncTask.TaskType,
                Enabled = false,
                StopOnError = false
            }).GetAwaiter().GetResult();
        }

        // Ensure ImportApiKey exists (for stores upgraded before this setting was added)
        var settingService = application.ApplicationServices
            .GetService(typeof(ISettingService)) as ISettingService;
        if (settingService != null)
        {
            var settings = settingService.LoadSettingAsync<CloudflareImagesSettings>()
                .GetAwaiter().GetResult();
            if (string.IsNullOrEmpty(settings.ImportApiKey))
            {
                settings.ImportApiKey = Guid.NewGuid().ToString("N");
                settingService.SaveSettingAsync(settings).GetAwaiter().GetResult();
            }
        }
    }

    /// <summary>
    /// Gets order of this startup configuration implementation
    /// </summary>
    public int Order => 3000;
}