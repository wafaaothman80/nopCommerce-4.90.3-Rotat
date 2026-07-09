using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Shipping.ShipmentMergeDiscount.Services;
using Nop.Services.Orders;

namespace Nop.Plugin.Shipping.ShipmentMergeDiscount.Infrastructure;

/// <summary>
/// Registers plugin services. Runs at <see cref="Order"/> = 3000, after the core
/// framework (Order = 0), so the <see cref="IOrderTotalCalculationService"/> registration
/// here replaces the core one — giving us the decorator pattern without touching core code.
/// </summary>
public class NopStartup : INopStartup
{
    public int Order => 3000;

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Plugin's own business-logic service.
        services.AddScoped<IShipmentMergeDiscountService, ShipmentMergeDiscountService>();

        // ── Decorator pattern ─────────────────────────────────────────────────────────
        // ActivatorUtilities.CreateInstance<OrderTotalCalculationService>(sp) creates the
        // real inner service with all its constructor dependencies resolved from DI.
        // ShipmentMergeOrderTotalCalculationService then wraps it, intercepting only the
        // three shipping-total / rate-adjustment methods.
        services.AddScoped<IOrderTotalCalculationService>(sp =>
        {
            var inner = Microsoft.Extensions.DependencyInjection.ActivatorUtilities
                .CreateInstance<OrderTotalCalculationService>(sp);

            return Microsoft.Extensions.DependencyInjection.ActivatorUtilities
                .CreateInstance<ShipmentMergeOrderTotalCalculationService>(sp, inner);
        });
    }

    public void Configure(IApplicationBuilder application) { }
}
