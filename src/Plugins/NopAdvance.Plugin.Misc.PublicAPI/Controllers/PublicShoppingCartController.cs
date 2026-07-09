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
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Models.ShoppingCart;
using NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;
using NopAdvance.Plugin.Misc.PublicAPI.Services;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers;

/// <summary>
/// Shipping cart methods
/// </summary>
public partial class PublicShoppingCartController : BaseAPIController
{
    #region Fields

    private readonly OrderSettings _orderSettings;
    private readonly CustomerSettings _customerSettings;
    private readonly ShoppingCartSettings _shoppingCartSettings;
    private readonly ShippingSettings _shippingSettings;
    private readonly IPermissionService _permissionService;
    private readonly IStoreContext _storeContext;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IWorkContext _workContext;
    private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
    private readonly ICustomerService _customerService;
    private readonly ILocalizationService _localizationService;
    private readonly IWorkflowMessageService _workflowMessageService;
    private readonly IProductService _productService;
    private readonly IProductAttributeParser _productAttributeParser;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly IDiscountService _discountService;
    private readonly IGiftCardService _giftCardService;
    private readonly IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeParser;
    private readonly IAttributeService<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeService;
    private readonly IDownloadService _downloadService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IProductAttributeService _productAttributeService;
    private readonly INopFileProvider _fileProvider;
    private readonly IShippingService _shippingService;
    private readonly IHtmlFormatter _htmlFormatter;
    protected readonly IStoreMappingService _storeMappingService;
    protected readonly IStaticCacheManager _staticCacheManager;
    private readonly IUrlHelperFactory _urlHelperFactory;

    #endregion

    #region Ctor

    public PublicShoppingCartController(OrderSettings orderSettings,
        CustomerSettings customerSettings,
        ShoppingCartSettings shoppingCartSettings,
        IPermissionService permissionService,
        IStoreContext storeContext,
        IShoppingCartService shoppingCartService,
        IWorkContext workContext,
        IShoppingCartModelFactory shoppingCartModelFactory,
        ICustomerService customerService,
        ILocalizationService localizationService,
        IWorkflowMessageService workflowMessageService,
        IProductService productService,
        IProductAttributeParser productAttributeParser,
        ICustomerActivityService customerActivityService,
        IDiscountService discountService,
        IGiftCardService giftCardService,
        IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
        IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService,
        IDownloadService downloadService,
        IGenericAttributeService genericAttributeService,
        IProductAttributeService productAttributeService,
        INopFileProvider fileProvider,
        ShippingSettings shippingSettings,
        IShippingService shippingService,
        IHtmlFormatter htmlFormatter,
        IStoreMappingService storeMappingService,
        IStaticCacheManager staticCacheManager, IUrlHelperFactory urlHelperFactory)
    {
        _orderSettings = orderSettings;
        _customerSettings = customerSettings;
        _shoppingCartSettings = shoppingCartSettings;
        _permissionService = permissionService;
        _storeContext = storeContext;
        _shoppingCartService = shoppingCartService;
        _workContext = workContext;
        _shoppingCartModelFactory = shoppingCartModelFactory;
        _customerService = customerService;
        _localizationService = localizationService;
        _workflowMessageService = workflowMessageService;
        _productService = productService;
        _productAttributeParser = productAttributeParser;
        _customerActivityService = customerActivityService;
        _discountService = discountService;
        _giftCardService = giftCardService;
        _checkoutAttributeParser = checkoutAttributeParser;
        _checkoutAttributeService = checkoutAttributeService;
        _downloadService = downloadService;
        _genericAttributeService = genericAttributeService;
        _productAttributeService = productAttributeService;
        _fileProvider = fileProvider;
        _shippingSettings = shippingSettings;
        _shippingService = shippingService;
        _htmlFormatter = htmlFormatter;
        _storeMappingService = storeMappingService;
        _staticCacheManager = staticCacheManager;
        _urlHelperFactory= urlHelperFactory;
    }

    #endregion

    #region Utilities

    protected virtual async Task ParseAndSaveCheckoutAttributesAsync(IList<ShoppingCartItem> cart, IFormCollection form)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        if (form == null)
            throw new ArgumentNullException(nameof(form));

        var attributesXml = string.Empty;
        var excludeShippableAttributes = !await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
        var checkoutAttributes = await _checkoutAttributeService.GetAllAttributesAsync(_staticCacheManager,_storeMappingService,(await _storeContext.GetCurrentStoreAsync()).Id, excludeShippableAttributes);
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

