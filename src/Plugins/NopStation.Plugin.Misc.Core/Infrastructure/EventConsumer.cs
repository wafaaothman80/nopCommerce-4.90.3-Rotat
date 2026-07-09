using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Events;

namespace NopStation.Plugin.Misc.Core.Infrastructure;

public class EventConsumer : IConsumer<PageRenderingEvent>, IConsumer<AdminMenuEvent>
{
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;

    public EventConsumer(IActionContextAccessor actionContextAccessor,
        ILocalizationService localizationService,
        IPermissionService permissionService)
    {
        _actionContextAccessor = actionContextAccessor;
        _localizationService = localizationService;
        _permissionService = permissionService;
    }

    public Task HandleEventAsync(PageRenderingEvent eventMessage)
    {
        var area = _actionContextAccessor.ActionContext.HttpContext.GetRouteValue("area");

        if (area != null && area.ToString().Equals("admin", StringComparison.InvariantCultureIgnoreCase))
            eventMessage.Helper.AppendCssFileParts("~/Plugins/NopStation.Core/contents/css/style.css", "");

        return Task.CompletedTask;
    }

    public async Task HandleEventAsync(AdminMenuEvent createdEvent)
    {
        if (await _permissionService.AuthorizeAsync(CorePermissionProvider.MANAGE_CONFIGURATION))
        {
            var configuration = new NopStationAdminMenuItem()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.Configuration"),
                Visible = true,
                IconClass = "far fa-circle",
                Url = "~/Admin/NopStationCore/Configure",
                SystemName = "NopStationCore.Configure"
            };
            createdEvent.CoreChildNodes.Add(configuration);

            var resource = new NopStationAdminMenuItem()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.LocaleResources"),
                Visible = true,
                IconClass = "far fa-circle",
                Url = "~/Admin/NopStationCore/LocaleResource",
                SystemName = "NopStationCore.LocaleResources"
            };
            createdEvent.CoreChildNodes.Add(resource);
        }

        if (await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_ACL))
        {
            var acl = new NopStationAdminMenuItem()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.ACL"),
                Visible = true,
                IconClass = "far fa-circle",
                Url = "~/Admin/NopStationCore/Permissions",
                SystemName = "NopStationCore.ACL"
            };
            createdEvent.CoreChildNodes.Add(acl);
        }

        if (await _permissionService.AuthorizeAsync(CorePermissionProvider.MANAGE_LICENSE))
        {
            var license = new NopStationAdminMenuItem()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.License"),
                Visible = true,
                IconClass = "far fa-circle",
                Url = "~/Admin/NopStationLicense/License",
                SystemName = "NopStationCore.License"
            };
            createdEvent.CoreChildNodes.Add(license);
        }
    }
}
