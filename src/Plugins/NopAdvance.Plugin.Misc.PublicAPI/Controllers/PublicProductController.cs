// ***	 ** ****** ****** ****** ******* **     ** ****** ***   ** **** ****
// ****  ** **  ** **  ** **  **  **  **  **   **  **  ** ****  ** *    *  
// ** ** ** **  ** ****** ******  **  **   ** **   ****** ** ** ** *    ***
// **  **** **  ** **	  **  **  **  **    ***    **  ** **  **** *    *  
// **   *** ****** **	  **  ** *******     *     **  ** **   *** **** ****
// ***************************************************************************
// *                                                                         *
// *    NopCommerce Public RESTful API Plugin by NopAdvance team             *
// *    Copyright (c) NopAdvance LLP. All Rights Reserved.                   *
// *                                                                         *
// ***************************************************************************
// *                                                                         *
// *    This software is licensed for use under the terms accepted during    *
// *    the purchase of this product. A non-exclusive, non-transferable      *
// *    right is granted to use this product on the website for which it was *
// *    licensed.                                                            *
// *                                                                         *
// *    Companies purchasing this product for their customers are permitted, *
// *    provided the use complies with the terms outlined in the EULA:       *
// *    https://store.nopadvance.com/eula.                                   *
// *                                                                         *
// *    You may not reverse engineer, decompile, modify, or distribute this  *
// *    software without explicit permission from NopAdvance LLP. Any        *
// *    violation will result in the termination of your license and may     *
// *    lead to legal action.                                                *
// *                                                                         *
// ***************************************************************************
// *    Contact: contact@nopadvance.com                                      *
// *    Website: https://nopadvance.com                                      *
// ***************************************************************************
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;
using NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;
using NopAdvance.Plugin.Misc.PublicAPI.Helpers;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;
using NopAdvance.Plugin.Misc.PublicAPI.Services;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers;

/// <summary>
/// Product methods
/// </summary>
public class PublicProductController : BaseAPIController
{
    #region Fields

    private readonly CatalogSettings _catalogSettings;
    private readonly ShoppingCartSettings _shoppingCartSettings;
    private readonly MediaSettings _mediaSettings;
    private readonly LocalizationSettings _localizationSettings;
    private readonly IAclService _aclService;
    private readonly IProductModelFactory _productModelFactory;
    private readonly IProductService _productService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IStoreContext _storeContext;
    private readonly IOrderReportService _orderReportService;
    private readonly IPermissionService _permissionService;
    private readonly IWorkContext _workContext;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly ILocalizationService _localizationService;
    private readonly IProductAttributeParser _productAttributeParser;
    private readonly ITaxService _taxService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly ICurrencyService _currencyService;
    private readonly IProductAttributeService _productAttributeService;
    private readonly IPictureService _pictureService;
    private readonly IWebHelper _webHelper;
    private readonly ICustomerService _customerService;
    private readonly IWorkflowMessageService _workflowMessageService;
    private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
    private readonly IOrderService _orderService;
    private readonly ICatalogModelFactory _catalogModelFactory;
    private readonly IProductTagService _productTagService;
    private readonly IEventPublisher _eventPublisher;
    private readonly IReviewTypeService _reviewTypeService;
    private readonly IHtmlFormatter _htmlFormatter;
    private readonly INotificationService _notificationService;
    private readonly IProductReviewApiService _productReviewApiService;


    #endregion

    #region Ctor

