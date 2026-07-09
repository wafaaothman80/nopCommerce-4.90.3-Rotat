using System.Collections.Generic;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.MastercardGateway.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
        }

        [NopResourceDisplayName("Plugins.Payments.MastercardAPGateway.Fields.MerchantId")]
        public string MerchantId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.MastercardAPGateway.Fields.ApiPassword")]
        public string ApiPassword { get; set; }

        [NopResourceDisplayName("Plugins.Payments.MastercardAPGateway.Fields.ApiRegion")]
        public string ApiRegion { get; set; }

        [NopResourceDisplayName("Plugins.Payments.MastercardAPGateway.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }

        [NopResourceDisplayName("Plugins.Payments.MastercardAPGateway.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }


    }
}