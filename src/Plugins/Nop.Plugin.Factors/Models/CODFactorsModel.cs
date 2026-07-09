using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Factors.Models;




    public record CODFactorsModel : BaseNopEntityModel
{
        [NopResourceDisplayName("Plugins.Factors.Fields.CountryID")]
        public int CountryID { get; set; }

        [NopResourceDisplayName("Plugins.Factors.Fields.Name")]
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [NopResourceDisplayName("Plugins.Factors.Fields.CODFactor")]
       
        public decimal CODFactor { get; set; }

        [NopResourceDisplayName("Plugins.Factors.Fields.FactorID")]
        public int FactorID { get; set; }
    [NopResourceDisplayName("Plugins.Factors.Fields.CountryName")]
    public string CountryName { get; set; }
}

