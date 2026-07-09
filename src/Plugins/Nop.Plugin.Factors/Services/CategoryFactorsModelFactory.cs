using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Factors.Models;
using Nop.Plugin.Factors.Services;

using Nop.Web.Framework.Models.DataTables;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Factors.Services
{
    public class CategoryFactorsModelFactory : ICategoryFactorsModelFactory
    {
        private readonly ICategoryFactorsService _categoryFactorsService;

        public CategoryFactorsModelFactory(ICategoryFactorsService categoryFactorsService)
        {
            _categoryFactorsService = categoryFactorsService;
        }

        public virtual async Task<CategoryFactorsSearchModel> PrepareCategoryFactorsSearchModelAsync(CategoryFactorsSearchModel searchModel)
        {
            if (searchModel == null)
                searchModel = new CategoryFactorsSearchModel();

            // Get distinct category names for dropdown
            var categoryNames = await _categoryFactorsService.GetDistinctCategoryNamesAsync();

            searchModel.AvailableCategoryNames.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = "",
                Text = "All categories"
            });

            foreach (var categoryName in categoryNames)
            {
                searchModel.AvailableCategoryNames.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = categoryName,
                    Text = categoryName
                });
            }

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<CategoryFactorsListModel> PrepareCategoryFactorsListModelAsync(CategoryFactorsSearchModel searchModel)
        {
            var pagedFactors = await _categoryFactorsService.SearchAsync(
                searchModel.SearchCategoryName,
                searchModel.Page - 1,
                searchModel.PageSize
            );

            var model = new CategoryFactorsListModel().PrepareToGrid(searchModel, pagedFactors, () =>
            {
                return pagedFactors.Select(factor =>
                {
                    var factorModel = new CategoryFactorsModel
                    {
                        Id = factor.Id,
                        CategoryID = factor.CategoryID,
                        Path = factor.Path,
                        Factor = factor.Factor
                    };
                    return factorModel;
                });
            });

            return model;
        }
    }
}