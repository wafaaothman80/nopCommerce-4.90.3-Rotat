using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Data;
using Microsoft.AspNetCore.Mvc;
using Nop.Data.DataProviders;
using Nop.Plugin.Recommendations.SimilarProducts.Models;
using Nop.Plugin.Recommendations.SimilarProducts.Services;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Recommendations.SimilarProducts.Components
{
    [ViewComponent(Name = "SimilarProducts")]
    public class SimilarProductsViewComponent : NopViewComponent
    {
        private readonly ISimilarProductsDiscoveryService _simProdService;
        private readonly IAclService _aclService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IFeaturesConfigurationService _configurationService;

        public SimilarProductsViewComponent(
            ISimilarProductsDiscoveryService simProdService,
            IAclService aclService,
            IProductModelFactory productModelFactory,
            IProductService productService,            
            IStoreMappingService storeMappingService,
            IFeaturesConfigurationService configurationService)
        {
            _simProdService = simProdService;
            _aclService = aclService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _storeMappingService = storeMappingService;
            _configurationService = configurationService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var productId = (int)HttpContext.Request.RouteValues["productid"];

            var settings = await _configurationService.GetAsync();

            var simProducts = (await _simProdService.GetSimilarProductsAsync(productId, settings.NumOfSimilarProductsToDiscover))
                                .OrderByDescending(p => p.Similarity)
                                .ToDictionary(p => p.ProductId);

            var productIds = simProducts.Keys.ToArray();

            //load products
            var products = await (await _productService.GetProductsByIdsAsync(productIds))
            //ACL and store mapping
            .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
            //availability dates
            .Where(p => _productService.ProductIsAvailable(p))
            //visible individually
            .Where(p => p.VisibleIndividually)
            .Take(settings.NumOfSimilarProductsToDisplay)
            .ToListAsync();
             var substituteCodeByNopId = new Dictionary<int, string>();

            try
            {
                var sql = @"
       SELECT DISTINCT TOP (@take)
    snp.substitutesId    AS SubstitutesId,
    snp.substitute_code  AS SubstituteCode
  
FROM [substitutesNonProduct] snp WITH (NOLOCK)

WHERE snp.ProductId =@productId
  AND snp.substitutesId > 0
  AND NULLIF(LTRIM(RTRIM(snp.substitute_code)), '') IS NOT NULL";
                MsSqlNopDataProvider msSqlNopDataProvider33 = new MsSqlNopDataProvider();
                var subsRows = await msSqlNopDataProvider33.QueryAsync<SubstituteRow>(
                    sql,
                    new DataParameter("take", 50),
                    new DataParameter("productId", productId)
                );


               
                substituteCodeByNopId = subsRows
  .Where(x => x.substitutesId > 0 && !string.IsNullOrWhiteSpace(x.SubstituteCode))
    .GroupBy(x => x.substitutesId)
    .ToDictionary(g => g.Key, g => g.First().SubstituteCode.Trim());
                ViewBag.SubstituteCodeByNopId = substituteCodeByNopId;

            }
            catch (Exception)
            {


            }
            const string viewPath = "~/Plugins/Recommendations.SimilarProducts/Views/SimilarProducts.cshtml";


            if (!products.Any())
            {
                if (substituteCodeByNopId.Count > 0)
                {
                    return View<IList<SimilarProductOverviewModel>>(viewPath, (IList<SimilarProductOverviewModel>)null);

                }
                return Content(string.Empty);
            }


            //var model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, null))
            //    .Select(m => new SimilarProductOverviewModel() { Product = m, Similarity = simProducts[m.Id].Similarity })
            //    .OrderByDescending(m => m.Similarity)
            //    .ToList();

            // Build model with stock quantity
            var modelWithStock = await Task.WhenAll(
                (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, null))
                .Select(async m =>
                {
                    var product = products.First(p => p.Id == m.Id);
                    var stockQty = await _productService.GetTotalStockQuantityAsync(product);
                    return new SimilarProductOverviewModel
                    {
                        Product = m,
                        Similarity = simProducts[m.Id].Similarity,
                        StockQuantity = stockQty
                    };
                })
            );

            var model = modelWithStock
                .OrderByDescending(m => m.StockQuantity)
                .ThenByDescending(m => m.Similarity)
                .ToList();



            return View("~/Plugins/Recommendations.SimilarProducts/Views/SimilarProducts.cshtml", model);
        }
       
    }


    public record class SubstituteRow
    {
        public int substitutesId { get; set; }
        public string SubstituteCode { get; set; }
       
    }

}
