using Nop.Core;
using Nop.Plugin.AccountManager.Components;
using Nop.Services.Cms;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.AccountManager;

public class AccountManagerBasePligin : BasePlugin, IAdminMenuPlugin, IWidgetPlugin
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly ICustomerService _customerService;
    protected readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public AccountManagerBasePligin(
        ILocalizationService localizationService,
        ICustomerService customerService,
        IWorkContext workContext)
    {
        _localizationService = localizationService;
        _customerService = customerService;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Install the plugin
    /// </summary>
    public override async Task InstallAsync()
    {
        await base.InstallAsync();
    }

    public async Task ManageSiteMapAsync(AdminMenuItem rootNode)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var currentCustomerRoleIds = await _customerService.GetCustomerRoleIdsAsync(currentCustomer);

        if (currentCustomerRoleIds.Contains(1))
        {
            var ourPluginsNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "OurPlugins");

            if (ourPluginsNode == null)
            {
                ourPluginsNode = new AdminMenuItem
                {
                    SystemName = "Ourplugins",
                    Title = await _localizationService.GetResourceAsync("TheRoot.Nop.Plugin.OurPlugins"),
                    Visible = true,
                    IconClass = "fa-dot-circle-o"
                };
                rootNode.ChildNodes.Add(ourPluginsNode);
            }

            var child2 = new AdminMenuItem
            {
                Title = await _localizationService.GetResourceAsync("TheRoot.Nop.Plugin.OurPlugins.AccountManagers"),
                Url = "~/Admin/AccountManager/List",
                SystemName = "TheRoot.Nop.Plugin.OurPlugins.AccountManagers",
                Visible = true
            };
            ourPluginsNode.ChildNodes.Add(child2);

            var child3 = new AdminMenuItem
            {
                Title = await _localizationService.GetResourceAsync("TheRoot.Nop.Plugin.OurPlugins.Rigions"),
                Url = "~/Admin/Rigion/List",
                SystemName = "TheRoot.Nop.Plugin.OurPlugins.Rigions",
                Visible = true
            };
            ourPluginsNode.ChildNodes.Add(child3);
        }
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    public override async Task UninstallAsync()
    {
        await base.UninstallAsync();
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(AccountManagerContactViewComponent);
    }

    public bool HideInWidgetList => false;

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        IList<string> zones = new List<string>
        {
            PublicWidgetZones.HomepageTop
        };
        return Task.FromResult(zones);
    }

    #endregion
}