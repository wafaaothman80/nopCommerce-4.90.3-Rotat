using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Factors.Models;




    public record CategoryFactorsModel : BaseNopEntityModel
{
        [NopResourceDisplayName("Plugins.Factors.Fields.CountryID")]
        public int CategoryID { get; set; }

        [NopResourceDisplayName("Plugins.Factors.Fields.Name")]
        [Required]
        [StringLength(255)]
    public string Path { get; set; }

    [NopResourceDisplayName("Plugins.Factors.Fields.CODFactor")]
     
        public decimal Factor { get; set; }

        
}

