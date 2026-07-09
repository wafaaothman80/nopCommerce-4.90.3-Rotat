using Nop.Core;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.Shipping.ShipmentMergeDiscount.Components;
using Nop.Plugin.Shipping.ShipmentMergeDiscount.Services;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Core.Domain.Cms;
using Nop.Services.Cms;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Shipping.ShipmentMergeDiscount;

/// <summary>
/// Main plugin class. Implements <see cref="IDiscountRequirementRule"/> so the plugin
/// appears in the NopCommerce discount-requirements section, and <see cref="IWidgetPlugin"/>
/// to inject the discount line into the checkout order-totals summary.
/// </summary>
public class ShipmentMergeDiscountPlugin : BasePlugin, IDiscountRequirementRule, IWidgetPlugin
{
    #region Fields

    private readonly IDiscountService _discountService;
    private readonly ILocalizationService _localizationService;
    private readonly INopUrlHelper _nopUrlHelper;
    private readonly ISettingService _settingService;
    private readonly IShipmentMergeDiscountService _mergeDiscountService;
    private readonly IStoreContext _storeContext;
    private readonly IWebHelper _webHelper;
    private readonly WidgetSettings _widgetSettings;

    #endregion

    #region Ctor

    public ShipmentMergeDiscountPlugin(
        IDiscountService discountService,
        ILocalizationService localizationService,
        INopUrlHelper nopUrlHelper,
        ISettingService settingService,
        IShipmentMergeDiscountService mergeDiscountService,
        IStoreContext storeContext,
        IWebHelper webHelper,
        WidgetSettings widgetSettings)
    {
        _discountService = discountService;
        _localizationService = localizationService;
        _nopUrlHelper = nopUrlHelper;
        _settingService = settingService;
        _mergeDiscountService = mergeDiscountService;
        _storeContext = storeContext;
        _webHelper = webHelper;
        _widgetSettings = widgetSettings;
    }

    #endregion

    #region IDiscountRequirementRule

    /// <summary>
    /// Checks whether the customer meets the shipment-merge requirement.
    /// The requirement is met when the customer has at least the configured number of
    /// pending, unshipped orders going to the same shipping address.
    /// </summary>
    public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(
        DiscountRequirementValidationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = new DiscountRequirementValidationResult();

        if (request.Customer == null)
            return result;

        var storeId = request.Store?.Id ?? 0;
        result.IsValid = await _mergeDiscountService.IsEligibleForMergeDiscountAsync(
            request.Customer, shippingAddress: null, storeId);

        return result;
    }

    /// <summary>
    /// Returns the URL of the discount-requirement configuration page shown on the
    /// Admin → Promotions → Discounts edit screen.
    /// </summary>
    public string GetConfigurationUrl(int discountId, int? discountRequirementId)
    {
        return _nopUrlHelper.RouteUrl(
            ShipmentMergeDiscountDefaults.ConfigurationRouteName,
            new { discountId, discountRequirementId },
            _webHelper.GetCurrentRequestProtocol());
    }

