using Nop.Core.Configuration;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention
{
    public class GoogleMobileExternalAuthSettings : ISettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string IOSClientId { get; set; }
        public string ProjectId { get; set; }
 public string AndroidClientId { get; set; }


  


    }
}