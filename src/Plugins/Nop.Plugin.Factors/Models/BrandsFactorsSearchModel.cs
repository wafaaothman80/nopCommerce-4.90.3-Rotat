using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Factors.Models
{
    public record BrandsFactorsSearchModel : BaseSearchModel
    {
        public BrandsFactorsSearchModel()
        {
            AvailableBrandNames = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Factors.SearchBrandName")]
        public string SearchBrandName { get; set; }

        public IList<SelectListItem> AvailableBrandNames { get; set; }
    }
}