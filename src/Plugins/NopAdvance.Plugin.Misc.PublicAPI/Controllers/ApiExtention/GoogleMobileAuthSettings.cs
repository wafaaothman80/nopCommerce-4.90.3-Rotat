using Nop.Core.Configuration;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention
{
    public class GoogleMobileAuthSettings : ISettings
    {
        public bool Enabled { get; set; } = true;

        // Web credentials ()
   public string WebClientId { get; set; } = "";    
 public string WebClientSecret { get; set; } = "";
        public string AndroidClientId { get; set; } = "";
        public string IOSClientId { get; set; } = "";
        
        // Project info  
        public string ProjectId { get; set; } = "";
    }
}