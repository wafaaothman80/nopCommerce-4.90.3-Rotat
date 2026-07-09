using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Factors.Models;
using Nop.Plugin.Factors.Services;

using Nop.Web.Framework.Models.DataTables;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Factors.Services
{
    public class BrandFactorsModelFactory : IBrandFactorsModelFactory
    {
        private readonly IBrandFactorsService _BrandFactorsService;

        public BrandFactorsModelFactory(IBrandFactorsService BrandFactorsService)
        {
            _BrandFactorsService = BrandFactorsService;
        }

        public virtual async Task<BrandsFactorsSearchModel> PrepareBrandFactorsSearchModelAsync(BrandsFactorsSearchModel searchModel)
        {
            if (searchModel == null)
                searchModel = new BrandsFactorsSearchModel();

            // Get distinct Brand names for dropdown
            var BrandNames = await _BrandFactorsService.GetDistinctBrandNamesAsync();

            searchModel.AvailableBrandNames.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = "",
                Text = "All Brands"
            });

            foreach (var BrandName in BrandNames)
            {
                searchModel.AvailableBrandNames.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = BrandName,
                    Text = BrandName
                });
            }

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<BrandsFactorsListModel> PrepareBrandFactorsListModelAsync(BrandsFactorsSearchModel searchModel)
        {
            var pagedFactors = await _BrandFactorsService.SearchAsync(
                searchModel.SearchBrandName,
                searchModel.Page - 1,
                searchModel.PageSize
            );

            var model = new BrandsFactorsListModel().PrepareToGrid(searchModel, pagedFactors, () =>
            {
                return pagedFactors.Select(factor =>
                {
                    var factorModel = new BrandsFactorsModel
                    {
                        Id = factor.Id,
                        BrandID = factor.BrandID,
                        Name = factor.Name,
                        Factor = factor.Factor
                    };
                    return factorModel;
                });
            });

            return model;
        }
    }
}