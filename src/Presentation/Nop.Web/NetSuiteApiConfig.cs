using Newtonsoft.Json;

namespace Nop.Web
{
    public class NetSuiteApiConfig : IApiConfig
    {
       public string AccountId { get; } = "5350207_SB1";
        // live public string AccountId { get; } = "5350207";
        public string ClientId { get; } = "93b5d049fce4c892f2d2cb24a9881ce322ce2f2136254164212d03200ced9ce1";
        // live public string ClientId { get; } = "d77543ed1f3d00df2f7fde8a412d9c695c6fdd30ca2d115e5504f8532015c33e";
       public string ClientSecret { get; } = "5bf7acf0443596e333ce38bd8ca6dfed58b1b57a4b09eb85d11abdad5a97eb1a";

         public string TokenId { get; } = "9936522a3569be5eab853d2eb3fa5361e3b1884b67fed9b540a6ed23953f8746";

  //live public string TokenId { get; } = "eda800fe93bc9b500de24ffac17ba626e4300e0145040fe6bfe1b33ac1f9359c";
      // public string TokenSecret { get; } = "bcacf53446a35a68513e26a71b49afb9f3ec3bc25b932bd4e4921c281672df49";
   public string TokenSecret { get; } = "f168159e28feded5ff7f26765b9769b72aa6f7dcadd7e566f7f31c6674c19c6e";
   // live public string TokenSecret { get; } = "bc47fec4526f53b856503bc577dc6ca38283a2d1d520f1bb9ff71b718dd3397d";

        //live public string ApiRoot { get; } = $"https://5350207.restlets.api.netsuite.com/app/site/hosting/restlet.nl?script=1721&deploy=1";
        public string ApiRoot { get; } = $"https://5350207-sb1.restlets.api.netsuite.com/app/site/hosting/restlet.nl?script=1721&deploy=1";


    }


    public class ItemCategoryModel
    {
        public int ns_id { get; set; }
        public string name { get; set; }
       


    }

    public class ItemBrandModel
    {
        public int ns_id { get; set; }
        public string brand_name { get; set; }



    }


    public class ItemModel
    {
        


        public int ns_id { get; set; }
        public string itemid { get; set; }
        public string part_no { get; set; }
        public int? stock_type_id { get; set; }
        public string stock_type_name { get; set; }
        public string displayname { get; set; }
        public int? brand_id { get; set; }
        public string brand_name { get; set; }
       
        public int? origin_id { get; set; }
        public string origin_name { get; set; }
        public int? category_id { get; set; }
        public string category_name { get; set; }
        public int? subcategory { get; set; }
        public string subcategory_name { get; set; }
        public int? hs_code { get; set; }
        public string hs_code_name { get; set; }
        public int? process_family_id { get; set; }
        public string process_family_name { get; set; }
        public int? processgroup_id { get; set; }
        public string processgroup_name { get; set; }
        public string rotationtype_id { get; set; }
        public string rotationtype_name { get; set; }
        public int? line_of_business_id { get; set; }

    


        public int? length_in_mm { get; set; }
        public string line_of_business_name { get; set; }
  
        public int? dubai_trade_category_id { get; set; }
        public string dubai_trade_category_name { get; set; }

        public object description { get; set; }
        public decimal? last_purchase_price { get; set; }
        public int? quantity_available { get; set; }
        public int? reserved_quantity { get; set; }



    }


public class substitutesItem
    {
        public int ns_id { get; set; }

        [JsonProperty("product_id")]
        public int? product_id { get; set; }

        public int? stock_code { get; set; }

        public string item_code { get; set; }

        public string substitute_code { get; set; }

        public string substitute_type { get; set; }

