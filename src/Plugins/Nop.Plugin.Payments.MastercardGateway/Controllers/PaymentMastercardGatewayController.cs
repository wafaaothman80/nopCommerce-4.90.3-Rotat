using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ExCSS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;

//using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Http.Extensions;

using Nop.Plugin.Payments.MastercardGateway;
using Nop.Plugin.Payments.MastercardGateway.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

//using RestSharp;

namespace Nop.Plugin.Payments.Mastercard.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class PaymentMastercardGatewayController : BasePaymentController
    {
        private readonly MastercardGatewayPaymentSettings _mastercardPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IOrderProcessingService _orderProcessingService;

        protected readonly INotificationService _notificationService;
        protected readonly IOrderService _orderService;
        protected readonly ILogger _logger;
        protected readonly IWebHelper _webHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShoppingCartService _shoppingCartService;
        protected readonly IActionContextAccessor _actionContextAccessor;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly IPaymentService _paymentService;
        protected readonly ILocalizationService _localizationService;
        public PaymentMastercardGatewayController(
            MastercardGatewayPaymentSettings mastercardPaymentSettings,
            ISettingService settingService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IOrderProcessingService orderProcessingService,
            INotificationService notificationService, IOrderService orderService, ILogger logger, IWebHelper webHelper, IHttpContextAccessor httpContextAccessor, IShoppingCartService shoppingCartService, IActionContextAccessor actionContextAccessor, IOrderTotalCalculationService orderTotalCalculationService, IGenericAttributeService genericAttributeService, IPaymentService paymentService, ILocalizationService localizationService)
        {
            _mastercardPaymentSettings = mastercardPaymentSettings;
            _settingService = settingService;
            _storeContext = storeContext;
            _workContext = workContext;
            _orderProcessingService = orderProcessingService;

            _notificationService = notificationService;
            _orderService = orderService;
            _logger = logger;
            _webHelper = webHelper;
            _httpContextAccessor = httpContextAccessor;
            _shoppingCartService = shoppingCartService;
            _actionContextAccessor = actionContextAccessor;
            _orderTotalCalculationService = orderTotalCalculationService;
            _genericAttributeService = genericAttributeService;
            _paymentService = paymentService;
            _localizationService = localizationService;
        }

        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        public IActionResult Configure()
        {
            var model = new ConfigurationModel
            {
                UseSandbox = _mastercardPaymentSettings.UseSandbox,
                MerchantId = _mastercardPaymentSettings.MerchantId,
                ApiPassword = _mastercardPaymentSettings.ApiPassword,


            };

            return View("~/Plugins/Payments.MastercardGateway/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            _mastercardPaymentSettings.UseSandbox = model.UseSandbox;
            _mastercardPaymentSettings.MerchantId = model.MerchantId;
            _mastercardPaymentSettings.ApiPassword = model.ApiPassword;



            _settingService.SaveSetting(_mastercardPaymentSettings);

            _notificationService.SuccessNotification("Settings saved successfully");
            return Configure();
        }
        



        private async Task<bool> VerifyMastercardPayment(string orderId, MastercardGatewayPaymentSettings settings)
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);

                var baseUrl = "https://ap-gateway.mastercard.com";
                    

                var authToken = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"merchant.{settings.MerchantId}:{settings.ApiPassword}"));

                client.DefaultRequestHeaders.Authorization = new("Basic", authToken);
                client.DefaultRequestHeaders.Accept.Add(new("application/json"));

                // 1. First check the order status
                var orderResponse = await client.GetAsync(
                    $"{baseUrl}/api/rest/version/62/merchant/{settings.MerchantId}/order/{orderId}");

                var responseContent = await orderResponse.Content.ReadAsStringAsync();
                _logger.Information($"Order verification response: {responseContent}");

                if (!orderResponse.IsSuccessStatusCode)
                    return false;

                // 2. Check transaction history if order exists
                var transactionResponse = await client.GetAsync(
                    $"{baseUrl}/api/rest/version/62/merchant/{settings.MerchantId}/order/{orderId}/transaction");

                var transactionContent = await transactionResponse.Content.ReadAsStringAsync();
                _logger.Information($"Transaction verification response: {transactionContent}");

                // 3. Parse both responses
                var orderData = System.Text.Json.JsonSerializer.Deserialize<MastercardOrderResponse>(responseContent);
                var transactionData = System.Text.Json.JsonSerializer.Deserialize<MastercardTransactionResponse>(transactionContent);

                // 4. Multiple success indicators
                return (orderData?.Result == "SUCCESS") ||
                       (orderData?.Response?.GatewayCode == "APPROVED") ||
                       (transactionData?.Transactions?.Any(t => t.Status == "SUCCESS") ?? true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message+ "Payment verification failed");
                return false;
            }
        }

       

        public class MastercardTransactionResponse
        {
            public List<Transaction> Transactions { get; set; }

            public class Transaction
            {
                public string Id { get; set; }
                public string Status { get; set; } // "SUCCESS", "FAILED"
                public string Amount { get; set; }
            }
        }


        public class MastercardOrderResponse
        {
            public string Result { get; set; }
            public ResponseData Response { get; set; }
            public TransactionData Transaction { get; set; }

            public class ResponseData
            {
                public string GatewayCode { get; set; } // "APPROVED" or "DECLINED"
            }

            public class TransactionData
            {
                public string Status { get; set; } // "SUCCESS", "FAILED"
                public string Amount { get; set; }
                public string Currency { get; set; }
            }
        }

     




        [HttpGet]
        public async Task<IActionResult> Return(string resultIndicator, string sessionVersion)
        {
            try
            {
                var orderId = HttpContext.Session.GetInt32("MastercardOrderId");
                if (!orderId.HasValue)
                    throw new Exception("Order ID missing from session");

                var order = await _orderService.GetOrderByIdAsync(orderId.Value);
                if (order == null)
                    throw new Exception($"Order not found: {orderId}");

                var settings = await _settingService.LoadSettingAsync<MastercardGatewayPaymentSettings>(await _storeContext.GetActiveStoreScopeConfigurationAsync());

                // Verify payment with Mastercard
                var isPaymentValid = await VerifyMastercardPayment(order.Id.ToString(), settings);

                if (isPaymentValid)
                {
                    order.PaymentStatus = PaymentStatus.Paid;
                    order.OrderStatus = OrderStatus.Processing;
                    await _orderService.UpdateOrderAsync(order);

                    await _orderService.InsertOrderNoteAsync(new OrderNote
                    {
                        OrderId = order.Id,
                        Note = $"Mastercard payment completed. Result: {resultIndicator}",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });

                    HttpContext.Session.Remove("MastercardOrderId");
                    return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
                }
                TempData["PaymentError"] = "Payment return handler failed";
                return RedirectToAction("PaymentError");
            }
            catch (Exception ex)
            {
                TempData["PaymentError"] = ex.Message + "Payment return handler failed";
                _logger.Error(ex.Message+ "Payment return handler failed");
                return RedirectToAction("PaymentError");
            }
        }


        

      
        [HttpGet]
        public async Task<IActionResult> Cancel()
        {
            try
            {
                var order = await _orderService.SearchOrdersAsync(
                    storeId: 1,
                    customerId: _workContext.GetCurrentCustomerAsync().Result.Id,
                    pageSize: 1);

                if (order.FirstOrDefault() != null)
                {
                    await _orderService.InsertOrderNoteAsync(new OrderNote
                    {
                        OrderId = order.FirstOrDefault().Id,
                        Note = "Customer cancelled payment",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                }

                HttpContext.Session.Remove("MastercardPaymentInfo");
                HttpContext.Session.Remove("MastercardOrderId");
                HttpContext.Session.Remove("MastercardSessionId");
                _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Plugins.Payments.Mastercard.PaymentCancelled"));

                return RedirectToRoute("ShoppingCart");
            }
            catch (Exception ex)
            {
                _logger.Error("Cancel failed", ex);
                return RedirectToRoute("Homepage");
            }
        }





        private void LogError(string message)
        {
            
           _logger.ErrorAsync(message);
        }
    

    




   


       
        public async Task<IActionResult> CancelOrder()
        {
            try
            {
               
                HttpContext.Session.Remove("OrderPaymentInfo");
                HttpContext.Session.Remove("PayOrderId");
                HttpContext.Session.Remove("successIndicator");

               
                var customer =await _workContext.GetCurrentCustomerAsync();
                var order = _orderService.SearchOrdersAsync(
                    storeId: 1,
                    customerId: customer.Id,
                    pageSize: 1
                ).Result.FirstOrDefault();

              
                if (order != null)
                {
                    
                 await _orderService.InsertOrderNoteAsync(new OrderNote
                    {
                        OrderId = order.Id,
                        Note = "Payment was cancelled by customer",
                        DisplayToCustomer = true,
                        CreatedOnUtc = DateTime.UtcNow
                    });

                    return RedirectToRoute("OrderDetails", new { orderId = order.Id });
                }

              
                var cart = _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart,1);
                if (cart.Result.Any())
                {
                    return RedirectToRoute("ShoppingCart");
                }

                return RedirectToRoute("Homepage");
            }
            catch (Exception ex)
            {
                _logger.Error("CancelOrder error", ex);
                return RedirectToRoute("Homepage");
            }
        }

      

      
        [HttpGet]
        public async Task<IActionResult> PaymentError()
        {
            // Retrieve error message from TempData
            var errorMessage = TempData["PaymentError"]?.ToString() ??
               await _localizationService.GetResourceAsync("Plugins.Payments.Mastercard.PaymentGenericError");
            HttpContext.Session.Remove("OrderPaymentInfo");
            HttpContext.Session.Remove("PayOrderId");
            HttpContext.Session.Remove("successIndicator");
            // Pass error to view
            return View("~/Plugins/Payments.MastercardGateway/Views/PaymentError.cshtml", errorMessage);
        }


        

    }
}