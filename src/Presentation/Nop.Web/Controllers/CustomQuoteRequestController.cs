using LinqToDB.Data;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Services.Request;

[Route("CustomQuoteRequest/[action]")]
public class CustomQuoteRequestController : BasePublicController
{
    private readonly IQuoteRequestService _quoteRequestService;
    private readonly INotificationService _notificationService;
    private readonly Nop.Services.Logging.ILogger _logger;
    private readonly IWebHelper _webHelper;
    private readonly ICustomNumberFormatter _customNumberFormatter;
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly INopDataProvider _nopDataProvider;

    public CustomQuoteRequestController(
        IQuoteRequestService quoteRequestService,
        INotificationService notificationService,
        Nop.Services.Logging.ILogger logger,
        IWebHelper webHelper,
        ICustomNumberFormatter customNumberFormatter,
        IOrderService orderService,
        IProductService productService,
        INopDataProvider nopDataProvider)
    {
        _quoteRequestService = quoteRequestService;
        _notificationService = notificationService;
        _logger = logger;
        _webHelper = webHelper;
        _customNumberFormatter = customNumberFormatter;
        _orderService = orderService;
        _productService = productService;
        _nopDataProvider = nopDataProvider;
    }

    [HttpPost]
    public virtual async Task<IActionResult> ConvertToOrder()
    {
        // Read the quote request integer Id from the hidden form field (asp-for="Id")
        if (!int.TryParse(Request.Form["Id"], out var quoteId) || quoteId <= 0)
        {
            _notificationService.ErrorNotification("Invalid quote request ID.");
            return RedirectToAction("Index", "QuoteRequest");
        }

        // Read the referer URL so we can redirect back to the same page after processing.
        // The URL is: /en/quoterequestdetails/{guid}  — we use it directly to avoid
        // needing to know the route name or the GUID separately.
        var refererUrl = Request.Headers["Referer"].ToString();
        if (string.IsNullOrWhiteSpace(refererUrl))
            refererUrl = Url.Action("Index", "QuoteRequest");

        QuoteRequest quoteRequest = null;

        try
        {
            quoteRequest = await _quoteRequestService.GetQuoteRequestByIdAsync(quoteId);
            if (quoteRequest == null)
            {
                _notificationService.ErrorNotification("Quote request not found.");
                return Redirect(refererUrl);
            }

            // Read comma-separated IDs from the hidden field JS populates before submit
            var selectedItemsRaw = Request.Form["SelectedQuoteRequestItemIds"].ToString();

            await _logger.InformationAsync(
                $"[ConvertToOrder] quoteId={quoteId}, raw selectedItems='{selectedItemsRaw}'");

            var selectedIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(selectedItemsRaw))
            {
                selectedIds = selectedItemsRaw
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => int.TryParse(x.Trim(), out var pid) ? pid : 0)
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();
            }

            if (!selectedIds.Any())
            {
                _notificationService.ErrorNotification(
                    "Please select at least one item to convert to order.");
                return Redirect(refererUrl);
            }

            var allQuoteItems = await _quoteRequestService.GetItemsByQuoteRequestId(quoteId);
            var selectedQuoteItems = allQuoteItems
                .Where(item => selectedIds.Contains(item.Id))
                .ToList();

            if (!selectedQuoteItems.Any())
            {
                _notificationService.ErrorNotification(
                    "None of the selected items were found on this quote.");
                return Redirect(refererUrl);
            }

            var order = await CreateOrderFromQuoteItems(quoteRequest, selectedQuoteItems);

            if (order != null)
            {
                // Update per-item NopOrderId in the DB via SP
                var selectedItemsForSp = string.Join(",", selectedIds);

                DataParameter[] updateParams =
                {
                    new DataParameter(name: "@SelectedQuoteItems", value: selectedItemsForSp),
                    new DataParameter(name: "@NopOrderId",         value: order.Id)
                };
                await _nopDataProvider.QueryProcAsync<int>(
                    "UpdateQuoteRequestItemsWithOrderId", updateParams);

                await _logger.InformationAsync(
                    $"[ConvertToOrder] SP UpdateQuoteRequestItemsWithOrderId called. " +
                    $"items='{selectedItemsForSp}', orderId={order.Id}");

                // Check if ALL items are now converted; if so mark the whole request Complete
                DataParameter[] checkParams =
                {
                    new DataParameter(name: "@QuoteRequestId", value: quoteId)
                };
                var allItemData = await _nopDataProvider
                    .QueryProcAsync<QuoteRequestItemDto>(
                        "GetQuoteRequestItemsWithOrderId", checkParams);

                bool allConverted = allItemData.All(
                    x => x.NopOrderId.HasValue && x.NopOrderId.Value > 0);

                if (allConverted)
                {
                    quoteRequest.NopOrderId = order.Id;
                    quoteRequest.RequestStatus = RequestStatus.Complete;
                    await _quoteRequestService.UpdateQuoteRequestAsync(quoteRequest);
                }

                _notificationService.SuccessNotification(
                    $"Selected items successfully converted to order #{order.CustomOrderNumber}.");
            }
            else
            {
                _notificationService.ErrorNotification(
                    "Failed to create order from quote request.");
            }

