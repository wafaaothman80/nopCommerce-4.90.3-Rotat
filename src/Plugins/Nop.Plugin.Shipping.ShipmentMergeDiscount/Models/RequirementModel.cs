using Nop.Web.Framework.Models;

namespace Nop.Plugin.Shipping.ShipmentMergeDiscount.Models;

/// <summary>
/// View model for the discount-requirement configuration page.
/// Shown inside the admin Discounts edit screen when this rule is added.
/// There are no per-requirement settings — eligibility is determined automatically
/// based on global plugin settings — so this model is read-only (informational only).
/// </summary>
public record RequirementModel : BaseNopModel
{
    /// <summary>The discount this requirement belongs to.</summary>
    public int DiscountId { get; set; }

    /// <summary>The requirement identifier (0 when creating a new one).</summary>
    public int RequirementId { get; set; }

    // ── Informational summary of current global settings ─────────────────────
    public bool PluginEnabled { get; set; }
    public int MinimumEligibleOrders { get; set; }
    public bool UsePercentage { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal DiscountAmount { get; set; }
}
