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
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Factories;
using Nop.Web.Models.Order;
using NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;
using NopAdvance.Plugin.Misc.PublicAPI.Services;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers;

/// <summary>
/// Return request methods
/// </summary>
public partial class PublicReturnRequestController : BaseAPIController
{
    #region Fields

    private readonly LocalizationSettings _localizationSettings;
    private readonly OrderSettings _orderSettings;
    private readonly ICustomerService _customerService;
    private readonly ICustomNumberFormatter _customNumberFormatter;
    private readonly IDownloadService _downloadService;
    private readonly ILocalizationService _localizationService;
    private readonly INopFileProvider _fileProvider;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly IOrderService _orderService;
    private readonly IReturnRequestModelFactory _returnRequestModelFactory;
    private readonly IReturnRequestService _returnRequestService;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    private readonly IWorkflowMessageService _workflowMessageService;

    #endregion

    #region Ctor

    public PublicReturnRequestController(LocalizationSettings localizationSettings,
        OrderSettings orderSettings,
        ICustomerService customerService,
        ICustomNumberFormatter customNumberFormatter,
        IDownloadService downloadService,
        ILocalizationService localizationService,
        INopFileProvider fileProvider,
        IOrderProcessingService orderProcessingService,
        IOrderService orderService,
        IReturnRequestModelFactory returnRequestModelFactory,
        IReturnRequestService returnRequestService,
        IStoreContext storeContext,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService)
    {
        _localizationSettings = localizationSettings;
        _orderSettings = orderSettings;
        _customerService = customerService;
        _customNumberFormatter = customNumberFormatter;
        _downloadService = downloadService;
        _localizationService = localizationService;
        _fileProvider = fileProvider;
        _orderProcessingService = orderProcessingService;
        _orderService = orderService;
        _returnRequestModelFactory = returnRequestModelFactory;
        _returnRequestService = returnRequestService;
        _storeContext = storeContext;
        _workContext = workContext;
        _workflowMessageService = workflowMessageService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get customer return requests
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(CustomerReturnRequestsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetCustomerReturnRequests()
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        var model = await _returnRequestModelFactory.PrepareCustomerReturnRequestsModelAsync();
        return Ok(model);
    }

    /// <summary>
    /// Prepare return request model
    /// </summary>
    /// <param name="orderId">The order idntifier</param>
    [HttpGet("{orderId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(SubmitReturnRequestModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetReturnRequest(int orderId)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null || order.Deleted || (await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(order)));

        if (!await _orderProcessingService.IsReturnRequestAllowedAsync(order))
            return BadRequest("Return request not allowed");

        var model = new SubmitReturnRequestModel();
        model = await _returnRequestModelFactory.PrepareSubmitReturnRequestModelAsync(model, order);
        return Ok(model);
    }

    /// <summary>
    /// Submit a return request
    /// </summary>
    /// <param name="orderId">The order idntifier</param>
    [HttpPost("{orderId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(SubmitReturnRequestModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> ReturnRequest(int orderId, ReturnRequestRequest request)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null || order.Deleted || (await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(order)));

        if (!await _orderProcessingService.IsReturnRequestAllowedAsync(order))
            return BadRequest("Return request not allowed");

        var count = 0;
        var downloadId = 0;
        var model = new SubmitReturnRequestModel
        {
            UploadedFileGuid = request.UploadedFileGuid,
            ReturnRequestReasonId = request.ReturnRequestReasonId,
            ReturnRequestActionId = request.ReturnRequestActionId,
            Comments = request.Comments
        };
        if (_orderSettings.ReturnRequestsAllowFiles)
        {
            var download = await _downloadService.GetDownloadByGuidAsync(model.UploadedFileGuid);
            if (download != null)
                downloadId = download.Id;
        }

        //returnable products
        var orderItems = await _orderService.GetOrderItemsAsync(order.Id, isNotReturnable: false);
        foreach (var orderItem in orderItems)
        {
            var quantity = 0; //parse quantity
            if (request.OrderItemsQuantity.ContainsKey(orderItem.Id))
                quantity = request.OrderItemsQuantity[orderItem.Id];

            if (quantity > 0)
            {
                var rrr = await _returnRequestService.GetReturnRequestReasonByIdAsync(model.ReturnRequestReasonId);
                var rra = await _returnRequestService.GetReturnRequestActionByIdAsync(model.ReturnRequestActionId);

                var rr = new ReturnRequest
                {
                    CustomNumber = "",
                    StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                    OrderItemId = orderItem.Id,
                    Quantity = quantity,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    ReasonForReturn = rrr != null ? await _localizationService.GetLocalizedAsync(rrr, x => x.Name) : "not available",
                    RequestedAction = rra != null ? await _localizationService.GetLocalizedAsync(rra, x => x.Name) : "not available",
                    CustomerComments = model.Comments,
                    UploadedFileId = downloadId,
                    StaffNotes = string.Empty,
                    ReturnRequestStatus = ReturnRequestStatus.Pending,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow
                };

                await _returnRequestService.InsertReturnRequestAsync(rr);

                //set return request custom number
                rr.CustomNumber = _customNumberFormatter.GenerateReturnRequestCustomNumber(rr);
                await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());
                await _returnRequestService.UpdateReturnRequestAsync(rr);

                //notify store owner
                await _workflowMessageService.SendNewReturnRequestStoreOwnerNotificationAsync(rr, orderItem, order, _localizationSettings.DefaultAdminLanguageId);
                //notify customer
                await _workflowMessageService.SendNewReturnRequestCustomerNotificationAsync(rr, orderItem, order);

                count++;
            }
        }

        model = await _returnRequestModelFactory.PrepareSubmitReturnRequestModelAsync(model, order);
        if (count > 0)
            model.Result = await _localizationService.GetResourceAsync("ReturnRequests.Submitted");
        else
            model.Result = await _localizationService.GetResourceAsync("ReturnRequests.NoItemsSubmitted");

        return Ok(model);
    }

    /// <summary>
    /// Upload a return request file
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(UploadFileResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> UploadFileReturnRequest(UploadFileRequest request)
    {
        if (!_orderSettings.ReturnRequestsEnabled || !_orderSettings.ReturnRequestsAllowFiles)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var (fileBinary, contentType, _) = PluginCommonHelper.ConvertBase64ToFile(request.FileBase64String, request.FileName);
        if (fileBinary == null)
            return Ok(new UploadFileResponse
            {
                Message = "No file uploaded",
                UploadedFileGuid = Guid.Empty,
            });

        var fileName = request.FileName;

        var fileExtension = _fileProvider.GetFileExtension(fileName);
        if (!string.IsNullOrEmpty(fileExtension))
            fileExtension = fileExtension.ToLowerInvariant();

        var validationFileMaximumSize = _orderSettings.ReturnRequestsFileMaximumSize;
        if (validationFileMaximumSize > 0)
        {
            //compare in bytes
            var maxFileSizeBytes = validationFileMaximumSize * 1024;
            if (fileBinary.Length > maxFileSizeBytes)
                return Ok(new UploadFileResponse
                {
                    Message = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumUploadedFileSize"), validationFileMaximumSize),
                    UploadedFileGuid = Guid.Empty,
                });
        }

        var download = new Download
        {
            DownloadGuid = Guid.NewGuid(),
            UseDownloadUrl = false,
            DownloadUrl = "",
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
            UploadedFileGuid = download.DownloadGuid,
        });
    }

    #endregion
}
