using System.Threading.Tasks;
using Nop.Plugin.Factors.Models;

namespace Nop.Plugin.Factors.Services
{
    public interface ICustomerTypeModelFactory
    {
        Task<CustomerTypeSearchModel> PrepareCustomerTypeSearchModelAsync(CustomerTypeSearchModel searchModel);
        Task<CustomerTypeListModel> PrepareCustomerTypeListModelAsync(CustomerTypeSearchModel searchModel);
    }
}