using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Factors.Domain;

namespace Nop.Plugin.Factors.Services
{
    public class CustomerTypeService : ICustomerTypeService
    {
        private readonly IRepository<CustomerType> _CustomerTypeRepository;

        public CustomerTypeService(IRepository<CustomerType> CustomerTypeRepository)
        {
            _CustomerTypeRepository = CustomerTypeRepository;
        }

        public virtual async Task<CustomerType> GetByIdAsync(int id)
        {
            return await _CustomerTypeRepository.GetByIdAsync(id);
        }

        public virtual async Task<IList<CustomerType>> GetAllAsync()
        {
            var query = _CustomerTypeRepository.Table;
            return await query.ToListAsync();
        }

        public virtual async Task InsertAsync(CustomerType CustomerType)
        {
            await _CustomerTypeRepository.InsertAsync(CustomerType);
        }

        public virtual async Task UpdateAsync(CustomerType CustomerType)
        {
            await _CustomerTypeRepository.UpdateAsync(CustomerType);
        }

        public virtual async Task DeleteAsync(CustomerType CustomerType)
        {
            await _CustomerTypeRepository.DeleteAsync(CustomerType);
        }

        public virtual async Task<IPagedList<CustomerType>> SearchAsync( int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            return await _CustomerTypeRepository.GetAllPagedAsync(query =>
            {
                

                return query;
            }, pageIndex, pageSize, getOnlyTotalCount);
        }

       

       
    }
}