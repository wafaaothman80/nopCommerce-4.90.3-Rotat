using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Factors.Models;




    public record BrandsFactorsModel : BaseNopEntityModel
{
        [NopResourceDisplayName("Plugins.Factors.Fields.BrandID")]
        public int BrandID { get; set; }

        [NopResourceDisplayName("Plugins.Factors.Fields.Name")]
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [NopResourceDisplayName("Plugins.Factors.Fields.CODFactor")]
       
        public decimal Factor { get; set; }

        
}

