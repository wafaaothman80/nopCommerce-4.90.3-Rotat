using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Factors.Domain;

namespace Nop.Plugin.Factors.Services
{
    public class CODFactorsService : ICODFactorsService
    {
        private readonly IRepository<CODFactors> _codFactorsRepository;

        public CODFactorsService(IRepository<CODFactors> codFactorsRepository)
        {
            _codFactorsRepository = codFactorsRepository;
        }

        public virtual async Task<CODFactors> GetByIdAsync(int id)
        {
            return await _codFactorsRepository.GetByIdAsync(id);
        }

        public virtual async Task<IList<CODFactors>> GetAllAsync()
        {
            var query = _codFactorsRepository.Table;
            return await query.ToListAsync();
        }

        public virtual async Task InsertAsync(CODFactors codFactors)
        {
            await _codFactorsRepository.InsertAsync(codFactors);
        }

        public virtual async Task UpdateAsync(CODFactors codFactors)
        {
            await _codFactorsRepository.UpdateAsync(codFactors);
        }

        public virtual async Task DeleteAsync(CODFactors codFactors)
        {
            await _codFactorsRepository.DeleteAsync(codFactors);
        }

        public virtual async Task<IPagedList<CODFactors>> SearchAsync(int? countryId, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            return await _codFactorsRepository.GetAllPagedAsync(query =>
            {
                if (countryId.HasValue && countryId.Value > 0)
                    query = query.Where(x => x.CountryID == countryId.Value);

                query = query.OrderBy(x => x.Name);

                return query;
            }, pageIndex, pageSize, getOnlyTotalCount);
        }
    }
}