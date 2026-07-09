using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SevenSpikes.Nop.Plugins.SmartProductCollections.Domain;
using SevenSpikes.Nop.Framework.Domain.Enums;
using Nop.Web.Models.Catalog;


namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention
{
    //public enum EntityType
    //{
    //    Category,
    //    Manufacturer,
    //    Vendor,
    //    Product,
    //    Homepage
        
    //}

    public enum ErrorType
    {
        Ok = 200,
        NotOk = 400,
        AuthenticationError = 600
    }
    public class BaseResponseModel
    {

        public BaseResponseModel()
        {
            StatusCode = (int)ErrorType.Ok;
            ErrorList = new List<string>();
        }

        public string SuccessMessage { get; set; }
        public int StatusCode { get; set; }
        public List<string> ErrorList { get; set; }
    }
    public class GeneralResponseModel<TResult> : BaseResponseModel
    {
        public TResult Data { get; set; }
    }
    public class EntityWidgetMappingModel
    {
        public int EntityId { get; set; }
        public string WidgetZone { get; set; }
        public int DisplayOrder { get; set; }
    }
    
    public class HomePageCarouselResponseModel : GeneralResponseModel<IList<HomePageCarouselResponseModel.BannerModel>>
    {
        public HomePageCarouselResponseModel()
        {
        }
        public class BannerModel
        {
            public BannerModel()
            {
               
            Products = new List<ProductOverviewModel>();
                
                EntityWidgetMappings = new List<EntityWidgetMappingModel>();
            }

            public string Name { get; set; }
            public string Title { get; set; }
            public bool ShowTitle { get; set; }
            public EntityType EntityType { get; set; }
            public int CategoryID { get; set; }
            public bool ShowViewAll { get; set; }

            public int OrderNum { get; set; }
            public int DisplayOrder { get; set; }
            

            public IList<EntityWidgetMappingModel> EntityWidgetMappings { get; set; }
            public IList<ProductOverviewModel> Products { get; set; }

            public IList<CategoryModel> Categories { get; set; }
            public IList<ManufacturerModel> Manufactures { get; set; }

            public List<HomePageBannerResponseModel.BannerSliderModel> SliderImages { get; set; }
        }
    }



    public class HomePageBannerResponseModel : GeneralResponseModel<IList<HomePageBannerResponseModel.BannerSliderModel>>
    {
        public bool IsEnabled { get; set; }
        public string Text1 { get; set; }
        public string Text2 { get; set; }
        public string Text3 { get; set; }
        public string Text4 { get; set; }
        public string Text5 { get; set; }
        public string Text6 { get; set; }

        #region nested class
        public class BannerSliderModel
        {
            public string MobileImageUrl { get; set; }
            public string ImageUrl { get; set; }
            public string Text { get; set; }
            public string Alt { get; set; }
            public string Link { get; set; }

            public int IsProduct { get; set; }

            public string ProdOrCatId { get; set; }
            public int? ImageWidth { get; set; }
            public int? ImageHeight { get; set; }
            public int DisplayOrder { get; set; }

        }
        #endregion
    }


}
