using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Vendors;
using NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure;
using NopStation.Plugin.Misc.AlgoliaSearch.Models;
using NopStation.Plugin.Misc.AlgoliaSearch.Services;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Factories
{
    public class AlgoliaCatalogModelFactory : IAlgoliaCatalogModelFactory
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ICategoryService _categoryService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly AlgoliaSearchSettings _algoliaSearchSettings;
        private readonly IAlgoliaHelperFactory _algoliaHelperFactory;
        private readonly IAlgoliaCatalogService _algoliaCatalogService;
        private readonly IPriceFormatter _priceFormatter;

        #endregion

        #region Ctor

        public AlgoliaCatalogModelFactory(IWebHelper webHelper,
            ISpecificationAttributeService specificationAttributeService,
            ICategoryService categoryService,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            IStaticCacheManager cacheManager,
            IVendorService vendorService,
            IWorkContext workContext,
            AlgoliaSearchSettings algoliaSearchSettings,
            IAlgoliaHelperFactory algoliaHelperFactory,
            IAlgoliaCatalogService algoliaCatalogService,
            IPriceFormatter priceFormatter)
        {
            _webHelper = webHelper;
            _specificationAttributeService = specificationAttributeService;
            _categoryService = categoryService;
            _currencyService = currencyService;
            _localizationService = localizationService;
            _cacheManager = cacheManager;
            _vendorService = vendorService;
            _workContext = workContext;
            _algoliaSearchSettings = algoliaSearchSettings;
            _algoliaHelperFactory = algoliaHelperFactory;
            _algoliaCatalogService = algoliaCatalogService;
            _priceFormatter = priceFormatter;
        }

        #endregion

        #region Common

        public virtual async Task PrepareSortingOptionsAsyync(AlgoliaPagingFilteringModel pagingFilteringModel, AlgoliaPagingFilteringModel command)
        {
            if (pagingFilteringModel == null)
                throw new ArgumentNullException(nameof(pagingFilteringModel));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

          
            var incoming = command.OrderBy.HasValue && Enum.IsDefined(typeof(AlgoliaSortingEnum), command.OrderBy.Value)
                ? command.OrderBy.Value
                : 0;

          
            if (!_algoliaSearchSettings.AllowProductSorting)
            {
                pagingFilteringModel.AllowProductSorting = false;
                pagingFilteringModel.OrderBy = 0;
                command.OrderBy = 0;
                return;
            }

            var sortingOptions = _algoliaSearchSettings.AllowedSortingOptions ?? new List<int>();
            if (!sortingOptions.Contains(0))
                sortingOptions.Insert(0, 0);

         
            command.OrderBy = sortingOptions.Contains(incoming) ? incoming : sortingOptions.FirstOrDefault();
            pagingFilteringModel.AllowProductSorting = true;
            pagingFilteringModel.OrderBy = command.OrderBy;

            var currentPageUrl = _webHelper.GetThisPageUrl(true);
            foreach (var option in sortingOptions)
            {
                pagingFilteringModel.AvailableSortOptions.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedEnumAsync((AlgoliaSortingEnum)option),
                    Value = _webHelper.ModifyQueryString(currentPageUrl, "orderby", option.ToString()),
                    Selected = option == command.OrderBy
                });
            }
        }




        public virtual async Task PrepareViewModesAsync(AlgoliaPagingFilteringModel pagingFilteringModel, AlgoliaPagingFilteringModel command)
        {
            if (pagingFilteringModel == null)
                throw new ArgumentNullException(nameof(pagingFilteringModel));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            pagingFilteringModel.AllowProductViewModeChanging = _algoliaSearchSettings.AllowProductViewModeChanging;

            var viewMode = !string.IsNullOrEmpty(command.ViewMode)
                ? command.ViewMode
                : _algoliaSearchSettings.DefaultViewMode;
            pagingFilteringModel.ViewMode = viewMode;
            if (pagingFilteringModel.AllowProductViewModeChanging)
            {
                var currentPageUrl = _webHelper.GetThisPageUrl(true);
                //grid
                pagingFilteringModel.AvailableViewModes.Add(new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync("Catalog.ViewMode.Grid"),
                    Value = _webHelper.ModifyQueryString(currentPageUrl, "viewmode", "grid"),
                    Selected = viewMode == "grid"
                });
                //list
                pagingFilteringModel.AvailableViewModes.Add(new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync("Catalog.ViewMode.List"),
                    Value = _webHelper.ModifyQueryString(currentPageUrl, "viewmode", "list"),
                    Selected = viewMode == "list"
                });
            }
        }

        public virtual void PreparePageSizeOptions(AlgoliaPagingFilteringModel pagingFilteringModel, AlgoliaPagingFilteringModel command,
            bool allowCustomersToSelectPageSize, string pageSizeOptions, int fixedPageSize)
        {
            if (pagingFilteringModel == null)
                throw new ArgumentNullException(nameof(pagingFilteringModel));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (command.PageNumber <= 0)
            {
                command.PageNumber = 1;
            }
            pagingFilteringModel.AllowCustomersToSelectPageSize = false;
            if (allowCustomersToSelectPageSize && pageSizeOptions != null)
            {
                var pageSizes = pageSizeOptions.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (pageSizes.Any())
                {
                    // get the first page size entry to use as the default (category page load) or if customer enters invalid value via query string
                    if (command.PageSize <= 0 || !pageSizes.Contains(command.PageSize.ToString()))
                    {
                        if (int.TryParse(pageSizes.FirstOrDefault(), out int temp))
                        {
                            if (temp > 0)
                            {
                                command.PageSize = temp;
                            }
                        }
                    }

                    var currentPageUrl = _webHelper.GetThisPageUrl(true);
                    var sortUrl = _webHelper.RemoveQueryString(currentPageUrl, "pagenumber");

                    foreach (var pageSize in pageSizes)
                    {
                        if (!int.TryParse(pageSize, out int temp))
                        {
                            continue;
                        }
                        if (temp <= 0)
                        {
                            continue;
                        }

                        pagingFilteringModel.PageSizeOptions.Add(new SelectListItem
                        {
                            Text = pageSize,
                            Value = _webHelper.ModifyQueryString(sortUrl, "pagesize", pageSize),
                            Selected = pageSize.Equals(command.PageSize.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        });
                    }

                    if (pagingFilteringModel.PageSizeOptions.Any())
                    {
                        pagingFilteringModel.PageSizeOptions = pagingFilteringModel.PageSizeOptions.OrderBy(x => int.Parse(x.Text)).ToList();
                        pagingFilteringModel.AllowCustomersToSelectPageSize = true;

                        if (command.PageSize <= 0)
                        {
                            command.PageSize = int.Parse(pagingFilteringModel.PageSizeOptions.First().Text);
                        }
                    }
                }
            }
            else
            {
                //customer is not allowed to select a page size
                command.PageSize = fixedPageSize;
            }

            //ensure pge size is specified
            if (command.PageSize <= 0)
            {
                command.PageSize = fixedPageSize;
            }
        }

        protected void PrepareFilterOptions(AlgoliaPagingFilteringModel pagingFilteringContext)
        {
            pagingFilteringContext.AllowProductSorting = _algoliaSearchSettings.AllowProductSorting;
            pagingFilteringContext.AllowCustomersToSelectPageSize = _algoliaSearchSettings.AllowCustomersToSelectPageSize;
            pagingFilteringContext.AllowProductViewModeChanging = _algoliaSearchSettings.AllowProductViewModeChanging;
            pagingFilteringContext.ShowProductsCount = _algoliaSearchSettings.ShowProductsCount;
        }

        #endregion

        public async Task<SearchModel> PrepareSearchModelAsync(SearchModel model, AlgoliaPagingFilteringModel command)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var searchTerms = model.q?.Trim() ?? "";

           
            var onlyInStockRaw = _webHelper.QueryString<string>("OnlyInStock");
            var onlyInStock = !string.IsNullOrWhiteSpace(onlyInStockRaw) && onlyInStockRaw.Equals("true", StringComparison.OrdinalIgnoreCase);
            command.OnlyInStock = onlyInStock;
            model.PagingFilteringContext.OnlyInStock = onlyInStock;

            var discountOnlyRaw = _webHelper.QueryString<string>("DiscountOnly");
            var discountOnly = !string.IsNullOrWhiteSpace(discountOnlyRaw) && discountOnlyRaw.Equals("true", StringComparison.OrdinalIgnoreCase);
            command.DiscountOnly = discountOnly;
            model.PagingFilteringContext.DiscountOnly = discountOnly;

           
            await PrepareSortingOptionsAsyync(model.PagingFilteringContext, command);
            if (onlyInStock)
                command.OrderBy = (int)AlgoliaSortingEnum.StockQty;

            await PrepareViewModesAsync(model.PagingFilteringContext, command);
            PreparePageSizeOptions(model.PagingFilteringContext, command,
                _algoliaSearchSettings.AllowCustomersToSelectPageSize,
                _algoliaSearchSettings.SearchPagePageSizeOptions,
                _algoliaSearchSettings.SearchPageProductsPerPage);

           
            var selectedCatIds = model.PagingFilteringContext.CategoryFilter.GetAlreadyFilteredCategoryIds(_webHelper);
            var selectedManfIds = model.PagingFilteringContext.ManufacturerFilter.GetAlreadyFilteredManufacturerIds(_webHelper);
            var selectedVendIds = model.PagingFilteringContext.VendorFilter.GetAlreadyFilteredVendorIds(_webHelper);
            var selectedSpecIds = model.PagingFilteringContext.SpecificationFilter.GetAlreadyFilteredSpecOptionIds(_webHelper);
            var selectedRatings = model.PagingFilteringContext.RatingFilter.GetAlreadyFilteredRatingIds(_webHelper);
            var selectedPriceRange = model.PagingFilteringContext.PriceRangeFilter.GetSelectedPriceRange(_webHelper);

          
            decimal? minPriceConverted = null, maxPriceConverted = null;
            if (selectedPriceRange != null)
            {
                if (selectedPriceRange.From.HasValue)
                    minPriceConverted = await _currencyService.ConvertToPrimaryStoreCurrencyAsync(
                        selectedPriceRange.From.Value, await _workContext.GetWorkingCurrencyAsync());
                if (selectedPriceRange.To.HasValue)
                    maxPriceConverted = await _currencyService.ConvertToPrimaryStoreCurrencyAsync(
                        selectedPriceRange.To.Value, await _workContext.GetWorkingCurrencyAsync());
            }

           
            var categoryFacets = await _algoliaHelperFactory.GetAlgoliaFiltersAsync(
                searchTerms: searchTerms,
                cids: null,                    
                mids: selectedManfIds,
                vids: selectedVendIds,
                specids: selectedSpecIds,
                ratings: selectedRatings,
                minPrice: minPriceConverted,
                maxPrice: maxPriceConverted,
                onlyInStock: onlyInStock,
                discountOnly: discountOnly
            );

          
            var otherFacets = await _algoliaHelperFactory.GetAlgoliaFiltersAsync(
                searchTerms: searchTerms,
                cids: selectedCatIds,          
                mids: selectedManfIds,
                vids: selectedVendIds,
                specids: selectedSpecIds,
                ratings: selectedRatings,
                minPrice: minPriceConverted,
                maxPrice: maxPriceConverted,
                onlyInStock: onlyInStock,
                discountOnly: discountOnly
            );

           
            var products = await _algoliaHelperFactory.SearchProductsAsync(
                searchTerms: searchTerms,
                cids: selectedCatIds,
                mids: selectedManfIds,
                vids: selectedVendIds,
                specids: selectedSpecIds,
                ratings: selectedRatings,
                maxPrice: maxPriceConverted,
                minPrice: minPriceConverted,
                orderby: command.OrderBy,
                pageIndex: command.PageIndex,
                pageSize: command.PageSize,
                rangeFilters: model.RangeFilters,
                onlyInStock: onlyInStock,
                discountOnly: discountOnly
            );

            model.Products = products;
            model.PagingFilteringContext.LoadPagedList(products);

          
            await model.PagingFilteringContext.CategoryFilter.PrepareCategoriesFiltersAsync(
                selectedCatIds,
                categoryFacets.AvailableCategories,
                _categoryService,
                _localizationService,
                _webHelper,
                _workContext
            );

            await model.PagingFilteringContext.ManufacturerFilter.PrepareManufacsFiltersAsync(
                selectedManfIds,
                otherFacets.AvailableManufacturers,
                _algoliaCatalogService,
                _localizationService,
                _webHelper,
                _workContext
            );

            await model.PagingFilteringContext.VendorFilter.PrepareVendorsFiltersAsync(
                selectedVendIds,
                otherFacets.AvailableVendors,
                _vendorService,
                _localizationService,
                _webHelper,
                _workContext
            );

            await model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFiltersAsync(
                selectedSpecIds,
                otherFacets.AvailableSpecifications,
                _specificationAttributeService,
                _localizationService,
                _webHelper,
                _workContext,
                _cacheManager
            );

            await model.PagingFilteringContext.RatingFilter.PrepareRatingsFiltersAsync(
                selectedRatings,
                otherFacets.AvailableRatings,
                _localizationService,
                _webHelper,
                _workContext
            );

            await model.PagingFilteringContext.PriceRangeFilter.PreparePriceRangeFiltersAsync(
                selectedPriceRange,
                otherFacets.MinPrice,
                otherFacets.MaxPrice,
                _currencyService,
                _webHelper,
                _workContext,
                _priceFormatter
            );

            return model;
        }

        
    }
}
