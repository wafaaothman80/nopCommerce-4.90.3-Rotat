using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Factors.Models;




    public record FactorsModel : BaseNopEntityModel
{
        

        [NopResourceDisplayName("Plugins.Factors.Fields.RoleID")]
        [Required]
      
        public int RoleID { get; set; }

        [NopResourceDisplayName("Plugins.Factors.Fields.Factor")]
       
        public decimal Factor { get; set; }

        
}

