using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Factors;


public record CODFactorsSearchModel : BaseSearchModel
{
    public CODFactorsSearchModel()
    {
        AvailableCountries = new List<SelectListItem>();
    }

    [NopResourceDisplayName("Nop.Plugin.Factors.SearchCountry")]
    public int SearchCountryId { get; set; }

    public IList<SelectListItem> AvailableCountries { get; set; }
}