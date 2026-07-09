using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Data;
using Nop.Plugin.Recommendations.SimilarProducts.Domains;
using Nop.Plugin.Recommendations.SimilarProducts.Models;
using Nop.Plugin.Recommendations.SimilarProducts.Models.Admin;
using Nop.Plugin.Recommendations.SimilarProducts.Services;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Recommendations.Controllers
{
    public class RecommendationsSimilarProductsController : BasePluginController
    {
        private readonly IFeaturesConfigurationService _configurationService;
        private readonly ISimilarProductsDiscoveryService _similarProductsService;
        private readonly ILogger _logger;
        private readonly ISimilarProductsPersistanceService _persistence;
        private readonly IProductService _productService;
        private readonly INotificationService _notificationService;
        private readonly ICategoryService _categoryService;


        public RecommendationsSimilarProductsController(
           IFeaturesConfigurationService configurationService,
           ISimilarProductsDiscoveryService similarProductsService,
           ILogger logger,
           ISimilarProductsPersistanceService persistence, INotificationService notificationService, ICategoryService categoryService,
           IProductService productService)
        {
            _configurationService = configurationService;
            _similarProductsService = similarProductsService;
            _logger = logger;
            _persistence = persistence;
            _productService = productService;
            _notificationService = notificationService;
            _categoryService = categoryService;
        }
        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin] //confirms access to the admin panel
        [Area(AreaNames.ADMIN)] //specifies the area containing a controller or action
        public async Task<IActionResult> Configure()
        {
            var settings = await _configurationService.GetAsync();

            var model = settings is null ?
                new ConfigurationModel() :
                new ConfigurationModel(settings);

            return View("~/Plugins/Recommendations.SimilarProducts/Views/Configure.cshtml", model);
        }

        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            var settings = (await _configurationService.GetAsync()) ?? new FeaturesConfigurationRecord();

            settings.ProductFeaturesEnabled = model.GetFeaturesAsSumOfFlags();
            settings.MinAcceptedValueOfSimilarity = model.MinAcceptedValueOfSimilarity;
            settings.NumOfSimilarProductsToDiscover = model.NumOfSimilarProductsToDiscover;
            settings.NumOfSimilarProductsToDisplay = model.NumOfSimilarProductsToDisplay;

            await _configurationService.AddOrUpdateAsync(settings);

            return View("~/Plugins/Recommendations.SimilarProducts/Views/Configure.cshtml", model);
        }

        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        public async Task<IActionResult> TrainModel()
        {
            var pluginSettings = await _configurationService.GetAsync();

            var model = pluginSettings is null ?
                new ConfigurationModel() :
                new ConfigurationModel(pluginSettings);

            var appSettings = DataSettingsManager.LoadSettings();

            try
            {
                await _similarProductsService.TrainModelAndSaveSimilarProductsAsync(pluginSettings, appSettings);
            }
            catch(InvalidOperationException ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
                model.DisplayConfigurationTip = true;
            }

            return View("~/Plugins/Recommendations.SimilarProducts/Views/Configure.cshtml", model);
        }

       
        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        public async Task<IActionResult> Manual(int page = 1, int pageSize = 20)
        {
            var pageIndex = page - 1;
            var pageResult = await _persistence.SearchAsync(pageIndex: pageIndex, pageSize: pageSize);

            var productIds = pageResult.Select(x => x.ProductId)
                                       .Concat(pageResult.Select(x => x.SimilarProductId))
                                       .Distinct()
                                       .ToList();

            var products = await _productService.GetProductsByIdsAsync(productIds.ToArray());
            var dict = products.ToDictionary(p => p.Id, p => p.Name);

            var items = pageResult.Select(r => new SimilarityListItemModel
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = dict.ContainsKey(r.ProductId) ? dict[r.ProductId] : r.ProductId.ToString(),
                SimilarProductId = r.SimilarProductId,
                SimilarProductName = dict.ContainsKey(r.SimilarProductId) ? dict[r.SimilarProductId] : r.SimilarProductId.ToString(),
                SimilarityPercent = (int)System.Math.Round(r.Similarity * 100.0),
                CreatedOnUtc = r.CreatedOnUtc
            }).ToList();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = pageResult.TotalCount;

            return View("~/Plugins/Recommendations.SimilarProducts/Views/Manual.cshtml", items);
        }

        // 4.2 إنشاء سجل (POST)
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        public async Task<IActionResult> Create([FromForm] SimilarityCreateModel model)
        {
            if (!ModelState.IsValid)
                return await Manual();

            if (model.SimilarProductIds == null || model.SimilarProductIds.Length == 0)
            {
                ModelState.AddModelError("", "Select at least one similar product.");
                return await Manual();
            }

          
            var targets = model.SimilarProductIds
                .Where(id => id > 0 && id != model.ProductId)
                .Distinct()
                .ToList();

            if (targets.Count == 0)
            {
                ModelState.AddModelError("", "Products must be different.");
                return await Manual();
            }

          
            foreach (var spId in targets)
            {
                var exists = await _persistence.SearchAsync(
                    productId: model.ProductId,
                    similarProductId: spId,
                    pageIndex: 0, pageSize: 1);

                if (exists.TotalCount > 0)
                    continue; 

                var record = new SimilarProductRecord
                {
                    ProductId = model.ProductId,
                    SimilarProductId = spId,
                    Similarity = model.SimilarityPercent / 100.0
                };

                await _persistence.InsertAsync(record);
            }

            _notificationService.SuccessNotification("Similarities added successfully");
            return RedirectToAction(nameof(Manual));
        }

        // 4.3 حذف سجل
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        public async Task<IActionResult> Delete(int id)
        {
            await _persistence.DeleteAsync(id);
            _notificationService.SuccessNotification("Deleted");
            return RedirectToAction(nameof(Manual));
        }

       
        [HttpGet]
        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        public async Task<IActionResult> SearchProductsSimple(string term = "", int take = 20)
        {
           
            var products = await _productService.SearchProductsAsync(
                keywords: term,
                pageIndex: 0,
                pageSize: take);

            var result = products.Select(p => new { id = p.Id, text = $"{p.Name} (#{p.Id})" });
            return Json(result);
        }





      
        [HttpGet]
        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        public async Task<IActionResult> GetCategories(string term = "", int take = 50)
        {
          
            var cats = await _categoryService.GetAllCategoriesAsync(showHidden: true);
            if (!string.IsNullOrWhiteSpace(term))
                cats = cats.Where(c => c.Name.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();

            var result = cats.Take(take)
                             .Select(c => new { id = c.Id, text = c.Name })
                             .ToList();

            return Json(result);
        }

        [HttpGet]
        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        public async Task<IActionResult> SearchProducts(string term = "", int categoryId = 0, int page = 1, int take = 20)
        {
           
            if (page < 1)
                page = 1;
            if (take < 1)
                take = 20;

            var pageIndex = page - 1;
            term = term?.Trim() ?? string.Empty;

            
            var pagedProducts = await _productService.SearchProductsAsync(
                categoryIds: categoryId > 0 ? new List<int> { categoryId } : null,
                keywords: term,
                searchDescriptions: true,
                pageIndex: pageIndex,
                pageSize: take,
                showHidden: true
            );

         
            var resultList = pagedProducts.ToList();

           
            if (!string.IsNullOrWhiteSpace(term))
            {
                var exactBySku = await _productService.GetProductBySkuAsync(term);
                if (exactBySku != null && !resultList.Any(p => p.Id == exactBySku.Id))
                {
                    var inCategory = categoryId == 0;

                   
                    if (!inCategory)
                    {
                        var mappings = await _categoryService.GetProductCategoriesByProductIdAsync(exactBySku.Id);
                        inCategory = mappings?.Any(m => m.CategoryId == categoryId) == true;
                    }

                    if (inCategory)
                        resultList.Insert(0, exactBySku);
                }
            }

          
            var items = resultList.Select(p => new
            {
                id = p.Id,
                text = p.Name,
                sku = p.Sku
            }).ToList();

         
            var totalCount = pagedProducts.TotalCount;
            var hasMore = (page * take) < totalCount;

            return Json(new { items, hasMore });
        }





    }



}

