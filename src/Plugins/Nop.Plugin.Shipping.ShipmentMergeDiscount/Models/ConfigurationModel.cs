using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.ShipmentMergeDiscount.Models;

/// <summary>
/// View model for the Shipment Merge Discount admin configuration page.
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ShipmentMergeDiscount.Fields.Enabled")]
    public bool Enabled { get; set; }
    public bool Enabled_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ShipmentMergeDiscount.Fields.MinimumEligibleOrders")]
    public int MinimumEligibleOrders { get; set; }
    public bool MinimumEligibleOrders_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ShipmentMergeDiscount.Fields.UsePercentage")]
    public bool UsePercentage { get; set; }
    public bool UsePercentage_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ShipmentMergeDiscount.Fields.DiscountPercentage")]
    public decimal DiscountPercentage { get; set; }
    public bool DiscountPercentage_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ShipmentMergeDiscount.Fields.DiscountAmount")]
    public decimal DiscountAmount { get; set; }
    public bool DiscountAmount_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ShipmentMergeDiscount.Fields.MaxDiscountAmount")]
    public decimal MaxDiscountAmount { get; set; }
    public bool MaxDiscountAmount_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ShipmentMergeDiscount.Fields.StrictAddressMatch")]
    public bool StrictAddressMatch { get; set; }
    public bool StrictAddressMatch_OverrideForStore { get; set; }
}
