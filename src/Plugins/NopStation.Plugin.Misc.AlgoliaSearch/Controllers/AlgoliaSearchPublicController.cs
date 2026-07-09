using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Seo;
using Nop.Web.Controllers;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.AlgoliaSearch.Extensions;
using NopStation.Plugin.Misc.AlgoliaSearch.Factories;
using NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Controllers
{
    public class AlgoliaSearchPublicController : BasePublicController
    {
        private readonly IAlgoliaHelperFactory _algoliaHelperFactory;
        private readonly IProductService _productService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;

        public AlgoliaSearchPublicController(
            IAlgoliaHelperFactory algoliaHelperFactory,
            IProductService productService,
            IManufacturerService manufacturerService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper)
        {
            _algoliaHelperFactory = algoliaHelperFactory;
            _productService = productService;
            _manufacturerService = manufacturerService;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
        }


        [HttpGet]
        public async Task<IActionResult> AutoComplete(string term, int size = 6)
        {
            term = (term ?? "").Trim();
            if (term.Length < 2)
                return Json(Enumerable.Empty<object>());

            size = Math.Clamp(size, 1, 20);

            var productsPaged = await _algoliaHelperFactory.SearchProductsAsync(
                searchTerms: term,
                pageIndex: 0,
                pageSize: size,
                orderby: 0
            );

            if (productsPaged == null || productsPaged.Count == 0)
                return Json(Enumerable.Empty<object>());

            var idsShown = productsPaged.Select(p => p.Id).ToList();

            var hasSubSet = await _algoliaHelperFactory.GetProductsThatHaveSubstitutesAsync(idsShown);

            var result = await Task.WhenAll(productsPaged.Select(async p =>
            {
                var url = _webHelper.GetStoreLocation() + p.SeName;

                string manufacturerName = "";
                var mappings = await _manufacturerService.GetProductManufacturersByProductIdAsync(p.Id);
                var firstMap = mappings.FirstOrDefault();
                if (firstMap != null)
                {
                    var manu = await _manufacturerService.GetManufacturerByIdAsync(firstMap.ManufacturerId);
                    if (manu != null && !manu.Deleted)
                        manufacturerName = manu.Name ?? "";
                }

              
                var isSub = false;
                if (p.CustomProperties != null
                    && p.CustomProperties.TryGetValue("IsSubstituteResult", out var v)
                    && !string.IsNullOrWhiteSpace(v))
                {
                    isSub = v.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                            v.Equals("1");
                }

                return new
                {
                    Id = p.Id,
                    Name = p.Name,
                    Url = url,
                    ManufacturerName = manufacturerName,
                    AutoCompleteImageUrl = await _algoliaHelperFactory.GetAutoCompleteImageUrlByProductIdAsync(p.Id),

                  
                    HasSubstitute = hasSubSet.Contains(p.Id),

                  
                    IsSubstituteResult = isSub
                };
            }));

            return Json(result);
        }

    }















}

