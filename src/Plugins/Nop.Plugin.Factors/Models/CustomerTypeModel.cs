using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Factors.Models;




    public record CustomerTypeModel : BaseNopEntityModel
{
        

        [NopResourceDisplayName("Plugins.Factors.Fields.TypeName")]
        [Required]
        [StringLength(255)]
        public string TypeName { get; set; }

        [NopResourceDisplayName("Plugins.Factors.Fields.Factor")]
       
        public decimal Factor { get; set; }

        
}

