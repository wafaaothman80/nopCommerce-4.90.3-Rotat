using System;
using System.Collections.Generic;

namespace Nop.Plugin.AccountManager.Models
{
    public class AccountManagerDashboardModel
    {
        public string CustomerName { get; set; }
        public DateTime CurrentDateTime { get; set; }

        public decimal DailyOrderTotal { get; set; }
        public string CurrencyCode { get; set; }

        public AccountManagerContactModel Contact { get; set; } = new AccountManagerContactModel();

        public IList<OrderItemModel> Orders { get; set; } = new List<OrderItemModel>();
    }

  

    public class OrderItemModel
    {
        public DateTime OrderDate { get; set; }
        public string OrderNumber { get; set; }
        public decimal OrderTotal { get; set; }
        public string Status { get; set; }
        public string ShippingMethod { get; set; }
        public string TrackingUrl { get; set; }
    }
}