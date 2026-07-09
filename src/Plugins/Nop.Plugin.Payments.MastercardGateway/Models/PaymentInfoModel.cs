using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.MastercardGateway.Models
{
    public record 
        
        
        PaymentInfoModel : BaseNopModel
    {


       
        public string SessionId { get; set; }
    
        public string ApiUrl { get; set; }
        public string MerchantId { get; set; }
        
    }
}