    protected virtual async Task SaveItemAsync(ShoppingCartItem updatecartitem, List<string> addToCartWarnings, Product product,
       ShoppingCartType cartType, string attributes, decimal customerEnteredPriceConverted, DateTime? rentalStartDate,
       DateTime? rentalEndDate, int quantity)
    {
        if (updatecartitem == null)
            //add to the cart
            addToCartWarnings.AddRange(await _shoppingCartService.AddToCartAsync(await _workContext.GetCurrentCustomerAsync(),
                product, cartType, (await _storeContext.GetCurrentStoreAsync()).Id,
                attributes, customerEnteredPriceConverted,
                rentalStartDate, rentalEndDate, quantity, true));
        else
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), updatecartitem.ShoppingCartType, (await _storeContext.GetCurrentStoreAsync()).Id);

            var otherCartItemWithSameParameters = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(
                cart, updatecartitem.ShoppingCartType, product, attributes, customerEnteredPriceConverted,
                rentalStartDate, rentalEndDate);
            if (otherCartItemWithSameParameters != null &&
                otherCartItemWithSameParameters.Id == updatecartitem.Id)
                //ensure it's some other shopping cart item
                otherCartItemWithSameParameters = null;
            //update existing item
            addToCartWarnings.AddRange(await _shoppingCartService.UpdateShoppingCartItemAsync(await _workContext.GetCurrentCustomerAsync(),
                updatecartitem.Id, attributes, customerEnteredPriceConverted,
                rentalStartDate, rentalEndDate, quantity + (otherCartItemWithSameParameters?.Quantity ?? 0), true));
            if (otherCartItemWithSameParameters != null && !addToCartWarnings.Any())
                //delete the same shopping cart item (the other one)
                await _shoppingCartService.DeleteShoppingCartItemAsync(otherCartItemWithSameParameters);
        }
    }

    #endregion

    #region Methods

    #region Shopping cart

    /// <summary>
    /// Get selected checkout attributes
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetSelectedCheckoutAttributes()
    {
        return Ok(await _shoppingCartModelFactory.FormatSelectedCheckoutAttributesAsync());
    }

    /// <summary>
    /// Change and apply checkout attributes
    /// </summary>
    /// <param name="checkoutAttributes">Checkout attributes (e.g. checkout_attribute_{attribute.Id})
    /// <para>{</para>
    /// <para><em><b>   "checkout_attribute_1": "Checkout attribute text",</b></em>     //checkout_attribute_1(where 1 is checkout attribute id), "Checkout attribute text" is value of attribute - <em>Control type is Textbox/Multiline text box</em></para>
    /// <para><em><b>   "checkout_attribute_2": "3",</b></em>       //checkout_attribute_2(Where 2 is checkout attribute id), "3" is checkout attribute value id</para>
    /// <para><em><b>   "checkout_attribute_3": "4,5",</b></em>       //checkout_attribute_3(Where 3 is checkout attribute id), "4,5" are checkout attribute value ids(it can be single or multiple) - <em>Control type is Checkbox</em></para>
    /// <para><em><b>   "checkout_attribute_14_day": "1",</b></em>       //checkout_attribute_14_day(Where 14 is checkout attribute id),"1" is date value - <em>Control type is Date picker</em></para>
    /// <para><em><b>   "checkout_attribute_14_month": "10",</b></em>       //checkout_attribute_14_month(Where 14 is checkout attribute id),"10" is month value - <em>Control type is Date picker</em></para>
    /// <para><em><b>   "checkout_attribute_14_year": "2001",</b></em>       //checkout_attribute_14_year(Where 14 is checkout attribute id),"2001" is year value - <em>Control type is Date picker</em></para>
    /// <para><em><b>   "checkout_attribute_5": "2BD9E2C1-700C-4E81-A019-1889B8A5C0D3"</b></em>       //checkout_attribute_5(Where 5 is checkout attribute id),"2BD9E2C1-700C-4E81-A019-1889B8A5C0D3" is download guid for uploaded file - <em>Control type is File upload</em></para>
    /// <para>}</para>
    /// </param>
    [HttpPost]
    [ProducesResponseType(typeof(ChangeCheckoutAttributeResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> ChangeCheckoutAttribute(IDictionary<string, string> checkoutAttributes)
    {
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        //save selected attributes
        var form = new FormCollection(ConvertToFormCollection(checkoutAttributes));
        await ParseAndSaveCheckoutAttributesAsync(cart, form);
        var attributeXml = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
            NopCustomerDefaults.CheckoutAttributes, (await _storeContext.GetCurrentStoreAsync()).Id);

        //conditions
        var enabledAttributeIds = new List<int>();
        var disabledAttributeIds = new List<int>();
        var excludeShippableAttributes = !await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
        var attributes = await _checkoutAttributeService.GetAllAttributesAsync(_staticCacheManager, _storeMappingService, (await _storeContext.GetCurrentStoreAsync()).Id, excludeShippableAttributes);
        foreach (var attribute in attributes)
        {
            var conditionMet = await _checkoutAttributeParser.IsConditionMetAsync(attribute.ConditionAttributeXml, attributeXml);
            if (conditionMet.HasValue)
                if (conditionMet.Value)
                    enabledAttributeIds.Add(attribute.Id);
                else
                    disabledAttributeIds.Add(attribute.Id);
        }

        return Ok(new ChangeCheckoutAttributeResponse
        {
            OrderTotals = await _shoppingCartModelFactory.PrepareOrderTotalsModelAsync(cart, false),
            SelectedAttributes = await _shoppingCartModelFactory.FormatSelectedCheckoutAttributesAsync(),
            EnabledAttributeIds = enabledAttributeIds,
            DisabledAttributeIds = disabledAttributeIds
        });
    }

    /// <summary>
    /// Upload product attribute file
    /// </summary>
    /// <param name="attributeId">The product attribute identifier</param>
    [HttpPost("{attributeId}")]
    [ProducesResponseType(typeof(UploadFileResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> UploadFileProductAttribute(int attributeId, UploadFileRequest request)
    {
        var attribute = await _productAttributeService.GetProductAttributeMappingByIdAsync(attributeId);
        if (attribute == null || attribute.AttributeControlType != AttributeControlType.FileUpload)
            return Ok(new UploadFileResponse { UploadedFileGuid = Guid.Empty });

        var (fileBinary, contentType, _) = PluginCommonHelper.ConvertBase64ToFile(request.FileBase64String, request.FileName);

        if (fileBinary == null)
            return Ok(new UploadFileResponse
            {
                Message = "No file uploaded",
                UploadedFileGuid = Guid.Empty
            });

        var fileName = request.FileName;

        var fileExtension = _fileProvider.GetFileExtension(fileName);
        if (!string.IsNullOrEmpty(fileExtension))
            fileExtension = fileExtension.ToLowerInvariant();

        if (attribute.ValidationFileMaximumSize.HasValue)
        {
            //compare in bytes
            var maxFileSizeBytes = attribute.ValidationFileMaximumSize.Value * 1024;
            if (fileBinary.Length > maxFileSizeBytes)
                return Ok(new UploadFileResponse
                {
                    Message = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumUploadedFileSize"), attribute.ValidationFileMaximumSize.Value),
                    UploadedFileGuid = Guid.Empty
                });
        }

        var download = new Download
        {
            DownloadGuid = Guid.NewGuid(),
            UseDownloadUrl = false,
            DownloadUrl = string.Empty,
            DownloadBinary = fileBinary,
            ContentType = contentType,
            //we store filename without extension for downloads
            Filename = _fileProvider.GetFileNameWithoutExtension(fileName),
            Extension = fileExtension,
            IsNew = true
        };
        await _downloadService.InsertDownloadAsync(download);

        return Ok(new UploadFileResponse
        {
            Success = true,
            Message = await _localizationService.GetResourceAsync("ShoppingCart.FileUploaded"),
            UploadedFileGuid = download.DownloadGuid
        });
    }

    /// <summary>
    /// Upload checkout attribute file
    /// </summary>
    /// <param name="attributeId">The attribute identifier</param>
    /// <returns></returns>
    [HttpPost("{attributeId}")]
    [ProducesResponseType(typeof(UploadFileResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> UploadFileCheckoutAttribute(int attributeId, UploadFileRequest request)
    {
        var attribute = await _checkoutAttributeService.GetAttributeByIdAsync(attributeId);
        if (attribute == null || attribute.AttributeControlType != AttributeControlType.FileUpload)
            return Ok(new UploadFileResponse { UploadedFileGuid = Guid.Empty });

        var (fileBinary, contentType, _) = PluginCommonHelper.ConvertBase64ToFile(request.FileBase64String, request.FileName);
        if (fileBinary == null)
            return Ok(new UploadFileResponse
            {
                Message = "No file uploaded",
                UploadedFileGuid = Guid.Empty
            });

        var fileName = request.FileName;

        var fileExtension = _fileProvider.GetFileExtension(fileName);
        if (!string.IsNullOrEmpty(fileExtension))
            fileExtension = fileExtension.ToLowerInvariant();

        if (attribute.ValidationFileMaximumSize.HasValue)
        {
            //compare in bytes
            var maxFileSizeBytes = attribute.ValidationFileMaximumSize.Value * 1024;
            if (fileBinary.Length > maxFileSizeBytes)
                return Ok(new UploadFileResponse
                {
                    Message = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumUploadedFileSize"), attribute.ValidationFileMaximumSize.Value),
                    UploadedFileGuid = Guid.Empty
                });
        }

        var download = new Download
        {
            DownloadGuid = Guid.NewGuid(),
            UseDownloadUrl = false,
            DownloadUrl = string.Empty,
            DownloadBinary = fileBinary,
            ContentType = contentType,
            //we store filename without extension for downloads
            Filename = _fileProvider.GetFileNameWithoutExtension(fileName),
            Extension = fileExtension,
            IsNew = true
        };
        await _downloadService.InsertDownloadAsync(download);

        return Ok(new UploadFileResponse
        {
            Success = true,
            Message = await _localizationService.GetResourceAsync("ShoppingCart.FileUploaded"),
            UploadedFileGuid = download.DownloadGuid
        });
    }

    /// <summary>
    /// Get customer cart
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ShoppingCartModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetCart()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_SHOPPING_CART))
            return Unauthorized();

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
        var model = new ShoppingCartModel();
        model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);
        return Ok(model);
    }

    /// <summary>
    /// Delete cart items
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(typeof(ShoppingCartModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> DeleteCartItems(DeleteCartItemsRequest request)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_SHOPPING_CART))
            return Unauthorized();

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        foreach (var sci in cart)
        {
            var remove = request.CartItemIds.Contains(sci.Id);
            if (remove)
                await _shoppingCartService.DeleteShoppingCartItemAsync(sci);
        }

        if (request.PrepareCart)
        {
            //updated cart
            cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            var model = new ShoppingCartModel();
            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);
            return Ok(model);
        }

        return Ok();
    }

    /// <summary>
    /// Update quantities of the cart items
    /// </summary>
    /// <param name="prepareCart">Prepare cart model?
    /// </param>
    /// <param name="request">Update cart items quantity
    /// <para>[</para>
    /// <para>{</para>
    /// <para><em><b> "CartItemId": 1, </b></em></para>     
    /// <para><em><b> "Quantity": 3 </b></em></para>
    /// <para>},</para>
    /// <para>{</para>
    /// <para><em><b> "CartItemId": 2, </b></em></para>     
    /// <para><em><b> "Quantity": 4 </b></em></para>
    /// <para>}</para>
    /// <para>]</para>
    /// </param>
    [HttpPut]
    [ProducesResponseType(typeof(ShoppingCartModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> UpdateCartItemsQuantity([FromBody] IList<UpdateCartRequest> request, bool prepareCart = false)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_SHOPPING_CART))
            return Unauthorized();

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        var products = (await _productService.GetProductsByIdsAsync(cart.Select(item => item.ProductId).Distinct().ToArray()))
           .ToDictionary(item => item.Id, item => item);

        //get order items with changed quantity
        var itemsWithNewQuantity = cart.Select(item => new
        {
            //try to get a new quantity for the item, set 0 for items to remove
            NewQuantity = request.Where(r => r.CartItemId == item.Id).FirstOrDefault() != null ? request.Where(r => r.CartItemId == item.Id).FirstOrDefault().Quantity : item.Quantity,
            Item = item,
            Product = products.ContainsKey(item.ProductId) ? products[item.ProductId] : null
        }).Where(item => item.NewQuantity != item.Item.Quantity);

        //order cart items
        //first should be items with a reduced quantity and that require other products; or items with an increased quantity and are required for other products
        var orderedCart = await itemsWithNewQuantity
            .OrderByDescendingAwait(async cartItem =>
                cartItem.NewQuantity < cartItem.Item.Quantity &&
                 (cartItem.Product?.RequireOtherProducts ?? false) ||
                cartItem.NewQuantity > cartItem.Item.Quantity && cartItem.Product != null && (await _shoppingCartService
                     .GetProductsRequiringProductAsync(cart, cartItem.Product)).Any())
            .ToListAsync();

        //try to update cart items with new quantities and get warnings
        var warnings = await orderedCart.SelectAwait(async cartItem => new
        {
            ItemId = cartItem.Item.Id,
            Warnings = await _shoppingCartService.UpdateShoppingCartItemAsync(await _workContext.GetCurrentCustomerAsync(),
                cartItem.Item.Id, cartItem.Item.AttributesXml, cartItem.Item.CustomerEnteredPrice,
                cartItem.Item.RentalStartDateUtc, cartItem.Item.RentalEndDateUtc, cartItem.NewQuantity, true)
        }).ToListAsync();

        if (prepareCart)
        {
            //updated cart
            cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            var model = new ShoppingCartModel();
            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);

            //update current warnings
            foreach (var warningItem in warnings.Where(warningItem => warningItem.Warnings.Any()))
            {
                //find shopping cart item model to display appropriate warnings
                var itemModel = model.Items.FirstOrDefault(item => item.Id == warningItem.ItemId);
                if (itemModel != null)
                    itemModel.Warnings = warningItem.Warnings.Concat(itemModel.Warnings).Distinct().ToList();
            }

            return Ok(model);
        }

        return Ok();
    }

    /// <summary>
    /// Add product to the cart
    /// </summary>
    /// <param name="productId">The product identifier</param>
    [HttpPost("{productId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> AddToCart(int productId, AddToCartRequest request)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(product)));

        //we can add only simple products
        if (product.ProductType != ProductType.SimpleProduct)
            return BadRequest("Only simple products could be added to the cart");

        ShoppingCartItem updatecartitem = null;
        if (_shoppingCartSettings.AllowCartItemEditing && request.UpdateCartItemId > 0)
        {
            //search with the same cart type as specified
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            updatecartitem = cart.FirstOrDefault(x => x.Id == request.UpdateCartItemId);
            //is it this product?
            if (updatecartitem != null && product.Id != updatecartitem.ProductId)
                return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(updatecartitem)));
        }

        var addToCartWarnings = new List<string>();

        //customer entered price
        var form = new FormCollection(ConvertToFormCollection(request.Attributes));
        var customerEnteredPriceConverted = await _productAttributeParser.ParseCustomerEnteredPriceAsync(product, form);

        //product and gift card attributes
        var attributes = await _productAttributeParser.ParseProductAttributesAsync(product, form, addToCartWarnings);

        //rental attributes
        _productAttributeParser.ParseRentalDates(product, form, out var rentalStartDate, out var rentalEndDate);

        await SaveItemAsync(updatecartitem, addToCartWarnings, product, ShoppingCartType.ShoppingCart, attributes, customerEnteredPriceConverted, rentalStartDate, rentalEndDate, request.Quantity);
        if (addToCartWarnings.Any())
            return BadRequest(addToCartWarnings);

        //activity log
        await _customerActivityService.InsertActivityAsync("PublicStore.AddToShoppingCart",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToShoppingCart"), product.Name), product);

        return Ok();
    }

    /// <summary>
    /// Apply discount coupon
    /// </summary>
    /// <param name="discountCouponCode">Discount coupon code</param>
    /// <param name="prepareOrderTotals">Prepare the order totals model?</param>
    [HttpPost("{discountCouponCode}")]
    [ProducesResponseType(typeof(ApplyDiscountCouponResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> ApplyDiscountCoupon(string discountCouponCode, bool prepareOrderTotals = false)
    {
        //trim
        var discountcouponcode = discountCouponCode;
        if (discountcouponcode != null)
            discountcouponcode = discountcouponcode.Trim();

        var model = new ApplyDiscountCouponResponse();
        model.DiscountBox.Display = _shoppingCartSettings.ShowDiscountBox;
        if (!string.IsNullOrWhiteSpace(discountcouponcode))
        {
            //we find even hidden records here. this way we can display a user-friendly message if it's expired
            var discounts = (await _discountService.GetAllDiscountsAsync(couponCode: discountcouponcode, showHidden: true))
                .Where(d => d.RequiresCouponCode)
                .ToList();
            if (discounts.Any())
            {
                var userErrors = new List<string>();
                var anyValidDiscount = await discounts.AnyAwaitAsync(async discount =>
                {
                    var validationResult = await _discountService.ValidateDiscountAsync(discount, await _workContext.GetCurrentCustomerAsync(), new[] { discountcouponcode });
                    userErrors.AddRange(validationResult.Errors);

                    return validationResult.IsValid;
                });

                if (anyValidDiscount)
                {
                    //valid
                    await _customerService.ApplyDiscountCouponCodeAsync(await _workContext.GetCurrentCustomerAsync(), discountcouponcode);
                    model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.Applied"));
                    model.DiscountBox.IsApplied = true;
                }
                else
                    if (userErrors.Any())
                    //some user errors
                    model.DiscountBox.Messages = userErrors;
                else
                    //general error text
                    model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.WrongDiscount"));
            }
            else
                //discount cannot be found
                model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.CannotBeFound"));
        }
        else
            //empty coupon code
            model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.Empty"));

        //cart
        if (prepareOrderTotals)
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            model.OrderTotals = await _shoppingCartModelFactory.PrepareOrderTotalsModelAsync(cart, false);
        }

        return Ok(model);
    }

    /// <summary>
    /// Apply gift card coupon
    /// </summary>
    /// <param name="giftCardCouponCode">Gift card coupon code</param>
    /// <param name="prepareOrderTotals">Prepare the order totals model?</param>
    [HttpPost("{giftCardCouponCode}")]
    [ProducesResponseType(typeof(ApplyGiftCardResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> ApplyGiftCard(string giftCardCouponCode, bool prepareOrderTotals = false)
    {
        var giftcardcouponcode = giftCardCouponCode;
        //trim
        if (giftcardcouponcode != null)
            giftcardcouponcode = giftcardcouponcode.Trim();

        var model = new ApplyGiftCardResponse();
        model.GiftCardBox.Display = _shoppingCartSettings.ShowGiftCardBox;
        //cart
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (!await _shoppingCartService.ShoppingCartIsRecurringAsync(cart))
            if (!string.IsNullOrWhiteSpace(giftcardcouponcode))
            {
                var giftCard = (await _giftCardService.GetAllGiftCardsAsync(giftCardCouponCode: giftcardcouponcode)).FirstOrDefault();
                var isGiftCardValid = giftCard != null && await _giftCardService.IsGiftCardValidAsync(giftCard);
                if (isGiftCardValid)
                {
                    await _customerService.ApplyGiftCardCouponCodeAsync(await _workContext.GetCurrentCustomerAsync(), giftcardcouponcode);
                    model.GiftCardBox.Message = await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.Applied");
                    model.GiftCardBox.IsApplied = true;
                }
                else
                {
                    model.GiftCardBox.Message = await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                    model.GiftCardBox.IsApplied = false;
                }
            }
            else
            {
                model.GiftCardBox.Message = await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                model.GiftCardBox.IsApplied = false;
            }
        else
        {
            model.GiftCardBox.Message = await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.DontWorkWithAutoshipProducts");
            model.GiftCardBox.IsApplied = false;
        }

        if (prepareOrderTotals)
            model.OrderTotals = await _shoppingCartModelFactory.PrepareOrderTotalsModelAsync(cart, false);
        return Ok(model);
    }

    /// <summary>
    /// Prepare estimate shipping model
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(EstimateShippingModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetEstimateShipping()
    {
        if (!_shippingSettings.EstimateShippingCartPageEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        var model = await _shoppingCartModelFactory.PrepareEstimateShippingModelAsync(cart);
        if (!model.Enabled)
            return NoContent();

        return Ok(model);
    }

    /// <summary>
    /// Estimate shipping
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EstimateShippingResultModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> EstimateShipping(EstimateShippingRequest request)
    {
        var model = new EstimateShippingModel
        {
            ZipPostalCode = request.ZipPostalCode,
            City = request.City,
            CountryId = request.CountryId,
            StateProvinceId = request.StateProvinceId
        };

        var errors = new List<string>();

        if (!_shippingSettings.EstimateShippingCityNameEnabled && string.IsNullOrEmpty(model.ZipPostalCode))
            errors.Add(await _localizationService.GetResourceAsync("Shipping.EstimateShipping.ZipPostalCode.Required"));

        if (_shippingSettings.EstimateShippingCityNameEnabled && string.IsNullOrEmpty(model.City))
            errors.Add(await _localizationService.GetResourceAsync("Shipping.EstimateShipping.City.Required"));

        if (model.CountryId == null || model.CountryId == 0)
            errors.Add(await _localizationService.GetResourceAsync("Shipping.EstimateShipping.Country.Required"));

        if (errors.Count > 0)
            return BadRequest(errors);

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        var result = await _shoppingCartModelFactory.PrepareEstimateShippingResultModelAsync(cart, model, true);

        return Ok(result);
    }

    /// <summary>
    /// Select shipping option
    /// </summary>
    /// <param name="shippingOptionName">Shipping option name</param>
    [HttpPost("{shippingOptionName}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(OrderTotalsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> SelectShippingOption(string shippingOptionName, SelectShippingOptionRequest request)
    {
        var errors = new List<string>();
        if (string.IsNullOrEmpty(request.ZipPostalCode))
            errors.Add(await _localizationService.GetResourceAsync("Shipping.EstimateShipping.ZipPostalCode.Required"));

        if (request.CountryId == null || request.CountryId == 0)
            errors.Add(await _localizationService.GetResourceAsync("Shipping.EstimateShipping.Country.Required"));

        if (errors.Count > 0)
            return BadRequest(errors);

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        var shippingOptions = new List<ShippingOption>();
        ShippingOption selectedShippingOption = null;

        if (!string.IsNullOrWhiteSpace(shippingOptionName))
        {
            //find shipping options
            //performance optimization. try cache first
            shippingOptions = await _genericAttributeService.GetAttributeAsync<List<ShippingOption>>(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.OfferedShippingOptionsAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (shippingOptions == null || !shippingOptions.Any())
            {
                var address = new Address
                {
                    CountryId = request.CountryId,
                    StateProvinceId = request.StateProvinceId,
                    ZipPostalCode = request.ZipPostalCode,
                };

                //not found? let's load them using shipping service
                var getShippingOptionResponse = await _shippingService.GetShippingOptionsAsync(cart, address,
                    await _workContext.GetCurrentCustomerAsync(), storeId: (await _storeContext.GetCurrentStoreAsync()).Id);

                if (getShippingOptionResponse.Success)
                    shippingOptions = getShippingOptionResponse.ShippingOptions.ToList();
                else
                    foreach (var error in getShippingOptionResponse.Errors)
                        errors.Add(error);
            }
        }

        selectedShippingOption = shippingOptions.Find(so => !string.IsNullOrEmpty(so.Name) && so.Name.Equals(shippingOptionName, StringComparison.InvariantCultureIgnoreCase));
        if (selectedShippingOption == null)
            errors.Add(await _localizationService.GetResourceAsync("Shipping.EstimateShippingPopUp.ShippingOption.IsNotFound"));

        if (errors.Count > 0)
            return BadRequest(errors);

        //reset pickup point
        await _genericAttributeService.SaveAttributeAsync<PickupPoint>(await _workContext.GetCurrentCustomerAsync(),
            NopCustomerDefaults.SelectedPickupPointAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);

        //cache shipping option
        await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
            NopCustomerDefaults.SelectedShippingOptionAttribute, selectedShippingOption, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (request.PrepareOrderTotals)
        {
            var model = await _shoppingCartModelFactory.PrepareOrderTotalsModelAsync(cart, true);
            return Ok(model);
        }

        return Ok();
    }

    /// <summary>
    /// Remove discount coupon
    /// </summary>
    /// <param name="discountId">The discount identifier</param>
    /// <param name="prepareOrderTotals">Prepare the order totals model?</param>
    [HttpPost("{discountId}")]
    [ProducesResponseType(typeof(OrderTotalsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> RemoveDiscountCoupon(int discountId, bool prepareOrderTotals = false)
    {
        var discount = await _discountService.GetDiscountByIdAsync(discountId);
        if (discount != null)
            await _customerService.RemoveDiscountCouponCodeAsync(await _workContext.GetCurrentCustomerAsync(), discount.CouponCode);

        if (prepareOrderTotals)
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            var model = await _shoppingCartModelFactory.PrepareOrderTotalsModelAsync(cart, false);
            return Ok(model);
        }
        return Ok();
    }

    /// <summary>
    /// Remove gift card coupon
    /// </summary>
    /// <param name="giftCardId">The gift card identifier</param>
    /// <param name="prepareOrderTotals">Prepare the order totals model?</param>
    [HttpPost("{giftCardId}")]
    [ProducesResponseType(typeof(OrderTotalsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> RemoveGiftCardCode(int giftCardId, bool prepareOrderTotals = false)
    {
        var gc = await _giftCardService.GetGiftCardByIdAsync(giftCardId);
        if (gc != null)
            await _customerService.RemoveGiftCardCouponCodeAsync(await _workContext.GetCurrentCustomerAsync(), gc.GiftCardCouponCode);

        if (prepareOrderTotals)
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            var model = await _shoppingCartModelFactory.PrepareOrderTotalsModelAsync(cart, false);
            return Ok(model);
        }
        return Ok();
    }

    /// <summary>
    /// Get order totals
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(OrderTotalsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetOrderTotals()
    {
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        var model = await _shoppingCartModelFactory.PrepareOrderTotalsModelAsync(cart, false);
        return Ok(model);
    }

    /// <summary>
    /// Start checkout (to validate cart and go on checkout page)
    /// </summary>
    /// <param name="attributes">
    /// <para>{</para>
    /// <para><em><b>   "checkout_attribute_1": "Checkout attribute text",</b></em>     //checkout_attribute_1(where 1 is checkout attribute id), "Checkout attribute text" is value of attribute - <em>Control type is Textbox/Multiline text box</em></para>
    /// <para><em><b>   "checkout_attribute_2": "3",</b></em>       //checkout_attribute_2(Where 2 is checkout attribute id), "3" is checkout attribute value id</para>
    /// <para><em><b>   "checkout_attribute_3": "4,5",</b></em>       //checkout_attribute_3(Where 3 is checkout attribute id), "4,5" are checkout attribute value ids(it can be single or multiple) - <em>Control type is Checkbox</em></para>
    /// <para><em><b>   "checkout_attribute_14_day": "1",</b></em>       //checkout_attribute_14_day(Where 14 is checkout attribute id),"1" is date value - <em>Control type is Date picker</em></para>
    /// <para><em><b>   "checkout_attribute_14_month": "10",</b></em>       //checkout_attribute_14_month(Where 14 is checkout attribute id),"10" is month value - <em>Control type is Date picker</em></para>
    /// <para><em><b>   "checkout_attribute_14_year": "2001",</b></em>       //checkout_attribute_14_year(Where 14 is checkout attribute id),"2001" is year value - <em>Control type is Date picker</em></para>
    /// <para><em><b>   "checkout_attribute_5": "2BD9E2C1-700C-4E81-A019-1889B8A5C0D3"</b></em>       //checkout_attribute_5(Where 5 is checkout attribute id),"2BD9E2C1-700C-4E81-A019-1889B8A5C0D3" is download guid for uploaded file - <em>Control type is File upload</em></para>
    /// <para>}</para>
    /// </param>
    [HttpPost]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> StartCheckout(IDictionary<string, string> attributes)
    {
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        //parse and save checkout attributes
        var form = new FormCollection(ConvertToFormCollection(attributes));
        await ParseAndSaveCheckoutAttributesAsync(cart, form);

        //validate attributes
        var checkoutAttributes = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
            NopCustomerDefaults.CheckoutAttributes, (await _storeContext.GetCurrentStoreAsync()).Id);
        var checkoutAttributeWarnings = await _shoppingCartService.GetShoppingCartWarningsAsync(cart, checkoutAttributes, true);
        if (checkoutAttributeWarnings.Any())
            return BadRequest(checkoutAttributeWarnings);

        var anonymousPermissed = _orderSettings.AnonymousCheckoutAllowed
                                 && _customerSettings.UserRegistrationType == UserRegistrationType.Disabled;

        if (anonymousPermissed || !await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()))
            return Ok();

        var cartProductIds = cart.Select(ci => ci.ProductId).ToArray();
        var downloadableProductsRequireRegistration =
            _customerSettings.RequireRegistrationForDownloadableProducts && await _productService.HasAnyDownloadableProductAsync(cartProductIds);

        if (!_orderSettings.AnonymousCheckoutAllowed || downloadableProductsRequireRegistration)
            //verify user identity (it may be facebook login page, or google, or local)
            return BadRequest("Please login");

        //reset checkout data
        await _customerService.ResetCheckoutDataAsync(await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);

        return Ok();
    }

    #endregion

    #region Wishlist

    /// <summary>
    /// Get customer wish list
    /// </summary>
    /// <param name="customerGuid">The customer guid identifier (optional)</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(WishlistModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetWishlist(Guid? customerGuid)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_WISHLIST))
            return Unauthorized();

        var customer = customerGuid.HasValue ?
            await _customerService.GetCustomerByGuidAsync(customerGuid.Value)
            : await _workContext.GetCurrentCustomerAsync();
        if (customer == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(customer)));

        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

        var model = new WishlistModel();
        model = await _shoppingCartModelFactory.PrepareWishlistModelAsync(model, cart, !customerGuid.HasValue);
        return Ok(model);
    }

    /// <summary>
    /// Add product to wish list
    /// </summary>
    /// <param name="productId">The product identifier</param>
    [HttpPost("{productId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> AddToWishlist(int productId, AddToCartRequest request)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(product)));

        //we can add only simple products
        if (product.ProductType != ProductType.SimpleProduct)
            return BadRequest("Only simple products could be added to the wishlist");

        ShoppingCartItem updatecartitem = null;
        if (_shoppingCartSettings.AllowCartItemEditing && request.UpdateCartItemId > 0)
        {
            //search with the same cart type as specified
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

            updatecartitem = cart.FirstOrDefault(x => x.Id == request.UpdateCartItemId);
            //is it this product?
            if (updatecartitem != null && product.Id != updatecartitem.ProductId)
                return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(updatecartitem)));
        }

        var addToCartWarnings = new List<string>();

        //customer entered price
        var form = new FormCollection(ConvertToFormCollection(request.Attributes));
        var customerEnteredPriceConverted = await _productAttributeParser.ParseCustomerEnteredPriceAsync(product, form);

        //product and gift card attributes
        var attributes = await _productAttributeParser.ParseProductAttributesAsync(product, form, addToCartWarnings);

        //rental attributes
        _productAttributeParser.ParseRentalDates(product, form, out var rentalStartDate, out var rentalEndDate);

        await SaveItemAsync(updatecartitem, addToCartWarnings, product, ShoppingCartType.Wishlist, attributes, customerEnteredPriceConverted, rentalStartDate, rentalEndDate, request.Quantity);
        if (addToCartWarnings.Any())
            return BadRequest(addToCartWarnings);

        //activity log
        await _customerActivityService.InsertActivityAsync("PublicStore.AddToWishlist",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToWishlist"), product.Name), product);

        return Ok();
    }

    /// <summary>
    /// Delete wish list items
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(typeof(WishlistModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> DeleteWishlistItems(DeleteWishlistItemsRequest request)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_WISHLIST))
            return Unauthorized();

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

        foreach (var sci in cart)
        {
            var remove = request.WishListItemIds.Contains(sci.Id);
            if (remove)
                await _shoppingCartService.DeleteShoppingCartItemAsync(sci);
        }

        if (request.PrepareWishlist)
        {
            //updated wishlist
            cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);
            var model = new WishlistModel();
            model = await _shoppingCartModelFactory.PrepareWishlistModelAsync(model, cart);
            return Ok(model);
        }

        return Ok();
    }

    /// <summary>
    /// Update quantities of wish list items
    /// </summary>
    /// <param name="prepareWishlist">Prepare wish list model?</param>
    /// <param name="request">Update wishlist items quantity
    /// <para>[</para>
    /// <para>{</para>
    /// <para><em><b> "WishListItemId": 1, </b></em></para>     
    /// <para><em><b> "Quantity": 3 </b></em></para>
    /// <para>},</para>
    /// <para>{</para>
    /// <para><em><b> "WishListItemId": 2, </b></em></para>     
    /// <para><em><b> "Quantity": 4 </b></em></para>
    /// <para>}</para>
    /// <para>]</para>
    /// </param>
    [HttpPut]
    [ProducesResponseType(typeof(WishlistModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> UpdateWishlistItemsQuantity([FromBody] IList<UpdateWishlistRequest> request, bool prepareWishlist = false)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_WISHLIST))
            return Unauthorized();

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

        foreach (var item in request)
        {
            var sci = cart.Where(c => c.Id == item.WishListItemId).FirstOrDefault();
            if (sci != null)
                await _shoppingCartService.UpdateShoppingCartItemAsync(await _workContext.GetCurrentCustomerAsync(),
                            sci.Id, sci.AttributesXml, sci.CustomerEnteredPrice,
                            sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                            item.Quantity, true);
        }

        if (prepareWishlist)
        {
            //updated wishlist
            cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);
            var model = new WishlistModel();
            model = await _shoppingCartModelFactory.PrepareWishlistModelAsync(model, cart);
            return Ok(model);
        }

        return Ok();
    }

    /// <summary>
    /// Move wish list items to cart
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> MoveWishlistItemsToCart(MoveWishlistItemsToCartRequest request)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_SHOPPING_CART))
            return Unauthorized();

        if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_WISHLIST))
            return Unauthorized();

        var pageCustomer = request.CustomerGuid.HasValue
            ? await _customerService.GetCustomerByGuidAsync(request.CustomerGuid.Value)
            : await _workContext.GetCurrentCustomerAsync();
        if (pageCustomer == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(pageCustomer)));

        var pageCart = await _shoppingCartService.GetShoppingCartAsync(pageCustomer, ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

        var allWarnings = new List<string>();
        var countOfAddedItems = 0;
        foreach (var sci in pageCart)
            if (request.WishListItemIds.Contains(sci.Id))
            {
                var product = await _productService.GetProductByIdAsync(sci.ProductId);

                var warnings = await _shoppingCartService.AddToCartAsync(await _workContext.GetCurrentCustomerAsync(),
                    product, ShoppingCartType.ShoppingCart,
                    (await _storeContext.GetCurrentStoreAsync()).Id,
                    sci.AttributesXml, sci.CustomerEnteredPrice,
                    sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, true);
                if (!warnings.Any())
                    countOfAddedItems++;
                if (_shoppingCartSettings.MoveItemsFromWishlistToCart && //settings enabled
                    !request.CustomerGuid.HasValue && //own wishlist
                    !warnings.Any()) //no warnings ( already in the cart)
                    //let's remove the item from wishlist
                    await _shoppingCartService.DeleteShoppingCartItemAsync(sci);

                allWarnings.AddRange(warnings);
            }

        if (countOfAddedItems > 0)
        {
            //redirect to the shopping cart page

            if (allWarnings.Any())
                return BadRequest(await _localizationService.GetResourceAsync("Wishlist.AddToCart.Error"));

            return Ok();
        }
        else
            allWarnings.Add(await _localizationService.GetResourceAsync("Wishlist.AddToCart.NoAddedItems"));
        //no items added. redisplay the wishlist page

        if (allWarnings.Any())
            return BadRequest(await _localizationService.GetResourceAsync("Wishlist.AddToCart.Error"));
        return Ok();
    }

    /// <summary>
    /// Prepare email wish list model
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(WishlistEmailAFriendModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetEmailWishlist()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_WISHLIST) || !_shoppingCartSettings.EmailWishlistEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (!cart.Any())
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "cartItems"));

        var model = new WishlistEmailAFriendModel();
        model = await _shoppingCartModelFactory.PrepareWishlistEmailAFriendModelAsync(model, false);
        return Ok(model);
    }

    /// <summary>
    /// Send wish list link on email
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> EmailWishlist(EmailWishlistRequest request)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_WISHLIST) || !_shoppingCartSettings.EmailWishlistEnabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist, store.Id);
        if (!cart.Any())
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, "cartItems"));

        //check whether the current customer is guest and is allowed to email wishlist
        if (await _customerService.IsGuestAsync(customer) && !_shoppingCartSettings.AllowAnonymousUsersToEmailWishlist)
            ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Wishlist.EmailAFriend.OnlyRegisteredUsers"));

        if (ModelState.IsValid)
        {
            // ✅ Build wishlist URL required by nop 4.9 signature
            // This generates: /wishlist/{customerGuid}
            var urlHelper = _urlHelperFactory.GetUrlHelper(ControllerContext);

            var wishlistUrl = urlHelper.RouteUrl("Wishlist", new { customerGuid = customer.CustomerGuid }, protocol: Request.Scheme);

            // Fallback (in case route isn't available in your setup)
            if (string.IsNullOrWhiteSpace(wishlistUrl))
                wishlistUrl = $"{store.Url.TrimEnd('/')}/wishlist/{customer.CustomerGuid}";

            //email (✅ now 6 parameters)
            await _workflowMessageService.SendWishlistEmailAFriendMessageAsync(
                customer,
                (await _workContext.GetWorkingLanguageAsync()).Id,
                request.YourEmailAddress,
                request.FriendEmail,
                _htmlFormatter.FormatText(request.PersonalMessage, false, true, false, false, false, false),
                wishlistUrl
            );

            return Ok(await _localizationService.GetResourceAsync("Wishlist.EmailAFriend.SuccessfullySent"));
        }

        return PrepareBadRequest(ModelState);
    }


    #endregion

    #endregion
}
