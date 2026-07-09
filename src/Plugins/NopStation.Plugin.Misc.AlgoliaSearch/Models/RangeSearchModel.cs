using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Models
{
    public class RangeSearchModel
    {
       
        [NopResourceDisplayName("NopStation.AlgoliaSearch.Search.SearchTerm")]
        public string q { get; set; }

        
        public string attribute { get; set; }

        // range values
        public decimal? from { get; set; }
        public decimal? to { get; set; }
    }
}
