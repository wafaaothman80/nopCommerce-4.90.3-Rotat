using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Http.Extensions;
using Nop.Plugin.Payments.MastercardGateway.Components;
using Nop.Plugin.Payments.MastercardGateway.Models;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
//using static SkiaSharp.HarfBuzz.SKShaper;



namespace Nop.Plugin.Payments.MastercardGateway
{
    
    public class MastercardGatewayPaymentProcessor : BasePlugin, IPaymentMethod
    {


        private readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly ILogger _logger;
        protected readonly IWebHelper _webHelper;
     
        protected readonly IOrderService _orderService;
        protected readonly IShoppingCartService _shoppingCartService;
        protected readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        public MastercardGatewayPaymentProcessor(IHttpContextAccessor httpContextAccessor, ILogger logger, IWebHelper webHelper,  IOrderService orderService, IShoppingCartService shoppingCartService, ISettingService settingService, IStoreContext storeContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _webHelper = webHelper;
           
            _orderService = orderService;
            _shoppingCartService = shoppingCartService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        public bool SupportCapture => false;
        public bool SupportRefund => false;
        public bool SupportPartiallyRefund => false;
        public bool SupportVoid => false;
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;
       public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;
        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(new MastercardGatewayPaymentSettings
            {
                UseSandbox = true,
                MerchantId = "TEST944804000",
                ApiPassword = "2569537d5717f36417b6d1d24f91ef0c"
            });

            await base.InstallAsync();
        }
        
     

        private async Task<string> CreateMastercardSession(int orderId,decimal amount, MastercardGatewayPaymentSettings settings)
        {
            // 1. Configure HTTP client with proper SSL
            var handler = new HttpClientHandler
            {
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
                MaxConnectionsPerServer = 20
            };

            using var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30),
                BaseAddress = new Uri("https://ap-gateway.mastercard.com")
            };

            // 2. Authentication
            var authToken = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"merchant.{settings.MerchantId}:{settings.ApiPassword}"));

            client.DefaultRequestHeaders.Authorization = new("Basic", authToken);
            client.DefaultRequestHeaders.Accept.Add(new("application/json"));

            // 3. Make request
            try
            {
                var request = new
                {
                    apiOperation = "CREATE_CHECKOUT_SESSION",
                    order = new
                    {
                        id = orderId.ToString(),
                        currency = "AED",
                        amount = amount.ToString("0.00", CultureInfo.InvariantCulture),
                        description = $"Order #{orderId}"
                    },
                    interaction = new
                    {
                        operation = "PURCHASE",
                        returnUrl = GetMastercardReturnUrl(),
                        cancelUrl = GetMastercardCancelUrl(),
                        merchant = new
                        {
                            name = "Rotat",
                            address = new
                            {
                                line1 = "MerchantAddressLine1",
                                line2 = "MerchantAddressLine2"
                            }
                        }
                    }
                };

                var response = await client.PostAsJsonAsync(
                    $"/api/rest/version/62/merchant/{settings.MerchantId}/session",
                    request);

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadFromJsonAsync<MastercardSessionResponse>();
                return content.Session.Id; //
               
            }
            catch (HttpRequestException ex) when (ex.InnerException is System.Net.Sockets.SocketException)
            {
                _logger.Error(ex.Message+ "Network connectivity error");
                throw new Exception("Payment service unavailable. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message+ "Payment processing failed");
                throw;
            }
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest request)
        {
            _logger.Information("🔔 PostProcessPaymentAsync called for order #" + request.Order.Id);

            
                try
                {
                    var settings = await _settingService.LoadSettingAsync<MastercardGatewayPaymentSettings>(await _storeContext.GetActiveStoreScopeConfigurationAsync());

                   
                    var sessionId = await CreateMastercardSession(request.Order.Id, request.Order.OrderTotal, settings);

               
                _httpContextAccessor.HttpContext.Session.SetInt32("MastercardOrderId", request.Order.Id);

                
                _httpContextAccessor.HttpContext.Response.Redirect(
                       $"https://ap-gateway.mastercard.com/api/page/version/62/pay?session.id={sessionId}");
                     
            }
                catch (Exception ex)
                {
                    _logger.Error("Mastercard payment processing error", ex);
                    throw new Exception($"Payment processing error: {ex.Message}");
                }
            }
       

      

     

        private string GetMastercardReturnUrl()
        {
            var request = _httpContextAccessor.HttpContext.Request;

            
            return $"{request.Scheme}://{request.Host}/Plugins/PaymentMastercardGateway/Return";

          
        }
        private string GetMastercardCancelUrl()
        {
            var request = _httpContextAccessor.HttpContext.Request;

            
            return $"{request.Scheme}://{request.Host}/Plugins/PaymentMastercardGateway/Cancel";

          
        }
   

        private string GetAbsoluteUrl(string relativeUrl)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            return $"{request.Scheme}://{request.Host}{relativeUrl}";
        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart) => false;

       

        public void PostProcessPayment(PostProcessPaymentRequest request)
        {
            // Add Mastercard gateway redirect logic here
        }

        // Optional overrides (e.g., fees, validation)
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart) => 0;


        

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentMastercardGateway/Configure";
        }





        






        public Task<string> GetPaymentInfoHtmlAsync()
        {
            return Task.FromResult("<div>Redirecting to Mastercard...</div>");
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart) => Task.FromResult(false);
        public Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart) => Task.FromResult(0m);
      
      
     
        
        public Task ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest) => Task.CompletedTask;





       

       


        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest request)
        {
            return Task.FromResult(new CapturePaymentResult());
        }

        public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest request)
        {
            return Task.FromResult(new RefundPaymentResult());
        }

        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest request)
        {
            return Task.FromResult(new VoidPaymentResult());
        }

       

        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest request)
        {
            return Task.FromResult(new CancelRecurringPaymentResult());
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            return Task.FromResult(false);
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
        }

        public Type GetPublicViewComponent()
        {
            return typeof(PaymentMastercardViewComponent);
        }


        
        public   Task<string> GetPaymentMethodDescriptionAsync()
        {
            return Task.FromResult("Pay securely via Mastercard.");
        }

        Task<ProcessPaymentResult> IPaymentMethod.ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult
            {
               
                NewPaymentStatus = PaymentStatus.Pending
            };

            return Task.FromResult(result);
        }

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult
            {

                NewPaymentStatus = PaymentStatus.Pending
            };

            return Task.FromResult(result);
        }

        public bool SkipPaymentInfo => false;


       

    }
}