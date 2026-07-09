using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Factors.Models
{
    public record CategoryFactorsSearchModel : BaseSearchModel
    {
        public CategoryFactorsSearchModel()
        {
            AvailableCategoryNames = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Factors.SearchCategoryName")]
        public string SearchCategoryName { get; set; }

        public IList<SelectListItem> AvailableCategoryNames { get; set; }
    }
}