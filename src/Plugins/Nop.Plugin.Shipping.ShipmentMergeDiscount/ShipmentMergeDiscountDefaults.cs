namespace Nop.Plugin.Shipping.ShipmentMergeDiscount;

/// <summary>
/// Constants used throughout the Shipment Merge Discount plugin.
/// </summary>
public static class ShipmentMergeDiscountDefaults
{
    /// <summary>Plugin system name — must match plugin.json SystemName.</summary>
    public const string SystemName = "DiscountRequirement.ShipmentMerge";

    /// <summary>The display name shown to customers on the checkout summary.</summary>
    public const string DiscountName = "Shipment Consolidation Discount";

    /// <summary>
    /// Key used by AdjustShippingRateAsync to store the per-option calculated amount.
    /// May be overwritten multiple times (once per shipping option) — do not read this
    /// for display; use <see cref="CustomerMergeDiscountDisplayAttribute"/> instead.
    /// </summary>
    public const string CustomerMergeDiscountAmountAttribute = "ShipmentMergeDiscount.CalculatedAmount";

    /// <summary>
    /// Key written ONLY by GetShoppingCartTotalAsync after applying Math.Max(configuredMin, stored).
    /// This is the authoritative display amount — ROPC's repeated AdjustShippingRateAsync calls
    /// (one per shipping option) cannot overwrite it.
    /// </summary>
    public const string CustomerMergeDiscountDisplayAttribute = "ShipmentMergeDiscount.DisplayAmount";

    /// <summary>
    /// The shipping rate (primary store currency) that GetShoppingCartTotalAsync actually used
    /// to compute the discount. Stored so GetAmount can compute the correct display discount
    /// even when SelectedShippingOptionAttribute is temporarily corrupted by ROPC.
    /// </summary>
    public const string CustomerSelectedShippingRateAttribute = "ShipmentMergeDiscount.SelectedRate";

    /// <summary>
    /// Settings key template for the discount requirement configuration.
    /// The placeholder is replaced with the <c>DiscountRequirementId</c>.
    /// </summary>
    public const string SettingsKey = "DiscountRequirement.ShipmentMerge-{0}";

    /// <summary>Route name for the discount-requirement configuration page (used by IDiscountRequirementRule).</summary>
    public const string ConfigurationRouteName = "DiscountRequirement.ShipmentMerge.Configure";

    /// <summary>Route name for the global plugin settings page (used by Plugin Manager → Configure).</summary>
    public const string SettingsRouteName = "DiscountRequirement.ShipmentMerge.Settings";
}
