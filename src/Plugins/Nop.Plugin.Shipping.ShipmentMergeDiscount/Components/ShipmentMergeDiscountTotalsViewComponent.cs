using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Shipping.ShipmentMergeDiscount.Components;

/// <summary>
/// Injects the shipment-merge discount JavaScript into every page via widget zones
/// (BodyStartHtmlTagAfter and OrderSummaryTotals). The script fetches the discount
/// amount via AJAX and injects the row only when eligible — so this component always
/// renders unconditionally, never returning empty.
/// </summary>
public class ShipmentMergeDiscountTotalsViewComponent : NopViewComponent
{
    public Task<IViewComponentResult> InvokeAsync(string widgetZone, object? additionalData = null)
    {
        return Task.FromResult<IViewComponentResult>(View(
            "~/Plugins/Shipping.ShipmentMergeDiscount/Views/Components/ShipmentMergeDiscountTotals/Default.cshtml",
            string.Empty));
    }
}
