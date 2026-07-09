using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.MastercardGateway.Components
{
    [ViewComponent(Name = "PaymentMastercardViewComponent")]
    public class PaymentMastercardViewComponent : NopViewComponent
    {


        public PaymentMastercardViewComponent()
        {


        }
            public async Task<IViewComponentResult> InvokeAsync()
            {


            return View("~/Plugins/Payments.MastercardGateway/Views/PaymentInfo.cshtml");
        }
    }
}