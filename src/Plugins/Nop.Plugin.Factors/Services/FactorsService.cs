using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Factors.Domain;

namespace Nop.Plugin.Factors.Services
{
    public class FactorsService : IFactorsService
    {
        private readonly IRepository<Domain.Factors> _FactorsRepository;

        public FactorsService(IRepository<Domain.Factors> FactorsRepository)
        {
            _FactorsRepository = FactorsRepository;
        }

        public virtual async Task<Domain.Factors> GetByIdAsync(int id)
        {
            return await _FactorsRepository.GetByIdAsync(id);
        }

        public virtual async Task<IList<Domain.Factors>> GetAllAsync()
        {
            var query = _FactorsRepository.Table;
            return await query.ToListAsync();
        }

        public virtual async Task InsertAsync(Domain.Factors Factors)
        {
            await _FactorsRepository.InsertAsync(Factors);
        }

        public virtual async Task UpdateAsync(Domain.Factors Factors)
        {
            await _FactorsRepository.UpdateAsync(Factors);
        }

        public virtual async Task DeleteAsync(Domain.Factors Factors)
        {
            await _FactorsRepository.DeleteAsync(Factors);
        }

        public virtual async Task<IPagedList<Domain.Factors>> SearchAsync( int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            return await _FactorsRepository.GetAllPagedAsync(query =>
            {
                

                return query;
            }, pageIndex, pageSize, getOnlyTotalCount);
        }

       

       
    }
}