    public PublicProductController(CatalogSettings catalogSettings,
        ShoppingCartSettings shoppingCartSettings,
        MediaSettings mediaSettings,
        LocalizationSettings localizationSettings,
        IAclService aclService,
        IProductModelFactory productModelFactory,
        IProductService productService,
        IStoreMappingService storeMappingService,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        IOrderReportService orderReportService,
        IPermissionService permissionService,
        IWorkContext workContext,
        ICustomerActivityService customerActivityService,
        IShoppingCartService shoppingCartService,
        ILocalizationService localizationService,
        IProductAttributeParser productAttributeParser,
        ITaxService taxService,
        IPriceFormatter priceFormatter,
        ICurrencyService currencyService,
        IProductAttributeService productAttributeService,
        IPictureService pictureService,
        IWebHelper webHelper,
        ICustomerService customerService,
        IWorkflowMessageService workflowMessageService,
        IShoppingCartModelFactory shoppingCartModelFactory,
        IOrderService orderService,
        ICatalogModelFactory catalogModelFactory,
        IProductTagService productTagService,
        IEventPublisher eventPublisher,
        IReviewTypeService reviewTypeService,
        IHtmlFormatter htmlFormatter,
        INotificationService notificationService, IProductReviewApiService productReviewApiService)
    {
        _catalogSettings = catalogSettings;
        _shoppingCartSettings = shoppingCartSettings;
        _mediaSettings = mediaSettings;
        _localizationSettings = localizationSettings;
        _aclService = aclService;
        _productModelFactory = productModelFactory;
        _productService = productService;
        _storeMappingService = storeMappingService;
        _staticCacheManager = staticCacheManager;
        _storeContext = storeContext;
        _orderReportService = orderReportService;
        _permissionService = permissionService;
        _workContext = workContext;
        _customerActivityService = customerActivityService;
        _shoppingCartService = shoppingCartService;
        _localizationService = localizationService;
        _productAttributeParser = productAttributeParser;
        _taxService = taxService;
        _priceFormatter = priceFormatter;
        _currencyService = currencyService;
        _productAttributeService = productAttributeService;
        _pictureService = pictureService;
        _webHelper = webHelper;
        _customerService = customerService;
        _workflowMessageService = workflowMessageService;
        _shoppingCartModelFactory = shoppingCartModelFactory;
        _orderService = orderService;
        _catalogModelFactory = catalogModelFactory;
        _productTagService = productTagService;
        _eventPublisher = eventPublisher;
        _reviewTypeService = reviewTypeService;
        _htmlFormatter = htmlFormatter;
        _notificationService = notificationService;
        _productService = productService;
    }

    #endregion

    #region Utilities

    protected virtual async Task ValidateProductReviewAvailabilityAsync(Product product)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (await _customerService.IsGuestAsync(customer) && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
            ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Reviews.OnlyRegisteredUsersCanWriteReviews"));

        if (!_catalogSettings.ProductReviewPossibleOnlyAfterPurchasing)
            return;

        var hasCompletedOrders = product.ProductType == ProductType.SimpleProduct
            ? await HasCompletedOrdersAsync(product)
            : await (await _productService.GetAssociatedProductsAsync(product.Id)).AnyAwaitAsync(HasCompletedOrdersAsync);

        if (!hasCompletedOrders)
            ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Reviews.ProductReviewPossibleOnlyAfterPurchasing"));
    }

