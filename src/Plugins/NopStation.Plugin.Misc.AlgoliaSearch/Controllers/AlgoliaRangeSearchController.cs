using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using NopStation.Plugin.Misc.AlgoliaSearch.Factories;
using NopStation.Plugin.Misc.AlgoliaSearch.Models;
using NopStation.Plugin.Misc.Core.Controllers;
using static NopStation.Plugin.Misc.AlgoliaSearch.Models.SearchModel;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Controllers
{
    public class AlgoliaRangeSearchController : NopStationPublicController
    {
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IAlgoliaCatalogModelFactory _algoliaCatalogModelFactory;
        private readonly AlgoliaSearchSettings _algoliaSearchSettings;

        public AlgoliaRangeSearchController(
            IWorkContext workContext,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IAlgoliaCatalogModelFactory algoliaCatalogModelFactory,
            AlgoliaSearchSettings algoliaSearchSettings)
        {
            _workContext = workContext;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _algoliaCatalogModelFactory = algoliaCatalogModelFactory;
            _algoliaSearchSettings = algoliaSearchSettings;
        }
        private static readonly HashSet<string> AllowedRangeAttributes =
      new(StringComparer.OrdinalIgnoreCase)
      {
        "InnerDiameter",
        "OuterDiameter",
        "Thickness"
      };




        //[HttpGet]
        //public async Task<IActionResult> Range(string q, AlgoliaPagingFilteringModel command)
        //{
        //    // allow empty q
        //    q = (q ?? "").Trim();

        //    var attrs = Request.Query["attr"];      
        //    var froms = Request.Query["from"];     
        //    var tos = Request.Query["to"];       


        //    var ranges = new List<(string Attr, decimal? From, decimal? To)>();
        //    for (int i = 0; i < attrs.Count; i++)
        //    {
        //        var attr = attrs[i];
        //        if (string.IsNullOrWhiteSpace(attr))
        //            continue;

        //        if (!AllowedRangeAttributes.Contains(attr))
        //            continue;

        //        decimal? f = null, t = null;

        //        if (i < froms.Count && decimal.TryParse(froms[i], System.Globalization.NumberStyles.Any,
        //                System.Globalization.CultureInfo.InvariantCulture, out var fval))
        //            f = fval;

        //        if (i < tos.Count && decimal.TryParse(tos[i], System.Globalization.NumberStyles.Any,
        //                System.Globalization.CultureInfo.InvariantCulture, out var tval))
        //            t = tval;


        //        if (!f.HasValue && !t.HasValue)
        //            continue;

        //        ranges.Add((attr, f, t));
        //    }

        //    var model = new SearchModel
        //    {
        //        q = q,

        //        RangeFilters = ranges
        //            .Select(r => new RangeFilterModel { Attribute = r.Attr, From = r.From, To = r.To })
        //            .ToList()
        //    };



        //    model = await _algoliaCatalogModelFactory.PrepareSearchModelAsync(model, command);

        //    return View("~/Plugins/NopStation.Plugin.Misc.AlgoliaSearch/Views/AlgoliaSearch/Search.cshtml", model);
        //}


   
        [HttpGet]
        public async Task<IActionResult> Range(string q, AlgoliaPagingFilteringModel command)
        {
            q = (q ?? "").Trim();

            var rawAttrs = Request.Query["attr"].ToList();
            var rawFroms = Request.Query["from"].ToList();
            var rawTos = Request.Query["to"].ToList();

            var attrs = rawAttrs
                .SelectMany(x => (x ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(x => x.Trim())
                .ToList();

            var froms = rawFroms
                .SelectMany(x => (x ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(x => x.Trim())
                .ToList();

            var tos = rawTos
                .SelectMany(x => (x ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(x => x.Trim())
                .ToList();

            var rangeFilters = new List<RangeFilterModel>();

            for (int i = 0; i < attrs.Count; i++)
            {
                var attr = attrs[i];

                if (string.IsNullOrWhiteSpace(attr))
                    continue;

                if (!AllowedRangeAttributes.Contains(attr))
                    continue;

                decimal? from = null;
                decimal? to = null;

                if (i < froms.Count &&
                    decimal.TryParse(froms[i], NumberStyles.Any, CultureInfo.InvariantCulture, out var f))
                {
                    from = f;
                }

                if (i < tos.Count &&
                    decimal.TryParse(tos[i], NumberStyles.Any, CultureInfo.InvariantCulture, out var t))
                {
                    to = t;
                }

                if (!from.HasValue && !to.HasValue)
                    continue;

                rangeFilters.Add(new RangeFilterModel
                {
                    Attribute = attr,
                    From = from,
                    To = to
                });
            }

            var model = new SearchModel
            {
                q = q,
                RangeFilters = rangeFilters
            };

            await _genericAttributeService.SaveAttributeAsync(
                await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(true),
                (await _storeContext.GetCurrentStoreAsync()).Id);

            model = await _algoliaCatalogModelFactory.PrepareSearchModelAsync(model, command);

            return View("~/Plugins/NopStation.Plugin.Misc.AlgoliaSearch/Views/AlgoliaSearch/Search.cshtml", model);
        }


    }
}