    /// <summary>
    /// Returns the URL of the global plugin settings page shown on the Plugin Manager list.
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return _nopUrlHelper.RouteUrl(
            ShipmentMergeDiscountDefaults.SettingsRouteName,
            null,
            _webHelper.GetCurrentRequestProtocol());
    }

    #endregion

    #region IWidgetPlugin

    public Task<IList<string>> GetWidgetZonesAsync() =>
        Task.FromResult<IList<string>>(new List<string>
        {
            PublicWidgetZones.OrderSummaryTotals,
            PublicWidgetZones.BodyStartHtmlTagAfter,
        });

    public Type GetWidgetViewComponent(string widgetZone) =>
        typeof(ShipmentMergeDiscountTotalsViewComponent);

    public bool HideInWidgetList => false;

    #endregion

    #region BasePlugin overrides

    /// <summary>
    /// Creates the hidden "Shipment Merge Discount" discount entry with default settings,
    /// adds locale resources, and saves initial plugin settings.
    /// </summary>
    public override async Task InstallAsync()
    {
        // ── Default settings ─────────────────────────────────────────────────────────
        await _settingService.SaveSettingAsync(new ShipmentMergeDiscountSettings
        {
            Enabled = true,
            MinimumEligibleOrders = 1,
            UsePercentage = true,
            DiscountPercentage = 50m,
            DiscountAmount = 0m,
            MaxDiscountAmount = 0m,
            StrictAddressMatch = false
        });

        // ── Locale resources ─────────────────────────────────────────────────────────
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Shipping.ShipmentMergeDiscount.Fields.Enabled"] = "Enabled",
            ["Plugins.Shipping.ShipmentMergeDiscount.Fields.Enabled.Hint"] =
                "Check to activate the shipment-merge discount for this store.",

            ["Plugins.Shipping.ShipmentMergeDiscount.Fields.MinimumEligibleOrders"] =
                "Minimum eligible previous orders",
            ["Plugins.Shipping.ShipmentMergeDiscount.Fields.MinimumEligibleOrders.Hint"] =
                "How many pending, unshipped orders (to the same address) must exist before the discount applies.",

            ["Plugins.Shipping.ShipmentMergeDiscount.Fields.UsePercentage"] =
                "Use percentage discount",
            ["Plugins.Shipping.ShipmentMergeDiscount.Fields.UsePercentage.Hint"] =
                "When checked, the discount is a percentage of the shipping cost; otherwise a fixed amount.",

            ["Plugins.Shipping.ShipmentMergeDiscount.Fields.DiscountPercentage"] =
                "Discount percentage (%)",
            ["Plugins.Shipping.ShipmentMergeDiscount.Fields.DiscountPercentage.Hint"] =
                "Percentage of the shipping cost to discount (0–100).",

            ["Plugins.Shipping.ShipmentMergeDiscount.Fields.DiscountAmount"] =
                "Fixed discount amount",
            ["Plugins.Shipping.ShipmentMergeDiscount.Fields.DiscountAmount.Hint"] =
                "Fixed monetary amount deducted from the shipping cost.",

            ["Plugins.Shipping.ShipmentMergeDiscount.Fields.MaxDiscountAmount"] =
                "Maximum discount amount",
            ["Plugins.Shipping.ShipmentMergeDiscount.Fields.MaxDiscountAmount.Hint"] =
                "Caps the discount regardless of percentage (0 = no cap).",

            ["Plugins.Shipping.ShipmentMergeDiscount.Fields.StrictAddressMatch"] =
                "Require strict address match",
            ["Plugins.Shipping.ShipmentMergeDiscount.Fields.StrictAddressMatch.Hint"] =
                "When checked, all address fields must match exactly (street, city, postal code, country, state). " +
                "When unchecked, only city, postal code, and country must match.",

            ["Plugins.Shipping.ShipmentMergeDiscount.Configure"] = "Configure Shipment Merge Discount",
            ["Plugins.Shipping.ShipmentMergeDiscount.Saved"] = "Settings saved."
        });

        // Register as an active widget so the order-totals discount line renders.
        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(ShipmentMergeDiscountDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(ShipmentMergeDiscountDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await base.InstallAsync();
    }

    /// <summary>
    /// Removes the discount requirement rules registered by this plugin, deletes locale
    /// resources, and removes plugin settings.
    /// </summary>
    public override async Task UninstallAsync()
    {
        // Remove any discount requirement rules that point to this plugin.
        var requirements = (await _discountService.GetAllDiscountRequirementsAsync())
            .Where(r => r.DiscountRequirementRuleSystemName == ShipmentMergeDiscountDefaults.SystemName);

        foreach (var req in requirements)
            await _discountService.DeleteDiscountRequirementAsync(req, false);

        // Remove from active widgets.
        if (_widgetSettings.ActiveWidgetSystemNames.Contains(ShipmentMergeDiscountDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Remove(ShipmentMergeDiscountDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        // Remove settings.
        await _settingService.DeleteSettingAsync<ShipmentMergeDiscountSettings>();

        // Remove locale resources.
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Shipping.ShipmentMergeDiscount");

        await base.UninstallAsync();
    }

    #endregion
}
