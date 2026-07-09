using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using FastMember;
using Google.Apis.Auth;
using Google.Apis.Http;
using HtmlAgilityPack;
using Humanizer;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using MailKit.Search;
using MaxMind.GeoIP2.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Core.Http.Extensions;
using Nop.Data;
using Nop.Data.DataProviders;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
//using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Services.Security;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Factories;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Boards;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Media;
using Nop.Web.Models.Order;
using Nop.Web.Models.ShoppingCart;
using NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses.Payments;
using NopAdvance.Plugin.Misc.PublicAPI.Services;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using SevenSpikes.Nop.Framework;
using SevenSpikes.Nop.Framework.AutoMapper;
using SevenSpikes.Nop.Framework.Domain.Enums;
using SevenSpikes.Nop.Framework.Mappings;
using SevenSpikes.Nop.Plugins.AnywhereSliders.Areas.Admin.Helpers;
using SevenSpikes.Nop.Plugins.AnywhereSliders.Services;
using SevenSpikes.Nop.Plugins.JCarousel.Domain;
using SevenSpikes.Nop.Plugins.JCarousel.Models;
using SevenSpikes.Nop.Plugins.JCarousel.Services;
using SevenSpikes.Nop.Plugins.ProductRibbons;
using SevenSpikes.Nop.Plugins.ProductRibbons.Services;
using SevenSpikes.Nop.Plugins.SmartProductCollections.Data;
using SevenSpikes.Nop.Plugins.SmartProductCollections.Domain;
using SevenSpikes.Nop.Plugins.SmartProductCollections.Domain.Enums;
using SevenSpikes.Nop.Plugins.SmartProductCollections.Models;
using SevenSpikes.Nop.Plugins.SmartProductCollections.Services;
using static System.Net.WebRequestMethods;
using static Nop.Web.Models.Catalog.ProductDetailsModel;
using static NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention.HomePageBannerResponseModel;
using static NopAdvance.Plugin.Misc.PublicAPI.Controllers.ProductApiOverviewModel;
using IWebAddressModelFactory = Nop.Web.Factories.IAddressModelFactory;
using WebAddressModel = Nop.Web.Models.Common.AddressModel;


namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention
{
    public class SSMapEntityMapping : BaseEntity
    {
        public int EntityType { get; set; }
        public int EntityId { get; set; }
        public int MappedEntityId { get; set; }
        public int DisplayOrder { get; set; }
        public int MappingType { get; set; }
    }
    public partial class ApiExtentionController : BaseAPIController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IRepository<ShipmentItem> _shipmentItemRepository;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IPictureService _pictureService;
        private readonly VendorSettings _vendorSettings;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly MediaSettings _mediaSettings;
        private readonly IVendorService _vendorService;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerService _customerService;
        private readonly IOrderReportService _orderReportService;
        private readonly IPermissionService _permissionService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly Nop.Web.Factories.IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;

        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly Nop.Web.Factories.IProductModelFactory _productModelFactory;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICategoryService _categoryService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly CatalogSettings _catalogSettings;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        protected readonly ILogger _logger;
        protected readonly IProductRibbonService _ProductRibbonService;
        protected readonly IJCarouselService _JCarouselService;
        protected readonly IItemsMappingService _ItemsMappingService;
        protected readonly ISmartProductCollectionsService _smartProductCollectionsService;
        private readonly IProductsGroupService _iproductsGroupService;
        private readonly IProductsGroupItemService _iproductsGroupItemService;
        private readonly SmartProductCollectionsSettings _smartProductCollectionsSettings;
        private readonly IEntityWidgetMappingService _entityWidgetMappingService;
        private readonly JCarouselGeneralSettings _jCarouselGeneralSettings;
        private readonly ISliderService _sliderService;

        private readonly ISlidePictureHelper _slidePictureHelper;
        private readonly IMapper _mapper;
        protected readonly OrderSettings _orderSettings;
        protected readonly ICheckoutModelFactory _checkoutModelFactory;
        protected readonly IShipmentService _shipmentService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IOrderModelFactory _orderModelFactory;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPluginPaymentService _pluginPaymentService;
        private readonly IPaymentService _paymentService;
        protected readonly AddressSettings _addressSettings;
        private readonly ICustomerModelFactory _customerModelFactory;
        protected readonly IRepository<Product> _productRepository;
        protected readonly IShippingService _shippingService;
        private readonly IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeParser;
        private readonly IAttributeService<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeService;
        private readonly IDownloadService _downloadService;
        private readonly System.Net.Http.IHttpClientFactory _httpClientFactory;
        private readonly IRepository<Recommendations_SimilarProducts_Similarities> _similarProductRecordRepository;
        private readonly IRepository<SSMapEntityMapping> _entityMappingRepository;




        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;

        private readonly IConfiguration _config;

        private readonly IWebAddressModelFactory _addressModelFactory; // public factory
        protected readonly PaymentSettings _paymentSettings;
        protected readonly ICurrencyService _currencyService;
        protected readonly ITaxService _taxService;
        protected readonly IPriceFormatter _priceFormatter;
        private readonly INopDataProvider _dataProvider;
        #endregion



        #region Ctor

        public ApiExtentionController(IProductService productService,
            IRepository<ShipmentItem> shipmentItemRepository,
            ISpecificationAttributeService specificationAttributeService, IPictureService pictureService, VendorSettings vendorSettings, ICatalogModelFactory catalogModelFactory,
            ILocalizationService localizationService, MediaSettings mediaSettings, IVendorService vendorService, IStoreContext storeContext,
            IGenericAttributeService genericAttributeService, IWorkContext workContext, IWebHelper webHelper, ICustomerService customerService,
            IOrderReportService orderReportService, IPermissionService permissionService, IShoppingCartService shoppingCartService,
            Nop.Web.Factories.IShoppingCartModelFactory shoppingCartModelFactory,
            IUrlRecordService urlRecordService, IWebHostEnvironment hostEnvironment, IAddressService addressService,
            ICountryService countryService, IStateProvinceService stateProvinceService, IHttpContextAccessor httpContextAccessor,
            IStoreMappingService storeMappingService,

            Nop.Web.Factories.IProductModelFactory productModelFactory,
            IAclService aclService, ICategoryService categoryService,
            ICustomerActivityService customerActivityService, IStaticCacheManager staticCacheManager, CatalogSettings catalogSettings, IRecentlyViewedProductsService recentlyViewedProductsService


            , IOrderProcessingService orderProcessingService,
            IOrderService orderService,
              CustomerSettings customerSettings,
               ICustomerRegistrationService customerRegistrationService, ILogger logger, IProductRibbonService ProductRibbonService, IJCarouselService JCarouselService, IItemsMappingService ItemsMappingService, ISmartProductCollectionsService smartProductCollectionsService
          , IProductsGroupService iproductsGroupService, IProductsGroupItemService iproductsGroupItemService, SmartProductCollectionsSettings smartProductCollectionsSettings, JCarouselGeneralSettings jCarouselGeneralSettings, IEntityWidgetMappingService entityWidgetMappingService, ISliderService sliderService, ISlidePictureHelper slidePictureHelper, IMapper mapper, OrderSettings orderSettings, ICheckoutModelFactory checkoutModelFactory, IShipmentService shipmentService, IManufacturerService manufacturerService, IOrderModelFactory orderModelFactory, IPaymentPluginManager paymentPluginManager, IPluginPaymentService pluginPaymentService, IPaymentService paymentService, AddressSettings addressSettings, ICustomerModelFactory customerModelFactory, ShoppingCartSettings shoppingCartSettings, IRepository<Product> productRepository, IShippingService shippingService, IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser, System.Net.Http.IHttpClientFactory httpClientFactory,
                //IUserService users,
                //ITokenService tokenService,
                IConfiguration config,
            IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService, IDownloadService downloadService, IRepository<Recommendations_SimilarProducts_Similarities> similarroductRecordRepository, IWebAddressModelFactory addressModelFactory, PaymentSettings paymentSettings, ICurrencyService currencyService, ITaxService taxService, IPriceFormatter priceFormatter, IRepository<SSMapEntityMapping> entityMappingRepository, INopDataProvider dataProvider)
        {
            _productService = productService;
            _shipmentItemRepository = shipmentItemRepository;
            _specificationAttributeService = specificationAttributeService;
            _pictureService = pictureService;
            _vendorSettings = vendorSettings;
            _catalogModelFactory = catalogModelFactory;
            _localizationService = localizationService;
            _mediaSettings = mediaSettings;
            _vendorService = vendorService;
            _storeContext = storeContext;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
            _webHelper = webHelper;
            _customerService = customerService;
            _orderReportService = orderReportService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _permissionService = permissionService;
            _shoppingCartService = shoppingCartService;
            _urlRecordService = urlRecordService;
            _hostEnvironment = hostEnvironment;
            _addressService = addressService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;

            _productModelFactory = productModelFactory;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _httpContextAccessor = httpContextAccessor;
            _categoryService = categoryService;
            _customerActivityService = customerActivityService;
            _staticCacheManager = staticCacheManager;
            _catalogSettings = catalogSettings;
            _recentlyViewedProductsService = recentlyViewedProductsService;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _customerSettings = customerSettings;
            _customerRegistrationService = customerRegistrationService;
            _logger = logger;
            _ProductRibbonService = ProductRibbonService;
            _JCarouselService = JCarouselService;
            _ItemsMappingService = ItemsMappingService;
            _smartProductCollectionsService = smartProductCollectionsService;
            _iproductsGroupService = iproductsGroupService;
            _iproductsGroupItemService = iproductsGroupItemService;
            _smartProductCollectionsSettings = smartProductCollectionsSettings;
            _jCarouselGeneralSettings = jCarouselGeneralSettings;
            _entityWidgetMappingService = entityWidgetMappingService;
            _sliderService = sliderService;
            _slidePictureHelper = slidePictureHelper;
            _mapper = mapper;
            _orderSettings = orderSettings;
            _checkoutModelFactory = checkoutModelFactory;
            _shipmentService = shipmentService;
            _manufacturerService = manufacturerService;
            _orderModelFactory = orderModelFactory;
            _paymentPluginManager = paymentPluginManager;
            _pluginPaymentService = pluginPaymentService;
            _paymentService = paymentService;
            _addressSettings = addressSettings;
            _customerSettings = customerSettings;
            _customerModelFactory = customerModelFactory;
            _shoppingCartSettings = shoppingCartSettings;
            _productRepository = productRepository;
            _shippingService = shippingService;
            _checkoutAttributeParser = checkoutAttributeParser;
            _checkoutAttributeService = checkoutAttributeService;
            _downloadService = downloadService;
            _httpClientFactory = httpClientFactory;
            // _users = users;
            _config = config;
            _similarProductRecordRepository = similarroductRecordRepository;
            _addressModelFactory = addressModelFactory;
            _paymentSettings = paymentSettings;

            _currencyService = currencyService;
            _taxService = taxService;
            _priceFormatter = priceFormatter;
            _entityMappingRepository = entityMappingRepository;
            _dataProvider = dataProvider;
        }

        #endregion

        #region Methods

        private async Task<IList<Product>> GetProductsBasedOnSourceTypeAsync(ProductsGroupItem item, int take = 0)
        {
            if (item == null || !item.Active)
                return new List<Product>();

          
            if ((int)item.SourceType == 70)
            {
                const int entityTypeProductsGroupItem = 50;
                const int mappingTypeProduct = 2;

              
                var mappedIds = await GetMappedProductIdsAsync(item.Id);

                mappedIds = mappedIds.Distinct().ToList();

                if (!mappedIds.Any())
                    return new List<Product>();

                var products = await _productService.GetProductsByIdsAsync(mappedIds.ToArray());

                var ordered = mappedIds
                    .Select(id => products.FirstOrDefault(p => p.Id == id))
                    .Where(p => p != null)
                    .Where(p => !p.Deleted && p.Published && p.VisibleIndividually)
                    .ToList();

                if (take > 0)
                    ordered = ordered.Take(take).ToList();

                return ordered;
            }

            // لو مش CustomList رجّع فاضي هنا (وسيتم التعامل مع الأنواع الأخرى من SevenSpikes service)
            return new List<Product>();
        }




        private async Task<IList<int>> GetMappedProductIdsAsync(int groupItemId)
    {
       
        var sql = @"
        SELECT MappedEntityId
        FROM SS_MAP_EntityMapping
        WHERE EntityType = 50
          AND EntityId = @EntityId
          AND MappingType = 2
        ORDER BY DisplayOrder";



            var parameters = new[]
                    {
            new DataParameter("@EntityId", groupItemId)
        };

           
            var ids = await _dataProvider.QueryAsync<int>(sql, parameters);

            return ids.ToList();
        }

        //public async Task<IList<Product>> GetProductsForCustomListAsync(int groupItemId)
        //{
        //    return await GetProductsForCustomListAsync(groupItemId, 0);
        //}

        private async Task<IList<Product>> GetProductsForCustomListAsync(int groupItemId, int take)
        {
            var ids = await GetMappedProductIdsAsync(groupItemId);

            if (!ids.Any())
                return new List<Product>();

            var products = await _productService.GetProductsByIdsAsync(ids.ToArray());

           
            var ordered = ids
                .Select(id => products.FirstOrDefault(p => p.Id == id))
                .Where(p => p != null)
                .Where(p => !p.Deleted && p.Published && p.VisibleIndividually)
                .ToList();

            if (take > 0)
                ordered = ordered.Take(take).ToList();

            return ordered;
        }


        [HttpGet]
        [ProducesResponseType(typeof(IList<HomePageCarouselResponseModel>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> HomeJCarouselAndSmartCollection()
        {

            HomePageCarouselResponseModel result = new HomePageCarouselResponseModel
            {
                Data = new List<HomePageCarouselResponseModel.BannerModel>()
            };

            List<string> wigatezoneallowed = new List<string>();

            wigatezoneallowed = GetallWidgateAvilable();



            //Custome Smart Collection
            if (_smartProductCollectionsSettings.Enabled == true)
            {



                IList<ProductsGroup> prodgroups = await _iproductsGroupService.GetAllCustomGroupsAsync();

                foreach (ProductsGroup Pgroup in prodgroups)
                {
                    IList<ProductsGroupItem> smartProductstabs = await _iproductsGroupItemService.GetAllItemsForGroupAsync(Pgroup.Id);
                    foreach (ProductsGroupItem smartProducttab in smartProductstabs.Where(x=>x.Active==true))
                    {
                        HomePageCarouselResponseModel.BannerModel bannerModel = new HomePageCarouselResponseModel.BannerModel
                        {
                            //Title = StripHTML(await _localizationService.GetLocalizedAsync(Pgroup, x => x.Title) )+ "  ( "+ smartProducttab.Title +" ) " 
Title = 
                    !StripHTML(await _localizationService.GetLocalizedAsync(Pgroup, x => x.Title)).Equals(smartProducttab.Title, StringComparison.OrdinalIgnoreCase)
    ? $"{StripHTML(await _localizationService.GetLocalizedAsync(Pgroup, x => x.Title))} ({smartProducttab.Title})"
    : StripHTML(await _localizationService.GetLocalizedAsync(Pgroup, x => x.Title))



                        };
                        IList<Product> smartProduct;

                        if ((int)smartProducttab.SourceType == 70)
                        {
                           
                            smartProduct = await GetProductsForCustomListAsync(smartProducttab.Id, Pgroup.NumberOfProductsPerItem);
                        }
                        else
                        {
                           
                            smartProduct = await _iproductsGroupItemService.GetProductsBasedOnSourceTypeAsync(
                                smartProducttab.GroupId,
                                smartProducttab.SourceType,
                                Pgroup.NumberOfProductsPerItem,
                                smartProducttab.EntityId,
                                smartProducttab.SortMethod);
                        }

                        // IList<Nop.Core.Domain.Catalog.Product> smartProduct = await _iproductsGroupItemService.GetProductsBasedOnSourceTypeAsync(smartProducttab.GroupId, smartProducttab.SourceType, Pgroup.NumberOfProductsPerItem, smartProducttab.EntityId, smartProducttab.SortMethod);
                        EntityWidgetMappingModel item = new EntityWidgetMappingModel
                        {
                            DisplayOrder = Pgroup.DisplayOrder,
                            EntityId = Pgroup.Id,
                            WidgetZone = Pgroup.WidgetZone

                        };
                        bannerModel.EntityWidgetMappings.Add(item);
                        bannerModel.ShowTitle = true;
                        bannerModel.OrderNum = SeekNo(Pgroup.WidgetZone);
                        bannerModel.DisplayOrder = Pgroup.DisplayOrder;

                        if (smartProducttab.SourceType == SevenSpikes.Nop.Plugins.SmartProductCollections.Domain.Enums.ProductsSourceType.Category)
                        {
                            bannerModel.CategoryID = smartProducttab.EntityId;
                            bannerModel.ShowViewAll = true;

                        }
                        else
                        {
                            bannerModel.CategoryID = 0;
                            bannerModel.ShowViewAll = false;
                        }

                        try
                        {


                            var _productModel2 = await _productModelFactory.PrepareProductOverviewModelsAsync(smartProduct);

                            //  var productmodelDto = _productModel2.Select(p => p.ToDto<ProductOverviewModel>()).ToList();

                            foreach (var pro in _productModel2)
                            {



                                // if (pro.PictureModels == null)
                                pro.PictureModels = new List<PictureModel>();

                                var productPictures = await _productService.GetProductPicturesByProductIdAsync(pro.Id);

                                var addedUrls = new HashSet<string>();

                                foreach (var productPicture in productPictures.OrderBy(pp => pp.DisplayOrder))
                                {
                                    var picture = await _pictureService.GetPictureByIdAsync(productPicture.PictureId);
                                    if (picture == null)
                                        continue;

                                    var pictureResult = await _pictureService.GetPictureUrlAsync(picture, targetSize: 510);
                                    var pictureUrl = pictureResult.Url;

                                    if (!addedUrls.Add(pictureUrl))
                                        continue; // skip duplicates

                                    var _pic = new PictureModel
                                    {
                                        ImageUrl = pictureUrl,
                                        FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(picture)).Url,
                                        Title = picture.TitleAttribute,
                                        AlternateText = picture.AltAttribute
                                    };


                                    if (!pro.PictureModels.Any(p => p.ImageUrl == _pic.ImageUrl && p.FullSizeImageUrl == _pic.FullSizeImageUrl))
                                    {
                                        pro.PictureModels.Add(_pic);
                                    }
                                }


                                productPictures.Clear();
                                addedUrls.Clear();
                                var ProductDetails = GetProductAvailabilityAsync(pro.Id);


                                pro.CustomProperties.Add("StockQuantity", ProductDetails.Result.ToString());


                            }

                            bannerModel.Products = _productModel2.ToList();
                            bannerModel.Categories = new List<Nop.Web.Models.Catalog.CategoryModel>();
                            bannerModel.Manufactures = new List<Nop.Web.Models.Catalog.ManufacturerModel>();


                        }
                        catch (Exception)
                        {

                            //
                        }



                        result.Data.Add(bannerModel);
                    }

                }
            }

            //JCarousel
            if (_jCarouselGeneralSettings.Enable == true)
            {
                IList<Carousel> carousels = await _JCarouselService.GetAllCarouselsAsync();

                foreach (Carousel carousel in carousels)
                {
                    HomePageCarouselResponseModel.BannerModel bannerModel = new HomePageCarouselResponseModel.BannerModel
                    {


                        Title = StripHTML(await _localizationService.GetLocalizedAsync(carousel, x => x.Title))
                    };



                    bannerModel.Name = carousel.Name;
                    if (carousel.JCarouselEntitySettings == null)
                    {
                        bannerModel.ShowTitle = true;
                    }
                    else
                    {
                        bannerModel.ShowTitle = carousel.JCarouselEntitySettings.ShowTitle;
                    }

                    bannerModel.EntityType = EntityType.Carousel;
                    bannerModel.CategoryID = 0;
                    bannerModel.ShowViewAll = false;

                    var jcarousel = await _JCarouselService.GetCarouselByIdAsync(carousel.Id);

                    var type = jcarousel.DataSourceType;
                    int NoOfItems = jcarousel.JCarouselEntitySettings.NumberOfItems;

                    //IList<SS_MAP_EntityWidgetMapping> widgetMappings = _entityWidgetMappingService.GetAllEntityWidgetMappingsByEntityTypeAndEntityIdAsync(int.Parse(EntityType.Carousel.ToString()),
                    //    carousel.Id).Where(x => wigatezoneallowed.Contains(x.WidgetZone)).ToList();

                    var widgetMappingsAll = await _entityWidgetMappingService
        .GetAllEntityWidgetMappingsByEntityTypeAndEntityIdAsync((int)EntityType.Carousel, carousel.Id);

                    var widgetMappings = widgetMappingsAll
                        .Where(x => wigatezoneallowed.Contains(x.WidgetZone))
                        .ToList();

                    foreach (SS_MAP_EntityWidgetMapping widgetMapping in widgetMappings)
                    {


                        EntityWidgetMappingModel item = new EntityWidgetMappingModel
                        {
                            DisplayOrder = widgetMapping.DisplayOrder,
                            EntityId = widgetMapping.EntityId,
                            WidgetZone = widgetMapping.WidgetZone
                        };
                        bannerModel.OrderNum = SeekNo(widgetMapping.WidgetZone);
                        bannerModel.DisplayOrder = widgetMapping.DisplayOrder;

                        bannerModel.EntityWidgetMappings.Add(item);
                    }


                    if (bannerModel.EntityWidgetMappings.Any())
                    {

                        if (type == "ProductsfromCategories")
                        {
                            List<Nop.Core.Domain.Catalog.Category> _Categories = await _ItemsMappingService.GetCategoriesMappedToTheJCarouselAsync(carousel.Id);

                            var productsToReturn = await (await _productService.SearchProductsAsync(overridePublished: true, categoryIds: _Categories.Select(c => c.Id).ToList(),
                                                                            storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                                                                            visibleIndividuallyOnly: true,
                                                                            orderBy: ProductSortingEnum.PriceDesc,
                                                                            pageSize: NoOfItems + 1,
                                                                            languageId: (await _workContext.GetWorkingLanguageAsync()).Id))
                                     .WhereAwait(async c => await _storeMappingService.AuthorizeAsync(c)).ToListAsync();



                            var _productModel2 = await _productModelFactory.PrepareProductOverviewModelsAsync(productsToReturn);

                            //  var productmodelDto = _productModel2.Select(p => p.ToDto<ProductOverviewModel>()).ToList();
                            foreach (var pro in _productModel2)
                            {

                                // if (pro.PictureModels == null)
                                pro.PictureModels = new List<PictureModel>();

                                var productPictures = await _productService.GetProductPicturesByProductIdAsync(pro.Id);

                                var addedUrls = new HashSet<string>();

                                foreach (var productPicture in productPictures.OrderBy(pp => pp.DisplayOrder))
                                {
                                    var picture = await _pictureService.GetPictureByIdAsync(productPicture.PictureId);
                                    if (picture == null)
                                        continue;

                                    var pictureResult = await _pictureService.GetPictureUrlAsync(picture, targetSize: 510);
                                    var pictureUrl = pictureResult.Url;

                                    if (!addedUrls.Add(pictureUrl))
                                        continue; // skip duplicates
                                    var _pic = new PictureModel
                                    {
                                        ImageUrl = pictureUrl,
                                        FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(picture)).Url,
                                        Title = picture.TitleAttribute,
                                        AlternateText = picture.AltAttribute
                                    };


                                    if (!pro.PictureModels.Any(p => p.ImageUrl == _pic.ImageUrl && p.FullSizeImageUrl == _pic.FullSizeImageUrl))
                                    {
                                        pro.PictureModels.Add(_pic);
                                    }

                                }

                                productPictures.Clear();
                                addedUrls.Clear();

                                var ProductDetails = GetProductAvailabilityAsync(pro.Id);

                                pro.CustomProperties.Add("StockQuantity", ProductDetails.Result.ToString());


                            }


                            bannerModel.Products = _productModel2.ToList();

                        }

                        if (type == "None")
                        {



                            IList<Nop.Core.Domain.Catalog.Product> products = (await _ItemsMappingService.GetProductsMappedToTheJCarouselAsync(carousel.Id)).Take(NoOfItems).ToList();



                            var _productModel2 = await _productModelFactory.PrepareProductOverviewModelsAsync(products);

                            //  var productmodelDto = _productModel2.Select(p => p.ToDto<ProductOverviewModel>()).ToList();
                            foreach (var pro in _productModel2)
                            {

                                // if (pro.PictureModels == null)
                                pro.PictureModels = new List<PictureModel>();

                                var productPictures = await _productService.GetProductPicturesByProductIdAsync(pro.Id);

                                var addedUrls = new HashSet<string>();

                                foreach (var productPicture in productPictures.OrderBy(pp => pp.DisplayOrder))
                                {
                                    var picture = await _pictureService.GetPictureByIdAsync(productPicture.PictureId);
                                    if (picture == null)
                                        continue;

                                    var pictureResult = await _pictureService.GetPictureUrlAsync(picture, targetSize: 510);
                                    var pictureUrl = pictureResult.Url;

                                    if (!addedUrls.Add(pictureUrl))
                                        continue; // skip duplicates
                                    var _pic = new PictureModel
                                    {
                                        ImageUrl = pictureUrl,
                                        FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(picture)).Url,
                                        Title = picture.TitleAttribute,
                                        AlternateText = picture.AltAttribute
                                    };


                                    if (!pro.PictureModels.Any(p => p.ImageUrl == _pic.ImageUrl && p.FullSizeImageUrl == _pic.FullSizeImageUrl))
                                    {
                                        pro.PictureModels.Add(_pic);
                                    }

                                }

                                productPictures.Clear();
                                addedUrls.Clear();

                                var ProductDetails = GetProductAvailabilityAsync(pro.Id);

                                pro.CustomProperties.Add("StockQuantity", ProductDetails.Result.ToString());


                            }

                            bannerModel.Products = _productModel2.ToList();
                        }
                        if (type == "RecentlyAddedProducts")
                        {
                            var RecentlyArrivedproducts = _productService.SearchProductsAsync(

    visibleIndividuallyOnly: true,

    orderBy: ProductSortingEnum.CreatedOn,
    pageSize: NoOfItems);


                            var model2 = (await _productModelFactory.PrepareProductOverviewModelsAsync(RecentlyArrivedproducts.Result)).ToList();


                            // var modelDto2 = model2.Select(p => p.ToDto<ProductOverviewModel>()).ToList();
                            foreach (var pro in model2)
                            {
                                // if (pro.PictureModels == null)
                                pro.PictureModels = new List<PictureModel>();

                                var productPictures = await _productService.GetProductPicturesByProductIdAsync(pro.Id);

                                var addedUrls = new HashSet<string>();

                                foreach (var productPicture in productPictures.OrderBy(pp => pp.DisplayOrder))
                                {
                                    var picture = await _pictureService.GetPictureByIdAsync(productPicture.PictureId);
                                    if (picture == null)
                                        continue;

                                    var pictureResult = await _pictureService.GetPictureUrlAsync(picture, targetSize: 510);
                                    var pictureUrl = pictureResult.Url;

                                    if (!addedUrls.Add(pictureUrl))
                                        continue; // skip duplicates

                                    var _pic = new PictureModel
                                    {
                                        ImageUrl = pictureUrl,
                                        FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(picture)).Url,
                                        Title = picture.TitleAttribute,
                                        AlternateText = picture.AltAttribute
                                    };


                                    if (!pro.PictureModels.Any(p => p.ImageUrl == _pic.ImageUrl && p.FullSizeImageUrl == _pic.FullSizeImageUrl))
                                    {
                                        pro.PictureModels.Add(_pic);
                                    }
                                }


                                productPictures.Clear();
                                addedUrls.Clear();

                                var ProductDetails = GetProductAvailabilityAsync(pro.Id);

                                pro.CustomProperties.Add("StockQuantity", ProductDetails.Result.ToString());

                            }
                            bannerModel.Products = model2.ToList();

                        }

                        if (type == "MarkedAsNewProducts")
                        {
                            var nowUtc = DateTime.UtcNow;

                            var productsQuery = _productRepository.Table
                                .Where(p => p.Published && !p.Deleted && p.VisibleIndividually &&
                                            p.MarkAsNew &&
                                            (p.MarkAsNewStartDateTimeUtc == null || p.MarkAsNewStartDateTimeUtc <= nowUtc) &&
                                            (p.MarkAsNewEndDateTimeUtc == null || p.MarkAsNewEndDateTimeUtc >= nowUtc));

                            // Apply store mapping, ACL, etc. if needed
                            var customer = await _workContext.GetCurrentCustomerAsync();
                            productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, 0); // or storeId
                            productsQuery = await _aclService.ApplyAcl(productsQuery, customer);

                            var markedAsNewProducts = await productsQuery
                                .OrderByDescending(p => p.CreatedOnUtc) // or any sorting
                                .Take(NoOfItems)
                                .ToListAsync();

                            var model2 = (await _productModelFactory.PrepareProductOverviewModelsAsync(markedAsNewProducts)).ToList();

                            foreach (var pro in model2)
                            {
                                pro.PictureModels = new List<PictureModel>();
                                var productPictures = await _productService.GetProductPicturesByProductIdAsync(pro.Id);
                                var addedUrls = new HashSet<string>();

                                foreach (var productPicture in productPictures.OrderBy(pp => pp.DisplayOrder))
                                {
                                    var picture = await _pictureService.GetPictureByIdAsync(productPicture.PictureId);
                                    if (picture == null)
                                        continue;

                                    var pictureResult = await _pictureService.GetPictureUrlAsync(picture, targetSize: 510);
                                    var pictureUrl = pictureResult.Url;

                                    if (!addedUrls.Add(pictureUrl))
                                        continue;

                                    var _pic = new PictureModel
                                    {
                                        ImageUrl = pictureUrl,
                                        FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(picture)).Url,
                                        Title = picture.TitleAttribute,
                                        AlternateText = picture.AltAttribute
                                    };

                                    if (!pro.PictureModels.Any(p => p.ImageUrl == _pic.ImageUrl && p.FullSizeImageUrl == _pic.FullSizeImageUrl))
                                    {
                                        pro.PictureModels.Add(_pic);
                                    }
                                }

                                productPictures.Clear();
                                addedUrls.Clear();

                                var productDetails = await GetProductAvailabilityAsync(pro.Id);
                                pro.CustomProperties.Add("StockQuantity", productDetails.ToString());
                            }

                            bannerModel.Products = model2;
                        }

                        if (type.Equals("HomePageFeaturedProducts", StringComparison.OrdinalIgnoreCase)
    || type.Equals("Home Page Featured Products", StringComparison.OrdinalIgnoreCase)
    || type.Contains("Featured", StringComparison.OrdinalIgnoreCase))
                        {
                            
                            var homepageProducts = await (await _productService.GetAllProductsDisplayedOnHomepageAsync())
                              
                                .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
                               
                                .Where(p => _productService.ProductIsAvailable(p))
                               
                                .Where(p => p.VisibleIndividually)
                                .Take(NoOfItems)
                                .ToListAsync();

                            var models = (await _productModelFactory.PrepareProductOverviewModelsAsync(homepageProducts)).ToList();

                           
                            foreach (var pro in models)
                            {
                                pro.PictureModels = new List<PictureModel>();

                                var productPictures = await _productService.GetProductPicturesByProductIdAsync(pro.Id);
                                var addedUrls = new HashSet<string>();

                                foreach (var productPicture in productPictures.OrderBy(pp => pp.DisplayOrder))
                                {
                                    var picture = await _pictureService.GetPictureByIdAsync(productPicture.PictureId);
                                    if (picture == null)
                                        continue;

                                    var pictureResult = await _pictureService.GetPictureUrlAsync(picture, targetSize: 510);
                                    var pictureUrl = pictureResult.Url;

                                    if (!addedUrls.Add(pictureUrl))
                                        continue;

                                    var picModel = new PictureModel
                                    {
                                        ImageUrl = pictureUrl,
                                        FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(picture)).Url,
                                        Title = picture.TitleAttribute,
                                        AlternateText = picture.AltAttribute
                                    };

                                    if (!pro.PictureModels.Any(p => p.ImageUrl == picModel.ImageUrl && p.FullSizeImageUrl == picModel.FullSizeImageUrl))
                                        pro.PictureModels.Add(picModel);
                                }

                                var stock = await GetProductAvailabilityAsync(pro.Id);
                                pro.CustomProperties["StockQuantity"] = stock.ToString();
                            }

                            bannerModel.Products = models;
                        }









                        List<Nop.Core.Domain.Catalog.Manufacturer> Manufacturers = (await _ItemsMappingService.GetManufacturersMappedToTheJCarouselAsync(carousel.Id)).Take(NoOfItems).ToList();
                        bannerModel.Manufactures = new List<Nop.Web.Models.Catalog.ManufacturerModel>();
                        var ManufacturerListmodel = await _catalogModelFactory.PrepareManufacturerAllModelsAsync();
                        //  var ManufacturerListmodelDto = ManufacturerListmodel.Select(c => c.ToDto<ManufacturerModel>()).ToList();
                        foreach (Nop.Core.Domain.Catalog.Manufacturer Manufacturer in Manufacturers)
                        {

                            bannerModel.Manufactures.Add(ManufacturerListmodel.Where(x => x.Id == Manufacturer.Id).FirstOrDefault());
                        }

                        List<Nop.Core.Domain.Catalog.Category> Categories = await _ItemsMappingService.GetCategoriesMappedToTheJCarouselAsync(carousel.Id);
                        bannerModel.Categories = new List<Nop.Web.Models.Catalog.CategoryModel>();



                        foreach (Nop.Core.Domain.Catalog.Category Category in Categories)
                        {
                            var command = new CatalogProductsCommand();

                            var model = await _catalogModelFactory.PrepareCategoryModelAsync(Category, command);

                            bannerModel.Categories.Add(model);
                        }




                        if (!string.IsNullOrEmpty(type) &&
                            (type.Equals("Brand", StringComparison.OrdinalIgnoreCase) ||
                             type.Equals("Manufacturers", StringComparison.OrdinalIgnoreCase)))
                        {


                            var store = await _storeContext.GetCurrentStoreAsync();

                            var allManufacturers = await _manufacturerService.GetAllManufacturersAsync(storeId: store.Id, showHidden: true);



                            var filtered = new List<Manufacturer>();
                            foreach (var m in allManufacturers)
                            {
                                if (m == null || m.Deleted || !m.Published)
                                    continue;



                                filtered.Add(m);
                            }



                            // build models
                            bannerModel.Manufactures = new List<Nop.Web.Models.Catalog.ManufacturerModel>();


                            var allManufacturerModels = await _catalogModelFactory.PrepareManufacturerAllModelsAsync();

                            foreach (var m in filtered.Take(NoOfItems).ToList())
                            {
                                var model = allManufacturerModels.FirstOrDefault(x => x.Id == m.Id);
                                if (model != null)
                                    bannerModel.Manufactures.Add(model);
                            }
                        }


                        bool IsCollectBannerModel = false;

                        if ((bannerModel.Categories != null))
                        {
                            if (bannerModel.Categories.Count >= 0)
                            {
                                IsCollectBannerModel = true;
                            }
                        }
                        if (bannerModel.Manufactures != null)
                        {
                            if (bannerModel.Manufactures.Count > 0)
                            {
                                IsCollectBannerModel = true;
                            }
                        }

                        if ((bannerModel.Products != null))
                        {
                            if (bannerModel.Products.Count > 0)
                            {
                                IsCollectBannerModel = true;
                            }
                        }

                        if (IsCollectBannerModel == true)
                        {
                            result.Data.Add(bannerModel);
                        }
                    }
                }




            }




            IList<SevenSpikes.Nop.Plugins.AnywhereSliders.Domain.Sliders.Slider> sliderList = await _sliderService.GetAllSlidersAsync();


            foreach (SevenSpikes.Nop.Plugins.AnywhereSliders.Domain.Sliders.Slider slid in sliderList)
            {
                List<HomePageBannerResponseModel.BannerSliderModel> pictureList = new List<HomePageBannerResponseModel.BannerSliderModel>();
                HomePageCarouselResponseModel.BannerModel bannerModel = new HomePageCarouselResponseModel.BannerModel
                {
                    Title = slid.SystemName,
                    Name = "Slider No : " + slid.Id.ToString(),
                    ShowTitle = false

                };
                bannerModel.SliderImages = new List<HomePageBannerResponseModel.BannerSliderModel>();




                var widgetMappingsAll = await _entityWidgetMappingService
.GetAllEntityWidgetMappingsByEntityTypeAndEntityIdAsync((int)EntityType.Slider, slid.Id);

                var widgetMappings = widgetMappingsAll
                    .Where(x => wigatezoneallowed.Contains(x.WidgetZone))
                    .ToList();



                foreach (SS_MAP_EntityWidgetMapping widgetMapping in widgetMappings)
                {

                    //if ((widgetMapping.WidgetZone == "home_page_main_slider") || (widgetMapping.WidgetZone == "Slider1"))
                    //{
                    var WorkingLanguage = await _workContext.GetWorkingLanguageAsync();
                    if (slid.LanguageId == WorkingLanguage.Id || slid.LanguageId == 0)
                    {

                        List<SevenSpikes.Nop.Plugins.AnywhereSliders.Domain.Sliders.Slide> sliderDomainList = await _sliderService.GetAllSlidesBySliderIdAsync(slid.Id).Result.ToListAsync();
                        try
                        {
                            foreach (var slide in sliderDomainList.Where(x => x.Visible == true).ToList())
                            {
                                var Mobilepicture = await _pictureService.GetPictureByIdAsync(slide.MobilePictureId);
                                var MobilepictureURL = await _pictureService.GetPictureUrlAsync(Mobilepicture);
                                var picture = await _pictureService.GetPictureByIdAsync(slide.PictureId);
                                var pictureURL = await _pictureService.GetPictureUrlAsync(picture);
                                var bannerSliderModel = new BannerSliderModel
                                {
                                    MobileImageUrl = MobilepictureURL.Url, // or slide.PictureUrl if available
                                    ImageUrl = pictureURL.Url,
                                    Alt = slide.Alt,

                                    DisplayOrder = slide.DisplayOrder
                                };

                                pictureList.Add(bannerSliderModel);

                            }


                            bannerModel.SliderImages.AddRange(pictureList.OrderBy(x => x.DisplayOrder));

                        }
                        catch (Exception)
                        {

                            //throw;
                        }


                        EntityWidgetMappingModel item = new EntityWidgetMappingModel
                        {
                            DisplayOrder = widgetMapping.DisplayOrder,
                            EntityId = widgetMapping.EntityId,
                            WidgetZone = widgetMapping.WidgetZone
                        };
                        bannerModel.OrderNum = SeekNo(widgetMapping.WidgetZone);
                        bannerModel.DisplayOrder = widgetMapping.DisplayOrder;
                        bannerModel.EntityWidgetMappings.Add(item);
                        bannerModel.Categories = new List<Nop.Web.Models.Catalog.CategoryModel>();
                        bannerModel.Manufactures = new List<Nop.Web.Models.Catalog.ManufacturerModel>();

                    }

                    // }

                    if (bannerModel.SliderImages.Count > 0)
                    {
                        result.Data.Add(bannerModel);
                    }
                }



            }

            List<HomePageCarouselResponseModel.BannerModel> Alldata = (from data in result.Data
                                                                       orderby data.OrderNum, data.DisplayOrder
                                                                       select data).ToList();

            result.Data.Clear();
            result.Data = Alldata;
            return Ok(result);
        }


        private async Task<List<T>> ToListAsyncSafe<T>(IAsyncEnumerable<T> source, int? take = null)
        {
            var list = new List<T>();
            if (source == null)
                return list;

            var count = 0;
            await foreach (var item in source)
            {
                list.Add(item);
                count++;
                if (take.HasValue && count >= take.Value)
                    break;
            }

            return list;
        }

        private async Task FillProductsPicturesAndStockAsync(IList<ProductOverviewModel> models)
        {
            if (models == null || models.Count == 0)
                return;

            foreach (var pro in models)
            {
                pro.PictureModels ??= new List<PictureModel>();

                var productPictures = await _productService.GetProductPicturesByProductIdAsync(pro.Id);

                var addedUrls = new HashSet<string>();
                foreach (var productPicture in productPictures.OrderBy(pp => pp.DisplayOrder))
                {
                    var picture = await _pictureService.GetPictureByIdAsync(productPicture.PictureId);
                    if (picture == null)
                        continue;

                    var pictureResult = await _pictureService.GetPictureUrlAsync(picture, targetSize: 510);
                    var pictureUrl = pictureResult.Url;

                    if (!addedUrls.Add(pictureUrl))
                        continue;

                    var full = await _pictureService.GetPictureUrlAsync(picture);

                    pro.PictureModels.Add(new PictureModel
                    {
                        ImageUrl = pictureUrl,
                        FullSizeImageUrl = full.Url,
                        Title = picture.TitleAttribute,
                        AlternateText = picture.AltAttribute
                    });
                }

                var stock = await GetProductAvailabilityAsync(pro.Id);
                pro.CustomProperties["StockQuantity"] = stock.ToString();
            }
        }


        private List<string> GetallWidgateAvilable()
        {
            List<string> wigatezones = new List<string>
            {
                "home_page_main_slider",
                "homepage_slider_after",
                "home_page_top",

                "Slider1",

                "SmallAd",

                "Category1",

                "Slider2",

                "Category2",

                "Category3",

                "Slider3",

                "Category4",

                "Category5",

                "Category6",

                "Slider4",

                "Category7",

                "Category8",

                "DaySales",

                "HomePageBeforeCategories",

                "Category9",

                "HomePageBeforeNews",

                "HomePageBeforePoll",
                "home_page_before_poll",

                "HomePageBottom",

                "emporium_home_page_sale_of_the_day",

                "HomePageBeforeProducts",

                "HomePageBeforeBestSellers",

                "HomepageProducts",

                "HomepageBestSellers",

                "home_page_before_best_sellers",

                "home_page_bottom","home_page_before_news","footer_top","home_page_before_products","productdetails_related_carosuel"
                ,"productdetails_purchased_carosuel"
                ,"homepage_content_after"
};

            return wigatezones;

        }

        private int SeekNo(string WidgetZone)
        {



            switch (WidgetZone)
            {

                case "home_page_main_slider":
                    return 10;
                case "homepage_slider_after":
                return 15;
                case "home_page_top":
                    return 25;
                case "Slider1":
                    return 30;
                case "SmallAd":
                    return 40;
                case "home_page_before_news":
                    return 50;
                case "productdetails_related_carosuel":
                    return 60;
                case "home_page_before_poll":
                    return 70;
                case "Category3":
                    return 80;
                case "Slider3":
                    return 90;
                case "Category4":
                    return 100;
                case "Category5":
                    return 110;
                case "Category6":
                    return 120;
                case "Slider4":
                    return 130;
                case "Category7":
                    return 140;
                case "Category8":
                    return 150;
                case "DaySales":
                    return 160;
                case "HomePageBeforeCategories":
                    return 170;
                case "Category9":
                    return 180;
                case "HomePageBeforeNews":
                    return 190;
                case "HomePageBeforePoll":
                    return 200;
                case "HomePageBottom":
                    return 210;
                case "emporium_home_page_sale_of_the_day":
                    return 220;
                case "home_page_before_products":
                    return 230;
                case "HomePageBeforeBestSellers":
                    return 240;
                case "HomepageProducts":
                    return 250;
                case "HomepageBestSellers":
                    return 260;
                case "home_page_before_best_sellers":
                    return 270;
                case "home_page_bottom":
                    return 280;
                case "homepage_content_after":
                    return 285;
                default:
                    return 999;
            }






        }




        [HttpGet]
        [ProducesResponseType(typeof(IList<SliderModel>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetSilderHomepage()
        {
            //For home sliders  @EntityType = 15
            MsSqlNopDataProvider msSqlNopDataProvider = new MsSqlNopDataProvider();
            DataParameter parameter = new DataParameter(name: "@EntityType", value: 15);
            var allSliderHomepage = await msSqlNopDataProvider.QueryProcAsync<SliderModel>("GetSilderHomepage", parameter);

            // Group by SliderId
            var groupedSliders = allSliderHomepage
                .GroupBy(x => x.SliderId)
                .ToList();

            var result = new List<SliderGroupModel>();

            foreach (var group in groupedSliders)
            {
                var slides = new List<SliderModel>();

                foreach (var item in group)
                {
                    item.MobilePictureId = item.MobilePictureId == 0 ? item.PictureId : item.MobilePictureId;
                    var picture = await _pictureService.GetPictureByIdAsync(item.MobilePictureId);
                    var pictureURL = await _pictureService.GetPictureUrlAsync(picture);

                    item.MobilePictureUrl = pictureURL.Url ?? string.Empty;

                    slides.Add(item);
                }

                result.Add(new SliderGroupModel
                {
                    SliderId = group.Key,
                    Slides = slides
                });
            }

            return Ok(result.OrderBy(x => x.SliderId).ToList());

        }










        private string StripHTML(string source)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(source))
                    return string.Empty;


                string result = source.Replace("\r", " ")
                                      .Replace("\n", " ")
                                      .Replace("\t", " ");


                result = System.Text.RegularExpressions.Regex.Replace(result, @"<script[^>]*>.*?</script>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"<style[^>]*>.*?</style>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);


                result = Regex.Replace(result, @"<(br|p|div|tr|li)[^>]*>", "\n", RegexOptions.IgnoreCase);


                result = Regex.Replace(result, @"<[^>]+>", " ");


                result = System.Net.WebUtility.HtmlDecode(result);


                result = Regex.Replace(result, @"\s+", " ").Trim();

                return result;
            }
            catch
            {
                return source;
            }
        }














        [HttpGet]

        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Country), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAllCountrie()

        {
            var lang = await _workContext.GetWorkingLanguageAsync();
            var countries = await _countryService.GetAllCountriesAsync(languageId: lang.Id, showHidden: true);

            return Ok(countries);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IList<ProductOverviewModel>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> HomePageProducts()
        {
            var products = await (await _productService.GetAllProductsDisplayedOnHomepageAsync())
            //ACL and store mapping
            .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
            //availability dates
            .Where(p => _productService.ProductIsAvailable(p))
            //visible individually
            .Where(p => p.VisibleIndividually).ToListAsync();

            var model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();
            // var modelDto = model.Select(p => p.ToModel<ProductApiOverviewModel>()).ToList();

            return Ok(model);
        }






        private async Task<bool> CheckCategoryAvailabilityAsync(Nop.Core.Domain.Catalog.Category category)
        {
            if (category is null)
                return false;

            var isAvailable = !category.Deleted;

            var notAvailable =
                //published?
                !category.Published ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(category) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(category);
            //Check whether the current user has a "Manage categories" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync("AccessAdminPanel") && await _permissionService.AuthorizeAsync("ManageCategories");
            if (notAvailable && !hasAdminAccess)
                isAvailable = false;

            return isAvailable;
        }


        [HttpGet("{categoryId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Nop.Web.Models.Catalog.CategoryModel), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCategory(int categoryId, [FromQuery] CatalogRequest request)
        {
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);

            if (!await CheckCategoryAvailabilityAsync(category))
                return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(category)));

            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(false),
                (await _storeContext.GetCurrentStoreAsync()).Id);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ViewCategory",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewCategory"), category.Name), category);

            //model
            var command = new CatalogProductsCommand();
            if (request != null)
                command = new CatalogProductsCommand
                {
                    Price = !string.IsNullOrEmpty(request.Price) ? request.Price : string.Empty,
                    //SpecificationOptionIds = request.SpecificationOptionIds != null ? request.SpecificationOptionIds : new List<int>(),
                  //  ManufacturerIds = request.ManufacturerIds != null ? request.ManufacturerIds : new List<int>(),
                    OrderBy = request.OrderBy != null ? (int)request.OrderBy : (int)ProductSortingEnum.Position,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                };
            var model = await _catalogModelFactory.PrepareCategoryModelAsync(category, command);





            // var _CategoryApiModel = _mapper.Map<CategoryApiModel>(model);
            foreach (var item in model.CatalogProductsModel.Products)
            {
                var ProductDetails = GetProductAvailabilityAsync(item.Id);
                item.CustomProperties.Add("StockQuantity", ProductDetails.Result.ToString());


            }


            return Ok(model);

        }
        [HttpGet("GetProductAvailability")]
        public async Task<int> GetProductAvailabilityAsync(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return 0;

            if (product.ManageInventoryMethod != ManageInventoryMethod.ManageStock)
                return 0;

            if (!product.UseMultipleWarehouses)
                return product.StockQuantity > 0 ? 1 : 0;

            var warehouseInventory = await _productService.GetAllProductWarehouseInventoryRecordsAsync(product.Id);

            int totalStock = warehouseInventory.Sum(w => w.StockQuantity);
            int reserved = warehouseInventory.Sum(w => w.ReservedQuantity);
            int availableStock = totalStock - reserved;

            return availableStock > 0 ? 1 : 0;
        }



        private async Task PrepareCategoryImageAsync(Nop.Web.Models.Catalog.CategoryModel model)
        {
            var secured = _webHelper.IsCurrentConnectionSecured();
            var pictureSize = _mediaSettings.CategoryThumbPictureSize;
            var language = await _workContext.GetWorkingLanguageAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            var category = await _categoryService.GetCategoryByIdAsync(model.Id);
            var categoryPictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(Nop.Web.Infrastructure.Cache.NopModelCacheDefaults.CategoryPictureModelKey,
                    category.Id, pictureSize, true, language, secured, store);

            var pictureModel = await _staticCacheManager.GetAsync(categoryPictureCacheKey, async () =>
            {
                var picture = await _pictureService.GetPictureByIdAsync(category.PictureId);

                (var fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                var (imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);

                var pictureModel = new PictureModel
                {
                    FullSizeImageUrl = fullSizeImageUrl,
                    ImageUrl = imageUrl,
                    Title = string.Format(await _localizationService
                        .GetResourceAsync("Media.Category.ImageLinkTitleFormat"), category.Name),
                    AlternateText = string.Format(await _localizationService
                        .GetResourceAsync("Media.Category.ImageAlternateTextFormat"), category.Name)
                };

                return pictureModel;
            });

            model.PictureModel = pictureModel;
        }





        public static string ToHex(string input)
        {
            var enc = Encoding.GetEncoding(0);

            byte[] buffer = enc.GetBytes(input);
            var sha1 = SHA1.Create();
            var hash = BitConverter.ToString(sha1.ComputeHash(buffer)).Replace("-", "");
            return hash;
        }








        //[HttpGet]
        //[ProducesResponseType(typeof(CheckUsernameAvailabilityResponse), StatusCodes.Status200OK)]
        //public virtual async Task<IActionResult> CheckUsernameAvailability([FromQuery][Required] string username)
        //{
        //    var usernameAvailable = false;
        //    var statusText = await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.NotAvailable");

        //    if (!UsernamePropertyValidator<string, string>.IsValid(username, _customerSettings))
        //        statusText = await _localizationService.GetResourceAsync("Account.Fields.Username.NotValid");
        //    else if (_customerSettings.UsernamesEnabled && !string.IsNullOrWhiteSpace(username))
        //    {
        //        var customer = await _customerService.GetCustomerByUsernameAsync(username);

        //        if (customer != null &&
        //            customer.Username != null &&
        //            customer.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase))
        //            statusText = await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.CurrentUsername");
        //        else
        //        {
        //            if (customer == null)
        //            {
        //                statusText = await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.Available");
        //                usernameAvailable = true;
        //            }
        //        }
        //    }

        //    return Ok(new CheckUsernameAvailabilityResponse { Available = usernameAvailable, Text = statusText });
        //}



        //[HttpPost("{username}")]
        //[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        //public virtual async Task<IActionResult> CheckUsernameAvailability(string username)
        //{
        //    if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
        //        return BadRequest(await _localizationService.GetResourceAsync("Account.Register.Result.Disabled"));

        //    var usernameAvailable = false;
        //    var statusText = await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.NotAvailable");

        //    if (!UsernamePropertyValidator<string, string>.IsValid(username, _customerSettings))
        //        statusText = await _localizationService.GetResourceAsync("Account.Fields.Username.NotValid");
        //    else if (_customerSettings.UsernamesEnabled && !string.IsNullOrWhiteSpace(username))
        //        if (await _workContext.GetCurrentCustomerAsync() != null &&
        //            (await _workContext.GetCurrentCustomerAsync()).Username != null &&
        //            (await _workContext.GetCurrentCustomerAsync()).Username.Equals(username, StringComparison.InvariantCultureIgnoreCase))
        //            statusText = await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.CurrentUsername");
        //        else
        //        {
        //            var customer = await _customerService.GetCustomerByUsernameAsync(username);
        //            if (customer == null)
        //            {
        //                statusText = await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.Available");
        //                usernameAvailable = true;
        //            }
        //        }

        //    if (usernameAvailable)
        //        return Ok(statusText);

        //    return BadRequest(statusText);
        //}


        [HttpPost("{value}")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CheckUserAvailability(string value)
        {
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return BadRequest(await _localizationService.GetResourceAsync("Account.Register.Result.Disabled"));

            var isAvailable = false;
            var statusText = string.Empty;

            // 1️⃣ Detect if input is an email or username
            bool isEmail = CommonHelper.IsValidEmail(value);

            if (isEmail)
            {
                // === Email Availability Check ===
                statusText = await _localizationService.GetResourceAsync("Account.CheckEmailAvailability.NotAvailable");

                if (!isEmail)
                {
                    statusText = await _localizationService.GetResourceAsync("Account.Fields.Email.NotValid");
                }
                else
                {
                    var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                    if (currentCustomer != null &&
                        !string.IsNullOrEmpty(currentCustomer.Email) &&
                        currentCustomer.Email.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                    {
                        statusText = await _localizationService.GetResourceAsync("Account.CheckEmailAvailability.CurrentEmail");
                    }
                    else
                    {
                        var customer = await _customerService.GetCustomerByEmailAsync(value);
                        if (customer == null)
                        {
                            statusText = await _localizationService.GetResourceAsync("Account.CheckEmailAvailability.Available");
                            isAvailable = true;
                        }
                    }
                }
            }
            else
            {
                // === Username Availability Check ===
                statusText = await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.NotAvailable");

                if (!UsernamePropertyValidator<string, string>.IsValid(value, _customerSettings))
                {
                    statusText = await _localizationService.GetResourceAsync("Account.Fields.Username.NotValid");
                }
                else if (_customerSettings.UsernamesEnabled && !string.IsNullOrWhiteSpace(value))
                {
                    var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                    if (currentCustomer != null &&
                        !string.IsNullOrEmpty(currentCustomer.Username) &&
                        currentCustomer.Username.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                    {
                        statusText = await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.CurrentUsername");
                    }
                    else
                    {
                        var customer = await _customerService.GetCustomerByUsernameAsync(value);
                        if (customer == null)
                        {
                            statusText = await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.Available");
                            isAvailable = true;
                        }
                    }
                }
            }

            if (isAvailable)
                return Ok(statusText);

            return BadRequest(statusText);
        }




        [HttpPost]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DeleteAccountModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountModelDto model)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return BadRequest(new List<string> { "Customer is not registered." });
            var errors = "";
            //  var previousPasswords = await _customerService.GetCustomerPasswordsAsync(customer.Id, passwordsToReturn: _customerSettings.UnduplicatedPasswordsNumber);

            var loginResult = await _customerRegistrationService.ValidateCustomerAsync(customer.Username, model.CurrentPassword);
            //errors
            errors = "";
            if (loginResult == CustomerLoginResults.Successful)
            {
                MsSqlNopDataProvider msSqlNopDataProvider2 = new MsSqlNopDataProvider();



                DataParameter parameter1 = new DataParameter("@customerId", customer.Id);





                await msSqlNopDataProvider2.ExecuteNonQueryAsync("EXEC [dbo].[DeleteAccount]@customerId", parameter1);






            }
            else
            {
                return BadRequest(errors);
            }


            //If we got this far, something failed, redisplay form
            return Ok(model);
        }

        [HttpGet("{CustomerId}")]


        public virtual async Task<string> GetCustomerData(int CustomerId)
        {

            var Custmer = await _customerService.GetCustomerByIdAsync(CustomerId);
            var AllCustomers = new List<Customer>();
            AllCustomers.Add(Custmer);
            var CustmerAddresses = await _customerService.GetAddressesByCustomerIdAsync(CustomerId);
            var AllAddresses = new List<Address>();

            foreach (var Cu_Ad in CustmerAddresses)
            {
                var _Address = new Address();

                _Address.FirstName = Cu_Ad.FirstName;
                _Address.LastName = Cu_Ad.LastName;
                _Address.Email = Cu_Ad.Email;
                _Address.CountryId = Cu_Ad.CountryId;
                _Address.StateProvinceId = Cu_Ad.StateProvinceId;
                _Address.Address1 = Cu_Ad.Address1;
                _Address.ZipPostalCode = Cu_Ad.ZipPostalCode;
                _Address.PhoneNumber = Cu_Ad.PhoneNumber;
                _Address.CustomAttributes = Cu_Ad.CustomAttributes;
                AllAddresses.Add(_Address);

            }
            string fileName = "CustomerData" + Custmer.CustomerGuid + ".xlsx";

            try
            {
                DataTable table1 = new DataTable();

                using (var reader1 = ObjectReader.Create(AllCustomers))
                {
                    table1.Load(reader1);
                }

                table1.Columns.Remove("Id");
                table1.Columns.Remove("CreatedOnUtc");
                table1.Columns["Email"].SetOrdinal(0);
                table1.Columns["FirstName"].SetOrdinal(1);
                table1.Columns["LastName"].SetOrdinal(2);
                table1.Columns["Gender"].SetOrdinal(3);
                table1.Columns["CountryId"].SetOrdinal(4);
                table1.Columns["StateProvinceId"].SetOrdinal(5);
                table1.Columns["Phone"].SetOrdinal(6);
                //table1.Columns["CustomAttributes"].SetOrdinal(7);


                DataTable table = new DataTable();

                using (var reader = ObjectReader.Create(AllAddresses))
                {
                    table.Load(reader);
                }


                table.Columns.Remove("Id");
                table.Columns.Remove("CreatedOnUtc");

                //table.Columns["FirstName"].Caption = (await _localizationService.GetResourceAsync("Admin.AllAddresses.Fields.FirstName"));
                //table.Columns["LastName"].Caption = (await _localizationService.GetResourceAsync("AllAddresses.Fields.LastName"));

                //table.Columns["Email"].Caption = (await _localizationService.GetResourceAsync("AllAddresses.Fields.Email"));

                table.Columns["FirstName"].SetOrdinal(0);
                table.Columns["LastName"].SetOrdinal(1);
                table.Columns["Email"].SetOrdinal(2);
                table.Columns["PhoneNumber"].SetOrdinal(3);
                table.Columns["Address1"].SetOrdinal(4);
                table.Columns["ZipPostalCode"].SetOrdinal(5);
                table.Columns["Address2"].SetOrdinal(6);

                table.Columns["CustomAttributes"].SetOrdinal(7);


                using (XLWorkbook wb = new XLWorkbook())
                {

                    var worksheet = wb.Worksheets.Add("Customer Info");

                    var peopleRange = worksheet.Cell("A1").InsertTable(table1, "AllCustomers", true);

                    // Add second DataTable (products) below the first
                    // Find the next empty row after the first table
                    int startRow = peopleRange.RangeAddress.LastAddress.RowNumber + 2;
                    var startCell = worksheet.Cell(startRow, 1);
                    startCell.InsertTable(table, "AllAddresses", true);
                    // worksheet.Row(1).InsertRowsAbove(1); // Make room for the header at row 1
                    worksheet.Cell("A3").Value = "Addresses List";
                    worksheet.Cell("A3").Style.Font.Bold = true;
                    // wb.Worksheets.Add(table, "Customer Info").Columns().AdjustToContents();

                    wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    wb.Style.Font.Bold = true;

                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);

                        // var content = stream.ToArray();
                        //return File(content, MimeTypes.TextXlsx, "AllVisitors.xlsx");
                        string path = _hostEnvironment.WebRootPath + "\\downloads";



                        string filepath = Path.Combine(path + "\\" + fileName);
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        if (!System.IO.File.Exists(filepath))
                        {

                            wb.SaveAs(filepath);
                            //return File(System.IO.File.OpenRead(filepath), "application/octet-stream", Path.GetFileName(filepath));

                            return Path.Combine("downloads\\" + fileName);

                        }
                        else
                        {
                            System.IO.File.Delete(filepath);
                            wb.SaveAs(filepath);
                            return Path.Combine("downloads\\" + fileName);



                        }






                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message);

            }


            return "";
        }




        [HttpGet]
        [ProducesResponseType(typeof(IList<CategoryMenuDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetMMenuRoot()
        {
            var lang = await _workContext.GetWorkingLanguageAsync();
            MsSqlNopDataProvider msSqlNopDataProvider = new MsSqlNopDataProvider();
            DataParameter parameter = new DataParameter(name: "LanguageIdID", lang.Id);
            var MenuIDS = await msSqlNopDataProvider.QueryProcAsync<MenuRootModel>("GetMMenuRoot", parameter);

            List<CategoryMenuDto> _CategoryMenuDtoList = new List<CategoryMenuDto>();
            var type = 0;
            foreach (var menuID in MenuIDS.Where(x => !x.CustomLinksRef.Contains("SMSOrders")).ToList())
            {
                CategoryMenuDto categoryMenuDto = new CategoryMenuDto();
                type = menuID.EntityId < 1 ? 1 : 0;

                categoryMenuDto.type = type;
                categoryMenuDto.id = menuID.EntityId;
                categoryMenuDto.MenuId = menuID.id;
                categoryMenuDto.title = menuID.LocalizedTitle ?? menuID.title;
                categoryMenuDto.image = "MobileIcons/MobileIcon_" + type + "_" + menuID.id + ".png";
                categoryMenuDto.CustomLinksPath = menuID.id == 56 ? "/home/manufacturers" : "";
                categoryMenuDto.CustomLinksRef = menuID.CustomLinksRef;
                _CategoryMenuDtoList.Add(categoryMenuDto);

            }







            return Ok(_CategoryMenuDtoList);
        }



        protected async Task<(Customer customer, Store store, IList<ShoppingCartItem> cart, IActionResult actionResult)> ValidateRequestAsync()
        {
            if (_orderSettings.CheckoutDisabled)
                return (null, null, null, NotFound($"The setting {nameof(_orderSettings.CheckoutDisabled)} is true."));

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            if (!cart.Any())
                return (null, null, null, BadRequest("Your cart is empty"));

            if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                return (null, null, null, BadRequest("Anonymous checkout is not allowed"));

            return (customer, store, cart, null);
        }


        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CheckoutConfirmModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetConfirmOrder()
        {
            //validation
            var (_, _, cart, actionResult) = await ValidateRequestAsync();

            if (actionResult != null)
                return actionResult;

            //model
            var model = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);

            // var dto = model.ToDto<CheckoutConfirmModelDto>();
            var dto = _mapper.Map<CheckoutConfirmModelDto>(model);
            var shoppingCartModel = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(
                new ShoppingCartModel(),
                cart,
                isEditable: false,
                prepareAndDisplayOrderReviewData: true);
            if (!cart.Any())
                return NotFound(string.Format(MessageDefaults.NOT_FOUND, "cartItems"));

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
                return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });
            // dto.ShoppingCart = shoppingCartModel.ToDto<ShoppingCartModelDto>();
            dto.ShoppingCart = _mapper.Map<ShoppingCartModelDto>(shoppingCartModel);
            var orderTotals = await _shoppingCartModelFactory.PrepareOrderTotalsModelAsync(cart, false);
            //   dto.OrderTotals = orderTotals.ToDto<OrderTotalsModelDto>();
            dto.OrderTotals = _mapper.Map<OrderTotalsModelDto>(orderTotals);

            return Ok(dto);
        }



        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Shipment), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAllShipments(string trackingNumber = null, bool loadNotShipped = false, bool loadNotDelivered = false, string Email = null)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerOrders = await _orderService.SearchOrdersAsync(
                customerId: customer.Id,
                billingEmail: Email,
                pageIndex: 0,
                pageSize: int.MaxValue // get all
            );

            var allShipmentsWithStatus = new List<ShipmentWithStatusDto>();

            foreach (var order in customerOrders)
            {
                var shipments = await _shipmentService.GetAllShipmentsAsync(
                    trackingNumber: trackingNumber,
                    loadNotShipped: loadNotShipped,
                    loadNotDelivered: loadNotDelivered,
                    orderId: order.Id
                );

                foreach (var shipment in shipments)
                {
                    allShipmentsWithStatus.Add(new ShipmentWithStatusDto
                    {
                        Shipment = shipment,
                        ShippingStatus = await _localizationService.GetLocalizedEnumAsync(order.ShippingStatus)
                    });
                }
            }


            return Ok(allShipmentsWithStatus);
        }


        [HttpGet]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CheckoutShippingMethodModel), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetShippingMethods()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (!cart.Any())
                return NotFound(string.Format(MessageDefaults.NOT_FOUND, "cartItems"));

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
                return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

            if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
            {
                await _genericAttributeService.SaveAttributeAsync<ShippingOption>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedShippingOptionAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);
                return BadRequest("Shipping is not required");
            }

            //check if pickup point is selected on the shipping address step
            if (!_orderSettings.DisplayPickupInStoreOnShippingMethodPage)
            {
                var selectedPickUpPoint = await _genericAttributeService
                    .GetAttributeAsync<PickupPoint>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedPickupPointAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
                if (selectedPickUpPoint != null)
                    return Ok();
            }

            var model = await _checkoutModelFactory.PrepareShippingMethodModelAsync(cart, await _customerService.GetCustomerShippingAddressAsync(await _workContext.GetCurrentCustomerAsync()));

            var firstShippingMethod = model.ShippingMethods.FirstOrDefault();
            model.ShippingMethods = firstShippingMethod != null
                ? new List<CheckoutShippingMethodModel.ShippingMethodModel> { firstShippingMethod }
                : new List<CheckoutShippingMethodModel.ShippingMethodModel>();




            if (firstShippingMethod != null && !string.IsNullOrEmpty(firstShippingMethod.Description))
            {
                firstShippingMethod.Description = await ConvertHtmlToJsonAsync(firstShippingMethod.Description);
            }
            return Ok(model);
        }







        [HttpGet("ConvertHtmlToJsonAsync")]

        public async Task<string> ConvertHtmlToJsonAsync(string html)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var shipmentBoxes = doc.DocumentNode.SelectNodes("//div[contains(@class,'shipment-box')]");
            var result = new List<ShipmentInfo>();

            if (shipmentBoxes != null)
            {
                foreach (var box in shipmentBoxes)
                {
                    var shipment = new ShipmentInfo();

                    // Extract shipment header
                    var header = box.SelectSingleNode(".//div[contains(@class,'shipment-header')]");
                    if (header != null)
                    {
                        var headerDivs = header.SelectNodes("./div");
                        if (headerDivs?.Count >= 2)
                        {
                            shipment.Shipment = System.Net.WebUtility.HtmlDecode(headerDivs[0].InnerText.Trim());
                            shipment.Status = System.Net.WebUtility.HtmlDecode(headerDivs[1].InnerText.Trim());
                        }
                    }

                    // Extract products
                    var productNodes = box.SelectNodes(".//div[contains(@class,'product')]");
                    var tempProductList = new List<ProductInfo>();

                    if (productNodes != null)
                    {
                        foreach (var productNode in productNodes)
                        {
                            var product = new ProductInfo();

                            // Image
                            var imgNode = productNode.SelectSingleNode(".//img");
                            product.Image = imgNode?.GetAttributeValue("src", "")?.Trim() ?? "";

                            // Name
                            var nameNode = productNode.SelectSingleNode(".//div[contains(@class,'product-name')]");
                            product.Name = System.Net.WebUtility.HtmlDecode(nameNode?.InnerText.Trim() ?? "");

                            // Quantity
                            var qtyNode = productNode.SelectSingleNode(".//div[contains(@class,'product-qty')]");
                            string quantityText = "0";

                            if (qtyNode != null)
                            {
                                var textNodes = qtyNode.ChildNodes
                                    .Where(n => n.NodeType == HtmlAgilityPack.HtmlNodeType.Text)
                                    .Select(n => n.InnerText.Trim())
                                    .Where(t => !string.IsNullOrEmpty(t))
                                    .ToList();

                                quantityText = textNodes.FirstOrDefault() ?? "1";
                            }

                            product.Quantity = quantityText;

                            if (!string.IsNullOrEmpty(product.Name))
                            {
                                tempProductList.Add(product);
                            }
                        }
                    }

                  
                    var groupedProducts = tempProductList
                        .GroupBy(p => new { p.Name, p.Image })
                        .Select(g =>
                        {
                            int totalQty = g
                                .Select(p => int.TryParse(p.Quantity, out var q) ? q : 0)
                                .Sum();

                            return new ProductInfo
                            {
                                Name = g.Key.Name,
                                Image = g.Key.Image,
                                Quantity = totalQty.ToString()
                            };
                        });

                    shipment.Products.AddRange(groupedProducts);
                    result.Add(shipment);
                }
            }

            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }





        protected virtual async Task<bool> CheckManufacturerAvailabilityAsync(Manufacturer manufacturer)
        {
            var isAvailable = true;

            if (manufacturer == null || manufacturer.Deleted)
                isAvailable = false;

            var notAvailable =
                //published?
                !manufacturer.Published ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(manufacturer) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(manufacturer);
            //Check whether the current user has a "Manage categories" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess =  await _permissionService.AuthorizeAsync("AccessAdminPanel") && await _permissionService.AuthorizeAsync("ManageManufacturers");
            if (notAvailable && !hasAdminAccess)
                isAvailable = false;

            return isAvailable;
        }


        [HttpGet("{manufacturerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Nop.Web.Models.Catalog.ManufacturerModel), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetManufacturer(int manufacturerId, [FromQuery] CatalogRequest request)
        {
            var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(manufacturerId);

            if (!await CheckManufacturerAvailabilityAsync(manufacturer))
                return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(manufacturer)));

            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(false),
                (await _storeContext.GetCurrentStoreAsync()).Id);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ViewManufacturer",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewManufacturer"), manufacturer.Name), manufacturer);

            //model
            var command = new CatalogProductsCommand();
            if (request != null)
                command = new  CatalogProductsCommand
                {
                    Price = !string.IsNullOrEmpty(request.Price) ? request.Price : string.Empty,
                    //SpecificationOptionIds = request.SpecificationOptionIds != null ? request.SpecificationOptionIds : new List<int>(),
                   // ManufacturerIds = request.ManufacturerIds != null ? request.ManufacturerIds : new List<int>(),
                    OrderBy = request.OrderBy != null ? (int)request.OrderBy : (int)ProductSortingEnum.Position,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                };
            var model = await _catalogModelFactory.PrepareManufacturerModelAsync(manufacturer, command);


            foreach (var pro in model.CatalogProductsModel.Products)
            {



                // if (pro.PictureModels == null)
                pro.PictureModels = new List<PictureModel>();

                var productPictures = await _productService.GetProductPicturesByProductIdAsync(pro.Id);

                var addedUrls = new HashSet<string>();

                foreach (var productPicture in productPictures.OrderBy(pp => pp.DisplayOrder))
                {
                    var picture = await _pictureService.GetPictureByIdAsync(productPicture.PictureId);
                    if (picture == null)
                        continue;

                    var pictureResult = await _pictureService.GetPictureUrlAsync(picture, targetSize: 510);
                    var pictureUrl = pictureResult.Url;

                    if (!addedUrls.Add(pictureUrl))
                        continue; // skip duplicates

                    var _pic = new PictureModel
                    {
                        ImageUrl = pictureUrl,
                        FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(picture)).Url,
                        Title = picture.TitleAttribute,
                        AlternateText = picture.AltAttribute
                    };


                    if (!pro.PictureModels.Any(p => p.ImageUrl == _pic.ImageUrl && p.FullSizeImageUrl == _pic.FullSizeImageUrl))
                    {
                        pro.PictureModels.Add(_pic);
                    }
                }


                productPictures.Clear();
                addedUrls.Clear();
                var ProductDetails = GetProductAvailabilityAsync(pro.Id);


                pro.CustomProperties.Add("StockQuantity", ProductDetails.Result.ToString());


            }

            return Ok(model);
        }


        /// <param name="orderId">The order identifier</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(OrderDetailsModelWithShipments), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetOrderDetails(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted || (await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
                return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(order)));
            var _OrderDetailsModelWithShipments = new OrderDetailsModelWithShipments();
            var model = await _orderModelFactory.PrepareOrderDetailsModelAsync(order);

            model.CustomProperties.Add("PaymentMethodSystemName", order.PaymentMethodSystemName);

            _OrderDetailsModelWithShipments._OrderDetailsMode = model;
            var modelShipmentsList = new List<OrderShipmentInfo>();

            var shipments = model.Shipments;

            foreach (var shipment in shipments)
            {
                var modelShipment = new OrderShipmentInfo();
                var _shipment = await _shipmentService.GetShipmentItemsByShipmentIdAsync(shipment.Id);
                List<Product> _shipmentProducts = new List<Product>();
                Dictionary<int, string> StockQuantityDictionary = new Dictionary<int, string>();
                foreach (var item in _shipment)
                {
                    var orderItem = await _orderService.GetOrderItemByIdAsync(item.OrderItemId);
                    var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                    StockQuantityDictionary.Add(orderItem.ProductId, orderItem.Quantity.ToString());
                    _shipmentProducts.Add(product);
                }
                var ShipmentProducts = await _productModelFactory.PrepareProductOverviewModelsAsync(_shipmentProducts);

                foreach (var pro in ShipmentProducts)
                {
                    var stock = StockQuantityDictionary.Where(x => x.Key == pro.Id);
                    pro.CustomProperties.Add("StockQuantity", stock.FirstOrDefault().Value);
                }
                modelShipment.Id = shipment.Id;
                modelShipment.Products = ShipmentProducts.ToList();



                modelShipment.TrackingNumber = shipment.TrackingNumber;
                modelShipment.ShippedDate = shipment.ShippedDate;
                modelShipment.ReadyForPickupDate = shipment.ReadyForPickupDate;
                modelShipment.DeliveryDate = shipment.DeliveryDate;

                modelShipmentsList.Add(modelShipment);

            }

            _OrderDetailsModelWithShipments._ShipmentInfo = modelShipmentsList;

            return Ok(_OrderDetailsModelWithShipments);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductOverviewModel>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetRelatedProducts(int productId, int? productThumbPictureSize)
        {
            //load and cache report
            var productIds = (await _productService.GetRelatedProductsByProductId1Async(productId)).Select(x => x.ProductId2).ToArray();

            //load products
            var products = await (await _productService.GetProductsByIdsAsync(productIds))
            //ACL and store mapping
            .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
            //availability dates
            .Where(p => _productService.ProductIsAvailable(p))
            //visible individually
            .Where(p => p.VisibleIndividually).ToListAsync();

            var model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, productThumbPictureSize)).ToList();


            foreach (var pro in model)
            {
                var ProductDetails = GetProductAvailabilityAsync(pro.Id);
                pro.CustomProperties.Add("StockQuantity", ProductDetails.Result.ToString());
            }



            return Ok(model);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductOverviewModel>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetSimilarProductProducts(int productId, int? productThumbPictureSize)
        {
            try
            {
                // Get similar products - order in memory after database query
                var similarProducts = (await _similarProductRecordRepository.GetAllAsync(
                    query => query.Where(r => r.ProductId == productId)))
                    .AsEnumerable()
                    .OrderByDescending(r => r.Similarity)
                    .ToList();

                // If no similar products found, return empty list
                if (!similarProducts.Any())
                {
                    return Ok(new List<ProductOverviewModel>());
                }

                var productIds = similarProducts.Select(p => p.SimilarProductId).ToArray();

                // Load products
                var products = await (await _productService.GetProductsByIdsAsync(productIds))
                    // ACL and store mapping
                    .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
                    // Availability dates
                    .Where(p => _productService.ProductIsAvailable(p))
                    // Visible individually
                    .Where(p => p.VisibleIndividually).ToListAsync();

                var model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, productThumbPictureSize)).ToList();

                // Fix async operation - use await properly
                foreach (var pro in model)
                {
                    var stockQuantity = await GetProductAvailabilityAsync(pro.Id);
                    pro.CustomProperties.Add("StockQuantity", stockQuantity.ToString());
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                // Log error and return empty result
                _logger.Error("Error getting similar products", ex);
                return Ok(new List<ProductOverviewModel>());
            }
        }


        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductDetailsModel), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductDetails(int productId, int updatecartitemid = 0)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted)
                return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(product)));

            //visible individually?
            if (!product.VisibleIndividually)
            {
                //is this one an associated products?
                var parentGroupedProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
                if (parentGroupedProduct == null)
                    return NotFound(string.Format(MessageDefaults.NOT_FOUND, "product"));

                while (true)
                    if (!parentGroupedProduct.VisibleIndividually)
                    {
                        parentGroupedProduct = await _productService.GetProductByIdAsync(parentGroupedProduct.ParentGroupedProductId);
                        if (parentGroupedProduct == null)
                            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "product"));
                    }
                    else
                    {
                        product = parentGroupedProduct;
                        break;
                    }
            }

            //update existing shopping cart or wishlist  item?
            ShoppingCartItem updatecartitem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
            {
                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), storeId: (await _storeContext.GetCurrentStoreAsync()).Id);
                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                //not found?
                if (updatecartitem == null)
                    return NotFound(string.Format(MessageDefaults.NOT_FOUND, "cartItem"));
                //is it this product?
                if (product.Id != updatecartitem.ProductId)
                    return NotFound(string.Format(MessageDefaults.NOT_FOUND, "product"));
            }

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ViewProduct",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewProduct"), product.Name), product);

            //model
            var model = await _productModelFactory.PrepareProductDetailsModelAsync(product, updatecartitem, false);
            bool MarkAsNew = false;

            if (product.MarkAsNew != null)
            {
                MarkAsNew = product.MarkAsNew &&  // Use GetValueOrDefault to handle null
                           (!product.MarkAsNewStartDateTimeUtc.HasValue || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) &&
                           (!product.MarkAsNewEndDateTimeUtc.HasValue || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow);
            }

            if (model != null && product.MarkAsNew != null)
            {
                model.CustomProperties.Add("MarkAsNew", MarkAsNew.ToString());
            }


            return Ok(model);
        }
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ConfirmOrder(PaymentInfoRequest request)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (!cart.Any())
                return NotFound(string.Format(MessageDefaults.NOT_FOUND, "cartItems"));

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
                return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

            var validationResult = await ValidatePaymentInfoAsync(cart, request.PaymentInfo, request.PreviousOrderGuid,
                request.PreviousOrderGuidGeneratedOnUtc);

            if (!validationResult.Success)
                return BadRequest(validationResult.Errors);

            return Ok(await ConfirmOrderAsync(validationResult, request.PaymentInfo, request.PreviousOrderGuid,
                request.PreviousOrderGuidGeneratedOnUtc));
        }
        protected virtual async Task<ValidatePaymentInfoResponse> ValidatePaymentInfoAsync(IList<ShoppingCartItem> cart,
     IDictionary<string, string> paymentInfo, Guid? previousOrderGuid, DateTime? previousOrderGuidGeneratedOnUtc,
     bool validatePaymentWorkflow = false)
        {
            var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
            var response = new ValidatePaymentInfoResponse
            {
                IsPaymentWorkflowRequired = isPaymentWorkflowRequired
            };
            if (isPaymentWorkflowRequired)
            {
                //load payment method
                var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);

                if (string.IsNullOrEmpty(paymentMethodSystemName))
                {
                    response.AddError(MessageDefaults.PAYMENT_METHOD_REQUIRED);
                    return response;
                }

                var paymentMethod = await _paymentPluginManager
                    .LoadPluginBySystemNameAsync(paymentMethodSystemName, await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);

                if (paymentMethod == null)
                {
                    response.AddError(MessageDefaults.PAYMENT_METHOD_NOT_FOUND);
                    return response;
                }

                var form = new FormCollection(ConvertToFormCollection(paymentInfo));
                var warnings = await paymentMethod.ValidatePaymentFormAsync(form);
                if (warnings.Count <= 0)
                {
                    //set previous order GUID (if exists)
                    var (orderGuid, orderGuidGeneratedOnUtc) = _pluginPaymentService.GenerateOrderGuid(previousOrderGuid,
                        previousOrderGuidGeneratedOnUtc);
                    response.OrderGuid = orderGuid;
                    response.OrderGuidGeneratedOnUtc = orderGuidGeneratedOnUtc;
                }
                else
                    foreach (var warning in warnings)
                        response.AddError(warning);
            }
            else
                if (validatePaymentWorkflow)
                response.AddError("Payment workflow is not required");

            return response;
        }
        protected virtual async Task<ApiExConfirmOrderResponse> ConfirmOrderAsync(ValidatePaymentInfoResponse validatePaymentInfoResponse,
         IDictionary<string, string> paymentInfo, Guid? previousOrderGuid, DateTime? previousOrderGuidGeneratedOnUtc)
        {
            var result = new ApiExConfirmOrderResponse();
            ProcessPaymentRequest processPaymentRequest;
            if (validatePaymentInfoResponse.IsPaymentWorkflowRequired)
            {
                var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
                var paymentMethod = await _paymentPluginManager
                    .LoadPluginBySystemNameAsync(paymentMethodSystemName, await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);
                if (paymentMethod == null)
                    result.Errors.Add(string.Format(MessageDefaults.NOT_FOUND, nameof(paymentMethod)));

                var form = new FormCollection(ConvertToFormCollection(paymentInfo));
                processPaymentRequest = await paymentMethod.GetPaymentInfoAsync(form);
                processPaymentRequest.OrderGuid = validatePaymentInfoResponse.OrderGuid.Value;
                processPaymentRequest.OrderGuidGeneratedOnUtc = validatePaymentInfoResponse.OrderGuidGeneratedOnUtc;
            }
            else
            {
                var (orderGuid, orderGuidGeneratedOnUtc) = _pluginPaymentService.GenerateOrderGuid(previousOrderGuid,
                            previousOrderGuidGeneratedOnUtc);

                processPaymentRequest = new ProcessPaymentRequest
                {
                    OrderGuid = orderGuid,
                    OrderGuidGeneratedOnUtc = orderGuidGeneratedOnUtc
                };
            }
            var (placeOrderError, placeOrderResult, redirectUrl) = await PlaceOrderAsync(processPaymentRequest);




            if (!string.IsNullOrEmpty(placeOrderError))
                result.Errors.Add(placeOrderError);

            result.RedirectionUrl = redirectUrl;

            if (placeOrderResult.Success)
            {
                result.OrderId = placeOrderResult.PlacedOrder.Id;
                var order = await _orderService.GetOrderByIdAsync(result.OrderId);
                result.OrderCustomNumber = order.CustomOrderNumber;

            }
            else
                foreach (var error in placeOrderResult.Errors)
                    result.Errors.Add(error);
            return result;
        }
        protected virtual async Task<(string, PlaceOrderResult, string)> PlaceOrderAsync(ProcessPaymentRequest processPaymentRequest)
        {
            try
            {
                //prevent 2 orders being placed within an X seconds time frame
                if (!await IsMinimumOrderPlacementIntervalValidAsync())
                    return (await _localizationService.GetResourceAsync("Checkout.MinOrderPlacementInterval"), null, "");

                processPaymentRequest.StoreId = (await _storeContext.GetCurrentStoreAsync()).Id;
                processPaymentRequest.CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;
                processPaymentRequest.PaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);

                var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);
                var redirectUrl = string.Empty;
                if (placeOrderResult.Success)
                {
                    var customer = await _customerService.GetCustomerByIdAsync(placeOrderResult.PlacedOrder.CustomerId);
                    var paymentMethod = await _paymentPluginManager
                        .LoadPluginBySystemNameAsync(placeOrderResult.PlacedOrder.PaymentMethodSystemName, customer, placeOrderResult.PlacedOrder.StoreId);

                    if (paymentMethod == null)
                        return ("Payment method couldn't be loaded", null, "");

                    if (paymentMethod.PaymentMethodType == PaymentMethodType.Standard)
                        await _paymentService.PostProcessPaymentAsync(new PostProcessPaymentRequest { Order = placeOrderResult.PlacedOrder });
                    else if (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection)
                    {
                        //already paid or order.OrderTotal == decimal.Zero
                        if (placeOrderResult.PlacedOrder.PaymentStatus == PaymentStatus.Paid)
                            return (string.Empty, placeOrderResult, redirectUrl);

                        switch (placeOrderResult.PlacedOrder.PaymentMethodSystemName)
                        {
                            case PaymentMethodDefaults.PAY_PAL_STANDARD:
                                redirectUrl = await _pluginPaymentService.GetPayPalStandardRedirectionUrl(placeOrderResult.PlacedOrder);
                                break;
                            case PaymentMethodDefaults.SKRILL:
                                redirectUrl = await _pluginPaymentService.GetSkrillRedirectionUrl(placeOrderResult.PlacedOrder);
                                break;
                            case PaymentMethodDefaults.TWO_CHECKOUT:
                                redirectUrl = await _pluginPaymentService.GetTwoCheckoutRedirectionUrl(placeOrderResult.PlacedOrder);
                                break;
                        }
                    }

                }
                return (string.Empty, placeOrderResult, redirectUrl);

            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc);
                return (exc.Message, null, "");
            }
        }


        protected virtual async Task<bool> IsMinimumOrderPlacementIntervalValidAsync()
        {
            //prevent 2 orders being placed within an X seconds time frame
            if (_orderSettings.MinimumOrderPlacementInterval == 0)
                return true;

            var lastOrder = (await _orderService.SearchOrdersAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                customerId: (await _workContext.GetCurrentCustomerAsync()).Id, pageSize: 1))
                .FirstOrDefault();
            if (lastOrder == null)
                return true;

            var interval = DateTime.UtcNow - lastOrder.CreatedOnUtc;
            return interval.TotalSeconds > _orderSettings.MinimumOrderPlacementInterval;
        }


        //protected async Task<bool> ValidateAddressAsync(Address address)
        //{
        //    if (address == null)
        //        return false;

        //    // Check required fields
        //    if (string.IsNullOrWhiteSpace(address.Address1))
        //        return false;



        //    // Check if _addressSettings is null
        //    if (_addressSettings == null)
        //        return false;



        //    if (_addressSettings.PhoneEnabled && _addressSettings.PhoneRequired && string.IsNullOrWhiteSpace(address.PhoneNumber))
        //        return false;

        //    if (_addressSettings.ZipPostalCodeEnabled && _addressSettings.ZipPostalCodeRequired && string.IsNullOrWhiteSpace(address.ZipPostalCode))
        //        return false;

        //    // Validate Country
        //    if (address.CountryId.HasValue)
        //    {
        //        if (_countryService == null || _storeMappingService == null)
        //            return false;

        //        var country = await _countryService.GetCountryByIdAsync(address.CountryId.Value);
        //        if (country == null || !country.Published || !await _storeMappingService.AuthorizeAsync(country))
        //            return false;
        //    }

        //    // Validate State/Province (if required)
        //    if (address.StateProvinceId.HasValue)
        //    {
        //        if (_stateProvinceService == null)
        //            return false;

        //        var state = await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId.Value);
        //        if (state == null || !state.Published)
        //            return false;
        //    }

        //    return true;
        //}




        //[HttpGet("GetAddresses")]
        //[ProducesResponseType(typeof(CustomerAddressListModel), StatusCodes.Status200OK)]
        //public virtual async Task<IActionResult> GetAddresses()
        //{
        //    try
        //    {
        //        // Check for null services
        //        if (_customerService == null || _workContext == null || _customerModelFactory == null || _mapper == null)
        //        {
        //            return StatusCode(StatusCodes.Status500InternalServerError, "Required services are not available");
        //        }

        //        var customer = await _workContext.GetCurrentCustomerAsync();
        //        if (customer == null || !await _customerService.IsRegisteredAsync(customer))
        //        {
        //            return Unauthorized();
        //        }

        //        // Ensure you're using the correct model type (frontend version)
        //        var model = await _customerModelFactory.PrepareCustomerAddressListModelAsync();
        //        if (model?.Addresses == null)
        //        {
        //            return Ok(new CustomerAddressListModel());
        //        }

        //        var validAddresses = new List<Nop.Web.Models.Common.AddressModel>(); // Explicitly use frontend model

        //        foreach (var address in model.Addresses)
        //        {
        //            try
        //            {
        //                // Explicitly specify the source and destination types
        //                var mappedAddress = _mapper.Map<Nop.Web.Models.Common.AddressModel, Address>(address);
        //                if (mappedAddress != null && await ValidateAddressAsync(mappedAddress))
        //                {
        //                    validAddresses.Add(address);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                // Log the error if needed
        //                continue;
        //            }
        //        }

        //        model.Addresses = validAddresses;
        //        return Ok(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception here
        //        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
        //    }
        //}



        [HttpPost]
        [ProducesResponseType(typeof(RecalculateShippingResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RecalculateShipping()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            // Clear all shipping-related cache
            await _genericAttributeService.SaveAttributeAsync<string>(
                customer,
                NopCustomerDefaults.SelectedShippingOptionAttribute,
                null,
                store.Id);

            await _genericAttributeService.SaveAttributeAsync<PickupPoint>(
                customer,
                NopCustomerDefaults.SelectedPickupPointAttribute,
                null,
                store.Id);

            // Force shipping recalculation
            var shippingAddress = await _customerService.GetCustomerShippingAddressAsync(customer);
            var getShippingOptionResponse = await _shippingService.GetShippingOptionsAsync(cart, shippingAddress, customer);

            if (!getShippingOptionResponse.Success)
            {
                return BadRequest(new { errors = getShippingOptionResponse.Errors });
            }

            // Get updated order totals
            var orderTotals = await _shoppingCartModelFactory.PrepareOrderTotalsModelAsync(cart, true);

            return Ok(new RecalculateShippingResponse
            {
                ShippingOptions = getShippingOptionResponse.ShippingOptions,
                OrderTotals = orderTotals
            });
        }
        protected virtual async Task ParseAndSaveCheckoutAttributesAsync(IList<ShoppingCartItem> cart, IFormCollection form)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = string.Empty;
            var excludeShippableAttributes = !await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
            var checkoutAttributes = await _checkoutAttributeService.GetAllAttributesAsync(_staticCacheManager, _storeMappingService, (await _storeContext.GetCurrentStoreAsync()).Id, excludeShippableAttributes);
            foreach (var attribute in checkoutAttributes)
            {
                var controlId = $"checkout_attribute_{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }

                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                                foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                        }

                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = await _checkoutAttributeService.GetAttributeValuesAsync(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                                attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                        }

                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }

                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var date = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(date));
                            }
                            catch
                            {
                                // ignored
                            }

                            if (selectedDate.HasValue)
                                attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                    attribute, selectedDate.Value.ToString("D"));
                        }

                        break;
                    case AttributeControlType.FileUpload:
                        {
                            _ = Guid.TryParse(form[controlId], out var downloadGuid);
                            var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                            if (download != null)
                                attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                           attribute, download.DownloadGuid.ToString());
                        }

                        break;
                    default:
                        break;
                }
            }

            //validate conditional attributes (if specified)
            foreach (var attribute in checkoutAttributes)
            {
                var conditionMet = await _checkoutAttributeParser.IsConditionMetAsync(attribute.ConditionAttributeXml, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                    attributesXml = _checkoutAttributeParser.RemoveAttribute(attributesXml, attribute.Id);
            }

            //save checkout attributes
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.CheckoutAttributes, attributesXml, (await _storeContext.GetCurrentStoreAsync()).Id);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ChangeCheckoutAttributeResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ChangeCheckoutAttribute(IDictionary<string, string> checkoutAttributes)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            // Save selected attributes
            var form = new FormCollection(ConvertToFormCollection(checkoutAttributes));
            await ParseAndSaveCheckoutAttributesAsync(cart, form);

            // Clear cached shipping methods to force recalculation
            await _genericAttributeService.SaveAttributeAsync<string>(
                customer,
                NopCustomerDefaults.SelectedShippingOptionAttribute,
                null,
                store.Id);

            // Also clear any cached pickup points
            await _genericAttributeService.SaveAttributeAsync<PickupPoint>(
                customer,
                NopCustomerDefaults.SelectedPickupPointAttribute,
                null,
                store.Id);

            // Force shipping method recalculation by getting shipping options
            var shippingAddress = await _customerService.GetCustomerShippingAddressAsync(customer);
            var shippingOptions = await _shippingService.GetShippingOptionsAsync(cart, shippingAddress, customer);

            // If there are shipping options available, select the first one automatically
            if (shippingOptions.Success && shippingOptions.ShippingOptions.Any())
            {
                var selectedShippingOption = shippingOptions.ShippingOptions.First();
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.SelectedShippingOptionAttribute,
                    selectedShippingOption,
                    store.Id);
            }

            // Get updated attribute XML after save
            var attributeXml = await _genericAttributeService.GetAttributeAsync<string>(customer,
                NopCustomerDefaults.CheckoutAttributes, store.Id);

            // Conditions
            var enabledAttributeIds = new List<int>();
            var disabledAttributeIds = new List<int>();
            var excludeShippableAttributes = !await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
            var attributes = await _checkoutAttributeService.GetAllAttributesAsync(_staticCacheManager, _storeMappingService, store.Id, excludeShippableAttributes);

            foreach (var attribute in attributes)
            {
                var conditionMet = await _checkoutAttributeParser.IsConditionMetAsync(attribute.ConditionAttributeXml, attributeXml);
                if (conditionMet.HasValue)
                {
                    if (conditionMet.Value)
                        enabledAttributeIds.Add(attribute.Id);
                    else
                        disabledAttributeIds.Add(attribute.Id);
                }
            }

            // Get updated order totals with recalculated shipping
            var updatedOrderTotals = await _shoppingCartModelFactory.PrepareOrderTotalsModelAsync(cart, true);

            return Ok(new ChangeCheckoutAttributeResponse
            {


                OrderTotals = updatedOrderTotals,
                SelectedAttributes = await _shoppingCartModelFactory.FormatSelectedCheckoutAttributesAsync(),
                EnabledAttributeIds = enabledAttributeIds,
                DisabledAttributeIds = disabledAttributeIds
                // ,  ShippingOptions = shippingOptions.ShippingOptions // Return available shipping options
            });
        }


        [HttpGet]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CheckoutPaymentMethodModel), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetActivePaymentMethods()
        {
            //validation

            var filterByCountryId = (await _customerService.GetCustomerBillingAddressAsync(await _workContext.GetCurrentCustomerAsync()))?.CountryId;

            var paymentMethods = await (await _paymentPluginManager
                .LoadActivePluginsAsync(await _workContext.GetCurrentCustomerAsync(), 1, filterByCountryId.GetValueOrDefault()))
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Standard || pm.PaymentMethodType == PaymentMethodType.Redirection)
                .ToListAsync();

            IList<ShoppingCartItem> cart = new List<ShoppingCartItem>();



            foreach (var pm in paymentMethods)
            {
                var paymentAdditionalFee = await _paymentService.GetAdditionalHandlingFeeAsync(cart, pm.PluginDescriptor.SystemName);

                // PluginLogoUrl
                pm.PluginDescriptor.Author = await _paymentPluginManager.GetPluginLogoUrlAsync(pm);


                pm.PluginDescriptor.AssemblyFileName = paymentAdditionalFee.ToString();
            }


            return Ok(paymentMethods);
        }











        [HttpGet]
        [ProducesResponseType(typeof(CustomerAddressListModel), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAddresses()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            var model = await _customerModelFactory.PrepareCustomerAddressListModelAsync();
            model.Addresses = model.Addresses
    .GroupBy(a => new
    {
        a.FirstName,
        a.LastName,
        a.Email,
        a.Address1,
        a.Address2,
        a.CountryId,
        a.StateProvinceId,
        a.PhoneNumber
    })
        .Select(g => g.First())
        .ToList();
            return Ok(model);
        }



        private bool IsValidAddress(Nop.Web.Models.Common.AddressModel address)
        {
            return !string.IsNullOrEmpty(address?.CountryName) &&
                   !string.IsNullOrEmpty(address?.City) &&
                   !string.IsNullOrEmpty(address?.FirstName) &&
                   !string.IsNullOrEmpty(address?.LastName) &&
                   !string.IsNullOrEmpty(address?.Email);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CheckoutPaymentMethodModel), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetPaymentMethods(int verison = 0)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (!cart.Any())
                return NotFound(string.Format(MessageDefaults.NOT_FOUND, "cartItems"));

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
                return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

            //Check whether payment workflow is required
            //we ignore reward points during cart total calculation
            var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart, false);
            if (!isPaymentWorkflowRequired)
            {
                await _genericAttributeService.SaveAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);
                return BadRequest("Payment workflow is not required");
            }

            //filter by country
            var filterByCountryId = 0;
            if (_addressSettings.CountryEnabled)
                filterByCountryId = (await _customerService.GetCustomerBillingAddressAsync(await _workContext.GetCurrentCustomerAsync()))?.CountryId ?? 0;

            //model
            var paymentMethodModel = await _checkoutModelFactory.PreparePaymentMethodModelAsync(cart, filterByCountryId);
            if (verison == 0)
            {
                paymentMethodModel.PaymentMethods = paymentMethodModel.PaymentMethods
                    .Where(pm => !pm.PaymentMethodSystemName.Contains("noon", StringComparison.OrdinalIgnoreCase))
                    .ToList();







                if (!paymentMethodModel.PaymentMethods.Any(pm => pm.PaymentMethodSystemName.Contains("Payments.PayFort.MPage", StringComparison.OrdinalIgnoreCase)))
                {

                    var paymentMethods = await (await _paymentPluginManager.LoadAllPluginsAsync
       ())
   .Where(pm => pm.PaymentMethodType == PaymentMethodType.Standard || pm.PaymentMethodType == PaymentMethodType.Redirection && pm.PluginDescriptor.SystemName.Contains("Payments.PayFort.MPage", StringComparison.OrdinalIgnoreCase))
   .WhereAwait(async pm => !await pm.HidePaymentMethodAsync(cart))
   .ToListAsync();


                    foreach (var pm in paymentMethods)
                    {
                        if (await _shoppingCartService.ShoppingCartIsRecurringAsync(cart) && pm.RecurringPaymentType == RecurringPaymentType.NotSupported)
                            continue;

                        var pmModel = new CheckoutPaymentMethodModel.PaymentMethodModel
                        {
                            Name = await _localizationService.GetLocalizedFriendlyNameAsync(pm, (await _workContext.GetWorkingLanguageAsync()).Id),
                            Description = _paymentSettings.ShowPaymentMethodDescriptions ? await pm.GetPaymentMethodDescriptionAsync() : string.Empty,
                            PaymentMethodSystemName = pm.PluginDescriptor.SystemName,
                            LogoUrl = await _paymentPluginManager.GetPluginLogoUrlAsync(pm)
                        };

                        var customer = await _workContext.GetCurrentCustomerAsync();
                        var paymentMethodAdditionalFee = await _paymentService.GetAdditionalHandlingFeeAsync(cart, pm.PluginDescriptor.SystemName);
                        var taxResult = await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFee, customer);
                        var rateBase = taxResult.taxRate; // Use the actual property name from the tax result
                        var rate = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(rateBase, await _workContext.GetWorkingCurrencyAsync());

                        if (rate > decimal.Zero)
                            pmModel.Fee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(rate, true);

                        paymentMethodModel.PaymentMethods.Add(pmModel);
                    }
                }
            }

            return Ok(paymentMethodModel);

        }

      



    }
}



    








 







    #endregion


 







