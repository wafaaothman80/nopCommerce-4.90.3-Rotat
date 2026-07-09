using System.Collections.Generic;
using Autofac;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.Mapper;



using Nop.Web.Models.Catalog;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.ShoppingCart;
using static Nop.Web.Models.ShoppingCart.MiniShoppingCartModel;
using static NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention.ShoppingCartModelDto;


namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention;

/// <summary>
/// Represents AutoMapper configuration for plugin models
/// </summary>
public class MapperConfiguration : Profile
{
    #region Ctor

    public MapperConfiguration()
    {
         
        CreateMap<CheckoutConfirmModel, CheckoutConfirmModelDto>();
 CreateMap<ShoppingCartModel, ShoppingCartModelDto>();

        CreateMap<Nop.Web.Models.ShoppingCart.ShoppingCartModel, ShoppingCartModelDto>();
        CreateMap<Nop.Web.Models.ShoppingCart.ShoppingCartModel.ShoppingCartItemModel, ShoppingCartItemModelDto>();

        CreateMap<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem, SelectListItemDto>();
        CreateMap<Nop.Web.Models.ShoppingCart.ShoppingCartModel.DiscountBoxModel, DiscountBoxModelDto>();
        CreateMap<Nop.Web.Models.ShoppingCart.ShoppingCartModel.GiftCardBoxModel, GiftCardBoxModelDto>();
        CreateMap<Nop.Web.Models.ShoppingCart.ShoppingCartModel.OrderReviewDataModel, OrderReviewDataModelDto>();
        CreateMap<Nop.Web.Models.ShoppingCart.OrderTotalsModel.TaxRate, TaxRateDto>();
        CreateMap<Nop.Web.Models.Common.AddressModel, AddressModelDto>();

        CreateMap<CategoryModel, CategoryApiModel>();
        CreateMap<CategoryModel.SubCategoryModel, CategoryApiModel.SubCategoryModel>();
        CreateMap<ProductOverviewModel, ProductApiOverviewModel>();
        CreateMap<Nop.Web.Models.Media.PictureModel, PictureModelDto>();
        CreateMap<OrderTotalsModel, OrderTotalsModelDto>()
    .ForMember(d => d.GiftCards, o => o.MapFrom(s => s.GiftCards ?? new List<OrderTotalsModel.GiftCard>()));

        CreateMap<OrderTotalsModel.GiftCard, GiftCardDto>();

    }

    #endregion

    #region Properties

    /// <summary>
    /// Order of this mapper implementation
    /// </summary>
    public int Order => 1;

    #endregion
}