    protected virtual async ValueTask<bool> HasCompletedOrdersAsync(Product product)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        return (await _orderService.SearchOrdersAsync(customerId: customer.Id,
            productId: product.Id,
            osIds: new List<int> { (int)OrderStatus.Complete },
            pageSize: 1)).Any();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get products to be displayed on home page
    /// </summary>
    /// <param name="productThumbPictureSize">Product thumbnail picture size(optional)</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductOverviewModel>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetHomepageProducts(int? productThumbPictureSize)
    {
        var products = await (await _productService.GetAllProductsDisplayedOnHomepageAsync())
        //ACL and store mapping
        .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
        //availability dates
        .Where(p => _productService.ProductIsAvailable(p))
        //visible individually
        .Where(p => p.VisibleIndividually).ToListAsync();

        var model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, productThumbPictureSize)).ToList();

        return Ok(model);
    }

    /// <summary>
    /// Get best selling products
    /// </summary>
    /// <param name="productThumbPictureSize">Product thumbnail picture size(optional)</param>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IEnumerable<ProductOverviewModel>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetBestSellers(int? productThumbPictureSize)
    {
        if (!_catalogSettings.ShowBestsellersOnHomepage || _catalogSettings.NumberOfBestsellersOnHomepage == 0)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        //load and cache report
        var report = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.HomepageBestsellersIdsKey,
                await _storeContext.GetCurrentStoreAsync()),
            async () => await (await _orderReportService.BestSellersReportAsync(
                storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                pageSize: _catalogSettings.NumberOfBestsellersOnHomepage)).ToListAsync());

        //load products
        var products = await (await _productService.GetProductsByIdsAsync(report.Select(x => x.ProductId).ToArray()))
        //ACL and store mapping
        .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
        //availability dates
        .Where(p => _productService.ProductIsAvailable(p)).ToListAsync();

        //prepare model
        var model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, productThumbPictureSize)).ToList();
        return Ok(model);
    }

    #region Product details page

    /// <summary>
    /// Get a product details
    /// </summary>
    /// <param name="productId">The product identifier</param>
    /// <param name="updatecartitemid">The cart item identifier to update (Pass 0 if not in update)</param>
    [HttpGet("{productId}")]
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

        return Ok(model);
    }

    /// <summary>
    /// Change and apply product attributes
    /// </summary>
    /// <param name="productId">The product identifier</param>
    [HttpPost("{productId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProductDetailsAttributeChangeResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> ChangeProductAttribute(int productId, ChangeProductAttributeRequest request)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(product)));

        var errors = new List<string>();
        var form = new FormCollection(ConvertToFormCollection(request.Attributes));
        var attributeXml = await _productAttributeParser.ParseProductAttributesAsync(product, form, errors);

        //rental attributes
        DateTime? rentalStartDate = null;
        DateTime? rentalEndDate = null;
        if (product.IsRental)
            _productAttributeParser.ParseRentalDates(product, form, out rentalStartDate, out rentalEndDate);

        //sku, mpn, gtin
        var sku = await _productService.FormatSkuAsync(product, attributeXml);
        var mpn = await _productService.FormatMpnAsync(product, attributeXml);
        var gtin = await _productService.FormatGtinAsync(product, attributeXml);

        // calculating weight adjustment
        var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(attributeXml);
        var totalWeight = product.BasepriceAmount;

        foreach (var attributeValue in attributeValues)
            switch (attributeValue.AttributeValueType)
            {
                case AttributeValueType.Simple:
                    //simple attribute
                    totalWeight += attributeValue.WeightAdjustment;
                    break;
                case AttributeValueType.AssociatedToProduct:
                    //bundled product
                    var associatedProduct = await _productService.GetProductByIdAsync(attributeValue.AssociatedProductId);
                    if (associatedProduct != null)
                        totalWeight += associatedProduct.BasepriceAmount * attributeValue.Quantity;
                    break;
            }

        //price
        var price = string.Empty;
        //base price
        var basepricepangv = string.Empty;
        if (await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES) && !product.CustomerEntersPrice)
        {
            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            //we do not calculate price of "customer enters price" option is enabled
            var (finalPrice, _, _) = await _shoppingCartService.GetUnitPriceAsync(product,
                currentCustomer,
                currentStore,
                ShoppingCartType.ShoppingCart,
                1, attributeXml, 0,
                rentalStartDate, rentalEndDate, true);
            var (finalPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, finalPrice);
            var finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());
            price = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
            basepricepangv = await _priceFormatter.FormatBasePriceAsync(product, finalPriceWithDiscountBase, totalWeight);
        }

        //stock
        var stockAvailability = await _productService.FormatStockMessageAsync(product, attributeXml);

        //conditional attributes
        var enabledAttributeMappingIds = new List<int>();
        var disabledAttributeMappingIds = new List<int>();
        if (request.ValidateAttributeConditions)
        {
            var attributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            foreach (var attribute in attributes)
            {
                var conditionMet = await _productAttributeParser.IsConditionMetAsync(attribute, attributeXml);
                if (conditionMet.HasValue)
                    if (conditionMet.Value)
                        enabledAttributeMappingIds.Add(attribute.Id);
                    else
                        disabledAttributeMappingIds.Add(attribute.Id);
            }
        }

        //picture. used when we want to override a default product picture when some attribute is selected
        var pictureFullSizeUrl = string.Empty;
        var pictureDefaultSizeUrl = string.Empty;
        if (request.LoadPicture)
        {
            //first, try to get product attribute combination picture
            var pictureId = (await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributeXml))?.PictureId ?? 0;

            //then, let's see whether we have attribute values with pictures
            if (pictureId == 0)
                pictureId = (await _productAttributeParser.ParseProductAttributeValuesAsync(attributeXml))
                    .FirstOrDefault(attributeValue => attributeValue.PictureId > 0)?.PictureId ?? 0;

            if (pictureId > 0)
            {
                var productAttributePictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductAttributePictureModelKey,
                    pictureId, _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());
                var pictureModel = await _staticCacheManager.GetAsync(productAttributePictureCacheKey, async () =>
                {
                    var picture = await _pictureService.GetPictureByIdAsync(pictureId);
                    string fullSizeImageUrl, imageUrl;

                    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                    (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductDetailsPictureSize);

                    return picture == null ? new PictureModel() : new PictureModel
                    {
                        FullSizeImageUrl = fullSizeImageUrl,
                        ImageUrl = imageUrl
                    };
                });
                pictureFullSizeUrl = pictureModel.FullSizeImageUrl;
                pictureDefaultSizeUrl = pictureModel.ImageUrl;
            }
        }

        var isFreeShipping = product.IsFreeShipping;
        if (isFreeShipping && !string.IsNullOrEmpty(attributeXml))
            isFreeShipping = await (await _productAttributeParser.ParseProductAttributeValuesAsync(attributeXml))
                .Where(attributeValue => attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                .SelectAwait(async attributeValue => await _productService.GetProductByIdAsync(attributeValue.AssociatedProductId))
                .AllAsync(associatedProduct => associatedProduct == null || !associatedProduct.IsShipEnabled || associatedProduct.IsFreeShipping);

        return Ok(new ProductDetailsAttributeChangeResponse
        {
            ProductId = productId,
            Gtin = gtin,
            Mpn = mpn,
            Sku = sku,
            Price = price,
            Basepricepangv = basepricepangv,
            StockAvailability = stockAvailability,
            Enabledattributemappingids = enabledAttributeMappingIds.ToArray(),
            Disabledattributemappingids = disabledAttributeMappingIds.ToArray(),
            PictureFullSizeUrl = pictureFullSizeUrl,
            PictureDefaultSizeUrl = pictureDefaultSizeUrl,
            IsFreeShipping = isFreeShipping,
            Message = errors.Any() ? errors.ToArray() : null
        });
    }

    /// <summary>
    /// Estimate shipping
    /// </summary>
    /// <param name="productId">The product identifier</param>
    [HttpPost("{productId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EstimateShippingResultModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> EstimateShipping(int productId, EstimateShippingRequest request)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null || product.Deleted)
            return NotFound(await _localizationService.GetResourceAsync("Shipping.EstimateShippingPopUp.Product.IsNotFound"));

        var wrappedProduct = new ShoppingCartItem()
        {
            StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
            ShoppingCartTypeId = (int)ShoppingCartType.ShoppingCart,
            CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
            ProductId = product.Id,
            CreatedOnUtc = DateTime.UtcNow
        };

        var form = new FormCollection(ConvertToFormCollection(request.Attributes));
        var addToCartWarnings = new List<string>();
        //customer entered price
        wrappedProduct.CustomerEnteredPrice = await _productAttributeParser.ParseCustomerEnteredPriceAsync(product, form);

        //entered quantity
        wrappedProduct.Quantity = _productAttributeParser.ParseEnteredQuantity(product, form);

        //product and gift card attributes
        wrappedProduct.AttributesXml = await _productAttributeParser.ParseProductAttributesAsync(product, form, addToCartWarnings);

        //rental attributes
        _productAttributeParser.ParseRentalDates(product, form, out var rentalStartDate, out var rentalEndDate);
        wrappedProduct.RentalStartDateUtc = rentalStartDate;
        wrappedProduct.RentalEndDateUtc = rentalEndDate;

        var model = new EstimateShippingModel
        {
            ZipPostalCode = request.ZipPostalCode,
            City = request.City,
            CountryId = request.CountryId,
            StateProvinceId = request.StateProvinceId
        };

        var result = await _shoppingCartModelFactory.PrepareEstimateShippingResultModelAsync(new[] { wrappedProduct }, model, false);

        return Ok(result);
    }

    /// <summary>
    /// Get product combinations
    /// </summary>
    /// <param name="productId">The product identifier</param>
    [HttpGet("{productId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(IList<ProductCombinationModel>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetProductCombinations(int productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(product)));

        var model = await _productModelFactory.PrepareProductCombinationModelsAsync(product);
        return Ok(model);
    }

    #endregion

    #region New (recently added) products page

    /// <summary>
    /// Get marked as new products
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CatalogProductsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetNewProducts([FromQuery] BasePageableRequest request)
    {
        if (!_catalogSettings.NewProductsEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var command = new CatalogProductsCommand
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
        };

        var model = await _catalogModelFactory.PrepareNewProductsModelAsync(command);

        return Ok(model);
    }

    #endregion

    #region Product reviews

    /// <summary>
    /// Get product reviews
    /// </summary>
    /// <param name="productId">The product identifier</param>
    [HttpGet("{productId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProductReviewsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetProductReviews(int productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(product)));

        var model = new ProductReviewsModel();
        model = await _productModelFactory.PrepareProductReviewsModelAsync(product);

        await ValidateProductReviewAvailabilityAsync(product);

        //default value
        model.AddProductReview.Rating = _catalogSettings.DefaultProductRatingValue;

        //default value for all additional review types
        if (model.ReviewTypeList.Count > 0)
            foreach (var additionalProductReview in model.AddAdditionalProductReviewList)
                additionalProductReview.Rating = additionalProductReview.IsRequired ? _catalogSettings.DefaultProductRatingValue : 0;

        return Ok(model);
    }

    /// <summary>
    /// Add a new product review
    /// </summary>
    [HttpPost("{productId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> AddProductReviews(int productId, AddProductReviewRequest request)
    {
        var product = await _productService.GetProductByIdAsync(productId);

        if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(product)));

        var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (!await _productReviewApiService.CanAddReviewAsync(product.Id, customer.Id, storeId))
            return BadRequest(new ErrorResponse { Error = "You already reviewed this product." });

        await ValidateProductReviewAvailabilityAsync(product);

        if (!ModelState.IsValid)
            return PrepareBadRequest(ModelState);

        var rating = request.Rating;
        if (rating < 1 || rating > 5)
            rating = _catalogSettings.DefaultProductRatingValue;

        var isApproved = !_catalogSettings.ProductReviewsMustBeApproved;

        var productReview = new ProductReview
        {
            ProductId = product.Id,
            CustomerId = customer.Id,
            Title = request.Title,
            ReviewText = request.ReviewText,
            Rating = rating,
            HelpfulYesTotal = 0,
            HelpfulNoTotal = 0,
            IsApproved = isApproved,
            CreatedOnUtc = DateTime.UtcNow,
            StoreId = storeId,
        };

        await _productReviewApiService.InsertProductReviewAsync(productReview);

        foreach (var additionalReview in request.AdditionalProductReviewList)
        {
            var reviewType = await _reviewTypeService.GetReviewTypeByIdAsync(additionalReview.ReviewTypeId);
            if (reviewType != null)
            {
                var additionalProductReview = new ProductReviewReviewTypeMapping
                {
                    ProductReviewId = productReview.Id,
                    ReviewTypeId = reviewType.Id,
                    Rating = additionalReview.Rating
                };

                await _reviewTypeService.InsertProductReviewReviewTypeMappingsAsync(additionalProductReview);
            }
        }

        await _productReviewApiService.UpdateProductReviewTotalsAsync(product);

        if (_catalogSettings.NotifyStoreOwnerAboutNewProductReviews)
            await _workflowMessageService.SendProductReviewStoreOwnerNotificationMessageAsync(productReview, _localizationSettings.DefaultAdminLanguageId);

        await _customerActivityService.InsertActivityAsync("PublicStore.AddProductReview",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddProductReview"), product.Name), product);

        if (productReview.IsApproved)
            await _eventPublisher.PublishAsync(new ProductReviewApprovedEvent(productReview));

        if (request.PrepareReviews)
        {
            var model = new ProductReviewsModel();
            model = await _productModelFactory.PrepareProductReviewsModelAsync(product);
            model.AddProductReview.Title = null;
            model.AddProductReview.ReviewText = null;

            return Ok(model);
        }

        return Ok(!isApproved
            ? await _localizationService.GetResourceAsync("Reviews.SeeAfterApproving")
            : await _localizationService.GetResourceAsync("Reviews.SuccessfullyAdded"));
    }

    /// <summary>
    /// Set product review helpfulness
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(SetProductReviewHelpfulnessResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> SetProductReviewHelpfulness(SetProductReviewHelpfulnessRequest request)
    {
        var productReview = await _productReviewApiService.GetProductReviewByIdAsync(request.ProductReviewId);
        if (productReview == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(productReview)));

        var customer = await _workContext.GetCurrentCustomerAsync();

        if (await _customerService.IsGuestAsync(customer) && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
            return Ok(new SetProductReviewHelpfulnessResponse
            {
                Result = await _localizationService.GetResourceAsync("Reviews.Helpfulness.OnlyRegistered"),
                TotalYes = productReview.HelpfulYesTotal,
                TotalNo = productReview.HelpfulNoTotal
            });

        if (productReview.CustomerId == customer.Id)
            return Ok(new SetProductReviewHelpfulnessResponse
            {
                Result = await _localizationService.GetResourceAsync("Reviews.Helpfulness.YourOwnReview"),
                TotalYes = productReview.HelpfulYesTotal,
                TotalNo = productReview.HelpfulNoTotal
            });

        // ✅ plugin service handles insert/update helpfulness row
        await _productReviewApiService.SetProductReviewHelpfulnessAsync(productReview, customer.Id, request.Washelpful);

        // ✅ plugin service recalculates totals
        await _productReviewApiService.UpdateProductReviewHelpfulnessTotalsAsync(productReview);

        return Ok(new SetProductReviewHelpfulnessResponse
        {
            Result = await _localizationService.GetResourceAsync("Reviews.Helpfulness.SuccessfullyVoted"),
            TotalYes = productReview.HelpfulYesTotal,
            TotalNo = productReview.HelpfulNoTotal
        });
    }


    /// <summary>
    /// Get customer product reviews
    /// </summary>
    /// <param name="pageNumber">Page number of the pager(optional)</param>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomerProductReviewsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetCustomerProductReviews(int? pageNumber)
    {
        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (!_catalogSettings.ShowProductReviewsTabOnAccountPage)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = await _productModelFactory.PrepareCustomerProductReviewsModelAsync(pageNumber);

        return Ok(model);
    }

    #endregion

    #region Email a friend

    /// <summary>
    /// Prepare product email a friend model
    /// </summary>
    /// <param name="productId">The product identifer</param>
    [HttpGet("{productId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProductEmailAFriendModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetProductEmailAFriend(int productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null || product.Deleted || !product.Published || !_catalogSettings.EmailAFriendEnabled)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(product)));

        var model = new ProductEmailAFriendModel();
        model = await _productModelFactory.PrepareProductEmailAFriendModelAsync(model, product, false);
        return Ok(model);
    }

    /// <summary>
    /// Email the product to a friend
    /// </summary>
    /// <param name="productId">The product identifer</param>
    [HttpPost("{productId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> ProductEmailAFriend(int productId, ProductEmailAFriendRequest request)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null || product.Deleted || !product.Published || !_catalogSettings.EmailAFriendEnabled)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(product)));

        //check whether the current customer is guest and ia allowed to email a friend
        if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_catalogSettings.AllowAnonymousUsersToEmailAFriend)
            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Products.EmailAFriend.OnlyRegisteredUsers"));

        if (ModelState.IsValid)
        {
            //email
            await _workflowMessageService.SendProductEmailAFriendMessageAsync(await _workContext.GetCurrentCustomerAsync(),
                    (await _workContext.GetWorkingLanguageAsync()).Id, product,
                    request.YourEmailAddress, request.FriendEmail,
                    _htmlFormatter.FormatText(request.PersonalMessage, false, true, false, false, false, false));

            return Ok(await _localizationService.GetResourceAsync("Products.EmailAFriend.SuccessfullySent"));
        }

        return PrepareBadRequest(ModelState);
    }

    #endregion

    #region Product tags

    /// <summary>
    /// Get popular product tags
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PopularProductTagsModel>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetPopularProductTags()
    {
        var model = await _catalogModelFactory.PreparePopularProductTagsModelAsync(_catalogSettings.NumberOfProductTags);
        return Ok(model);
    }

    /// <summary>
    /// Get products by tag
    /// </summary>
    /// <param name="productTagId">The product tag identifer</param>
    [HttpGet("{productTagId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(IEnumerable<ProductsByTagModel>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetTagProducts(int productTagId, [FromQuery] CatalogRequest request)
    {
        var productTag = await _productTagService.GetProductTagByIdAsync(productTagId);
        if (productTag == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(productTag)));

        var command = new CatalogProductsCommand();
        if (request != null)
            command = new CatalogProductsCommand
            {
                Price = !string.IsNullOrEmpty(request.Price) ? request.Price : string.Empty,
                Specs = request.SpecificationOptionIds != null ? request.SpecificationOptionIds : new List<int>(),
                Ms = request.ManufacturerIds != null ? request.ManufacturerIds : new List<int>(),
                OrderBy = request.OrderBy != null ? (int)request.OrderBy : (int)ProductSortingEnum.Position,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };

        var model = await _catalogModelFactory.PrepareProductsByTagModelAsync(productTag, command);

        return Ok(model);
    }

    #endregion

    #region Related products

    /// <summary>
    /// Get related products
    /// </summary>
    /// <param name="productId">The product identifer</param>
    /// <param name="productThumbPictureSize">Product thumbnail picture size(optional)</param>
    [HttpGet("{productId}")]
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
        return Ok(model);
    }

    #endregion

    #region Cross sell products

    /// <summary>
    /// Get cross sell products
    /// </summary>
    /// <param name="productThumbPictureSize">Product thumbnail picture size(optional)</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductOverviewModel>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetCrossSellProducts(int? productThumbPictureSize)
    {
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        var products = await (await _productService.GetCrossSellProductsByShoppingCartAsync(cart, _shoppingCartSettings.CrossSellsNumber))
        //ACL and store mapping
        .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
        //availability dates
        .Where(p => _productService.ProductIsAvailable(p))
        //visible individually
        .Where(p => p.VisibleIndividually).ToListAsync();

        var model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products,
                productThumbPictureSize: productThumbPictureSize, forceRedirectionAfterAddingToCart: true))
            .ToList();

        return Ok(model);
    }

    /// <summary>
    /// Get also purchased products
    /// </summary>
    /// <param name="productId">The product identifier</param>
    /// <param name="productThumbPictureSize">Product thumbnail picture size(optional)</param>
    [HttpGet("{productId}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IEnumerable<ProductOverviewModel>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetProductsAlsoPurchased(int productId, int? productThumbPictureSize)
    {
        if (!_catalogSettings.ProductsAlsoPurchasedEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        //load and cache report
        var productIds = await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductsAlsoPurchasedIdsKey, productId, await _storeContext.GetCurrentStoreAsync()),
            async () => await _orderReportService.GetAlsoPurchasedProductsIdsAsync((await _storeContext.GetCurrentStoreAsync()).Id, productId, _catalogSettings.ProductsAlsoPurchasedNumber)
        );

        //load products
        var products = await (await _productService.GetProductsByIdsAsync(productIds))
        //ACL and store mapping
        .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
        //availability dates
        .Where(p => _productService.ProductIsAvailable(p)).ToListAsync();

        var model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, productThumbPictureSize)).ToList();
        return Ok(model);
    }

    /// <summary>
    /// Prepare the product overview models
    /// </summary>
    /// <param name="productIds">Collection of product identifiers(separator - ;)</param>
    /// <param name="productThumbPictureSize">Product thumbnail picture size(optional)</param>
    [HttpGet("{productIds}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(IEnumerable<ProductOverviewModel>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetProductOverview(string productIds,
        bool preparePriceModel = true,
        bool preparePictureModel = true,
        int? productThumbPictureSize = null,
        bool prepareSpecificationAttributes = true)
    {
        if (string.IsNullOrEmpty(productIds))
            return BadRequest(new ErrorResponse { Error = string.Format(MessageDefaults.NOT_FOUND, nameof(Product)) });

        var productsId = productIds.ToIdArray();

        if (productsId.Length == 0)
            return BadRequest(new ErrorResponse { Error = string.Format(MessageDefaults.NOT_FOUND, nameof(Product)) });

        //load products
        var products = await (await _productService.GetProductsByIdsAsync(productsId))
        //ACL and store mapping
        .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
        //availability dates
        .Where(p => _productService.ProductIsAvailable(p))
        .Where(p => p.Published).ToListAsync();

        if (!products.Any())
            return BadRequest(new ErrorResponse { Error = string.Format(MessageDefaults.NOT_FOUND, nameof(Product)) });

        var model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products,
            preparePriceModel: preparePriceModel,
            preparePictureModel: preparePictureModel,
            productThumbPictureSize: productThumbPictureSize,
            prepareSpecificationAttributes: prepareSpecificationAttributes)).ToList();

        return Ok(model);
    }

    #endregion

    #endregion
}