            return Redirect(refererUrl);
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync(
                $"[ConvertToOrder] Error converting quote request {quoteId} to order", ex);

            var message = ex.InnerException?.Message ?? ex.Message;
            _notificationService.ErrorNotification($"Error converting to order: {message}");

            return Redirect(refererUrl);
        }
    }

    private async Task<Order> CreateOrderFromQuoteItems(
        QuoteRequest quoteRequest,
        List<QuoteRequestItem> selectedItems)
    {
        int billingAddressId = (quoteRequest.BillingAddressId > 0
            ? quoteRequest.BillingAddressId
            : quoteRequest.ShippingAddressId) ?? 0;

        var order = new Order
        {
            OrderGuid        = Guid.NewGuid(),
            CustomerId       = quoteRequest.CustomerId,
            CustomerIp       = _webHelper.GetCurrentIpAddress() ?? string.Empty,
            StoreId          = quoteRequest.StoreId,
            BillingAddressId = billingAddressId,
            ShippingAddressId = quoteRequest.ShippingAddressId,
            PickupInStore    = quoteRequest.PickupInStore,
            OrderStatus      = OrderStatus.Pending,
            PaymentStatus    = PaymentStatus.Pending,
            ShippingStatus   = ShippingStatus.NotYetShipped,
            CurrencyRate     = 1,
            CustomerCurrencyCode = "AED",
            CustomerTaxDisplayType =
                Nop.Core.Domain.Tax.TaxDisplayType.ExcludingTax,
            PaymentMethodSystemName              = string.Empty,
            ShippingMethod                       = string.Empty,
            ShippingRateComputationMethodSystemName = string.Empty,
            CheckoutAttributeDescription         = string.Empty,
            CheckoutAttributesXml                = string.Empty,
            TaxRates                             = string.Empty,
            OrderTax                             = 0,
            OrderDiscount                        = 0,
            OrderShippingInclTax                 = 0,
            OrderShippingExclTax                 = 0,
            PaymentMethodAdditionalFeeInclTax    = 0,
            PaymentMethodAdditionalFeeExclTax    = 0,
            CreatedOnUtc = DateTime.UtcNow
        };

        // Temporary placeholder so INSERT succeeds (CustomOrderNumber is NOT NULL in DB)
        order.CustomOrderNumber = Guid.NewGuid().ToString("N");
        await _orderService.InsertOrderAsync(order);

        // Now order.Id is assigned — generate the real formatted number
        order.CustomOrderNumber =
            _customNumberFormatter.GenerateOrderCustomNumber(order);
        await _orderService.UpdateOrderAsync(order);

        decimal orderSubTotal = 0;

        foreach (var quoteItem in selectedItems)
        {
            var product = await _productService.GetProductByIdAsync(quoteItem.ProductId);
            if (product == null) continue;

            decimal unitPrice = quoteItem.DiscountedPrice;
            decimal itemTotal = unitPrice * quoteItem.Quantity;

            var orderItem = new OrderItem
            {
                OrderItemGuid        = Guid.NewGuid(),
                OrderId              = order.Id,
                ProductId            = quoteItem.ProductId,
                Quantity             = quoteItem.Quantity,
                UnitPriceInclTax     = unitPrice,
                UnitPriceExclTax     = unitPrice,
                PriceInclTax         = itemTotal,
                PriceExclTax         = itemTotal,
                DiscountAmountInclTax = 0,
                DiscountAmountExclTax = 0,
                OriginalProductCost  = product.ProductCost,
                AttributeDescription = quoteItem.AttributesXml ?? string.Empty,
                AttributesXml        = quoteItem.AttributesXml ?? string.Empty,
                DownloadCount        = 0,
                IsDownloadActivated  = false,
                LicenseDownloadId    = 0
            };

            await _orderService.InsertOrderItemAsync(orderItem);
            orderSubTotal += itemTotal;
        }

        order.OrderSubtotalInclTax = orderSubTotal;
        order.OrderSubtotalExclTax = orderSubTotal;
        order.OrderTotal           = orderSubTotal;
        await _orderService.UpdateOrderAsync(order);

        await _orderService.InsertOrderNoteAsync(new OrderNote
        {
            OrderId          = order.Id,
            Note             = $"Order created from quote request #{quoteRequest.Id}",
            DisplayToCustomer = false,
            CreatedOnUtc     = DateTime.UtcNow
        });

        return order;
    }

    // Optional GET endpoint — kept for debugging / future AJAX use
    [HttpGet]
    public virtual async Task<IActionResult> GetQuoteRequestItemsWithOrderId(
        int quoteRequestId)
    {
        try
        {
            DataParameter[] parameters =
            {
                new DataParameter(name: "@QuoteRequestId", value: quoteRequestId)
            };
            var result = await _nopDataProvider
                .QueryProcAsync<QuoteRequestItemDto>(
                    "GetQuoteRequestItemsWithOrderId", parameters);

            return Json(new { success = true, items = result.ToList() });
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync(
                $"[GetQuoteRequestItemsWithOrderId] Error for id={quoteRequestId}", ex);
            return Json(new { success = false, error = "Error loading items" });
        }
    }

    public class QuoteRequestItemDto
    {
        public int  Id         { get; set; }
        public int? NopOrderId { get; set; }
    }
}
