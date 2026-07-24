using Nop.Core.Configuration;

namespace Nop.Web
{
    /// <summary>
    /// Dexatel OTP settings, stored in the Setting table and editable via
    /// Admin > Configuration > Settings > All settings (advanced):
    ///   dexatelsettings.apikey
    ///   dexatelsettings.templateid
    /// Values below are fallback defaults used only when no Setting record exists.
    /// </summary>
    public class DexatelSettings : ISettings
    {
        /// <summary>
        /// Dexatel API key, sent as the X-Dexatel-Key header
        /// </summary>
        public string ApiKey { get; set; }
            //= "218e25c2e5939f7b92654303f0a50b9d";

        /// <summary>
        /// Dexatel verification template id
        /// </summary>
        public string TemplateId { get; set; }
            //= "9c9dcaa2-990a-45c2-9efd-ae0bd4d63cbf";

        /// <summary>
        /// Approved sender name (Dexatel dashboard > Senders); must match an
        /// approved sender on the account or the API returns 1504 "Invalid message sender"
        /// </summary>
        public string Sender { get; set; } = "Dexatel";
    }
}
