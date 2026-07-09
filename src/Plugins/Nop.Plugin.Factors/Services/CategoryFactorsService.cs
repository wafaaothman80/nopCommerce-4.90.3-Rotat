using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Factors.Domain;

namespace Nop.Plugin.Factors.Services
{
    public class CategoryFactorsService : ICategoryFactorsService
    {
        private readonly IRepository<CategoryFactors> _categoryFactorsRepository;

        public CategoryFactorsService(IRepository<CategoryFactors> categoryFactorsRepository)
        {
            _categoryFactorsRepository = categoryFactorsRepository;
        }

        public virtual async Task<CategoryFactors> GetByIdAsync(int id)
        {
            return await _categoryFactorsRepository.GetByIdAsync(id);
        }

        public virtual async Task<IList<CategoryFactors>> GetAllAsync()
        {
            var query = _categoryFactorsRepository.Table;
            return await query.ToListAsync();
        }

        public virtual async Task InsertAsync(CategoryFactors categoryFactors)
        {
            await _categoryFactorsRepository.InsertAsync(categoryFactors);
        }

        public virtual async Task UpdateAsync(CategoryFactors categoryFactors)
        {
            await _categoryFactorsRepository.UpdateAsync(categoryFactors);
        }

        public virtual async Task DeleteAsync(CategoryFactors categoryFactors)
        {
            await _categoryFactorsRepository.DeleteAsync(categoryFactors);
        }

        public virtual async Task<IPagedList<CategoryFactors>> SearchAsync(string categoryName = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            return await _categoryFactorsRepository.GetAllPagedAsync(query =>
            {
                if (!string.IsNullOrEmpty(categoryName))
                    query = query.Where(x => x.Name.Contains(categoryName));

                query = query.OrderBy(x => x.Name);

                return query;
            }, pageIndex, pageSize, getOnlyTotalCount);
        }

        public virtual async Task<IList<string>> GetDistinctCategoryNamesAsync()
        {
            var query = _categoryFactorsRepository.Table;
            return await query
                .Select(x => x.Name)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
        }
    }
}