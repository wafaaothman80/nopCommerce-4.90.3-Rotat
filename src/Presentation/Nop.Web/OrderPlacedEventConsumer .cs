using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Services.Orders;

namespace Nop.Web
{
    public class OrderShippingDateEventConsumer :
        IConsumer<OrderPlacedEvent>,
        IConsumer<OrderPaidEvent>
    {
        private readonly IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeParser;
        private readonly IAttributeService<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeService;
        private readonly IOrderService _orderService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INopDataProvider _dataProvider;
        private readonly Nop.Services.Logging.ILogger _logger;

        private const string SHIPPING_PREFERENCE_ATTR = "Shipping Preference";
        private const string CHOOSE_DATE_VALUE = "Choose a shipping date";
        private const string SHIP_ASAP_VALUE = "Ship as soon as possible";
        private const string CUSTOM_PREFERENCE_KEY = "ShippingPreference";
        private const string CUSTOM_DATE_KEY = "RequestedShippingDate";
        private const string COOKIE_DATE = "nop_shipping_date";
        private const string FORM_DATE = "nop_shipping_date_selected";

        
        private const int VALUE_ID_ASAP = 1;
        private const int VALUE_ID_CHOOSE_DATE = 2;

        public OrderShippingDateEventConsumer(
            IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
            IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService,
            IOrderService orderService,
            IHttpContextAccessor httpContextAccessor,
            INopDataProvider dataProvider,
            Nop.Services.Logging.ILogger logger)
        {
            _checkoutAttributeParser = checkoutAttributeParser;
            _checkoutAttributeService = checkoutAttributeService;
            _orderService = orderService;
            _httpContextAccessor = httpContextAccessor;
            _dataProvider = dataProvider;
            _logger = logger;
        }

        public async Task HandleEventAsync(OrderPlacedEvent e) =>
            await SaveAsync(e.Order, "OrderPlacedEvent");

        public async Task HandleEventAsync(OrderPaidEvent e) =>
            await SaveAsync(e.Order, "OrderPaidEvent");

        private async Task SaveAsync(Order order, string src)
        {
            if (order == null)
                return;
            try
            {
                var fresh = await _orderService.GetOrderByIdAsync(order.Id);
                if (fresh == null)
                    return;

                // Parse value ID from XML
                var valueId = ParseValueId(fresh.CheckoutAttributesXml, attributeId: 1);
                _logger.Information(
                    $"{src}: Order {fresh.Id} — valueId={valueId} | xml='{fresh.CheckoutAttributesXml}'");

                if (!valueId.HasValue)
                    return;

                var preference = valueId == VALUE_ID_ASAP ? SHIP_ASAP_VALUE :
                                 valueId == VALUE_ID_CHOOSE_DATE ? CHOOSE_DATE_VALUE :
                                 await LookupValueNameAsync(valueId.Value);

                _logger.Information($"{src}: Order {fresh.Id} — preference='{preference}'");

                DateTime? date = null;
                if (valueId == VALUE_ID_CHOOSE_DATE)
                {
                    var raw = ReadDateRaw(src, fresh.Id);
                    date = ParseDate(raw);
                    _logger.Information($"{src}: Order {fresh.Id} — raw='{raw}' date={date:dd/MM/yyyy}");
                }

                // ── Use nopCommerce-compatible serialization ──
                var cv = DeserializeCustomValues(fresh.CustomValuesXml);
                cv[CUSTOM_PREFERENCE_KEY] = preference;
                if (date.HasValue)
                    cv[CUSTOM_DATE_KEY] = date.Value.ToString("dd/MM/yyyy");
                else
                    cv.Remove(CUSTOM_DATE_KEY);

                fresh.CustomValuesXml = SerializeCustomValues(cv);
                await _orderService.UpdateOrderAsync(fresh);

                _logger.Information(
                    $"{src}: Order {fresh.Id} — SAVED '{fresh.CustomValuesXml}'");
            }
            catch (Exception ex)
            {
                _logger.Error($"{src}: Order {order.Id} — {ex.Message}", ex);
            }
        }

       
        private int? ParseValueId(string xml, int attributeId)
        {
            if (string.IsNullOrEmpty(xml))
                return null;
            try
            {
                var doc = XDocument.Parse(xml);
                var attrEl = doc.Descendants("CheckoutAttribute").FirstOrDefault(el =>
                {
                    var a = el.Attribute("ID") ?? el.Attribute("Id") ?? el.Attribute("id");
                    return a != null && int.TryParse(a.Value, out var n) && n == attributeId;
                });
                var raw = attrEl?.Descendants("CheckoutAttributeValue")
                                 .FirstOrDefault()?.Element("Value")?.Value;
                return int.TryParse(raw, out var id) ? id : (int?)null;
            }
            catch (Exception ex) { _logger.Error($"ParseValueId: {ex.Message}", ex); return null; }
        }

        private async Task<string> LookupValueNameAsync(int id)
        {
            try
            {
                var r = await _dataProvider.QueryAsync<string>(
                    "SELECT Name FROM [dbo].[CheckoutAttributeValue] WHERE Id=@Id",
                    new LinqToDB.Data.DataParameter("@Id", id));
                return r.FirstOrDefault();
            }
            catch { return null; }
        }

        private string ReadDateRaw(string src, int orderId)
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx == null)
                return null;

