using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Factors.Domain;

namespace Nop.Plugin.Factors.Services
{
    public interface ICustomerTypeService
    {
        Task<CustomerType> GetByIdAsync(int id);
        Task<IList<CustomerType>> GetAllAsync();
        Task InsertAsync(CustomerType CustomerType);
        Task UpdateAsync(CustomerType CustomerType);
        Task DeleteAsync(CustomerType CustomerType);
        Task<IPagedList<CustomerType>> SearchAsync( int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);
       
    }
}
