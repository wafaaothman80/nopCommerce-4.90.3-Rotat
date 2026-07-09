using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Factors.Models;
using Nop.Plugin.Factors.Services;

using Nop.Web.Framework.Models.DataTables;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Factors.Services
{
    public class FactorsModelFactory : IFactorsModelFactory
    {
        private readonly IFactorsService _FactorsService;

        public FactorsModelFactory(IFactorsService FactorsService)
        {
            _FactorsService = FactorsService;
        }

        public virtual async Task<FactorsSearchModel> PrepareFactorsSearchModelAsync(FactorsSearchModel searchModel)
        {
            if (searchModel == null)
                searchModel = new FactorsSearchModel();


            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<FactorsListModel> PrepareFactorsListModelAsync(FactorsSearchModel searchModel)
        {
            var pagedFactors = await _FactorsService.SearchAsync(
              
                searchModel.Page - 1,
                searchModel.PageSize
            );

            var model = new FactorsListModel().PrepareToGrid(searchModel, pagedFactors, () =>
            {
                return pagedFactors.Select(factor =>
                {
                    var factorModel = new FactorsModel
                    {
                        Id = factor.Id,
                       
                      RoleID= factor.RoleID,
                        Factor = factor.Factor
                    };
                    return factorModel;
                });
            });

            return model;
        }

       
    }
}