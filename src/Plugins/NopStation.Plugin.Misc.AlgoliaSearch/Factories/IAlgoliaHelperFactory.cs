using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Models;
using NopStation.Plugin.Misc.AlgoliaSearch.Models;
using static NopStation.Plugin.Misc.AlgoliaSearch.Models.SearchModel;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Factories
{
    public interface IAlgoliaHelperFactory
    {
        Task UploadProductsAsync(UploadProductModel model);

        Task UpdateIndicesAsync(ConfigurationModel model);

        void ClearIndex();

        Task UpdateAlgoliaItemAsync();

        //Task<IPagedList<ProductOverviewModel>> SearchProductsAsync(string searchTerms = "", IList<int> cids = null,
        //    IList<int> mids = null, IList<int> vids = null, IList<FilteredGroupModel> specids = null,
        //    IList<FilteredGroupModel> attrids = null, IList<int> ratings = null, decimal? minPrice = null,
        //    decimal? maxPrice = null, int? orderby = null, int pageIndex = 0, int pageSize = int.MaxValue);
        Task<IPagedList<ProductOverviewModel>> SearchProductsAsync(
        string searchTerms = "",
        IList<int> cids = null, IList<int> mids = null, IList<int> vids = null,
        IList<FilteredGroupModel> specids = null, IList<FilteredGroupModel> attrids = null,
        IList<int> ratings = null,
        decimal? minPrice = null, decimal? maxPrice = null,
        int? orderby = null, int pageIndex = 0, int pageSize = int.MaxValue,
        IList<Models.RangeFilterModel> rangeFilters = null,
        bool onlyInStock = false,bool discountOnly=false);

        // Task<AlgoliaFilters> GetAlgoliaFiltersAsync(string searchTerms);
        Task<AlgoliaFilters> GetAlgoliaFiltersAsync(
        string searchTerms,
        IList<int> cids = null,                      // selected category ids
        IList<int> mids = null,                      // selected manufacturer ids
        IList<int> vids = null,                      // selected vendor ids
        IList<FilteredGroupModel> specids = null,    // selected specification options
        IList<int> ratings = null,                   // selected ratings (1-5)
        decimal? minPrice = null,                    // min price (already in primary currency)
        decimal? maxPrice = null,                    // max price (already in primary currency)
        bool onlyInStock = false,                    // stock filter
        bool discountOnly = false);
        Task UploadSubstitutesAsync(bool clearFirst = true);


        Task<HashSet<int>> GetProductsThatHaveSubstitutesAsync(IList<int> productIds);

        Task<List<int>> SearchProductIdsForAutoCompleteAsync(string term, int take, string indexName);
        Task<(long TotalHits, List<int> ProductIds)> SearchInSubstitutesBySubCodeGetProductIdsAsync(string term, int take);
        Task<string> GetAutoCompleteImageUrlByProductIdAsync(int productId);




    }
}