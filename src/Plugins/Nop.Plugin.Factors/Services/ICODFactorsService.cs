using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Factors.Domain;

namespace Nop.Plugin.Factors.Services
{
    public interface ICODFactorsService
    {
        Task<CODFactors> GetByIdAsync(int id);
        Task<IList<CODFactors>> GetAllAsync();
        Task InsertAsync(CODFactors codFactors);
        Task UpdateAsync(CODFactors codFactors);
        Task DeleteAsync(CODFactors codFactors);
        Task<IPagedList<CODFactors>> SearchAsync(int? countryId, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);
    }
}