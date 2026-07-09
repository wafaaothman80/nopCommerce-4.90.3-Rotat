using System.Threading.Tasks;
using Nop.Plugin.Factors.Models;

namespace Nop.Plugin.Factors.Services
{
    public interface ICategoryFactorsModelFactory
    {
        Task<CategoryFactorsSearchModel> PrepareCategoryFactorsSearchModelAsync(CategoryFactorsSearchModel searchModel);
        Task<CategoryFactorsListModel> PrepareCategoryFactorsListModelAsync(CategoryFactorsSearchModel searchModel);
    }
}