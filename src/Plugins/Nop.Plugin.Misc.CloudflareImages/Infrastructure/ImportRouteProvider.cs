using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.CloudflareImages.Infrastructure;

public class ImportRouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapControllerRoute(
            name: "cfimport_status",
            pattern: "cfimport/status",
            defaults: new { controller = "CloudflareImagesImport", action = "Status" });

        endpointRouteBuilder.MapControllerRoute(
            name: "cfimport_process_batch",
            pattern: "cfimport/process-batch",
            defaults: new { controller = "CloudflareImagesImport", action = "ProcessBatch" });

        endpointRouteBuilder.MapControllerRoute(
            name: "cfimport_check",
            pattern: "cfimport/check/{productId:int}",
            defaults: new { controller = "CloudflareImagesImport", action = "Check" });

        endpointRouteBuilder.MapControllerRoute(
            name: "cfimport_sync_cloudflare",
            pattern: "cfimport/sync-cloudflare",
            defaults: new { controller = "CloudflareImagesImport", action = "SyncCloudflare" });

        endpointRouteBuilder.MapControllerRoute(
            name: "cfimport_process_folder",
            pattern: "cfimport/process-folder",
            defaults: new { controller = "CloudflareImagesImport", action = "ProcessFolder" });

        endpointRouteBuilder.MapControllerRoute(
            name: "cfimport_protect",
            pattern: "cfimport/protect",
            defaults: new { controller = "CloudflareImagesImport", action = "Protect" });
    }

    public int Priority => 0;
}
