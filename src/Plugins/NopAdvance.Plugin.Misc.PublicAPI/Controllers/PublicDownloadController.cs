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
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Web.Factories;
using NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers;

/// <summary>
/// Download methods
/// </summary>
public partial class PublicDownloadController : BaseAPIController
{
    #region Fields

    private readonly CustomerSettings _customerSettings;
    private readonly IDownloadService _downloadService;
    private readonly ILocalizationService _localizationService;
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly IWorkContext _workContext;
    private readonly ICustomerModelFactory _customerModelFactory;

    #endregion

    #region Ctor

    public PublicDownloadController(CustomerSettings customerSettings,
        IDownloadService downloadService,
        ILocalizationService localizationService,
        IOrderService orderService,
        IProductService productService,
        IWorkContext workContext,
        ICustomerModelFactory customerModelFactory)
    {
        _customerSettings = customerSettings;
        _downloadService = downloadService;
        _localizationService = localizationService;
        _orderService = orderService;
        _productService = productService;
        _workContext = workContext;
        _customerModelFactory = customerModelFactory;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Download product sample
    /// </summary>
    /// <param name="productId">The product identifier</param>
    [HttpGet("{productId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetProductSample(int productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(product)));

        if (!product.HasSampleDownload)
            return NotFound("Product doesn't have a sample download.");

        var download = await _downloadService.GetDownloadByIdAsync(product.SampleDownloadId);
        if (download == null)
            return NotFound("Sample download is not available any more.");

        //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
        //In this case, it is not relevant. Url may not be local.
        if (download.UseDownloadUrl)
            return new RedirectResult(download.DownloadUrl);

        if (download.DownloadBinary == null)
            return NotFound("Download data is not available any more.");

        var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : product.Id.ToString();
        var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : Nop.Core.MimeTypes.ApplicationOctetStream;
        return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
    }

    /// <summary>
    /// Download the downloadable product
    /// </summary>
    /// <param name="orderItemGuid">The order item guid identifier</param>
    /// <param name="agree">Agreed to the user agreement?</param>
    [HttpGet("{orderItemGuid}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetDownloadableProduct(Guid orderItemGuid, bool agree = false)
    {
        var orderItem = await _orderService.GetOrderItemByGuidAsync(orderItemGuid);
        if (orderItem == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(orderItem)));

        var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);

        if (!await _orderService.IsDownloadAllowedAsync(orderItem))
            return NotFound("Downloads are not allowed");

        if (_customerSettings.DownloadableProductsValidateUser)
        {
            if (await _workContext.GetCurrentCustomerAsync() == null)
                return Unauthorized();

            if (order.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                return NotFound("This is not your order");
        }

        var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

        var download = await _downloadService.GetDownloadByIdAsync(product.DownloadId);
        if (download == null)
            return NotFound("Download is not available any more.");

        if (product.HasUserAgreement && !agree)
        {
            var model = await _customerModelFactory.PrepareUserAgreementModelAsync(orderItem, product);
            return Ok(model);
        }

        if (!product.UnlimitedDownloads && orderItem.DownloadCount >= product.MaxNumberOfDownloads)
            return NotFound(string.Format(await _localizationService.GetResourceAsync("DownloadableProducts.ReachedMaximumNumber"), product.MaxNumberOfDownloads));

        if (download.UseDownloadUrl)
        {
            //increase download
            orderItem.DownloadCount++;
            await _orderService.UpdateOrderItemAsync(orderItem);

            //return result
            //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
            //In this case, it is not relevant. Url may not be local.
            return new RedirectResult(download.DownloadUrl);
        }

        //binary download
        if (download.DownloadBinary == null)
            return NotFound("Download data is not available any more.");

        //increase download
        orderItem.DownloadCount++;
        await _orderService.UpdateOrderItemAsync(orderItem);

        //return result
        var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : product.Id.ToString();
        var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : Nop.Core.MimeTypes.ApplicationOctetStream;
        return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
    }

    /// <summary>
    /// Download the order license
    /// </summary>
    /// <param name="orderItemGuid">The order item guid identifier</param>
    [HttpGet("{orderItemGuid}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetOrderItemLicense(Guid orderItemGuid)
    {
        var orderItem = await _orderService.GetOrderItemByGuidAsync(orderItemGuid);
        if (orderItem == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(orderItem)));

        var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);

        if (!await _orderService.IsLicenseDownloadAllowedAsync(orderItem))
            return NotFound("Downloads are not allowed");

        if (_customerSettings.DownloadableProductsValidateUser)
            if (await _workContext.GetCurrentCustomerAsync() == null || order.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                return Unauthorized();

        var download = await _downloadService.GetDownloadByIdAsync(orderItem.LicenseDownloadId ?? 0);
        if (download == null)
            return NotFound("Download is not available any more.");

        //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
        //In this case, it is not relevant. Url may not be local.
        if (download.UseDownloadUrl)
            return new RedirectResult(download.DownloadUrl);

        //binary download
        if (download.DownloadBinary == null)
            return NotFound("Download data is not available any more.");

        //return result
        var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : orderItem.ProductId.ToString();
        var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : Nop.Core.MimeTypes.ApplicationOctetStream;
        return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
    }

    /// <summary>
    /// Download the file upload file
    /// </summary>
    /// <param name="downloadGuid">The download guid identifier</param>
    [HttpGet("{downloadGuid}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetFileUpload(Guid downloadGuid)
    {
        var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
        if (download == null)
            return NotFound("Download is not available any more.");

        //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
        //In this case, it is not relevant. Url may not be local.
        if (download.UseDownloadUrl)
            return new RedirectResult(download.DownloadUrl);

        //binary download
        if (download.DownloadBinary == null)
            return NotFound("Download data is not available any more.");

        //return result
        var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : downloadGuid.ToString();
        var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : Nop.Core.MimeTypes.ApplicationOctetStream;
        return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
    }

    /// <summary>
    /// Download the order note file
    /// </summary>
    /// <param name="orderNoteId">The order note identifier</param>
    [HttpGet("{orderNoteId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetOrderNoteFile(int orderNoteId)
    {
        var orderNote = await _orderService.GetOrderNoteByIdAsync(orderNoteId);
        if (orderNote == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(orderNote)));

        var order = await _orderService.GetOrderByIdAsync(orderNote.OrderId);

        if (await _workContext.GetCurrentCustomerAsync() == null || order.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
            return Unauthorized();

        var download = await _downloadService.GetDownloadByIdAsync(orderNote.DownloadId);
        if (download == null)
            return NotFound("Download is not available any more.");

        //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
        //In this case, it is not relevant. Url may not be local.
        if (download.UseDownloadUrl)
            return new RedirectResult(download.DownloadUrl);

        //binary download
        if (download.DownloadBinary == null)
            return NotFound("Download data is not available any more.");

        //return result
        var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : orderNote.Id.ToString();
        var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : Nop.Core.MimeTypes.ApplicationOctetStream;
        return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
    }

    #endregion
}