        public string description { get; set; }
    }
    public class ItemDetailsModel
    {
        public int ns_id { get; set; }
        public string itemid { get; set; }
        public string part_no { get; set; }
        public int? stock_type_id { get; set; }
        public string stock_type_name { get; set; }
        public string displayname { get; set; }
        public int? brand_id { get; set; }
        public string brand_name { get; set; }
        public int? origin_id { get; set; }
        public string origin_name { get; set; }
        public int? category_id { get; set; }
        public string category_name { get; set; }
        public int ?subcategory { get; set; }
        public string subcategory_name { get; set; }
        public double? weight_in_kg { get; set; }
        public int ?hs_code { get; set; }
        public string hs_code_name { get; set; }
        public int? process_family_id { get; set; }
        public string process_family_name { get; set; }
        public int? processgroup_id { get; set; }
        public string processgroup_name { get; set; }
        public string rotationtype_id { get; set; }
        public string rotationtype_name { get; set; }
        public int? line_of_business_id { get; set; }
        public string line_of_business_name { get; set; }
        public int? dubai_trade_category_id { get; set; }
        public string dubai_trade_category_name { get; set; }
 public string description { get; set; }

        
       
        public string inner_diameter_in_mm { get; set; }
        public string outer_diameter_in_mm { get; set; }
        public string thickness { get; set; }
        
        public string height_in_mm { get; set; }
        public decimal? width_in_kg { get; set; }
        public string packing_dimensions { get; set; }
        public string depth_in_mm { get; set; }
        public string width_outer_ring_in_mm { get; set; }
        public string bore_diameter { get; set; }
        public string carton_qty { get; set; } 
        public string size_qty { get; set; }
        public List<substitutesItem> substitutes { get; set; }

    public    decimal? basePrice { get; set; }
  public    int? reservedQuantity { get; set; }
 public    int? stockQuantity { get; set; }

    }



    public class AddressStatus
    {
        public bool success { get; set; }
        public string message { get; set; }
        public List<Datum> data { get; set; }
    }

    public class Datum
    {
        public string address { get; set; }
        public string status { get; set; }
    }

    public class CustomerRegisterRModel
    {
        public int ns_id { get; set; }
        public string message { get; set; }
        public List<AddressStatus> address_status { get; set; }
    }

    public class C_Address
    {
        public int ns_id { get; set; }
        public object firstname { get; set; }
        public object lastname { get; set; }
        public string email { get; set; }
        public string company { get; set; }
        public string country { get; set; }
        public object stateprovincename { get; set; }
        public string city { get; set; }
        public string address_1 { get; set; }
        public object address_2 { get; set; }
        public object zippostalcode { get; set; }
        public object phonenumber { get; set; }
        public string faxnumber { get; set; }
    }

    public class CustomerByPhoneOrEmail
    {
        public int ns_id { get; set; }
        public object first_name { get; set; }
        public object last_name { get; set; }
        public string company_name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public object zip { get; set; }
        public List<C_Address> addresses { get; set; }
    }



    public class StockItemDetailsModel
    {
        public int erpproductid { get; set; }
        public string itemid { get; set; }
        public decimal? baseprice { get; set; }
        public int? reservedquantity { get; set; }
        public int stockquantity { get; set; }

    }
    public class AccountManagerDto
    {
        public int Id { get; set; }
        public string AccountManagerName { get; set; }
        public int ERPAccountManagerId { get; set; }
    }

    public class SalesRepItem
    {
        public int id { get; set; }
        public string entityId { get; set; }
        public string salesRepName { get; set; }
        public bool isPrimary { get; set; }
    }

    public class SalesRepResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public List<SalesRepItem> data { get; set; }
    }

    public class ChargeCustomerCreditResponseModel
    {
        public bool status { get; set; }
        public string message { get; set; }
        public long? depositId { get; set; }
        public string depositNumber { get; set; }
        public object exception { get; set; }
    }

    public class OredrResponceModel
    {
        public int ns_id { get; set; }
        public string message { get; set; }
        public List<VslResponce> vsl_responce { get; set; }
    }

    public class VslResponce
    {
        public int ns_id { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
    }


}