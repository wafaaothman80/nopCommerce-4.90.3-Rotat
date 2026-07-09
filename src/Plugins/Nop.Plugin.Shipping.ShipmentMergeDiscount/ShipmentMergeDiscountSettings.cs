using Nop.Core.Configuration;

namespace Nop.Plugin.Shipping.ShipmentMergeDiscount;

/// <summary>
/// Store-level settings for the Shipment Merge Discount plugin.
/// Saved via <see cref="Nop.Services.Configuration.ISettingService"/> and therefore
/// fully supports multi-store overrides.
/// </summary>
public class ShipmentMergeDiscountSettings : ISettings
{
    /// <summary>
    /// Gets or sets a value indicating whether the plugin is active for this store.
    /// When false the decorator passes all calls through unchanged.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the minimum number of qualifying previous orders required
    /// before the consolidation discount is offered. Defaults to 1.
    /// </summary>
    public int MinimumEligibleOrders { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether the discount is expressed as a
    /// percentage of the current shipping cost (true) or a fixed amount (false).
    /// </summary>
    public bool UsePercentage { get; set; } = true;

    /// <summary>
    /// Gets or sets the percentage discount applied to the current order's
    /// shipping cost when shipment consolidation is possible.
    /// Only used when <see cref="UsePercentage"/> is <c>true</c>. Range: 0–100.
    /// </summary>
    public decimal DiscountPercentage { get; set; } = 50m;

    /// <summary>
    /// Gets or sets the fixed discount amount deducted from the shipping cost.
    /// Only used when <see cref="UsePercentage"/> is <c>false</c>.
    /// </summary>
    public decimal DiscountAmount { get; set; } = 0m;

    /// <summary>
    /// Gets or sets the maximum discount amount that may be applied, regardless of
    /// the calculated value. Zero means no cap.
    /// </summary>
    public decimal MaxDiscountAmount { get; set; } = 0m;

    /// <summary>
    /// Gets or sets a value indicating whether only orders whose shipping address
    /// exactly matches every address field must qualify.
    /// When false a looser match (city + postal code + country) is used.
    /// </summary>
    public bool StrictAddressMatch { get; set; } = false;
}
