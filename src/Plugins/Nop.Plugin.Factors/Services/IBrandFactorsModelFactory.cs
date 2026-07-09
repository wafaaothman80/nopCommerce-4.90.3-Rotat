using System.Threading.Tasks;
using Nop.Plugin.Factors.Models;

namespace Nop.Plugin.Factors.Services
{
    public interface IBrandFactorsModelFactory
    {
        Task<BrandsFactorsSearchModel> PrepareBrandFactorsSearchModelAsync(BrandsFactorsSearchModel searchModel);
        Task<BrandsFactorsListModel> PrepareBrandFactorsListModelAsync(BrandsFactorsSearchModel searchModel);
    }
}