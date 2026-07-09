using System.Threading.Tasks;
using Nop.Plugin.Factors.Models;

namespace Nop.Plugin.Factors.Services
{
    public interface IFactorsModelFactory
    {
        Task<FactorsSearchModel> PrepareFactorsSearchModelAsync(FactorsSearchModel searchModel);
        Task<FactorsListModel> PrepareFactorsListModelAsync(FactorsSearchModel searchModel);
    }
}