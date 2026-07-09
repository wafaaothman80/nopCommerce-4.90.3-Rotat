using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Algolia.Search;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Models;
using NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure;
using NopStation.Plugin.Misc.AlgoliaSearch.Models;
using NopStation.Plugin.Misc.AlgoliaSearch.Services;
using static NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Models.AlgoliaOverviewModel;
using static NopStation.Plugin.Misc.AlgoliaSearch.Models.SearchModel;


using Algolia.Search.Models;
using Newtonsoft.Json.Linq;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Factories
{
    public class AlgoliaHelperFactory : IAlgoliaHelperFactory
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly CatalogSettings _catalogSettings;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly ProductUploadHub _productUploadHub;
        private readonly IAlgoliaCatalogService _algoliaCatalogService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly AlgoliaSearchSettings _algoliaSearchSettings;
        private readonly IProductService _productService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IPictureService _pictureService;
        private readonly IVendorService _vendorService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IAlgoliaUpdatableItemService _algoliaUpdatableItemService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ICurrencyService _currencyService;
        private readonly IProductTagService _productTagService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly INopDataProvider _dataProvider;
        private readonly ILanguageService _languageService;
        private readonly ISubstitutesService _substitutesService;


        #endregion

        #region Ctor

        public AlgoliaHelperFactory(ILogger logger,
            CatalogSettings catalogSettings,
            IStoreMappingService storeMappingService,
            ISettingService settingService,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            ProductUploadHub productUploadHub,
            IAlgoliaCatalogService algoliaCatalogService,
            IProductModelFactory productModelFactory,
            AlgoliaSearchSettings algoliaSearchSettings,
            IProductService productService,
            IStaticCacheManager staticCacheManager,
            IWebHelper webHelper,
            IWorkContext workContext,
            IPictureService pictureService,
            IVendorService vendorService,
            IUrlRecordService urlRecordService,
            IAlgoliaUpdatableItemService algoliaUpdatableItemService,
            ISpecificationAttributeService specificationAttributeService,
            IProductAttributeService productAttributeService,
            ICurrencyService currencyService,
            IProductTagService productTagService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            INopDataProvider dataProvider,
            ILanguageService languageService,
            ISubstitutesService substitutesService)
        {
            _logger = logger;
            _catalogSettings = catalogSettings;
            _storeMappingService = storeMappingService;
            _settingService = settingService;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _productUploadHub = productUploadHub;
            _algoliaCatalogService = algoliaCatalogService;
            _productModelFactory = productModelFactory;
            _algoliaSearchSettings = algoliaSearchSettings;
            _productService = productService;
            _cacheManager = staticCacheManager;
            _webHelper = webHelper;
            _workContext = workContext;
            _pictureService = pictureService;
            _vendorService = vendorService;
            _urlRecordService = urlRecordService;
            _algoliaUpdatableItemService = algoliaUpdatableItemService;
            _specificationAttributeService = specificationAttributeService;
            _productAttributeService = productAttributeService;
            _currencyService = currencyService;
            _productTagService = productTagService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _dataProvider = dataProvider;
            _languageService = languageService;
            _substitutesService = substitutesService;
        }

        #endregion

        #region Utilities

        protected Algolia.Search.Index GetDefaultIndex(AlgoliaClient client, ConfigurationModel model, out dynamic settings)
        {
            var allIndices = client.ListIndexes();
            settings = new JObject();

            var searchableAttributes = AlgoliaDefaults.SearchableAttributes;
            if (_algoliaSearchSettings.EnableMultilingualSearch)
            {
                var languages = _languageService.GetAllLanguages();
                foreach (var language in languages)
                {
                    searchableAttributes = searchableAttributes
                        .Append(string.Format(AlgoliaDefaults.MultilingualProductNameFormate, language.UniqueSeoCode))
                        .ToArray();
                }
            }


            JArray numericAttrs = new JArray(new[]
       {
    "Id",
    "FilterableCategories.Id",
    "FilterableManufacturers.Id",
    "FilterableVendor.Id",
    "Rating",
    "LimitedToStores",
    "Stores",
    "PriceValue",
    "InnerDiameter",
    "OuterDiameter",
    "Thickness",
    "StockQty",
    "InStock","IsDiscounted"
});
            if (!allIndices["items"].Any(x => x["name"].ToString().Equals(AlgoliaDefaults.DefaultIndexName)))
            {
                var newIndex = client.InitIndex(AlgoliaDefaults.DefaultIndexName);

                settings.searchableAttributes = new JArray(searchableAttributes);
                settings.attributesForFaceting = new JArray(AlgoliaDefaults.FacetedAttributes);


                settings.numericAttributesForFiltering = numericAttrs;

                var setSettingsResponse = newIndex.SetSettings(settings, true);
                if (setSettingsResponse.taskID != null)
                    newIndex.WaitTask(setSettingsResponse.taskID.ToString());

                return newIndex;
            }

            var index = client.InitIndex(AlgoliaDefaults.DefaultIndexName);
            settings = (dynamic)index.GetSettings();

            if (model.UpdateIndicesModel.ResetSearchableAttributeSettings)
                settings.searchableAttributes = new JArray(searchableAttributes);

            if (model.UpdateIndicesModel.ResetFacetedAttributeSettings)
                settings.attributesForFaceting = new JArray(AlgoliaDefaults.FacetedAttributes);


            settings.numericAttributesForFiltering = numericAttrs;

            return index;
        }




        protected async Task<JObject> GetProductModelObjectAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productModel = await PrepareAlgoliaOverviewModelAsync(product);

            var dynamicObject = new ExpandoObject() as IDictionary<string, object>;

            var props = productModel.Product.GetType().GetProperties();
            foreach (var p in props.Where(x => x.CanRead))
                dynamicObject[p.Name] = p.GetValue(productModel.Product, null);

            dynamicObject["objectID"] = productModel.Product.Id;
            dynamicObject["AutoCompleteImageUrl"] = productModel.AutoCompleteImageUrl;
            dynamicObject["Rating"] = productModel.Rating;

            var name = productModel.Product?.Name ?? "";
            var sku = productModel.Product?.Sku ?? "";

            string mpn = "";
            try
            {
                var mpnProp = productModel.Product?.GetType().GetProperty("ManufacturerPartNumber");
                if (mpnProp != null)
                    mpn = mpnProp.GetValue(productModel.Product, null)?.ToString() ?? "";
            }
            catch { }

            if (string.IsNullOrWhiteSpace(mpn))
                mpn = product.ManufacturerPartNumber ?? "";

            dynamicObject["NameNormalized"] = NormalizePart(name);
            dynamicObject["SkuNormalized"] = NormalizePart(sku);
            dynamicObject["MPNNormalized"] = NormalizePart(mpn);
            dynamicObject["NameSegments"] = TokenizeSegments(name);
            dynamicObject["SkuSegments"] = TokenizeSegments(sku);

            dynamicObject["Price"] = productModel.Price.ToString("0.##", CultureInfo.InvariantCulture);
            dynamicObject["PriceValue"] = Convert.ToDouble(productModel.Price, CultureInfo.InvariantCulture);
            dynamicObject["OldPriceValue"] = Convert.ToDouble(productModel.OldPrice, CultureInfo.InvariantCulture);
            dynamicObject["OldPrice"] = productModel.OldPrice;

            dynamicObject["IsDiscounted"] =
                productModel.OldPrice > 0 && productModel.OldPrice > productModel.Price ? 1 : 0;

            dynamicObject["FilterableCategories"] = productModel.FilterableCategories;
            dynamicObject["FilterableManufacturers"] = productModel.FilterableManufacturers;
            dynamicObject["FilterableSpecifications"] = productModel.FilterableSpecifications;
            dynamicObject["FilterableAttributes"] = productModel.FilterableAttributes;
            dynamicObject["FilterableVendor"] = productModel.FilterableVendor;
            dynamicObject["FilterableKeywords"] = productModel.FilterableKeywords;
            dynamicObject["ProductCombinations"] = productModel.ProductCombinations;

            dynamicObject["CreatedOn"] = productModel.CreatedOn;
            dynamicObject["LimitedToStores"] = product.LimitedToStores ? 1 : 0;
            dynamicObject["GTIN"] = product.Gtin;

            var stockState = await GetSearchStockStateAsync(product);
            dynamicObject["StockQty"] = stockState.StockQty;
            dynamicObject["InStock"] = stockState.InStock;

            decimal? inner = null, outer = null, thick = null;

            // Dimensions must be read from ALL specification attributes of the product,
            // not from FilterableSpecifications: that list only contains mappings with
            // AllowFiltering = 1, so products whose dimension specs are stored with
            // AllowFiltering = 0 would be indexed without dimension fields.
            var allProductSpecs = await _specificationAttributeService.GetProductSpecificationAttributesAsync(product.Id);

            foreach (var psa in allProductSpecs)
            {
                var specOption = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(psa.SpecificationAttributeOptionId);
                if (specOption == null)
                    continue;

                var specAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(specOption.SpecificationAttributeId);

                var rawValue = psa.AttributeType == SpecificationAttributeType.Option
                    ? specOption.Name
                    : psa.CustomValue;

                var raw = WebUtility.HtmlDecode(rawValue ?? "").Trim();
                var val = SpecNumericParser.ParseFirstDecimal(raw);

                await _logger.InformationAsync(
                    $"SPEC DEBUG productId={product.Id}, specId={specOption.SpecificationAttributeId}, specName={specAttribute?.Name}, raw={raw}, parsed={val}");

                if (!val.HasValue)
                    continue;

                var attrNameRaw = (specAttribute?.Name ?? "").Trim();

                var attrName = attrNameRaw
                    .Replace(" ", "")
                    .Replace("-", "")
                    .Replace("_", "")
                    .Replace(":", "")
                    .Replace("Ø", "")
                    .Replace("ø", "")
                    .Replace("(mm)", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("mm", "", StringComparison.OrdinalIgnoreCase)
                    .Trim();


                if (specOption.SpecificationAttributeId == 1 ||
                    specOption.SpecificationAttributeId == 12 ||
                    attrName.Equals("Inside", StringComparison.OrdinalIgnoreCase) ||
                    attrName.Equals("InsideDiameter", StringComparison.OrdinalIgnoreCase) ||
                    attrName.Equals("InnerDiameter", StringComparison.OrdinalIgnoreCase) ||
                    attrName.Contains("Inside", StringComparison.OrdinalIgnoreCase) ||
                    attrName.Contains("InnerDiameter", StringComparison.OrdinalIgnoreCase) ||
                    attrName.Contains("Innerdiameter", StringComparison.OrdinalIgnoreCase))
                {
                    inner ??= val.Value;
                }

                else if (specOption.SpecificationAttributeId == 13 ||
                         attrName.Equals("OuterDiameter", StringComparison.OrdinalIgnoreCase) ||
                         attrName.Equals("OutsideDiameter", StringComparison.OrdinalIgnoreCase) ||
                         attrName.Contains("OuterDiameter", StringComparison.OrdinalIgnoreCase) ||
                         attrName.Contains("Outerdiameter", StringComparison.OrdinalIgnoreCase) ||
                         attrName.Contains("Outside", StringComparison.OrdinalIgnoreCase))
                {
                    outer ??= val.Value;
                }

                else if (specOption.SpecificationAttributeId == 14 ||
                         attrName.Equals("Thickness", StringComparison.OrdinalIgnoreCase) ||
                         attrName.Equals("Thick", StringComparison.OrdinalIgnoreCase) ||
                         attrName.Contains("Thickness", StringComparison.OrdinalIgnoreCase))
                {
                    thick ??= val.Value;
                }
            }

            if (inner.HasValue)
                dynamicObject["InnerDiameter"] = Convert.ToDouble(inner.Value, CultureInfo.InvariantCulture);

            if (outer.HasValue)
                dynamicObject["OuterDiameter"] = Convert.ToDouble(outer.Value, CultureInfo.InvariantCulture);

            if (thick.HasValue)
                dynamicObject["Thickness"] = Convert.ToDouble(thick.Value, CultureInfo.InvariantCulture);

            await _logger.InformationAsync(
                $"ALGOLIA DIMENSIONS productId={product.Id}, inner={inner}, outer={outer}, thickness={thick}");

            if (_algoliaSearchSettings.EnableMultilingualSearch)
            {
                var languages = await _languageService.GetAllLanguagesAsync();
                foreach (var language in languages)
                {
                    var localizedName = await _localizationService.GetLocalizedAsync(
                        product, e => e.Name, language.Id, false, false);

                    if (!string.IsNullOrEmpty(localizedName))
                    {
                        dynamicObject[string.Format(AlgoliaDefaults.MultilingualProductNameFormate, language.UniqueSeoCode)]
                            = localizedName;
                    }
                }
            }

            var storeIds = Array.Empty<int>();
            if (!_catalogSettings.IgnoreStoreLimitations && product.LimitedToStores)
            {
                var stores = await _storeMappingService.GetStoreMappingsAsync(product);
                storeIds = stores.Select(x => x.StoreId).ToArray();
            }

            dynamicObject["Stores"] = storeIds;

            return JObject.FromObject(dynamicObject, JsonSerializer.CreateDefault(new JsonSerializerSettings
            {
                Culture = CultureInfo.InvariantCulture,
                FloatFormatHandling = FloatFormatHandling.Symbol
            }));
        }
        public async Task<HashSet<int>> GetProductsThatHaveSubstitutesAsync(IList<int> productIds)
        {
            if (productIds == null || productIds.Count == 0)
                return new HashSet<int>();

            try
            {
                var subIndex = GetIndex(AlgoliaDefaults.SubstitutesIndexName);

                var filter = "(" + string.Join(" OR ",
                    productIds.Distinct().Select(id => $"{AlgoliaDefaults.SubstitutesProductIdField}={id}")) + ")";

                var res = subIndex.Search(new Query("")
                    .SetFilters(filter)
                    .SetNbHitsPerPage(Math.Min(productIds.Count, 1000))
                    .SetAttributesToRetrieve(new[] { AlgoliaDefaults.SubstitutesProductIdField })
                );

                var hits = res["hits"];
                if (hits == null || !hits.Any())
                    return new HashSet<int>();

                return hits
                    .Select(h => h[AlgoliaDefaults.SubstitutesProductIdField]?.ToString())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => int.TryParse(s, out var x) ? x : 0)
                    .Where(x => x > 0)
                    .ToHashSet();
            }
            catch
            {
                return new HashSet<int>();
            }
        }


        protected async Task<AlgoliaOverviewModel> PrepareAlgoliaOverviewModelAsync(Product product)
        {

            var productModel = (await _productModelFactory.PrepareProductOverviewModelsAsync(new List<Product>() { product })).FirstOrDefault();

            var model = new AlgoliaOverviewModel
            {
                Product = productModel,
                objectID = product.Id.ToString(),
                Price = product.Price,
                OldPrice = product.OldPrice,
                Rating = productModel.ReviewOverviewModel.TotalReviews < 1 ? 0 :
                productModel.ReviewOverviewModel.RatingSum / productModel.ReviewOverviewModel.TotalReviews,
                CreatedOn = product.CreatedOnUtc.Ticks,
                AutoCompleteImageUrl = await GetAutoCompleteImageUrlAsync(product),
            };

            var productTags = await _productTagService.GetAllProductTagsByProductIdAsync(product.Id);


            if (productTags.Any())
                model.FilterableKeywords = productTags.Select(x => x.Name).ToList();

            model.FilterableVendor = await PrepareVendorModelAsync(product);
            model.FilterableCategories = await PrepareCategoryListModelAsync(product);
            model.FilterableManufacturers = await PrepareManufacturerListModelAsync(product);
            model.FilterableSpecifications = await PrepareSpecificationListModelAsync(product);
            model.FilterableAttributes = await PrepareAttributeListModelAsync(product);
            model.ProductCombinations = await PrepareCombinationListModelAsync(product);

            return model;
        }

        protected async Task<IList<AlgoliaOverviewModel.AttributeModel>> PrepareAttributeListModelAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));


            var cache = new CacheKey(AlgoliaModelCacheDefaults.ProductAttrsModelKey);

            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(cache, product.Id, (await _workContext.GetWorkingLanguageAsync()).Id);

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var model = new List<AlgoliaOverviewModel.AttributeModel>();
                var productAttributeMappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
                foreach (var productAttributeMapping in productAttributeMappings)
                {
                    var productAttributeValues = await _productAttributeService.GetProductAttributeValuesAsync(productAttributeMapping.Id);

                    var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(productAttributeMapping.ProductAttributeId);
                    foreach (var value in productAttributeValues)
                    {
                        var m = new AlgoliaOverviewModel.AttributeModel()
                        {
                            AttributeId = productAttributeMapping.ProductAttributeId,
                            AttributeName = productAttribute.Name,
                            ColorSquaresRgb = value.ColorSquaresRgb,
                            AttributeValue = value.Name,
                        };
                        m.AttributeIdValueGroup = m.AttributeId + AlgoliaDefaults.Delimiter + m.AttributeName + AlgoliaDefaults.Delimiter + value.Name;
                        model.Add(m);
                    }
                }
                return model;
            });
        }
        protected async Task<IList<ProductCombinationOverviewModel>> PrepareCombinationListModelAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var cache = new CacheKey(AlgoliaModelCacheDefaults.ProductAttrscombinationModelKey);

            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(cache, product.Id, (await _workContext.GetWorkingLanguageAsync()).Id);

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var model = new List<AlgoliaOverviewModel.ProductCombinationOverviewModel>();
                var allProductAttributeCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);
                foreach (var productAttributeCombination in allProductAttributeCombinations)
                {
                    var m = new AlgoliaOverviewModel.ProductCombinationOverviewModel()
                    {
                        CombinationId = productAttributeCombination.Id,
                        GTIN = productAttributeCombination.Gtin,
                        Sku = productAttributeCombination.Sku,
                    };
                    model.Add(m);
                }
                return model;
            });
        }

        protected async Task<IList<AlgoliaOverviewModel.SpecificationModel>> PrepareSpecificationListModelAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            var cache = new CacheKey(AlgoliaModelCacheDefaults.ProductSpecsModelKey);

            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(cache, product.Id, (await _workContext.GetWorkingLanguageAsync()).Id);

            return await (await _cacheManager.GetAsync(cacheKey, async () => (await _specificationAttributeService.GetProductSpecificationAttributesAsync(product.Id, 0, true))
                .SelectAwait(async psa =>
                {
                    var specAttributeOption = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(psa.SpecificationAttributeOptionId);
                    var specAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(specAttributeOption.SpecificationAttributeId);
                    var m = new AlgoliaOverviewModel.SpecificationModel
                    {
                        OptionId = psa.SpecificationAttributeOptionId,
                        SpecificationAttributeId = specAttributeOption.SpecificationAttributeId,
                        SpecificationAttributeName = specAttribute.Name,
                        ColorSquaresRgb = specAttributeOption.ColorSquaresRgb,
                        AttributeTypeId = psa.AttributeTypeId
                    };

                    switch (psa.AttributeType)
                    {
                        case SpecificationAttributeType.Option:
                            m.ValueRaw = WebUtility.HtmlEncode(specAttributeOption.Name);
                            break;
                        case SpecificationAttributeType.CustomText:
                            m.ValueRaw = WebUtility.HtmlEncode(psa.CustomValue);
                            break;
                        case SpecificationAttributeType.Hyperlink:
                        case SpecificationAttributeType.CustomHtmlText:
                            m.ValueRaw = psa.CustomValue;
                            break;
                        default:
                            break;
                    }
                    m.SpecificationValueGroup = m.SpecificationAttributeId + AlgoliaDefaults.Delimiter +
                        m.SpecificationAttributeName + AlgoliaDefaults.Delimiter + m.ValueRaw;
                    m.OptionIdSpecificationId = $"{specAttributeOption.Id}-{specAttributeOption.SpecificationAttributeId}";

                    return m;
                }))).ToListAsync();
        }

        protected async Task<AlgoliaOverviewModel.VendorModel> PrepareVendorModelAsync(Product product)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
            if (vendor == null || vendor.Deleted || !vendor.Active)
                return null;

            var model = new AlgoliaOverviewModel.VendorModel
            {
                Name = vendor.Name,
                SeName = await _urlRecordService.GetSeNameAsync(vendor),
                Id = vendor.Id
            };

            return model;
        }

        protected async Task<IList<AlgoliaOverviewModel.CategoryModel>> PrepareCategoryListModelAsync(Product product)
        {
            var model = new List<AlgoliaOverviewModel.CategoryModel>();
            var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);

            if (productCategories.Any())
            {
                foreach (var productCategory in productCategories)
                {
                    var category = await _categoryService.GetCategoryByIdAsync(productCategory.CategoryId);
                    if (category == null || !category.Published || category.Deleted)
                        continue;

                    var sename = await _urlRecordService.GetSeNameAsync(category);

                    model.Add(new AlgoliaOverviewModel.CategoryModel()
                    {
                        Id = productCategory.CategoryId,
                        Name = category.Name,
                        SeName = sename
                    });
                    if (_algoliaSearchSettings.EnableMultilingualSearch)
                    {
                        var languages = await _languageService.GetAllLanguagesAsync();
                        foreach (var language in languages)
                        {
                            var categoryLocalizedName = await _localizationService.GetLocalizedAsync(category, entity => entity.Name, language.Id, false, false);
                            var categoryLocalizedSeName = await _urlRecordService.GetSeNameAsync(category, language.Id, false, false);

                            if (!string.IsNullOrEmpty(categoryLocalizedName))
                            {
                                model.Add(new AlgoliaOverviewModel.CategoryModel()
                                {
                                    Id = productCategory.CategoryId,
                                    Name = categoryLocalizedName,
                                    SeName = string.IsNullOrEmpty(categoryLocalizedSeName) ? sename : categoryLocalizedSeName
                                });
                            }
                        }
                    }
                }
            }
            return model;
        }

        protected async Task<IList<AlgoliaOverviewModel.ManufacturerModel>> PrepareManufacturerListModelAsync(Product product)
        {
            var model = new List<AlgoliaOverviewModel.ManufacturerModel>();
            var productManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id);

            if (productManufacturers.Any())
            {
                foreach (var productManufacturer in productManufacturers)
                {
                    var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(productManufacturer.ManufacturerId);
                    if (manufacturer == null || !manufacturer.Published || manufacturer.Deleted)
                        continue;

                    var sename = await _urlRecordService.GetSeNameAsync(manufacturer);
                    model.Add(new AlgoliaOverviewModel.ManufacturerModel()
                    {
                        Id = productManufacturer.ManufacturerId,
                        Name = manufacturer.Name,
                        SeName = sename
                    });
                    if (_algoliaSearchSettings.EnableMultilingualSearch)
                    {
                        var languages = await _languageService.GetAllLanguagesAsync();
                        foreach (var language in languages)
                        {
                            var manufacturerLocalizedName = await _localizationService.GetLocalizedAsync(manufacturer, entity => entity.Name, language.Id, false, false);
                            var manufacturerLocalizedSeName = await _urlRecordService.GetSeNameAsync(manufacturer, language.Id, false, false);

                            if (!string.IsNullOrEmpty(manufacturerLocalizedName))
                            {
                                model.Add(new AlgoliaOverviewModel.ManufacturerModel
                                {
                                    Id = productManufacturer.ManufacturerId,
                                    Name = manufacturerLocalizedName,
                                    SeName = string.IsNullOrEmpty(manufacturerLocalizedSeName) ? sename : manufacturerLocalizedSeName
                                });
                            }
                        }
                    }
                }
            }
            return model;
        }

        protected async Task<string> GetAutoCompleteImageUrlAsync(Product product)
        {
            var pictureSize = 50;

            var cache = new CacheKey(AlgoliaModelCacheDefaults.AutoCompletePictureModelKey);
            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(cache, product.Id, (await _workContext.GetWorkingLanguageAsync()).Id);

            var autoCompleteImageUrl = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var picture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
                return (await _pictureService.GetPictureUrlAsync(picture, pictureSize)).Url;
            });
            return autoCompleteImageUrl;
        }

        protected Algolia.Search.Index GetIndex(int? orderby = null)
        {
            var client = new AlgoliaClient(_algoliaSearchSettings.ApplicationId, _algoliaSearchSettings.AdminKey);


            if (!orderby.HasValue || orderby.Value == 0)
                return client.InitIndex(AlgoliaDefaults.DefaultIndexName);

            var sorting = (AlgoliaSortingEnum)orderby.Value;

            var indexName = sorting switch
            {
                AlgoliaSortingEnum.NameAsc => "NameAsc",
                AlgoliaSortingEnum.NameDesc => "NameDesc",
                AlgoliaSortingEnum.PriceAsc => "PriceAsc",
                AlgoliaSortingEnum.PriceDesc => "PriceDesc",
                AlgoliaSortingEnum.CreatedOn => "CreatedOn",
                AlgoliaSortingEnum.StockQty => "StockQty",
                _ => AlgoliaDefaults.DefaultIndexName
            };

            return client.InitIndex(indexName);
        }
        protected Algolia.Search.Index GetIndex(string indexName)
        {
            var client = new AlgoliaClient(_algoliaSearchSettings.ApplicationId, _algoliaSearchSettings.AdminKey);
            return client.InitIndex(indexName);
        }
        private string GetIndexName(AlgoliaSortingEnum sorting)
        {
            return sorting switch
            {
                AlgoliaSortingEnum.NameAsc => "NameAsc",
                AlgoliaSortingEnum.NameDesc => "NameDesc",
                AlgoliaSortingEnum.PriceAsc => "PriceAsc",
                AlgoliaSortingEnum.PriceDesc => "PriceDesc",
                AlgoliaSortingEnum.CreatedOn => "CreatedOn",
                AlgoliaSortingEnum.StockQty => "StockQty",
                _ => "Products"
            };
        }



        private async Task<List<int>> SearchInSubstitutesGetProductIdsAsync(
    string searchTerms,
    string filters,
    int pageIndex,
    int pageSize)
        {
            var subIndex = GetIndex(AlgoliaDefaults.SubstitutesIndexName);

            var subSearch = subIndex.Search(new Query(searchTerms ?? "")

                .SetPage(pageIndex)
                .SetNbHitsPerPage(pageSize)

                .SetAttributesToRetrieve(new[] { AlgoliaDefaults.SubstitutesProductIdField }));

            var hits = subSearch["hits"];
            if (hits == null || !hits.Any())
                return new List<int>();


            var ids = hits
                .Select(h => h[AlgoliaDefaults.SubstitutesProductIdField]?.ToString())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => int.TryParse(s, out var x) ? x : 0)
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            return ids;
        }



        private static string GetTaskId(dynamic response)
        {
            try
            {

                if (response == null)
                    return null;


                if (response is JObject jo)
                {
                    return jo["taskID"]?.ToString() ?? jo["taskId"]?.ToString();
                }


                return response.taskID?.ToString() ?? response.taskId?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private static void WaitTaskIfAny(Algolia.Search.Index index, dynamic response)
        {
            var taskId = GetTaskId(response);
            if (!string.IsNullOrWhiteSpace(taskId))
                index.WaitTask(taskId);
        }







        protected async Task<string> GetFilterStringAsync(
     IList<int> cids, IList<int> mids, IList<int> vids,
     IList<FilteredGroupModel> specids, IList<FilteredGroupModel> attrids,
     IList<int> ratings,
     decimal? minPrice, decimal? maxPrice,
     IList<Models.RangeFilterModel> rangeFilters = null, bool onlyInStock = false, bool discountOnly = false)
        {
            var sb = new StringBuilder();


            if (_algoliaSearchSettings.AllowCategoryFilter && cids != null && cids.Any())
                sb.Append($"(FilterableCategories.Id={string.Join(" OR FilterableCategories.Id=", cids)}) AND ");


            if (_algoliaSearchSettings.AllowManufacturerFilter && mids != null && mids.Any())
                sb.Append($"(FilterableManufacturers.Id={string.Join(" OR FilterableManufacturers.Id=", mids)}) AND ");


            if (_algoliaSearchSettings.AllowVendorFilter && vids != null && vids.Any())
                sb.Append($"(FilterableVendor.Id={string.Join(" OR FilterableVendor.Id=", vids)}) AND ");


            if (_algoliaSearchSettings.AllowSpecificationFilter && specids != null && specids.Any())
            {

                var groups = specids.GroupBy(x => x.Id);

                foreach (var g in groups)
                {

                    var tokens = g
    .Select(x => $"{x.OptionId}-{g.Key}")
    .Distinct()
    .ToList();

                    sb.Append("( " + string.Join(" OR ",
                        tokens.Select(t => $"FilterableSpecifications.OptionIdSpecificationId:\"{t}\"")) + " ) AND ");

                }
            }


            if (_algoliaSearchSettings.AllowRatingFilter && ratings != null && ratings.Any())
                sb.Append($"(Rating={string.Join(" OR Rating=", ratings)}) AND ");


            if (_algoliaSearchSettings.AllowPriceRangeFilter)
            {
                if (maxPrice.HasValue && maxPrice > 0)
                    sb.Append($"PriceValue <= {maxPrice.Value.ToString(CultureInfo.InvariantCulture)} AND ");

                if (minPrice.HasValue && minPrice > 0)
                    sb.Append($"PriceValue >= {minPrice.Value.ToString(CultureInfo.InvariantCulture)} AND ");
            }


            if (rangeFilters != null && rangeFilters.Any())
            {
                foreach (var rf in rangeFilters)
                {
                    if (rf == null || string.IsNullOrWhiteSpace(rf.Attribute))
                        continue;

                    if (!AllowedRangeAttributes.Contains(rf.Attribute))
                        continue;

                    var parts = new List<string>();

                    if (rf.From.HasValue)
                        parts.Add($"{rf.Attribute} >= {rf.From.Value.ToString(CultureInfo.InvariantCulture)}");

                    if (rf.To.HasValue)
                        parts.Add($"{rf.Attribute} <= {rf.To.Value.ToString(CultureInfo.InvariantCulture)}");

                    if (parts.Count > 0)
                        sb.Append("(" + string.Join(" AND ", parts) + ") AND ");
                }
            }


            if (onlyInStock)
                sb.Append("InStock = 1 AND ");

            if (discountOnly)
                sb.Append("IsDiscounted = 1 AND ");

            if (!_catalogSettings.IgnoreStoreLimitations)
                sb.Append($"(LimitedToStores = 0 OR Stores = {(await _storeContext.GetCurrentStoreAsync()).Id}) AND ");

            var filterString = sb.ToString().Trim();
            if (filterString.EndsWith("AND", StringComparison.OrdinalIgnoreCase))
                filterString = filterString.Substring(0, filterString.Length - 3).Trim();

            return filterString;
        }

        private static readonly HashSet<string> AllowedRangeAttributes =
            new(StringComparer.OrdinalIgnoreCase)
            {
        "InnerDiameter",
        "OuterDiameter",
        "Thickness"
            };





        #endregion

        #region Methods

        public async Task UploadProductsAsync(UploadProductModel model)
        {
            var index = GetPrimaryIndex();

            var pageIndex = 0;
            var currentPageProducts = 0;
            var totalProducts = 0;
            var totalPages = 0;
            var uploaded = 0;
            var failed = 0;

            try
            {
                while (true)
                {
                    await _productUploadHub.UploadProductsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, 0, failed, uploaded, -10, "Products fetching from database...");
                    var products = await _algoliaCatalogService.SearchProductsAsync(fromId: model.FromId, toId: model.ToId, pageIndex: pageIndex, pageSize: 100);
                    if (products == null || products.Count == 0)
                        break;

                    currentPageProducts = products.Count;
                    totalProducts = products.TotalCount;
                    totalPages = products.TotalPages;

                    var binding = 0;
                    var objects = new List<JObject>();

                    foreach (var product in products)
                    {
                        try
                        {
                            await _productUploadHub.UploadProductsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, binding + 1, failed, uploaded, 110);
                            objects.Add(await GetProductModelObjectAsync(product));

                            binding++;
                            await _productUploadHub.UploadProductsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, binding, failed, uploaded, 10);
                        }
                        catch (Exception ex)
                        {
                            await _logger.ErrorAsync("AlgoliaSearch: " + ex.Message + ", Product Id = " + product.Id, ex);
                            failed++;

                            await _productUploadHub.UploadProductsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, binding, failed, uploaded, -1, ex.Message);
                            continue;
                        }
                    }

                    await _productUploadHub.UploadProductsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, binding, failed, uploaded, 20);
                    var res = index.PartialUpdateObjects(objects, true);

                    uploaded += binding;
                    await _productUploadHub.UploadProductsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, binding, failed, uploaded, 10);
                    pageIndex++;
                }
                await _productUploadHub.UploadProductsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, 0, failed, uploaded, 100);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("AlgoliaSearch: " + ex.Message, ex);
                await _productUploadHub.UploadProductsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, 0, failed, uploaded, -1, ex.Message);
            }
        }

        public async Task UpdateIndicesAsync(ConfigurationModel model)
        {
            try
            {
                var client = new AlgoliaClient(_algoliaSearchSettings.ApplicationId, _algoliaSearchSettings.AdminKey);
                var defaultIndex = GetDefaultIndex(client, model, out dynamic settings);


                if (settings.searchableAttributes == null)
                    settings.searchableAttributes = new JArray();

                var searchable = new List<string>
        {
            "NameNormalized",
            "SkuNormalized",
            "MPNNormalized"
        };

                try
                {
                    if (settings.searchableAttributes is JArray sa)
                    {
                        foreach (var x in sa)
                        {
                            var v = x?.ToString();
                            if (!string.IsNullOrWhiteSpace(v))
                                searchable.Add(v);
                        }
                    }
                }
                catch { }

                searchable = searchable
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                settings.searchableAttributes = new JArray(searchable);


                if (settings.numericAttributesForFiltering == null)
                    settings.numericAttributesForFiltering = new JArray();

                var numeric = (JArray)settings.numericAttributesForFiltering;

                void EnsureNumeric(string name)
                {
                    if (!numeric.Any(x => x?.ToString().Equals(name, StringComparison.OrdinalIgnoreCase) == true))
                        numeric.Add(name);
                }
                EnsureNumeric("Id");
                EnsureNumeric("FilterableCategories.Id");
                EnsureNumeric("FilterableManufacturers.Id");
                EnsureNumeric("FilterableVendor.Id");
                EnsureNumeric("Rating");
                EnsureNumeric("LimitedToStores");
                EnsureNumeric("Stores");
                EnsureNumeric("PriceValue");
                EnsureNumeric("InnerDiameter");
                EnsureNumeric("OuterDiameter");
                EnsureNumeric("Thickness");
                EnsureNumeric("StockQty");
                EnsureNumeric("InStock");
                EnsureNumeric("IsDiscounted");

                var replicas = _algoliaSearchSettings.AllowProductSorting
                    ? (_algoliaSearchSettings.AllowedSortingOptions ?? new List<int>())
                    : new List<int>();

                if (!replicas.Contains((int)AlgoliaSortingEnum.StockQty))
                    replicas.Add((int)AlgoliaSortingEnum.StockQty);

                var replicaNames = replicas
                    .Where(x => x != 0)
                    .Select(x => ((AlgoliaSortingEnum)x) switch
                    {
                        AlgoliaSortingEnum.NameAsc => "NameAsc",
                        AlgoliaSortingEnum.NameDesc => "NameDesc",
                        AlgoliaSortingEnum.PriceAsc => "PriceAsc",
                        AlgoliaSortingEnum.PriceDesc => "PriceDesc",
                        AlgoliaSortingEnum.CreatedOn => "CreatedOn",
                        AlgoliaSortingEnum.StockQty => "StockQty",
                        _ => null
                    })
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();


                JArray RankingRelevanceFirst() => new JArray(new[]
                {
            "typo","geo","words","filters","proximity","attribute","exact","custom"
        });


                foreach (var idx in replicas.Where(x => x != 0))
                {
                    var sortEnum = (AlgoliaSortingEnum)idx;

                    var replicaIndexName = sortEnum switch
                    {
                        AlgoliaSortingEnum.NameAsc => "NameAsc",
                        AlgoliaSortingEnum.NameDesc => "NameDesc",
                        AlgoliaSortingEnum.PriceAsc => "PriceAsc",
                        AlgoliaSortingEnum.PriceDesc => "PriceDesc",
                        AlgoliaSortingEnum.CreatedOn => "CreatedOn",
                        AlgoliaSortingEnum.StockQty => "StockQty",
                        _ => null
                    };

                    if (string.IsNullOrWhiteSpace(replicaIndexName))
                        continue;

                    var replicaIndex = client.InitIndex(replicaIndexName);

                    dynamic replicaSettings = new JObject();


                    replicaSettings.ranking = RankingRelevanceFirst();
                    replicaSettings.customRanking = new JArray(new[]
                    {
                "desc(InStock)",
                "desc(StockQty)"
            });

                    replicaSettings.searchableAttributes = settings.searchableAttributes;
                    replicaSettings.attributesForFaceting = settings.attributesForFaceting;
                    replicaSettings.numericAttributesForFiltering = settings.numericAttributesForFiltering;

                    var resp = replicaIndex.SetSettings(replicaSettings, true);
                    WaitTaskIfAny(replicaIndex, resp);
                }


                settings.replicas = new JArray(replicaNames);

                settings.ranking = RankingRelevanceFirst();
                settings.customRanking = new JArray(new[]
                {
            "desc(InStock)",
            "desc(StockQty)"
        });

                var setSettingsResponse = defaultIndex.SetSettings(settings, true);
                WaitTaskIfAny(defaultIndex, setSettingsResponse);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("AlgoliaSearch(UpdateIndicesAsync): " + ex.Message, ex);
            }
        }



        private static List<int> MergeIdsKeepOrder(IEnumerable<int> first, IEnumerable<int> second)
        {
            var seen = new HashSet<int>();
            var merged = new List<int>();

            foreach (var id in first ?? Enumerable.Empty<int>())
                if (id > 0 && seen.Add(id))
                    merged.Add(id);

            foreach (var id in second ?? Enumerable.Empty<int>())
                if (id > 0 && seen.Add(id))
                    merged.Add(id);

            return merged;
        }

        private async Task<(List<int> Ids, int Total, HashSet<int> SubstituteResultIds)> SearchUnifiedIdsAsync(
        string searchTerms,
        string filters,
        int pageIndex,
        int pageSize,
        int? orderby,
        bool allowSubstitutesMerge,
        bool forceStockSort)
        {
            Algolia.Search.Index index;

            try
            {
                index = forceStockSort ? GetIndex("StockQty") : GetIndex(orderby);
            }
            catch
            {
                index = GetIndex();
            }

            var qTerm = (searchTerms ?? "").Trim();
            var termNorm = NormalizePart(qTerm);

            var substituteResultIds = new HashSet<int>();
            long subHits = 0;
            List<int> subExactIds = new();

            if (allowSubstitutesMerge && !string.IsNullOrWhiteSpace(qTerm))
            {
                var (sh, sids) = await SearchInSubstitutesBySubCodeGetProductIdsAsync(qTerm, take: 2000);
                subHits = sh;
                subExactIds = sids ?? new List<int>();
                substituteResultIds = subExactIds.Where(x => x > 0).ToHashSet();
            }

            var query = new Query(qTerm)
                .SetFilters(filters)
                .SetPage(0)
                .SetNbHitsPerPage(pageSize)
                .SetQueryType(Query.QueryType.PREFIX_ALL)
                // If no record matches all query words (e.g. "R0876-04" where the product
                // is named "R0876": the "-04" suffix becomes a separate word "04" that
                // matches nothing), let Algolia drop trailing words until results appear.
                .SetRemoveWordsIfNoResult(Query.RemoveWordsIfNoResult.LAST_WORDS);

            var productSearch = index.Search(query);

            var productNbHits = int.TryParse(productSearch["nbHits"]?.ToString(), out var pHits) ? pHits : 0;
            var hitsArray = productSearch["hits"] as JArray ?? new JArray();

            // If the normalized term differs from the original (i.e. original contained special chars
            // like hyphens or plus signs), run a second Algolia query using the stripped term so that
            // products whose name/sku only differs by punctuation are still returned.
            if (!string.IsNullOrWhiteSpace(termNorm) && !string.Equals(termNorm, qTerm, StringComparison.OrdinalIgnoreCase))
            {
                var normQuery = new Query(termNorm)
                    .SetFilters(filters)
                    .SetPage(0)
                    .SetNbHitsPerPage(pageSize)
                    .SetQueryType(Query.QueryType.PREFIX_ALL)
                    .RestrictSearchableAttributes("NameNormalized,SkuNormalized,MPNNormalized,NameSegments,SkuSegments");

                try
                {
                    var normSearch = index.Search(normQuery);
                    var normHits = normSearch["hits"] as JArray;
                    if (normHits != null && normHits.Count > 0)
                    {
                        // Merge: add hits not already present in hitsArray (deduplicate by Id/objectID)
                        var existingIds = hitsArray
                            .Select(h => h["Id"]?.ToString() ?? h["objectID"]?.ToString())
                            .Where(id => id != null)
                            .ToHashSet();

                        foreach (var normHit in normHits)
                        {
                            var hitId = normHit["Id"]?.ToString() ?? normHit["objectID"]?.ToString();
                            if (hitId != null && existingIds.Add(hitId))
                                hitsArray.Add(normHit);
                        }

                        var normNbHits = int.TryParse(normSearch["nbHits"]?.ToString(), out var nHits) ? nHits : 0;
                        productNbHits = Math.Max(productNbHits, normNbHits);
                    }
                }
                catch (Exception ex)
                {
                    await _logger.WarningAsync("AlgoliaSearch normalized fallback query failed: " + ex.Message, ex);
                }
            }

            // Last-resort fallback: the query is LONGER than the stored part number and has no
            // separator Algolia could split on (e.g. customer types "R087604" while the product
            // is named "R0876"). Prefix matching can never match a query word longer than the
            // indexed word, so progressively trim the normalized term from the right until
            // something matches (longest surviving prefix wins).
            if (hitsArray.Count == 0 && termNorm.Length > 3)
            {
                var minLen = Math.Max(3, termNorm.Length - 10);
                for (var len = termNorm.Length - 1; len >= minLen; len--)
                {
                    var shrunkTerm = termNorm.Substring(0, len);
                    var shrunkQuery = new Query(shrunkTerm)
                        .SetFilters(filters)
                        .SetPage(0)
                        .SetNbHitsPerPage(pageSize)
                        .SetQueryType(Query.QueryType.PREFIX_ALL)
                        .RestrictSearchableAttributes("NameNormalized,SkuNormalized,MPNNormalized,NameSegments,SkuSegments");

                    try
                    {
                        var shrunkSearch = index.Search(shrunkQuery);
                        var shrunkHits = shrunkSearch["hits"] as JArray;
                        if (shrunkHits != null && shrunkHits.Count > 0)
                        {
                            hitsArray = shrunkHits;
                            productNbHits = int.TryParse(shrunkSearch["nbHits"]?.ToString(), out var sHits) ? sHits : shrunkHits.Count;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        await _logger.WarningAsync("AlgoliaSearch shrink fallback query failed: " + ex.Message, ex);
                        break;
                    }
                }
            }

            var productItems = hitsArray
                .Select((h, rank) =>
                {
                    var name = h["Name"]?.ToString() ?? "";
                    var sku = h["Sku"]?.ToString() ?? "";
                    var mpn = h["ManufacturerPartNumber"]?.ToString() ?? "";

                    var nameN = h["NameNormalized"]?.ToString() ?? NormalizePart(name);
                    var skuN = h["SkuNormalized"]?.ToString() ?? NormalizePart(sku);
                    var mpnN = h["MPNNormalized"]?.ToString() ?? NormalizePart(mpn);

                    var best = BestMatch(
                        GetMatch(nameN, termNorm),
                        GetMatchSegments(name, termNorm),
                        GetMatch(skuN, termNorm),
                        GetMatchSegments(sku, termNorm),
                        GetMatch(mpnN, termNorm)
                    );

                    return new
                    {
                        Rank = rank,
                        Id = int.TryParse(h["Id"]?.ToString(), out var id) ? id : 0,
                        Name = h["Name"]?.ToString() ?? "",
                        Sku = h["Sku"]?.ToString() ?? "",
                        InStock = int.TryParse(h["InStock"]?.ToString(), out var ins) ? ins : 0,
                        StockQty = int.TryParse(h["StockQty"]?.ToString(), out var qty) ? qty : 0,
                        MatchType = best.MatchType,   // 3 exact, 2 starts-with, 1 contains, 0 none
                        ExtraLen = best.ExtraLen
                    };
                })
                .Where(x => x.Id > 0)
                .ToList();

            foreach (var x in productItems.Take(20))
            {
                await _logger.InformationAsync(
                    $"ALGOLIA HIT id={x.Id}, sku={x.Sku}, name={x.Name}, inStock={x.InStock}, stockQty={x.StockQty}, matchType={x.MatchType}, rank={x.Rank}");
            }

            var sortByHighestStock = orderby.HasValue && (AlgoliaSortingEnum)orderby.Value == AlgoliaSortingEnum.StockQty;

            var substituteStockMap = new Dictionary<int, (int InStock, int StockQty)>();
            if (subExactIds.Any())
            {
                var primary = GetPrimaryIndex();

                try
                {
                    var objectIds = subExactIds.Distinct().Select(x => x.ToString()).ToList();
                    JObject res;

                    try
                    {
                        res = await primary.GetObjectsAsync(objectIds, new[] { "Id", "InStock", "StockQty" });
                    }
                    catch
                    {
                        res = primary.GetObjects(objectIds, new[] { "Id", "InStock", "StockQty" });
                    }

                    var results = res?["results"] as JArray;
                    if (results != null)
                    {
                        foreach (var r in results)
                        {
                            if (r == null || r.Type == JTokenType.Null)
                                continue;

                            var idStr = r["Id"]?.ToString() ?? r["objectID"]?.ToString();
                            if (!int.TryParse(idStr, out var pid) || pid <= 0)
                                continue;

                            var inStock = int.TryParse(r["InStock"]?.ToString(), out var ins) ? ins : 0;
                            var stockQty = int.TryParse(r["StockQty"]?.ToString(), out var qty) ? qty : 0;

                            substituteStockMap[pid] = (inStock, stockQty);
                        }
                    }
                }
                catch
                {
                }
            }

            // -----------------------------
            // BUCKET 1
            // Product index first
            // normal mode: exact match + in stock
            // highest stock mode: all in-stock, highest qty first
            // -----------------------------
            List<int> bucket1;

            if (sortByHighestStock)
            {
                bucket1 = productItems
                    .Where(x => x.InStock == 1)
                    .OrderByDescending(x => x.StockQty)
                    .ThenByDescending(x => x.MatchType)
                    .ThenBy(x => x.MatchType > 0 ? x.ExtraLen : int.MaxValue)
                    .ThenBy(x => x.Rank)
                    .Select(x => x.Id)
                    .ToList();
            }
            else
            {
                bucket1 = productItems
                    .Where(x => x.InStock == 1 && x.MatchType == 3)
                    .OrderBy(x => x.ExtraLen)
                    .ThenBy(x => x.Rank)
                    .ThenByDescending(x => x.StockQty)
                    .Select(x => x.Id)
                    .ToList();
            }

            var used = new HashSet<int>(bucket1);

            // -----------------------------
            // BUCKET 2
            // Substitute exact match + in stock
            // only in normal relevance mode
            // -----------------------------
            List<int> bucket2;

            if (sortByHighestStock)
            {
                bucket2 = new List<int>();
            }
            else
            {
                bucket2 = subExactIds
                    .Where(id => !used.Contains(id))
                    .Select(id =>
                    {
                        var s = substituteStockMap.TryGetValue(id, out var v) ? v : (0, 0);
                        return new
                        {
                            Id = id,
                            InStock = s.Item1,
                            StockQty = s.Item2
                        };
                    })
                    .Where(x => x.InStock == 1)
                    .OrderByDescending(x => x.StockQty)
                    .Select(x => x.Id)
                    .ToList();

                foreach (var id in bucket2)
                    used.Add(id);
            }

            // -----------------------------
            // BUCKET 3
            // Remaining in-stock products
            // normal mode: relevance first
            // highest stock mode: normally empty because bucket1 already took in-stock
            // -----------------------------
            List<int> bucket3;

            if (sortByHighestStock)
            {
                bucket3 = new List<int>();
            }
            else
            {
                bucket3 = productItems
                    .Where(x => x.InStock == 1 && !used.Contains(x.Id))
                    .OrderByDescending(x => x.MatchType)
                    .ThenBy(x => x.MatchType > 0 ? x.ExtraLen : int.MaxValue)
                    .ThenBy(x => x.Rank)
                    .ThenByDescending(x => x.StockQty)
                    .Select(x => x.Id)
                    .ToList();

                foreach (var id in bucket3)
                    used.Add(id);
            }

            // -----------------------------
            // BUCKET 4
            // all out of stock always last
            // -----------------------------
            var bucket4Products = productItems
                .Where(x => x.InStock == 0 && !used.Contains(x.Id))
                .OrderByDescending(x => sortByHighestStock ? x.StockQty : x.MatchType)
                .ThenByDescending(x => sortByHighestStock ? x.MatchType : 0)
                .ThenBy(x => x.MatchType > 0 ? x.ExtraLen : int.MaxValue)
                .ThenBy(x => x.Rank)
                .Select(x => x.Id)
                .ToList();

            var bucket4Subs = subExactIds
                .Where(id => !used.Contains(id))
                .Select(id =>
                {
                    var s = substituteStockMap.TryGetValue(id, out var v) ? v : (0, 0);
                    return new
                    {
                        Id = id,
                        InStock = s.Item1,
                        StockQty = s.Item2
                    };
                })
                .Where(x => x.InStock == 0)
                .OrderByDescending(x => x.StockQty)
                .Select(x => x.Id)
                .ToList();

            var bucket4 = MergeIdsKeepOrder(bucket4Products, bucket4Subs);


            List<int> finalIds;

            if (sortByHighestStock)
            {
                finalIds = MergeIdsKeepOrder(bucket1, bucket4);
            }
            else
            {
                finalIds = MergeIdsKeepOrder(
                    MergeIdsKeepOrder(bucket1, bucket2),
                    MergeIdsKeepOrder(bucket3, bucket4)
                );
            }

            await _logger.InformationAsync("ALGOLIA bucket1 = " + string.Join(",", bucket1));
            await _logger.InformationAsync("ALGOLIA bucket2 = " + string.Join(",", bucket2));
            await _logger.InformationAsync("ALGOLIA bucket3 = " + string.Join(",", bucket3));
            await _logger.InformationAsync("ALGOLIA bucket4 = " + string.Join(",", bucket4));
            await _logger.InformationAsync("ALGOLIA finalIds = " + string.Join(",", finalIds));

            var total = Math.Max(productNbHits, (int)Math.Min(subHits, int.MaxValue));
            total = Math.Max(total, finalIds.Count);

            return (finalIds, total, substituteResultIds);
        }

        private static (int MatchType, int ExtraLen) GetMatch(string candidateNorm, string termNorm)
        {
            candidateNorm ??= "";
            termNorm ??= "";

            if (termNorm.Length == 0 || candidateNorm.Length == 0)
                return (0, int.MaxValue);

            if (candidateNorm.Equals(termNorm, StringComparison.OrdinalIgnoreCase))
                return (3, 0);

            if (candidateNorm.StartsWith(termNorm, StringComparison.OrdinalIgnoreCase))
                return (2, candidateNorm.Length - termNorm.Length);

            // Reverse prefix: the customer typed the full part number PLUS a suffix
            // (term "R087604" vs product "R0876"). Rank it like a starts-with match,
            // penalized by the length of the unmatched suffix.
            if (termNorm.StartsWith(candidateNorm, StringComparison.OrdinalIgnoreCase))
                return (2, termNorm.Length - candidateNorm.Length);

            var idx = candidateNorm.IndexOf(termNorm, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                int extraLen = candidateNorm.Length - termNorm.Length;

                // Detect if match starts at an alphanumeric-type boundary (letter→digit or digit→letter).
                // "UC207G2L3": term "207G" at idx=2, before='C'(letter), first char='2'(digit) → boundary ✓
                // "ES207G2":   term "207G" at idx=2, before='S'(letter), first char='2'(digit) → boundary ✓
                // Both are boundaries here, so we further distinguish: does the REMAINDER after the match
                // also start a new boundary? In "UC207G2L3": after "207G" comes "2L3" — '2' is digit, 'G' was
                // letter before it → not a clean segment end. In "ES207G2": after "207G" comes "2" only.
                // 
                // Better signal: how many non-overlapping "segments" does the candidate have before the match?
                // Fewer leading segments = match is closer to a top-level segment start = better rank.
                // Count letter↔digit transitions before idx as a proxy for "how deep" the match is.
                int transitionsBefore = 0;
                for (int i = 1; i < idx && i < candidateNorm.Length; i++)
                {
                    if (char.IsLetter(candidateNorm[i - 1]) != char.IsLetter(candidateNorm[i]))
                        transitionsBefore++;
                }

                // score: fewer transitions before = better (lower score)
                // then shorter candidate = better
                // then earlier position = better
                int score = transitionsBefore * 10000 + extraLen * 100 + idx;
                return (1, score);
            }

            return (0, int.MaxValue);
        }

        /// <summary>
        /// Splits the raw field value (e.g. "UC.207G2L3") on non-alphanumeric chars
        /// and checks if any resulting segment starts with or equals the normalized term.
        /// This lets "UC.207G2L3" score as a startsWith match for "207G" via segment "207G2L3",
        /// ranking it above "ES.207.G2" where "207" and "G2" are separate segments.
        /// </summary>
        private static (int MatchType, int ExtraLen) GetMatchSegments(string rawField, string termNorm)
        {
            if (string.IsNullOrWhiteSpace(rawField) || string.IsNullOrWhiteSpace(termNorm))
                return (0, int.MaxValue);

            // Split raw field on non-alphanumeric chars
            var segments = new List<string>();
            var buf = new System.Text.StringBuilder();
            foreach (var ch in rawField)
            {
                if (char.IsLetterOrDigit(ch))
                    buf.Append(ch);
                else if (buf.Length > 0)
                {
                    segments.Add(buf.ToString().ToUpperInvariant());
                    buf.Clear();
                }
            }
            if (buf.Length > 0)
                segments.Add(buf.ToString().ToUpperInvariant());

            var best = (MatchType: 0, ExtraLen: int.MaxValue);
            foreach (var seg in segments)
            {
                var m = GetMatch(seg, termNorm);
                if (m.MatchType > best.MatchType || (m.MatchType == best.MatchType && m.ExtraLen < best.ExtraLen))
                    best = m;
            }
            return best;
        }

        private static (int MatchType, int ExtraLen) BestMatch(params (int MatchType, int ExtraLen)[] matches)
        {
            return matches
                .OrderByDescending(m => m.MatchType)
                .ThenBy(m => m.ExtraLen)
                .FirstOrDefault();
        }




        private const int NopPositionSortId = 16; //

        private static bool IsDefaultSort(int? orderby)
            => !orderby.HasValue || orderby.Value == 0 || orderby.Value == NopPositionSortId;

        private static string NormalizeCode(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return "";

            s = WebUtility.HtmlDecode(s).Trim();

            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                if (char.IsLetterOrDigit(ch))
                    sb.Append(char.ToUpperInvariant(ch));
            }
            return sb.ToString();
        }

        private static int ClosenessScore(string fieldNorm, string termNorm)
        {
            if (string.IsNullOrWhiteSpace(fieldNorm) || string.IsNullOrWhiteSpace(termNorm))
                return 0;

            if (fieldNorm.Equals(termNorm, StringComparison.Ordinal))
                return 3; // exact

            if (fieldNorm.StartsWith(termNorm, StringComparison.Ordinal))
                return 2; // starts-with

            if (fieldNorm.Contains(termNorm, StringComparison.Ordinal))
                return 1; // contains

            return 0;     // far
        }
        public void ClearIndex()
        {
            var client = new AlgoliaClient(_algoliaSearchSettings.ApplicationId, _algoliaSearchSettings.AdminKey);


            client.InitIndex(AlgoliaDefaults.DefaultIndexName).ClearIndex();
        }
        private Algolia.Search.Index GetPrimaryIndex()
        {
            var client = new AlgoliaClient(_algoliaSearchSettings.ApplicationId, _algoliaSearchSettings.AdminKey);
            return client.InitIndex(AlgoliaDefaults.DefaultIndexName);
        }
        public async Task UpdateAlgoliaItemAsync()
        {
            var index = GetPrimaryIndex();

            var pageIndex = 0;

            var productItems = await _algoliaUpdatableItemService.SearchAlgoliaUpdatableItemsAsync("Product");
            var categoryItems = await _algoliaUpdatableItemService.SearchAlgoliaUpdatableItemsAsync("Category");
            var manufacturerItems = await _algoliaUpdatableItemService.SearchAlgoliaUpdatableItemsAsync("Manufacturer");
            var vendorItems = await _algoliaUpdatableItemService.SearchAlgoliaUpdatableItemsAsync("Vendor");

            var pIds = productItems.Any() ? productItems.Select(x => x.EntityId).ToList() : new List<int>();
            var cIds = categoryItems.Any() ? categoryItems.Select(x => x.EntityId).ToList() : new List<int>();
            var mIds = manufacturerItems.Any() ? manufacturerItems.Select(x => x.EntityId).ToList() : new List<int>();
            var vIds = vendorItems.Any() ? vendorItems.Select(x => x.EntityId).ToList() : new List<int>();

            while (true)
            {
                try
                {
                    var products = await _algoliaCatalogService.GetProductsByEntityIdsAsync(
                        productIds: pIds,
                        categoryIds: cIds,
                        manufacturerIds: mIds,
                        vendorIds: vIds,
                        pageIndex: pageIndex,
                        pageSize: 100);

                    var objects = new List<JObject>();

                    foreach (var product in products)
                    {
                        try
                        {
                            objects.Add(await GetProductModelObjectAsync(product));
                        }
                        catch (Exception ex)
                        {
                            await _logger.ErrorAsync("AlgoliaSearch: " + ex.Message + ", Product Id = " + product.Id, ex);
                            continue;
                        }
                    }

                    if (products != null && products.Count != 0)
                        index.SaveObjects(objects);

                    await _algoliaUpdatableItemService.DeleteAlgoliaUpdatableItemsByProductsAsync(products);

                    if (!objects.Any())
                        break;
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync("AlgoliaSearch: " + ex.Message, ex);
                }

                pageIndex++;
            }


            pageIndex = 0;
            while (true)
            {
                try
                {
                    var products = await _algoliaCatalogService.GetProductsByEntityIdsAsync(
                        productIds: pIds,
                        categoryIds: cIds,
                        manufacturerIds: mIds,
                        vendorIds: vIds,
                        pageIndex: pageIndex,
                        pageSize: 100,
                        deletedOrUnpublishProduct: true);

                    var pids = await products.Select(p => p.Id.ToString()).ToListAsync();
                    if (pids == null || pids.Count == 0)
                        break;

                    await index.DeleteObjectsAsync(pids);
                    await _algoliaUpdatableItemService.DeleteAlgoliaUpdatableItemsByProductsAsync(products);
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync("AlgoliaSearch: " + ex.Message, ex);
                }

                pageIndex++;
            }


            try
            {
                var substituteItems = await _algoliaUpdatableItemService.SearchAlgoliaUpdatableItemsAsync("Substitute");
                if (substituteItems != null && substituteItems.Any())
                {

                    var toUpsert = substituteItems

                        .Select(x => x.EntityId)
                        .Distinct()
                        .ToList();

                    var toDelete = substituteItems

                        .Select(x => x.EntityId)
                        .Distinct()
                        .ToList();

                    if (toUpsert.Any())
                        await UpsertSubstitutesByIdsAsync(toUpsert);

                    if (toDelete.Any())
                        await DeleteSubstitutesByIdsAsync(toDelete);


                    await _algoliaUpdatableItemService.DeleteAlgoliaUpdatableItemsByEntityAsync("Substitute", substituteItems.Select(x => x.EntityId).ToList());
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("AlgoliaSearch(Substitutes): " + ex.Message, ex);
            }
        }

        private async Task<(int InStock, int StockQty)> GetSearchStockStateAsync(Product product)
        {
            if (product == null)
                return (0, 0);

            var totalStock = await _productService.GetTotalStockQuantityAsync(product);


            var stockQty = totalStock;
            var inStock = totalStock > 0 ? 1 : 0;


            try
            {
                var combinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);

                if (combinations != null && combinations.Any())
                {
                    var validCombinationQty = combinations
                        .Where(c => c != null && !string.IsNullOrWhiteSpace(c.AttributesXml))
                        .Sum(c => c.StockQuantity);

                    if (validCombinationQty >= 0)
                    {
                        stockQty = validCombinationQty;
                        inStock = validCombinationQty > 0 ? 1 : 0;
                    }
                }
            }
            catch
            {
                // keep fallback
            }

            return (inStock, stockQty);
        }

        private Algolia.Search.Index GetWriteIndex()
        {
            var client = new AlgoliaClient(_algoliaSearchSettings.ApplicationId, _algoliaSearchSettings.AdminKey);
            return client.InitIndex(AlgoliaDefaults.DefaultIndexName); // Products
        }

        private Algolia.Search.Index GetSearchIndex(int? orderby)
        {
            var client = new AlgoliaClient(_algoliaSearchSettings.ApplicationId, _algoliaSearchSettings.AdminKey);


            if (!orderby.HasValue || orderby.Value == 0)
                return client.InitIndex(AlgoliaDefaults.DefaultIndexName);

            // other sorts...
            return GetIndex(orderby);
        }



        private static string NormalizePart(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return "";

            s = WebUtility.HtmlDecode(s).Trim();

            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                if (char.IsLetterOrDigit(ch))
                    sb.Append(char.ToUpperInvariant(ch));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Splits a product name/sku into searchable tokens so that Algolia can find
        /// substrings like "52-128" inside "JPU52-128+JF434".
        /// Strategy: split on non-alphanumeric boundaries AND on letter↔digit transitions,
        /// then emit every unique segment of length >= 2.
        /// Example: "JPU52-128+JF434" → "JPU52 52 128 JF434 JF 434"
        /// </summary>
        private static string TokenizeSegments(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return "";

            s = WebUtility.HtmlDecode(s).Trim().ToUpperInvariant();

            // Step 1: split on non-alphanumeric characters
            var parts = new List<string>();
            var buf = new StringBuilder();
            foreach (var ch in s)
            {
                if (char.IsLetterOrDigit(ch))
                    buf.Append(ch);
                else if (buf.Length > 0)
                {
                    parts.Add(buf.ToString());
                    buf.Clear();
                }
            }
            if (buf.Length > 0)
                parts.Add(buf.ToString());

            // Step 2: for each part, also split on letter↔digit transitions
            var segments = new HashSet<string>();
            foreach (var part in parts)
            {
                if (part.Length >= 2)
                    segments.Add(part);

                // sub-split on alpha/digit boundary
                var sub = new StringBuilder();
                bool? lastIsDigit = null;
                foreach (var ch in part)
                {
                    bool isDigit = char.IsDigit(ch);
                    if (lastIsDigit.HasValue && isDigit != lastIsDigit.Value && sub.Length > 0)
                    {
                        if (sub.Length >= 2)
                            segments.Add(sub.ToString());
                        sub.Clear();
                    }
                    sub.Append(ch);
                    lastIsDigit = isDigit;
                }
                if (sub.Length >= 2)
                    segments.Add(sub.ToString());
            }

            return string.Join(" ", segments);
        }

        public async Task UploadSubstitutesAsync(bool clearFirst = true)
        {
            var client = new AlgoliaClient(_algoliaSearchSettings.ApplicationId, _algoliaSearchSettings.AdminKey);
            var subIndex = client.InitIndex(AlgoliaDefaults.SubstitutesIndexName);

            dynamic settings = new JObject();

            // searchable: raw + normalized (to support typing with/without hyphen)
            settings.searchableAttributes = new JArray(new[]
            {
        AlgoliaDefaults.SubstitutesSubstituteCodeField,
        "SubstituteCodeNormalized",
        AlgoliaDefaults.SubstitutesStockCodeField,
        "StockCodeNormalized",
        AlgoliaDefaults.SubstitutesDescriptionField
    });

            // faceting (so we can filter exact)
            settings.attributesForFaceting = new JArray(new[]
            {
        AlgoliaDefaults.SubstitutesSubstituteCodeField,
        "SubstituteCodeNormalized",
        AlgoliaDefaults.SubstitutesStockCodeField,
        "StockCodeNormalized"
    });

            var resp = subIndex.SetSettings(settings, true);
            WaitTaskIfAny(subIndex, resp);

            if (clearFirst)
            {
                var clearResp = subIndex.ClearIndex();
                WaitTaskIfAny(subIndex, clearResp);
            }

            var pageIndex = 0;
            var pageSize = 5000;

            while (true)
            {
                var batch = await _substitutesService.GetSubstitutesBatchAsync(pageIndex, pageSize);
                if (batch == null || batch.Count == 0)
                    break;

                var objects = new List<JObject>(batch.Count);
                foreach (var r in batch)
                {
                    var subCode = r.SubstituteCode ?? "";
                    var stockCode = r.StockCode ?? "";

                    objects.Add(new JObject
                    {
                        ["objectID"] = r.SubstitutesId,
                        [AlgoliaDefaults.SubstitutesProductIdField] = r.ProductId,
                        [AlgoliaDefaults.SubstitutesStockCodeField] = stockCode,
                        [AlgoliaDefaults.SubstitutesSubstituteCodeField] = subCode,
                        [AlgoliaDefaults.SubstitutesDescriptionField] = r.Description ?? "",
                        ["substitute_type"] = r.SubstituteType ?? "",


                        ["SubstituteCodeNormalized"] = NormalizePart(subCode),
                        ["StockCodeNormalized"] = NormalizePart(stockCode)
                    });
                }

                var saveResp = subIndex.SaveObjects(objects);
                WaitTaskIfAny(subIndex, saveResp);

                pageIndex++;
            }
        }



        private static bool HasAnyFilters(
    IList<int> cids, IList<int> mids, IList<int> vids,
    IList<FilteredGroupModel> specids, IList<int> ratings,
    decimal? minPrice, decimal? maxPrice,
    IList<Models.RangeFilterModel> rangeFilters, bool onlyInStock = false, bool discountOnly = false)
        {
            return (cids?.Any() ?? false)
                || (mids?.Any() ?? false)
                || (vids?.Any() ?? false)
                || (specids?.Any() ?? false)
                || (ratings?.Any() ?? false)
                || (minPrice.HasValue && minPrice.Value > 0)
                || (maxPrice.HasValue && maxPrice.Value > 0)
                || (rangeFilters?.Any() ?? false)
             || onlyInStock || discountOnly;

        }

        public async Task<IPagedList<ProductOverviewModel>> SearchProductsAsync(
          string searchTerms = "",
          IList<int> cids = null, IList<int> mids = null, IList<int> vids = null,
          IList<FilteredGroupModel> specids = null, IList<FilteredGroupModel> attrids = null,
          IList<int> ratings = null,
          decimal? minPrice = null, decimal? maxPrice = null,
          int? orderby = null, int pageIndex = 0, int pageSize = int.MaxValue,
          IList<Models.RangeFilterModel> rangeFilters = null, bool onlyInStock = false, bool discountOnly = false)
        {
            pageIndex = Math.Max(pageIndex, 0);
            pageSize = pageSize <= 0 ? 12 : Math.Min(pageSize, 1000);

            searchTerms = WebUtility.HtmlDecode(searchTerms ?? "").Trim();

            var filters = await GetFilterStringAsync(
      cids, mids, vids, specids, attrids, ratings, minPrice, maxPrice, rangeFilters, onlyInStock, discountOnly);

            var hasFilters = HasAnyFilters(
                cids, mids, vids, specids, ratings, minPrice, maxPrice, rangeFilters, onlyInStock, discountOnly);

            var forceStockSort = false;
            var allowSubstitutesMerge = !string.IsNullOrWhiteSpace(searchTerms) && !hasFilters;

            var candidatesSize = Math.Min(2000, Math.Max(pageSize * (pageIndex + 1) * 5, pageSize * 10));

            var (allIds, total, substituteIds) = await SearchUnifiedIdsAsync(
                searchTerms: searchTerms,
                filters: filters,
                pageIndex: 0,
                pageSize: candidatesSize,
                orderby: orderby,
                allowSubstitutesMerge: allowSubstitutesMerge,
                forceStockSort: forceStockSort
            );

            if (allIds == null || allIds.Count == 0)
                return new PagedList<ProductOverviewModel>(new List<ProductOverviewModel>(), pageIndex, pageSize, 0);

            var pageIds = allIds.Skip(pageIndex * pageSize).Take(pageSize).ToList();

            if (pageIds.Count == 0)
                return new PagedList<ProductOverviewModel>(new List<ProductOverviewModel>(), pageIndex, pageSize, total);

            var products = await _algoliaCatalogService.SearchProductsAsync(productIds: pageIds, inProductIdsOnly: true);

            var orderedProducts = products
                .OrderBy(p => pageIds.IndexOf(p.Id))
                .ToList();

            var models = (await _productModelFactory.PrepareProductOverviewModelsAsync(orderedProducts))
                .OrderBy(m => pageIds.IndexOf(m.Id))
                .ToList();

            foreach (var m in models)
            {
                var isSub = substituteIds != null && substituteIds.Contains(m.Id);
                m.CustomProperties["IsSubstituteResult"] = isSub ? "true" : "false";
            }

            await _logger.InformationAsync("ALGOLIA SearchProductsAsync pageIds = " + string.Join(",", pageIds));
            await _logger.InformationAsync("ALGOLIA SearchProductsAsync modelIds = " + string.Join(",", models.Select(x => x.Id)));

            return new PagedList<ProductOverviewModel>(models, pageIndex, pageSize, total);
        }





        private async Task<List<int>> OrderIdsByStockDescAsync(IEnumerable<int> ids)
        {
            var list = ids?.Where(x => x > 0).Distinct().ToList() ?? new List<int>();
            if (list.Count <= 1)
                return list;

            var index = GetPrimaryIndex();


            var objectIds = list.Select(x => x.ToString()).ToList();

            JObject res = null;


            try
            {

                res = await index.GetObjectsAsync(objectIds, new[] { "Id", "InStock", "StockQty" });
            }
            catch
            {

                try
                {
                    res = index.GetObjects(objectIds, new[] { "Id", "InStock", "StockQty" });
                }
                catch
                {

                    return list;
                }
            }

            var results = res?["results"] as JArray;
            if (results == null || results.Count == 0)
                return list;


            var stockMap = new Dictionary<int, (int InStock, int StockQty)>();

            foreach (var r in results)
            {
                if (r == null || r.Type == JTokenType.Null)
                    continue;


                var idStr = r["Id"]?.ToString() ?? r["objectID"]?.ToString();
                if (!int.TryParse(idStr, out var pid) || pid <= 0)
                    continue;

                var inStock = int.TryParse(r["InStock"]?.ToString(), out var ins) ? ins : 0;
                var stockQty = int.TryParse(r["StockQty"]?.ToString(), out var st) ? st : 0;

                stockMap[pid] = (inStock, stockQty);
            }


            return list
                .OrderByDescending(id => stockMap.TryGetValue(id, out var v) ? v.InStock : 0)
                .ThenByDescending(id => stockMap.TryGetValue(id, out var v) ? v.StockQty : 0)
                .ToList();
        }





        private async Task<IList<Product>> SearchProductsBySubstituteAsync(string term, int pageSize)
        {

            var subProductIds = await SearchInSubstitutesGetProductIdsAsync(term, pageIndex: 0, pageSize: pageSize);

            if (subProductIds == null || subProductIds.Count == 0)
                return new List<Product>();


            var products = await _algoliaCatalogService.SearchProductsAsync(productIds: subProductIds, inProductIdsOnly: true);

            return products.OrderBy(p => subProductIds.IndexOf(p.Id)).ToList();
        }








        public async Task<IList<JObject>> SearchProductsForAutoCompleteAsync(string term, int take)
        {
            term = WebUtility.HtmlDecode(term ?? "").Trim();
            take = take <= 0 ? 6 : Math.Min(take, 20);

            if (string.IsNullOrWhiteSpace(term))
                return new List<JObject>();

            var filters = "";
            if (!_catalogSettings.IgnoreStoreLimitations)
                filters = $"(LimitedToStores = 0 OR Stores = {(await _storeContext.GetCurrentStoreAsync()).Id})";

            var candidatesSize = Math.Min(200, take * 20);

            var (allIds, total, substituteIds) = await SearchUnifiedIdsAsync(
                searchTerms: term,
                filters: filters,
                pageIndex: 0,
                pageSize: candidatesSize,
                orderby: null,
                allowSubstitutesMerge: true,
                forceStockSort: false
            );

            if (allIds == null || allIds.Count == 0)
                return new List<JObject>();

            var finalIds = allIds.Take(take).ToList();

            var products = await _algoliaCatalogService.SearchProductsAsync(productIds: finalIds, inProductIdsOnly: true);

            var ordered = products
                .OrderBy(p => finalIds.IndexOf(p.Id))
                .ToList();

            var list = new List<JObject>(ordered.Count);
            foreach (var p in ordered)
            {
                var isSub = substituteIds != null && substituteIds.Contains(p.Id);

                list.Add(new JObject
                {
                    ["Id"] = p.Id,
                    ["Name"] = p.Name,
                    ["Sku"] = p.Sku ?? "",
                    ["SeName"] = await _urlRecordService.GetSeNameAsync(p),
                    ["AutoCompleteImageUrl"] = await GetAutoCompleteImageUrlAsync(p),
                    ["IsSubstituteResult"] = isSub ? "true" : "false"
                });
            }

            return list;
        }

        public async Task<string> GetAutoCompleteImageUrlByProductIdAsync(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return string.Empty;

            return await GetAutoCompleteImageUrlAsync(product);
        }



        private async Task<List<int>> SearchInSubstitutesGetProductIdsAsync(string searchTerms, int pageIndex, int pageSize)
        {
            var subIndex = GetIndex(AlgoliaDefaults.SubstitutesIndexName);

            var res = subIndex.Search(new Query(searchTerms ?? "")
                .SetPage(pageIndex)
                .SetNbHitsPerPage(pageSize)
                .SetAttributesToRetrieve(new[] { "ProductId" }));

            var hits = res["hits"];
            if (hits == null || !hits.Any())
                return new List<int>();

            return hits
                .Select(h => h["ProductId"]?.ToString())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => int.TryParse(s, out var x) ? x : 0)
                .Where(x => x > 0)
                .Distinct()
                .ToList();
        }

        public async Task<AlgoliaFilters> GetAlgoliaFiltersAsync(
         string searchTerms,
         IList<int> cids = null,
         IList<int> mids = null,
         IList<int> vids = null,
         IList<FilteredGroupModel> specids = null,
         IList<int> ratings = null,
         decimal? minPrice = null,
         decimal? maxPrice = null,
         bool onlyInStock = false,
         bool discountOnly = false)
        {
            var model = new AlgoliaFilters();


            var filters = await GetFilterStringAsync(
                cids, mids, vids, specids, null, ratings, minPrice, maxPrice, null, onlyInStock, discountOnly);

            var storeFilter = "";
            if (!_catalogSettings.IgnoreStoreLimitations)
                storeFilter = $"LimitedToStores = 0 OR Stores = {(await _storeContext.GetCurrentStoreAsync()).Id}";

            var finalFilter = string.IsNullOrEmpty(storeFilter) ? filters
                : string.IsNullOrEmpty(filters) ? storeFilter
                : $"({storeFilter}) AND ({filters})";

            var index = GetPrimaryIndex();
            var query = new Query(searchTerms)
                .SetFacets(AlgoliaDefaults.FacetedAttributes)
                .SetRemoveWordsIfNoResult(Query.RemoveWordsIfNoResult.LAST_WORDS)
                .EnableFacetingAfterDistinct(true);

            if (!string.IsNullOrEmpty(finalFilter))
                query.SetFilters(finalFilter);


            if (minPrice.HasValue || maxPrice.HasValue)
            {
                var priceFilter = "";
                if (minPrice.HasValue && maxPrice.HasValue)
                    priceFilter = $"Price:{minPrice.Value} TO {maxPrice.Value}";
                else if (minPrice.HasValue)
                    priceFilter = $"Price >= {minPrice.Value}";
                else if (maxPrice.HasValue)
                    priceFilter = $"Price <= {maxPrice.Value}";
                if (!string.IsNullOrEmpty(priceFilter))
                    query.SetNumericFilters(priceFilter);
            }

            var res = index.Search(query);







            if (_algoliaSearchSettings.AllowPriceRangeFilter && res["facets"]["Price"] != null)
            {
                var json = res["facets"]["Price"].ToString();
                var values = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
                decimal max = decimal.MinValue, min = decimal.MaxValue;
                foreach (var item in values)
                {
                    if (decimal.TryParse(item.Key, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal val))
                    {
                        if (val > max)
                            max = val;
                        if (val < min)
                            min = val;
                    }
                }
                if (min != decimal.MaxValue && max != decimal.MinValue)
                {
                    model.MaxPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(max, await _workContext.GetWorkingCurrencyAsync());
                    model.MinPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(min, await _workContext.GetWorkingCurrencyAsync());
                }
            }


            if (_algoliaSearchSettings.AllowVendorFilter && res["facets"]["FilterableVendor.Id"] != null)
            {
                var values = JsonConvert.DeserializeObject<Dictionary<string, int>>(res["facets"]["FilterableVendor.Id"].ToString()).ToList();
                model.AvailableVendors = values.Take(_algoliaSearchSettings.MaximumVendorsShowInFilter)
                    .Select(x => new FilterItemModel { Count = x.Value, Id = int.Parse(x.Key) }).ToList();
            }


            if (_algoliaSearchSettings.AllowManufacturerFilter && res["facets"]["FilterableManufacturers.Id"] != null)
            {
                var values = JsonConvert.DeserializeObject<Dictionary<string, int>>(res["facets"]["FilterableManufacturers.Id"].ToString()).ToList();
                model.AvailableManufacturers = values.Take(_algoliaSearchSettings.MaximumManufacturersShowInFilter)
                    .Select(x => new FilterItemModel { Count = x.Value, Id = int.Parse(x.Key) }).ToList();
            }


            if (_algoliaSearchSettings.AllowCategoryFilter && res["facets"]["FilterableCategories.Id"] != null)
            {
                var values = JsonConvert.DeserializeObject<Dictionary<string, int>>(res["facets"]["FilterableCategories.Id"].ToString()).ToList();
                model.AvailableCategories = values.Take(_algoliaSearchSettings.MaximumCategoriesShowInFilter)
                    .Select(x => new FilterItemModel { Count = x.Value, Id = int.Parse(x.Key) }).ToList();
            }


            if (_algoliaSearchSettings.AllowSpecificationFilter && res["facets"]["FilterableSpecifications.OptionIdSpecificationId"] != null)
            {
                var values = JsonConvert.DeserializeObject<Dictionary<string, int>>(res["facets"]["FilterableSpecifications.OptionIdSpecificationId"].ToString()).ToList();
                model.AvailableSpecifications = values
                    .Select(x =>
                    {
                        var parts = (x.Key ?? "").Split('-', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length != 2)
                            return null;
                        return new
                        {
                            Count = x.Value,
                            OptionId = int.TryParse(parts[0], out var oid) ? oid : 0,
                            SpecId = int.TryParse(parts[1], out var sid) ? sid : 0
                        };
                    })
                    .Where(x => x != null && x.OptionId > 0 && x.SpecId > 0)
                    .GroupBy(x => x.SpecId)
                    .SelectMany(g => g.Take(_algoliaSearchSettings.MaximumSpecificationsShowInFilter))
                    .Select(x => new FilterItemModel { Count = x.Count, Id = x.OptionId })
                    .ToList();
            }


            if (_algoliaSearchSettings.AllowRatingFilter && res["facets"]["Rating"] != null)
            {
                var values = JsonConvert.DeserializeObject<Dictionary<string, int>>(res["facets"]["Rating"].ToString());
                for (int i = 1; i <= 5; i++)
                    model.AvailableRatings.Add(new FilterItemModel { Id = i, Count = values.TryGetValue(i.ToString(), out var v) ? v : 0 });
            }

            return model;
        }



        private static string EscapeAlgoliaFilterValue(string value)
        {
            if (value == null)
                return "";
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }


        public async Task<(long TotalHits, List<int> ProductIds)> SearchInSubstitutesBySubCodeGetProductIdsAsync(string term, int take)
        {
            term = (term ?? "").Trim();
            if (string.IsNullOrWhiteSpace(term))
                return (0, new List<int>());

            take = Math.Min(Math.Max(take, 1), 2000);

            var subIndex = GetIndex(AlgoliaDefaults.SubstitutesIndexName);


            var raw = EscapeAlgoliaFilterValue(term);
            var filterRaw = $"{AlgoliaDefaults.SubstitutesSubstituteCodeField}:\"{raw}\"";

            JObject res = null;
            try
            {
                res = subIndex.Search(new Query("")
                    .SetFilters(filterRaw)
                    .SetNbHitsPerPage(Math.Min(take, 1000))
                    .SetAttributesToRetrieve(new[]
                    {
                AlgoliaDefaults.SubstitutesProductIdField,
                AlgoliaDefaults.SubstitutesSubstituteCodeField
                    }));
            }
            catch { }


            var hits = res?["hits"] as JArray;
            if (hits == null || hits.Count == 0)
            {
                var norm = EscapeAlgoliaFilterValue(NormalizePart(term));
                var filterNorm = $"SubstituteCodeNormalized:\"{norm}\"";

                try
                {
                    res = subIndex.Search(new Query("")
                        .SetFilters(filterNorm)
                        .SetNbHitsPerPage(Math.Min(take, 1000))
                        .SetAttributesToRetrieve(new[]
                        {
                    AlgoliaDefaults.SubstitutesProductIdField,
                    "SubstituteCodeNormalized"
                        }));
                }
                catch { }

                hits = res?["hits"] as JArray;
            }




            if (hits == null || hits.Count == 0)
                return (0, new List<int>());

            var ids = hits
                .Select(h => h?[AlgoliaDefaults.SubstitutesProductIdField]?.ToString())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => int.TryParse(s, out var x) ? x : 0)
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            var totalHits = long.TryParse(res?["nbHits"]?.ToString(), out var th) ? th : ids.Count;
            return (totalHits, ids);
        }
        public async Task<List<int>> SearchProductIdsForAutoCompleteAsync(string term, int take, string indexName)
        {
            take = take <= 0 ? 6 : Math.Min(take, 20);

            Algolia.Search.Index index;
            if (!string.IsNullOrWhiteSpace(indexName))
                index = GetIndex(indexName);
            else
                index = GetIndex();

            var res = index.Search(new Query(term ?? "")
                .SetNbHitsPerPage(take)
                .SetAttributesToRetrieve(new[] { "Id" })
            );

            var hits = res["hits"];
            if (hits == null || !hits.Any())
                return new List<int>();

            return hits
                .Select(h => h["Id"]?.ToString())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => int.TryParse(s, out var x) ? x : 0)
                .Where(x => x > 0)
                .Distinct()
                .Take(take)
                .ToList();
        }



        private JObject BuildSubstituteObject(dynamic r)
        {
            var sub = (r.SubstituteCode ?? "").ToString();
            var stock = (r.StockCode ?? "").ToString();

            return new JObject
            {
                ["objectID"] = r.SubstitutesId?.ToString(),
                [AlgoliaDefaults.SubstitutesProductIdField] = r.ProductId ?? 0,
                [AlgoliaDefaults.SubstitutesStockCodeField] = stock,
                [AlgoliaDefaults.SubstitutesSubstituteCodeField] = sub,
                [AlgoliaDefaults.SubstitutesDescriptionField] = r.Description ?? "",
                ["substitute_type"] = r.SubstituteType ?? "",


                ["SubstituteCodeNormalized"] = NormalizePart(sub),
                ["StockCodeNormalized"] = NormalizePart(stock)
            };
        }
        private async Task UpsertSubstitutesByIdsAsync(IList<int> substituteIds)
        {
            if (substituteIds == null || substituteIds.Count == 0)
                return;

            var client = new AlgoliaClient(_algoliaSearchSettings.ApplicationId, _algoliaSearchSettings.AdminKey);
            var subIndex = client.InitIndex(AlgoliaDefaults.SubstitutesIndexName);


            var rows = await _substitutesService.GetSubstitutesByIdsAsync(substituteIds);

            if (rows == null || rows.Count == 0)
                return;

            var objects = new List<JObject>(rows.Count);
            foreach (var r in rows)
                objects.Add(BuildSubstituteObject(r));


            subIndex.SaveObjects(objects);
        }

        private async Task DeleteSubstitutesByIdsAsync(IList<int> substituteIds)
        {
            if (substituteIds == null || substituteIds.Count == 0)
                return;

            var client = new AlgoliaClient(_algoliaSearchSettings.ApplicationId, _algoliaSearchSettings.AdminKey);
            var subIndex = client.InitIndex(AlgoliaDefaults.SubstitutesIndexName);

            var objectIds = substituteIds.Select(x => x.ToString()).ToList();
            await subIndex.DeleteObjectsAsync(objectIds);
        }

        #endregion
    }

}
