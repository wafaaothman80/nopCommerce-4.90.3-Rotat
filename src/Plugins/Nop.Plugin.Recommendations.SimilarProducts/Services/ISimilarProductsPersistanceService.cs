using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Recommendations.SimilarProducts.Domains;
using Nop.Plugin.Recommendations.SimilarProducts.Models;

namespace Nop.Plugin.Recommendations.SimilarProducts.Services
{
    public interface ISimilarProductsPersistanceService
    {
        Task SaveSimilarProducts(IEnumerable<SimilarProductRecord> records);

        Task<IEnumerable<SimilarProduct>> GetSimilarProductsAsync(int productId, int take);

        // جديد:
        Task InsertAsync(SimilarProductRecord record);
        Task DeleteAsync(int id);

        Task<IPagedList<SimilarProductRecord>> SearchAsync(
            int? productId = null,
            int? similarProductId = null,
            int pageIndex = 0,
            int pageSize = 20);
    }
}