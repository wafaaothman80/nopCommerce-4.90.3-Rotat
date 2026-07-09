using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention;
public class MobileLoginRequest
{
    public string IdToken { get; set; }
    public string Platform { get; set; } 
}
