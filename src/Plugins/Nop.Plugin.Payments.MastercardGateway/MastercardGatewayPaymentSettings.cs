using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.MastercardGateway
{
    /// <summary>
    /// Represents settings of "Check money order" payment plugin
    /// </summary>
    public class MastercardGatewayPaymentSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool UseSandbox { get; set; }
        public string MerchantId { get; set; }
        public string ApiPassword { get; set; }
      
       
    }
}
