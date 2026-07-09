using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Data;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.AccountManager.Models;
using Nop.Plugin.AccountManager.Models;
using Nop.Services.Affiliates;
using Nop.Services.Authentication;
using Nop.Services.Blogs;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Events;
using Nop.Services.ExportImport;
using Nop.Services.Forums;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Helpers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.News;
using Nop.Services.Orders;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Polls;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Tax;
using Nop.Services.Topics;
using Nop.Services.Vendors;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.AccountManager.Components
{
    public class AccountManagerContactViewComponent : ViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly INopDataProvider _dataProvider;

        private readonly IOrderService _orderService;
        private readonly IShipmentService _shipmentService;
        private readonly IStoreContext _storeContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;

        public AccountManagerContactViewComponent(
            IWorkContext workContext,
            INopDataProvider dataProvider,
            IOrderService orderService,
            IShipmentService shipmentService,
            IStoreContext storeContext,
            IDateTimeHelper dateTimeHelper,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter)
        {
            _workContext = workContext;
            _dataProvider = dataProvider;
            _orderService = orderService;
            _shipmentService = shipmentService;
            _storeContext = storeContext;
            _dateTimeHelper = dateTimeHelper;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null || customer.Username==null)
                return Content(string.Empty);

            var store = await _storeContext.GetCurrentStoreAsync();

            // customer local time
            var userNowUtc = DateTime.UtcNow;
            var userNow = await _dateTimeHelper.ConvertToUserTimeAsync(userNowUtc, DateTimeKind.Utc);

           
            var currency = await _workContext.GetWorkingCurrencyAsync();
            var currencyCode = currency?.CurrencyCode ?? "EUR";

           
            var contact = await GetAccountManagerContactAsync(customer.Id);

           
            var orders = await _orderService.SearchOrdersAsync(
                storeId: store.Id,
                customerId: customer.Id,
                pageIndex: 0,
                pageSize: 10);

            var orderModels = new List<OrderItemModel>();

            foreach (var o in orders)
            {
               
                var orderDateUser = await _dateTimeHelper.ConvertToUserTimeAsync(o.CreatedOnUtc, DateTimeKind.Utc);

               
                var shippingMethod = string.IsNullOrWhiteSpace(o.ShippingMethod) ? "—" : o.ShippingMethod;

                
                var status = o.OrderStatus.ToString();

               
                var trackingUrl = await BuildTrackingUrlAsync(o.Id);

              
                var totalInWorking = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(o.OrderTotal, currency);
              
                orderModels.Add(new OrderItemModel
                {
                    OrderDate = orderDateUser,
                    OrderNumber = o.CustomOrderNumber,
                    OrderTotal = totalInWorking,
                    Status = status,
                    ShippingMethod = shippingMethod,
                    TrackingUrl = trackingUrl
                });
            }

          
            var userTodayStart = userNow.Date;
            var userTomorrowStart = userTodayStart.AddDays(1);

            var startUtc =  _dateTimeHelper.ConvertToUtcTime(userTodayStart, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endUtc =  _dateTimeHelper.ConvertToUtcTime(userTomorrowStart, await _dateTimeHelper.GetCurrentTimeZoneAsync());

            var todayOrders = await _orderService.SearchOrdersAsync(
                storeId: store.Id,
                customerId: customer.Id,
                createdFromUtc: startUtc,
                createdToUtc: endUtc,
                pageIndex: 0,
                pageSize: 1000);

            decimal dailyTotalWorking = 0m;
            foreach (var o in todayOrders)
            {
                dailyTotalWorking += await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(o.OrderTotal, currency);
            }

            var model = new AccountManagerDashboardModel
            {
                CustomerName = string.IsNullOrWhiteSpace(customer.FirstName) ? customer.Username : customer.FirstName,
                CurrentDateTime = userNow,
                DailyOrderTotal = dailyTotalWorking,
                CurrencyCode = currencyCode,
                Contact = contact,
                Orders = orderModels
            };

            return View(
                "~/Plugins/Nop.Plugin.AccountManager/Views/Shared/Components/AccountManagerContact/Default.cshtml",
                model
            );
        }

        private async Task<AccountManagerContactModel> GetAccountManagerContactAsync(int customerId)
        {
            var pCustomerId = new DataParameter("@CustomerId", customerId);

            var row = (await _dataProvider.QueryProcAsync<AccountManagerContactRow>(
                "GetCustomerAccountManagerContact",
                new[] { pCustomerId }
            )).FirstOrDefault();

            if (row == null)
                return new AccountManagerContactModel(); 

            return new AccountManagerContactModel
            {
                Name = row.AccountManagerName,
                Email = row.Email,
                Phone = row.Phone,
                PictureUrl = string.IsNullOrWhiteSpace(row.PictureUrl)
                    ? "/images/default-avatar.png"
                    : row.PictureUrl
            };
        }

        private async Task<string> BuildTrackingUrlAsync(int orderId)
        {
           
            var shipments = await _shipmentService.GetShipmentsByOrderIdAsync(orderId);
            var last = shipments?.OrderByDescending(s => s.CreatedOnUtc).FirstOrDefault();

            if (last == null)
                return null;

            if (string.IsNullOrWhiteSpace(last.TrackingNumber))
                return null;

            return Url.RouteUrl("OrderDetails", new { orderId = orderId });
        }

        private class AccountManagerContactRow
        {
            public string AccountManagerName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }

          
            public string PictureUrl { get; set; }
        }
    }
}