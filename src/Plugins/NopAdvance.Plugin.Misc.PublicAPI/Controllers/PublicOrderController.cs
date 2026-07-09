using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Web.Factories;
using Nop.Web.Models.Order;
using NopAdvance.Plugin.Misc.PublicAPI.Controllers.Public;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;
using NopAdvance.Plugin.Misc.PublicAPI.Services;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers;

public partial class PublicOrderController : BaseAPIController
{
    #region Fields

    private readonly RewardPointsSettings _rewardPointsSettings;
    private readonly ICustomerService _customerService;
    private readonly IOrderModelFactory _orderModelFactory;
    private readonly IOrderService _orderService;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly IWorkContext _workContext;
    private readonly IPdfService _pdfService;
    private readonly IShipmentService _shipmentService;
    private readonly IPaymentService _paymentService;
    private readonly IPluginPaymentService _pluginPaymentService;

    #endregion

    #region Ctor

    public PublicOrderController(
        RewardPointsSettings rewardPointsSettings,
        ICustomerService customerService,
        IOrderModelFactory orderModelFactory,
        IOrderService orderService,
        IOrderProcessingService orderProcessingService,
        IWorkContext workContext,
        IPdfService pdfService,
        IShipmentService shipmentService,
        IPaymentService paymentService,
        IPluginPaymentService pluginPaymentService)
    {
        _rewardPointsSettings = rewardPointsSettings;
        _customerService = customerService;
        _orderModelFactory = orderModelFactory;
        _orderService = orderService;
        _orderProcessingService = orderProcessingService;
        _workContext = workContext;
        _pdfService = pdfService;
        _shipmentService = shipmentService;
        _paymentService = paymentService;
        _pluginPaymentService = pluginPaymentService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get customer orders
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(CustomerOrderListModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetCustomerOrders(
        [FromQuery] int page = 1,
        [FromQuery] OrderHistoryPeriods limit = OrderHistoryPeriods.All)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (page <= 0)
            page = 1;

        var model = await _orderModelFactory.PrepareCustomerOrderListModelAsync(page, limit);
        return Ok(model);
    }

    /// <summary>
    /// Cancel recurring payment
    /// </summary>
    [HttpPost("{recurringPaymentId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> CancelRecurringPayment(
        int recurringPaymentId,
        [FromQuery] int page = 1,
        [FromQuery] OrderHistoryPeriods limit = OrderHistoryPeriods.All)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Unauthorized();

        if (page <= 0)
            page = 1;

        var recurringPayment = await _orderService.GetRecurringPaymentByIdAsync(recurringPaymentId);
        if (recurringPayment == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(recurringPayment)));

        if (!await _orderProcessingService.CanCancelRecurringPaymentAsync(customer, recurringPayment))
            return BadRequest("Can't cancel recurring order");

        var errors = await _orderProcessingService.CancelRecurringPaymentAsync(recurringPayment);

        var model = await _orderModelFactory.PrepareCustomerOrderListModelAsync(page, limit);

        // ✅ CustomerOrderListModel has no RecurringPaymentErrors in your nop version
        // Return errors separately (API-friendly + compiles)
        return Ok(new
        {
            model,
            recurringPaymentErrors = errors
        });
    }

    /// <summary>
    /// Retry last recurring payment
    /// </summary>
    [HttpPost("{recurringPaymentId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> RetryLastRecurringPayment(
        int recurringPaymentId,
        [FromQuery] int page = 1,
        [FromQuery] OrderHistoryPeriods limit = OrderHistoryPeriods.All)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Unauthorized();

        if (page <= 0)
            page = 1;

        var recurringPayment = await _orderService.GetRecurringPaymentByIdAsync(recurringPaymentId);
        if (recurringPayment == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(recurringPayment)));

        if (!await _orderProcessingService.CanRetryLastRecurringPaymentAsync(customer, recurringPayment))
            return BadRequest("Can't retry last recurring order");

        var errors = await _orderProcessingService.ProcessNextRecurringPaymentAsync(recurringPayment);

        var model = await _orderModelFactory.PrepareCustomerOrderListModelAsync(page, limit);

        return Ok(new
        {
            model,
            recurringPaymentErrors = errors?.ToList() ?? new List<string>()
        });
    }

    /// <summary>
    /// Get customer reward points
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomerRewardPointsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetCustomerRewardPoints(int? pageNumber)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        if (!_rewardPointsSettings.Enabled)
            return BadRequest(new ErrorResponse { Error = MessageDefaults.DISABLED_FROM_SETTINGS });

        var model = await _orderModelFactory.PrepareCustomerRewardPointsAsync(pageNumber);
        return Ok(model);
    }

    [HttpGet("{orderId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(OrderDetailsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetOrderDetails(int orderId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var order = await _orderService.GetOrderByIdAsync(orderId);

        if (order == null || order.Deleted || customer.Id != order.CustomerId)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(order)));

        var model = await _orderModelFactory.PrepareOrderDetailsModelAsync(order);
        return Ok(model);
    }

    [HttpGet("{orderId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetPdfInvoice(int orderId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var order = await _orderService.GetOrderByIdAsync(orderId);

        if (order == null || order.Deleted || customer.Id != order.CustomerId)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(order)));

        byte[] bytes;
        await using (var stream = new MemoryStream())
        {
            await _pdfService.PrintOrderToPdfAsync(stream, order, await _workContext.GetWorkingLanguageAsync());
            bytes = stream.ToArray();
        }

        return File(bytes, Nop.Core.MimeTypes.ApplicationPdf, $"order_{order.Id}.pdf");
    }

    [HttpPost("{orderId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> ReOrder(int orderId)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        var customer = await _workContext.GetCurrentCustomerAsync();
        var order = await _orderService.GetOrderByIdAsync(orderId);

        if (order == null || order.Deleted || customer.Id != order.CustomerId)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(order)));

        await _orderProcessingService.ReOrderAsync(order);
        return Ok();
    }

    [HttpPost("{orderId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> RetryPayment(int orderId)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        var customer = await _workContext.GetCurrentCustomerAsync();
        var order = await _orderService.GetOrderByIdAsync(orderId);

        if (order == null || order.Deleted || customer.Id != order.CustomerId)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(order)));

        if (!await _paymentService.CanRePostProcessPaymentAsync(order))
            return BadRequest("Retry payment is not allowed on this order");

        var redirectUrl = string.Empty;
        switch (order.PaymentMethodSystemName)
        {
            case PaymentMethodDefaults.PAY_PAL_STANDARD:
                redirectUrl = await _pluginPaymentService.GetPayPalStandardRedirectionUrl(order);
                break;
        }

        if (string.IsNullOrEmpty(redirectUrl))
            return BadRequest("Payment method not supported by api at this time");

        return Ok(redirectUrl);
    }

    [HttpGet("{shipmentId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ShipmentDetailsModel), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetShipmentDetails(int shipmentId)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Unauthorized();

        var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);
        if (shipment == null)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(shipment)));

        var customer = await _workContext.GetCurrentCustomerAsync();
        var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);

        if (order == null || order.Deleted || customer.Id != order.CustomerId)
            return NotFound(string.Format(MessageDefaults.NOT_FOUND, nameof(order)));

        var model = await _orderModelFactory.PrepareShipmentDetailsModelAsync(shipment);
        return Ok(model);
    }

    #endregion
}