            if (ctx.Request.Cookies.TryGetValue(COOKIE_DATE, out var cv) && !string.IsNullOrEmpty(cv))
            {
                var d = Uri.UnescapeDataString(cv);
                _logger.Information($"{src}: Order {orderId} — cookie='{d}'");
                return d;
            }

            if (ctx.Request.HasFormContentType)
            {
                var fv = ctx.Request.Form[FORM_DATE].FirstOrDefault();
                if (!string.IsNullOrEmpty(fv))
                    return fv;
            }

            _logger.Warning($"{src}: Order {orderId} — date not in cookie or form. " +
                $"Cookies: [{string.Join(", ", ctx.Request.Cookies.Keys)}]");
            return null;
        }

        private DateTime? ParseDate(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return null;
            foreach (var fmt in new[] { "dd/MM/yyyy", "dd/mm/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" })
                if (DateTime.TryParseExact(raw, fmt,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var d))
                    return d;
            return DateTime.TryParse(raw, out var fb) ? fb : (DateTime?)null;
        }

       
        private Dictionary<string, object> DeserializeCustomValues(string xml)
        {
            var result = new Dictionary<string, object>();
            if (string.IsNullOrEmpty(xml))
                return result;
            try
            {
                var doc = XDocument.Parse(xml);
                // Handles both <DictionarySerializer> and legacy <dictionary> formats
                foreach (var item in doc.Descendants("item"))
                {
                    var k = item.Element("key")?.Value;
                    var v = item.Element("value")?.Value;
                    if (!string.IsNullOrEmpty(k))
                        result[k] = v;
                }
            }
            catch (Exception ex) { _logger.Error($"DeserializeCustomValues: {ex.Message}", ex); }
            return result;
        }

        private string SerializeCustomValues(Dictionary<string, object> d)
        {
            if (d == null || !d.Any())
                return string.Empty;

           
            var items = d.Select(kv =>
                $"<item>" +
                $"<key>{System.Security.SecurityElement.Escape(kv.Key)}</key>" +
                $"<value>{System.Security.SecurityElement.Escape(kv.Value?.ToString())}</value>" +
                $"</item>");

           
            return $"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
                   $"<DictionarySerializer>{string.Join("", items)}</DictionarySerializer>";
        }
    }






public class OrderStatusChangedConsumer :
    IConsumer<OrderPlacedEvent>,          
    IConsumer<OrderStatusChangedEvent>
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        public OrderStatusChangedConsumer(
            IProductService productService,
            IOrderService orderService)
        {
            _productService = productService;
            _orderService = orderService;
        }

        
        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            var order = eventMessage.Order;

           
            if (order.PaymentStatus != Nop.Core.Domain.Payments.PaymentStatus.Pending)
                return;

            foreach (var item in await _orderService.GetOrderItemsAsync(order.Id))
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (product != null &&
                    product.ManageInventoryMethod != Nop.Core.Domain.Catalog.ManageInventoryMethod.DontManageStock)
                {
                    
                    await _productService.AdjustInventoryAsync(
                        product,
                        +item.Quantity,       
                        item.AttributesXml,
                        $"Pending order #{order.Id} – stock restored until payment confirmed");
                }
            }
        }

        
        public async Task HandleEventAsync(OrderStatusChangedEvent eventMessage)
        {
            var order = eventMessage.Order;
            var previousStatus = eventMessage.PreviousOrderStatus;
            var newStatus = order.OrderStatus;

           
            if (previousStatus == OrderStatus.Pending &&
                (newStatus == OrderStatus.Processing || newStatus == OrderStatus.Complete))
            {
                foreach (var item in await _orderService.GetOrderItemsAsync(order.Id))
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    if (product != null &&
                        product.ManageInventoryMethod != Nop.Core.Domain.Catalog.ManageInventoryMethod.DontManageStock)
                    {
                       
                        await _productService.AdjustInventoryAsync(
                            product,
                            -item.Quantity,   
                            item.AttributesXml,
                            $"Order #{order.Id} confirmed – stock deducted");
                    }
                }
            }

           
            if (previousStatus == OrderStatus.Pending &&
                newStatus == OrderStatus.Cancelled)
            {
               
                foreach (var item in await _orderService.GetOrderItemsAsync(order.Id))
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    if (product != null &&
                        product.ManageInventoryMethod != Nop.Core.Domain.Catalog.ManageInventoryMethod.DontManageStock)
                    {
                        await _productService.AdjustInventoryAsync(
                            product,
                            -item.Quantity,   
                            item.AttributesXml,
                            $"Order #{order.Id} cancelled – preventing double stock restore");
                    }
                }
            }
        }
    }

}