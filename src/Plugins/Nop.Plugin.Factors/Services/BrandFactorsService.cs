using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Factors.Domain;

namespace Nop.Plugin.Factors.Services
{
    public class BrandsFactorsService : IBrandFactorsService
    {
        private readonly IRepository<BrandsFactors> _BrandsFactorsRepository;

        public BrandsFactorsService(IRepository<BrandsFactors> BrandsFactorsRepository)
        {
            _BrandsFactorsRepository = BrandsFactorsRepository;
        }

        public virtual async Task<BrandsFactors> GetByIdAsync(int id)
        {
            return await _BrandsFactorsRepository.GetByIdAsync(id);
        }

        public virtual async Task<IList<BrandsFactors>> GetAllAsync()
        {
            var query = _BrandsFactorsRepository.Table;
            return await query.ToListAsync();
        }

        public virtual async Task InsertAsync(BrandsFactors BrandsFactors)
        {
            await _BrandsFactorsRepository.InsertAsync(BrandsFactors);
        }

        public virtual async Task UpdateAsync(BrandsFactors BrandsFactors)
        {
            await _BrandsFactorsRepository.UpdateAsync(BrandsFactors);
        }

        public virtual async Task DeleteAsync(BrandsFactors BrandsFactors)
        {
            await _BrandsFactorsRepository.DeleteAsync(BrandsFactors);
        }

        public virtual async Task<IPagedList<BrandsFactors>> SearchAsync(string BrandsName = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            return await _BrandsFactorsRepository.GetAllPagedAsync(query =>
            {
                if (!string.IsNullOrEmpty(BrandsName))
                    query = query.Where(x => x.Name.Contains(BrandsName));

                query = query.OrderBy(x => x.Name);

                return query;
            }, pageIndex, pageSize, getOnlyTotalCount);
        }

        public async Task<IList<string>> GetDistinctBrandNamesAsync()
        {
            var query = _BrandsFactorsRepository.Table;
            return await query
                .Select(x => x.Name)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
        }

       
    }
}