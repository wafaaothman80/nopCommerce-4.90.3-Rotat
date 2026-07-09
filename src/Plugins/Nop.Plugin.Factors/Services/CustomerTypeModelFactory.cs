using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Factors.Models;
using Nop.Plugin.Factors.Services;

using Nop.Web.Framework.Models.DataTables;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Factors.Services
{
    public class CustomerTypeModelFactory : ICustomerTypeModelFactory
    {
        private readonly ICustomerTypeService _CustomerTypeService;

        public CustomerTypeModelFactory(ICustomerTypeService CustomerTypeService)
        {
            _CustomerTypeService = CustomerTypeService;
        }

        public virtual async Task<CustomerTypeSearchModel> PrepareCustomerTypeSearchModelAsync(CustomerTypeSearchModel searchModel)
        {
            if (searchModel == null)
                searchModel = new CustomerTypeSearchModel();


            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<CustomerTypeListModel> PrepareCustomerTypeListModelAsync(CustomerTypeSearchModel searchModel)
        {
            var pagedFactors = await _CustomerTypeService.SearchAsync(
              
                searchModel.Page - 1,
                searchModel.PageSize
            );

            var model = new CustomerTypeListModel().PrepareToGrid(searchModel, pagedFactors, () =>
            {
                return pagedFactors.Select(factor =>
                {
                    var factorModel = new CustomerTypeModel
                    {
                        Id = factor.Id,
                       
                        TypeName = factor.TypeName,
                        Factor = factor.Factor
                    };
                    return factorModel;
                });
            });

            return model;
        }

       
    }
}