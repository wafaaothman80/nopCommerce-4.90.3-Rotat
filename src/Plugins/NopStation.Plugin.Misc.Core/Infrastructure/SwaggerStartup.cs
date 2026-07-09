using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.Core.Infrastructure;

public class SwaggerStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            var typeFinder = Singleton<ITypeFinder>.Instance;
            var apiDescriptors = typeFinder.FindClassesOfType<IApiDescriptor>()
                .Select(desc => (IApiDescriptor)Activator.CreateInstance(desc))
                .Where(desc => desc != null);

            foreach (var descriptor in apiDescriptors)
            {
                c.SwaggerDoc(descriptor.ApiGroup, new OpenApiInfo
                {
                    Title = descriptor.ApiTitle,
                    Version = descriptor.ApiVersion,
                    Description = descriptor.ApiDescription
                });
            }

            c.OperationFilter<AddSwaggerHeadersOperationFilter>();

            c.DocInclusionPredicate((docName, apiDescription) =>
            {
                foreach (var descriptor in apiDescriptors)
                    if (docName == descriptor.ApiGroup && apiDescription.GroupName == descriptor.ApiGroup)
                        return true;

                return false;
            });
            c.ResolveConflictingActions(apiDescriptions =>
            {
                return apiDescriptions.First();
            });
        });
    }

    public void Configure(IApplicationBuilder application)
    {
        var typeFinder = Singleton<ITypeFinder>.Instance;

        application.UseSwagger();
        application.UseSwaggerUI(c =>
        {
            var apiDescriptors = typeFinder.FindClassesOfType<IApiDescriptor>()
                .Select(desc => (IApiDescriptor)Activator.CreateInstance(desc))
                .Where(desc => desc != null);

            foreach (var descriptor in apiDescriptors)
            {
                c.SwaggerEndpoint($"/swagger/{descriptor.ApiGroup}/swagger.json", descriptor.ApiTitle);
                c.RoutePrefix = $"swagger/{descriptor.ApiGroup}";
            }
        });

        var headersOperations = typeFinder.FindClassesOfType<IHeadersOperation>()
            .Select(op => (IHeadersOperation)Activator.CreateInstance(op))
            .Where(op => op != null);

        var operations = new ApiHeadersOperations();
        foreach (var descriptor in headersOperations)
            operations[descriptor.GroupName] = descriptor;

        Singleton<ApiHeadersOperations>.Instance = operations;
    }

    public int Order => 0;
}
