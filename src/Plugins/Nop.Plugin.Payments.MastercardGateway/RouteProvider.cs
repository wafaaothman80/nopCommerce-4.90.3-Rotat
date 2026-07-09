using Nop.Web.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Localization;
using Microsoft.AspNetCore.Builder;

namespace Nop.Plugin.Payments.MastercardGateway
{
    public partial class RouteProvider : IRouteProvider
    {
       

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute("Nop.Plugin.Payments.MastercardGateway", "Plugins/PaymentMastercardGateway/Return",
              new { controller = "PaymentMastercardGateway", action = "Return" });


            endpointRouteBuilder.MapControllerRoute("Nop.Plugin.Payments.MastercardGateway", "Plugins/PaymentMastercardGateway/Cancel",
              new { controller = "PaymentMastercardGateway", action = "Cancel" });

        }

        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
