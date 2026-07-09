using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Shipping;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.ShoppingCart;
using static NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention.ShoppingCartModelDto;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention
{
    public partial class CheckoutConfirmModelDto 
    {
        public bool TermsOfServiceOnOrderConfirmPage { get; set; }
        public bool TermsOfServicePopup { get; set; }
        public string MinOrderTotalWarning { get; set; }
        public ShoppingCartModelDto ShoppingCart { get; set; }
        public IList<string> Warnings { get; set; }
        public OrderTotalsModelDto OrderTotals { get; set; }
    }
    public partial class DiscountInfoModelDto 
    {
        public string CouponCode { get; set; }
    }
    public partial class DiscountBoxModelDto 
    {
       
        public bool Display { get; set; }
        public string Message { get; set; }
        public bool IsApplied { get; set; }
        public string CurrentCode { get; set; }

    }
    public partial class GiftCardBoxModelDto 
    {
        public bool Display { get; set; }
        public string Message { get; set; }
        public string CurrentCode { get; set; }
    }





    public partial class AddressModelDto
    {
        public virtual int Id { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public bool CompanyEnabled { get; set; }

        public bool CompanyRequired { get; set; }

        public string Company { get; set; }

        public bool CountryEnabled { get; set; }

        public int? CountryId { get; set; }

        public string CountryName { get; set; }

        public bool StateProvinceEnabled { get; set; }

        public int? StateProvinceId { get; set; }

        public string StateProvinceName { get; set; }

        public bool CountyEnabled { get; set; }

        public bool CountyRequired { get; set; }

        public string County { get; set; }

        public bool CityEnabled { get; set; }

        public bool CityRequired { get; set; }

        public string City { get; set; }

        public bool StreetAddressEnabled { get; set; }

        public bool StreetAddressRequired { get; set; }

        public string Address1 { get; set; }

        public bool StreetAddress2Enabled { get; set; }

        public bool StreetAddress2Required { get; set; }

        public string Address2 { get; set; }

        public bool ZipPostalCodeEnabled { get; set; }

        public bool ZipPostalCodeRequired { get; set; }

        public string ZipPostalCode { get; set; }

        public bool PhoneEnabled { get; set; }

        public bool PhoneRequired { get; set; }

        public string PhoneNumber { get; set; }

        public bool FaxEnabled { get; set; }

        public bool FaxRequired { get; set; }

        public string FaxNumber { get; set; }
        // Add other properties as needed from Nop.Web.Models.Common.AddressModel
    }

    public partial class AddressAttributeModelDto 
    {
        public string ControlId { get; set; }

        public string Name { get; set; }

        public bool IsRequired { get; set; }

        public string DefaultValue { get; set; }

        public AttributeControlType AttributeControlType { get; set; }

        public IList<AddressAttributeValueModelDto> Values { get; set; }
    }
    public partial class AddressAttributeValueModelDto 
    {
        public string Name { get; set; }

        public bool IsPreSelected { get; set; }
    }
    public class OrderReviewDataModelDto
    {
        public bool Display { get; set; }

        public AddressModelDto BillingAddress { get; set; }

        public bool IsShippable { get; set; }

        public AddressModelDto ShippingAddress { get; set; }

        public bool SelectedPickupInStore { get; set; }

        public AddressModelDto PickupAddress { get; set; }

        public string ShippingMethod { get; set; }

        public string PaymentMethod { get; set; }

        public Dictionary<string, object> CustomValues { get; set; }
        //public bool Display { get; set; }
        //public string BillingAddressHtml { get; set; }
        //public string ShippingAddressHtml { get; set; }
        //public string PaymentMethod { get; set; }
        //public string ShippingMethod { get; set; }
        //public bool IsShippable { get; set; }
    }

    public partial class ShoppingCartModelDto 
    {
        public bool OnePageCheckoutEnabled { get; set; }

        public bool ShowSku { get; set; }

        public bool ShowProductImages { get; set; }

        public bool IsEditable { get; set; }

        public IList<ShoppingCartItemModelDto> Items { get; set; }

        public IList<CheckoutAttributeModelDto> CheckoutAttributes { get; set; }

        public IList<string> Warnings { get; set; }

        public string MinOrderSubtotalWarning { get; set; }

        public bool DisplayTaxShippingInfo { get; set; }

        public bool TermsOfServiceOnShoppingCartPage { get; set; }

        public bool TermsOfServiceOnOrderConfirmPage { get; set; }

        public bool TermsOfServicePopup { get; set; }

        public DiscountBoxModelDto DiscountBox { get; set; }

        public GiftCardBoxModelDto GiftCardBox { get; set; }

        public OrderReviewDataModelDto OrderReviewData { get; set; }

        public bool HideCheckoutButton { get; set; }

        public bool ShowVendorName { get; set; }

        #region Nested Classes


        public partial class PictureModelDto 
        {
            public string ImageUrl { get; set; }

            public string ThumbImageUrl { get; set; }

            public string FullSizeImageUrl { get; set; }

            public string Title { get; set; }

            public string AlternateText { get; set; }
        }
        public partial class SelectListGroupDto 
        {
            public bool Disabled { get; set; }

            public string Name { get; set; }
        }
        public class SelectListItemDto
        {
            public string Text { get; set; }
            public string Value { get; set; }
            public bool Selected { get; set; }
        }
        public partial class ShoppingCartItemModelDto 
        {
            public string Sku { get; set; }

            public string VendorName { get; set; }

            public PictureModelDto Picture { get; set; }

            public int ProductId { get; set; }

            public string ProductName { get; set; }

            public string ProductSeName { get; set; }

            public string UnitPrice { get; set; }
            public decimal UnitPriceValue { get; set; }

            public string SubTotal { get; set; }
            public decimal SubTotalValue { get; set; }

            public string Discount { get; set; }
            public decimal DiscountValue { get; set; }

            public int? MaximumDiscountedQty { get; set; }

            public int Quantity { get; set; }

            public List<SelectListItemDto> AllowedQuantities { get; set; }

            public string AttributeInfo { get; set; }

            public string RecurringInfo { get; set; }

            public string RentalInfo { get; set; }

            public bool AllowItemEditing { get; set; }

            public bool DisableRemoval { get; set; }

            public IList<string> Warnings { get; set; }
        }

        #endregion
    }

    public partial class CheckoutAttributeModelDto 
    {
        public string Name { get; set; }

        public string DefaultValue { get; set; }

        public string TextPrompt { get; set; }

        public bool IsRequired { get; set; }

        public int? SelectedDay { get; set; }

        public int? SelectedMonth { get; set; }

        public int? SelectedYear { get; set; }

        public IList<string> AllowedFileExtensions { get; set; }

      
    }


    public partial class OrderTotalsModelDto 
    {
        public bool IsEditable { get; set; }

        public string SubTotal { get; set; }

        public string SubTotalDiscount { get; set; }

        public string Shipping { get; set; }

        public bool RequiresShipping { get; set; }

        public string SelectedShippingMethod { get; set; }

        public bool HideShippingTotal { get; set; }

        public string PaymentMethodAdditionalFee { get; set; }

        public string Tax { get; set; }

        public IList<TaxRateDto> TaxRates { get; set; }

        public bool DisplayTax { get; set; }

        public bool DisplayTaxRates { get; set; }

        public IList<GiftCardDto> GiftCards { get; set; }

        public string OrderTotalDiscount { get; set; }

        public int RedeemedRewardPoints { get; set; }

        public string RedeemedRewardPointsAmount { get; set; }

        public int WillEarnRewardPoints { get; set; }

        public string OrderTotal { get; set; }
    }


    public partial class TaxRateDto 
    {
        public string Rate { get; set; }
        public string Value { get; set; }
    }


    public partial class GiftCardDto 
    {
        public string CouponCode { get; set; }

        public string Amount { get; set; }

        public string Remaining { get; set; }
    }
    public class RecalculateShippingResponse
    {
        public IList<ShippingOption> ShippingOptions { get; set; }
        public OrderTotalsModel OrderTotals { get; set; }
    }

}
