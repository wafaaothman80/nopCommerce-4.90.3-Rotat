using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.AlgoliaSearch.Extensions;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.Misc.AlgoliaSearch;

public class AdminMenuCreatedEventConsumer : IConsumer<AdminMenuEvent>
{
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;

    public AdminMenuCreatedEventConsumer(ILocalizationService localizationService,
        IPermissionService permissionService)
    {
        _localizationService = localizationService;
        _permissionService = permissionService;
    }

    public async Task HandleEventAsync(AdminMenuEvent createdEvent)
    {
        var menu = new NopStationAdminMenuItem()
        {
            Visible = true,
            IconClass = "far fa-dot-circle",
            Title = await _localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Menu.AlgoliaSearch"),
        };

        if (await _permissionService.AuthorizeAsync(AlgoliaSearchPermissionProvider.MANAGE_CONFIGURATION))
        {
            var settings = new AdminMenuItem()
            {
                Visible = true,
                IconClass = "far fa-circle",
                Url = "~/Admin/AlgoliaSearch/Configure",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Menu.Configuration"),
                SystemName = "AlgoliaSearch.Configuration"
            };
            menu.ChildNodes.Add(settings);
        }

        if (await _permissionService.AuthorizeAsync(AlgoliaSearchPermissionProvider.MANAGE_UPLOAD_PRODUCTS))
        {
            var updatableItems = new AdminMenuItem()
            {
                Visible = true,
                IconClass = "far fa-circle",
                Url = "~/Admin/AlgoliaSearch/UpdatableItem",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Menu.UpdatableItems"),
                SystemName = "AlgoliaSearch.UpdatableItems"
            };
            menu.ChildNodes.Add(updatableItems);
            var uploadProduct = new AdminMenuItem()
            {
                Visible = true,
                IconClass = "far fa-circle",
                Url = "~/Admin/AlgoliaSearch/UploadProduct",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Menu.UploadProducts"),
                SystemName = "AlgoliaSearch.UploadProducts"
            };
            menu.ChildNodes.Add(uploadProduct);
        }

        if (menu.ChildNodes.Any())
        {
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.SHOW_DOCUMENTATIONS))
            {
                var documentation = new AdminMenuItem()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/algolia-search-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=algolia-search",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true,
                    SystemName = "AlgoliaSearch.Documentation"
                };
                menu.ChildNodes.Add(documentation);
            }

            createdEvent.PluginChildNodes.Add(menu);
        }
    }
}
