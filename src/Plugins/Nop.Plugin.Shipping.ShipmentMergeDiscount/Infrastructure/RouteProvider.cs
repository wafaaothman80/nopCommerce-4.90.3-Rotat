using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Shipping.ShipmentMergeDiscount.Infrastructure;

/// <summary>
/// Registers the admin configuration route for the Shipment Merge Discount plugin.
/// </summary>
public class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        // Called from the admin Discounts page when adding/editing this requirement rule.
        endpointRouteBuilder.MapControllerRoute(
            name: ShipmentMergeDiscountDefaults.ConfigurationRouteName,
            pattern: "Admin/ShipmentMergeDiscount/Configure",
            defaults: new
            {
                controller = "ShipmentMergeDiscount",
                action = "Configure",
                area = AreaNames.ADMIN
            });

        // Called from Plugin Manager → Configure button (global plugin settings).
        endpointRouteBuilder.MapControllerRoute(
            name: ShipmentMergeDiscountDefaults.SettingsRouteName,
            pattern: "Admin/ShipmentMergeDiscount/Settings",
            defaults: new
            {
                controller = "ShipmentMergeDiscount",
                action = "Settings",
                area = AreaNames.ADMIN
            });

        // Public AJAX endpoint — returns current merge-discount amount as JSON.
        endpointRouteBuilder.MapControllerRoute(
            name: "ShipmentMergeDiscount.GetAmount",
            pattern: "shipmentmergediscount/getamount",
            defaults: new { controller = "ShipmentMergeDiscountPublic", action = "GetAmount" });

        // Debug endpoint — returns eligible order count for the current customer.
        endpointRouteBuilder.MapControllerRoute(
            name: "ShipmentMergeDiscount.EligibleCount",
            pattern: "shipmentmergediscount/eligiblecount",
            defaults: new { controller = "ShipmentMergeDiscountPublic", action = "EligibleCount" });
    }

    public int Priority => 0;
}
