using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Plugins;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.Core;

public class NopStationCorePlugin : BasePlugin, INopStationPlugin
{
    private readonly IWebHelper _webHelper;
    private readonly ISettingService _settingService;

    public NopStationCorePlugin(IWebHelper webHelper,
        ISettingService settingService)
    {
        _webHelper = webHelper;
        _settingService = settingService;
    }

    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + "Admin/NopStationCore/Configure";
    }

    public override async Task InstallAsync()
    {
        var settings = new NopStationCoreSettings()
        {
            AllowedCustomerRoleIds = new List<int> { 1, 2 }
        };
        await _settingService.SaveSettingAsync(settings);

        await this.InstallPluginAsync();
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await this.UninstallPluginAsync();
        await base.UninstallAsync();
    }

    public IDictionary<string, string> GetPluginResources()
    {
        var list = new Dictionary<string, string>
        {
            ["Admin.NopStation.Core.AssemblyInfo"] = "Nop-Station assembly information",
            ["Admin.NopStation.Core.Configuration"] = "Core settings",
            ["Admin.NopStation.Core.LocaleResources"] = "String resources",
            ["Admin.NopStation.Core.ACL"] = "Access control list",
            ["Admin.NopStation.Core.License"] = "License",
            ["Admin.NopStation.Core.Menu.NopStation"] = "Nop Station",
            ["Admin.NopStation.Core.Menu.AssemblyInfo"] = "Assembly information",
            ["Admin.NopStation.Core.Menu.Configuration"] = "Configuration",
            ["Admin.NopStation.Core.Menu.LocaleResources"] = "String resources",
            ["Admin.NopStation.Core.Menu.ACL"] = "Access control list",
            ["Admin.NopStation.Core.Menu.License"] = "License",
            ["Admin.NopStation.Core.Menu.Core"] = "Core settings",
            ["Admin.NopStation.Core.Menu.Themes"] = "Themes",
            ["Admin.NopStation.Core.Menu.Plugins"] = "Plugins",
            ["Admin.NopStation.Core.Menu.ReportBug"] = "Report a bug",
            ["Admin.NopStation.Core.License.InvalidProductKey"] = "Your product key is not valid.",
            ["Admin.NopStation.Core.License.InvalidForDomain"] = "Your product key is not valid for this domain.",
            ["Admin.NopStation.Core.License.InvalidForNOPVersion"] = "Your product key is not valid for this nopCommerce version.",
            ["Admin.NopStation.Core.License.Saved"] = "Your product key has been saved successfully.",
            ["Admin.NopStation.Core.License.LicenseString"] = "License string",
            ["Admin.NopStation.Core.License.LicenseString.Hint"] = "Nop-station plugin/theme license string.",
            ["Admin.NopStation.Common.Menu.Documentation"] = "Documentation",

            ["Admin.NopStation.Core.Resources.EditAccessDenied"] = "For security purposes, the feature you have requested is not available on this site.",
            ["Admin.NopStation.Core.Resources.FailedToSave"] = "Failed to save resource string.",
            ["Admin.NopStation.Core.Resources.Fields.Name"] = "Name",
            ["Admin.NopStation.Core.Resources.Fields.Value"] = "Value",
            ["Admin.NopStation.Core.Resources.List.SearchPluginSystemName"] = "Plugin",
            ["Admin.NopStation.Core.Resources.List.SearchPluginSystemName.Hint"] = "Search resource string by plugin.",
            ["Admin.NopStation.Core.Resources.List.SearchResourceName"] = "Resource name",
            ["Admin.NopStation.Core.Resources.List.SearchResourceName.Hint"] = "Search resource string by resource name.",
            ["Admin.NopStation.Core.Resources.List.SearchLanguageId"] = "Language",
            ["Admin.NopStation.Core.Resources.List.SearchLanguageId.Hint"] = "Search resource string by language.",
            ["Admin.NopStation.Core.Resources.List.SearchPluginSystemName.All"] = "All",

            ["Admin.NopStation.Core.Configuration.Fields.EnableCORS.ChangeHint"] = "Restart your application after changing this setting value.",
            ["Admin.NopStation.Core.Configuration.Fields.EnableCORS"] = "Enable CORS",
            ["Admin.NopStation.Core.Configuration.Fields.EnableCORS.Hint"] = "Check to enable CORS. It will add \"Access-Control-Allow-Origin\" header for every api response.",
            ["Admin.NopStation.Core.Configuration.AdminCanNotBeRestricted"] = "Admin role can not be restricted.",
            ["Admin.NopStation.Core.Configuration.Fields.RestrictMainMenuByCustomerRoles"] = "Restrict main menu by customer roles",
            ["Admin.NopStation.Core.Configuration.Fields.RestrictMainMenuByCustomerRoles.Hint"] = "Restrict main menu (Nop Station) by customer roles.",
            ["Admin.NopStation.Core.Configuration.Fields.AllowedCustomerRoles"] = "Allowed customer roles",
            ["Admin.NopStation.Core.Configuration.Fields.AllowedCustomerRoles.Hint"] = "Select allowed customer roles to access Nop Station plugin menus. Make sure proper access provided for these customer roles from 'Access control list' page.",

            ["NopStation.Core.Request.Common.Ok"] = "Request success",
            ["NopStation.Core.Request.Common.BadRequest"] = "Bad request",
            ["NopStation.Core.Request.Common.Unauthorized"] = "Unauthorized",
            ["NopStation.Core.Request.Common.NotFound"] = "Not found",
            ["NopStation.Core.Request.Common.InternalServerError"] = "Internal server error"
        };

        return list;
    }
}
