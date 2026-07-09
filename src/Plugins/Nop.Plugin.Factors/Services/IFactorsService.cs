using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Factors.Domain;

namespace Nop.Plugin.Factors.Services
{
    public interface IFactorsService
    {
        Task<Domain.Factors> GetByIdAsync(int id);
        Task<IList<Domain.Factors>> GetAllAsync();
        Task InsertAsync(Domain.Factors Factors);
        Task UpdateAsync(Domain.Factors Factors);
        Task DeleteAsync(Domain.Factors Factors);
        Task<IPagedList<Domain.Factors>> SearchAsync( int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);
       
    }
}
