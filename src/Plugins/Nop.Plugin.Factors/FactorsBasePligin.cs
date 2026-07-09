using Nop.Core;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;
using Nop.Services.Customers;

namespace Nop.Plugin.Factors;

public class FactorsBasePligin : BasePlugin, IAdminMenuPlugin
{
    #region Fields
    protected readonly ILocalizationService _localizationService;
    protected readonly ICustomerService _customerService;
    protected readonly IWorkContext _workContext;
    private readonly IWebHelper _webHelper;
    #endregion

    #region Ctor
    public FactorsBasePligin(
        ILocalizationService localizationService,
        ICustomerService customerService,
        IWebHelper webHelper,
        IWorkContext workContext)
    {
        _localizationService = localizationService;
        _customerService = customerService;
        _webHelper = webHelper;
        _workContext = workContext;
    }
    #endregion

    #region Methods

    public override async Task InstallAsync()
    {
        await base.InstallAsync();
    }

    public async Task ManageSiteMapAsync(AdminMenuItem rootNode)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var currentCustomerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

        if (currentCustomerRoleIds.Contains(1))
        {
            AdminMenuItem ourPluginsNode = rootNode.ChildNodes.FirstOrDefault<AdminMenuItem>(x => x.SystemName == "CODFactorRoot");
            if (ourPluginsNode == null)
            {
                ourPluginsNode = new AdminMenuItem
                {
                    SystemName = "CODFactorRoot",
                    Title = await _localizationService.GetResourceAsync("Plugins.Factors.MainMenu"),
                    Visible = true,
                    IconClass = "fa-gears"
                };
                rootNode.ChildNodes.Add(ourPluginsNode);
            }

            AdminMenuItem codFactorsNode = ourPluginsNode.ChildNodes.FirstOrDefault<AdminMenuItem>(x => x.SystemName == "CODFactors");
            if (codFactorsNode == null)
            {
                codFactorsNode = new AdminMenuItem
                {
                    SystemName = "CODFactors",
                    Title = await _localizationService.GetResourceAsync("Plugins.Factors.CODFactors"),
                    Visible = true,
                    Url = "~/Admin/CODFactors/List"
                };
                ourPluginsNode.ChildNodes.Add(codFactorsNode);
            }

            AdminMenuItem categoryFactorsNode = ourPluginsNode.ChildNodes.FirstOrDefault<AdminMenuItem>(x => x.SystemName == "CategoryFactors");
            if (categoryFactorsNode == null)
            {
                categoryFactorsNode = new AdminMenuItem
                {
                    SystemName = "CategoryFactors",
                    Title = await _localizationService.GetResourceAsync("Plugins.Factors.CategoryFactors"),
                    Visible = true,
                    Url = "~/Admin/CategoryFactors/List"
                };
                ourPluginsNode.ChildNodes.Add(categoryFactorsNode);
            }

            AdminMenuItem brandFactorsNode = ourPluginsNode.ChildNodes.FirstOrDefault<AdminMenuItem>(x => x.SystemName == "BrandFactors");
            if (brandFactorsNode == null)
            {
                brandFactorsNode = new AdminMenuItem
                {
                    SystemName = "BrandFactors",
                    Title = await _localizationService.GetResourceAsync("Plugins.Factors.BrandFactors"),
                    Visible = true,
                    Url = "~/Admin/BrandFactors/List"
                };
                ourPluginsNode.ChildNodes.Add(brandFactorsNode);
            }

            AdminMenuItem customerTypeNode = ourPluginsNode.ChildNodes.FirstOrDefault<AdminMenuItem>(x => x.SystemName == "CustomerType");
            if (customerTypeNode == null)
            {
                customerTypeNode = new AdminMenuItem
                {
                    SystemName = "CustomerType",
                    Title = await _localizationService.GetResourceAsync("Plugins.Factors.CustomerType"),
                    Visible = true,
                    Url = "~/Admin/CustomerType/List"
                };
                ourPluginsNode.ChildNodes.Add(customerTypeNode);
            }

            AdminMenuItem factorsNode = ourPluginsNode.ChildNodes.FirstOrDefault<AdminMenuItem>(x => x.SystemName == "Factors");
            if (factorsNode == null)
            {
                factorsNode = new AdminMenuItem
                {
                    SystemName = "Factors",
                    Title = await _localizationService.GetResourceAsync("Plugins.Factors.Factors"),
                    Visible = true,
                    Url = "~/Admin/Factors/List"
                };
                ourPluginsNode.ChildNodes.Add(factorsNode);
            }
        }
    }

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/Factors/Configure";
    }

    public override async Task UninstallAsync()
    {
        await base.UninstallAsync();
    }

    #endregion
}