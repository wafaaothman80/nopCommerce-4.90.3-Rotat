using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Factors.Domain;

namespace Nop.Plugin.Factors.Services
{
    public interface ICategoryFactorsService
    {
        Task<CategoryFactors> GetByIdAsync(int id);
        Task<IList<CategoryFactors>> GetAllAsync();
        Task InsertAsync(CategoryFactors categoryFactors);
        Task UpdateAsync(CategoryFactors categoryFactors);
        Task DeleteAsync(CategoryFactors categoryFactors);
        Task<IPagedList<CategoryFactors>> SearchAsync(string categoryName = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);
        Task<IList<string>> GetDistinctCategoryNamesAsync();
    }
}
