using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Factors.Models;
using Nop.Plugin.Factors.Services;
using Nop.Services.Directory;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.DataTables;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Factors.Services
{
    public interface ICODFactorsModelFactory
    {
        Task<CODFactorsSearchModel> PrepareCODFactorsSearchModelAsync(CODFactorsSearchModel searchModel);
        Task<CODFactorsListModel> PrepareCODFactorsListModelAsync(CODFactorsSearchModel searchModel);
    }

    public class CODFactorsModelFactory : ICODFactorsModelFactory
    {
        private readonly ICODFactorsService _codFactorsService;
        private readonly ICountryService _countryService;

        public CODFactorsModelFactory(
            ICODFactorsService codFactorsService,
            ICountryService countryService)
        {
            _codFactorsService = codFactorsService;
            _countryService = countryService;
        }

        public virtual async Task<CODFactorsSearchModel> PrepareCODFactorsSearchModelAsync(CODFactorsSearchModel searchModel)
        {
            if (searchModel == null)
                searchModel = new CODFactorsSearchModel();

            var countries = await _countryService.GetAllCountriesAsync(showHidden: true);

            searchModel.AvailableCountries.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = "0",
                Text = "All countries"
            });

            foreach (var country in countries)
            {
                searchModel.AvailableCountries.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = country.Id.ToString(),
                    Text = country.Name
                });
            }

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<CODFactorsListModel> PrepareCODFactorsListModelAsync(CODFactorsSearchModel searchModel)
        {
            var pagedFactors = await _codFactorsService.SearchAsync(
                searchModel.SearchCountryId > 0 ? searchModel.SearchCountryId : null,
                searchModel.Page - 1,
                searchModel.PageSize
            );

            var countries = await _countryService.GetAllCountriesAsync(showHidden: true);
            var countryLookup = countries.ToDictionary(c => c.Id, c => c.Name);

            var model = new CODFactorsListModel().PrepareToGrid(searchModel, pagedFactors, () =>
            {
                return pagedFactors.Select(factor =>
                {
                    var factorModel = new CODFactorsModel
                    {
                        Id = factor.Id, // Make sure this is set!
                        CountryID = factor.CountryID,
                        CountryName = countryLookup.TryGetValue(factor.CountryID, out var name) ? name : factor.CountryID.ToString(),
                        Name = factor.Name,
                        CODFactor = factor.CODFactor,
                        FactorID = factor.FactorID
                    };

                  

                    return factorModel;
                });
            });

            return model;
        }
    }
}