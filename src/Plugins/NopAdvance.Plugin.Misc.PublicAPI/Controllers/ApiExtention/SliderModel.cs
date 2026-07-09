using System;
using System.Collections.Generic;

using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Order;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention
{

    public class SliderGroupModel
    {
        public int SliderId { get; set; }
        public List<SliderModel> Slides { get; set; }
    }
    public record SliderModel : BaseNopModel
    {
      
        public int Id { get; set; }
        public string Url { get; set; }
        public int MobilePictureId { get; set; }
  public int PictureId { get; set; }
 public int SliderId { get; set; }
        public string SystemName { get; set; }
        public string Alt { get; set; }
        
        public string MobilePictureUrl { get; set; }


    }

    public record SildeProdut : BaseNopModel
    {

        public int ProId { get; set; }
       


    }
    public record Notification : BaseNopModel
    {

     
        public string Url { get; set; }
       

        public string Type { get; set; }
        public string Title { get; set; }

      


    }


    public partial class DeleteAccountModelDto 
    {
        public string CurrentPassword { get; set; }

       
    }


    public record MenuRootModel
    {

        public int id { get; set; }
        public int EntityId { get; set; }
        public string title { get; set; }
        public string LocalizedTitle { get; set; }
    public string CustomLinksRef { get; set; }
        public int CatalogTemplate { get; set; }

    }

   


    public partial class CategoryMenuDto 
    {
        public int MenuId { get; set; }
        public int id { get; set; }
        public string title { get; set; }
     
        public string image { get; set; }
         public int type { get; set; }
        
       public string CustomLinksPath { get; set; }
        public string CustomLinksRef { get; set; }



    }



    public class ShipmentInfo
    {
        public string Shipment { get; set; }
        public string Status { get; set; }
      
        
        public List<ProductInfo> Products { get; set; } = new();
    }

    public class ProductInfo
    {
        public string Image { get; set; }
        public string Name { get; set; }
        public string Quantity { get; set; }
    }
    public class OrderDetailsModelWithShipments
    {
        public OrderDetailsModel _OrderDetailsMode { get; set; }
        public List<OrderShipmentInfo> _ShipmentInfo { get; set; } = new();



    }

    public class OrderShipmentInfo
    {
       
       public int Id { get; set; }
        public string TrackingNumber { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? ReadyForPickupDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public List<ProductOverviewModel> Products { get; set; } = new();
    }

    public class FcmTokenRequest
    {
        public string Token { get; set; }
    }



}
