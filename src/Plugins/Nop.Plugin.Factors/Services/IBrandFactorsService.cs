using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Factors.Domain;

namespace Nop.Plugin.Factors.Services
{
    public interface IBrandFactorsService
    {
        Task<BrandsFactors> GetByIdAsync(int id);
        Task<IList<BrandsFactors>> GetAllAsync();
        Task InsertAsync(BrandsFactors brandsFactors);
        Task UpdateAsync(BrandsFactors brandsFactors);
        Task DeleteAsync(BrandsFactors brandsFactors);
        Task<IPagedList<BrandsFactors>> SearchAsync(string categoryName = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);
        Task<IList<string>> GetDistinctBrandNamesAsync();
    }
}
