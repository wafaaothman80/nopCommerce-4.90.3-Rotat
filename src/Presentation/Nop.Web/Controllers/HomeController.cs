using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

using DocumentFormat.OpenXml.Spreadsheet;
using FirebirdSql.Data.Services;
using LinqToDB.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Data.DataProviders;
using Nop.Services.Authentication;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Vendors;


using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;
using StackExchange.Redis;
using static LinqToDB.Sql;
using static Org.BouncyCastle.Math.EC.ECCurve;

using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace Nop.Web.Controllers;
public partial class HomeController : BasePublicController
{
    protected readonly HttpClient _httpClient;
    private static NetSuiteApiConfig _config;
    private static readonly RestClient _restClient;
    protected readonly ICustomerService _customerService;
    protected readonly IAuthenticationService _authenticationService;
    protected readonly IWorkContext _workContext;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly ILocalizationService _localizationService;

    protected readonly INotificationService _notificationService;
    protected readonly Nop.Services.Logging.ILogger _logger;
    protected readonly IWebHelper _webHelper;
    protected readonly ICustomNumberFormatter _customNumberFormatter;
    protected readonly IOrderService _orderService;
    protected readonly IProductService _productService;
    protected readonly IEmailSender _emailSender;
    protected readonly IEmailAccountService _emailAccountService;
    protected readonly IHttpClientFactory _httpClientFactory;
    public HomeController(HttpClient client, ICustomerService customerService, IAuthenticationService authenticationService, IWorkContext workContext, IHttpContextAccessor httpContextAccessor, ILocalizationService localizationService, INotificationService notificationService, Nop.Services.Logging.ILogger logger, IWebHelper webHelper
       , ICustomNumberFormatter customNumberFormatter, IOrderService orderService, IProductService productService,
        IEmailSender emailSender,
 IEmailAccountService emailAccountService, IHttpClientFactory httpClientFactory
    )
    {
        _httpClient = client;
        _customerService = customerService;
        _authenticationService = authenticationService;
        _workContext = workContext;
        _httpContextAccessor = httpContextAccessor;
        _localizationService = localizationService;

        _notificationService = notificationService;
        _logger = logger;
        _webHelper = webHelper;

        _customNumberFormatter = customNumberFormatter;
        _orderService = orderService;
        _productService = productService;
        _emailSender = emailSender;
        _emailAccountService = emailAccountService;
        _httpClientFactory = httpClientFactory;
    }
    static HomeController()
    {
        _config = new NetSuiteApiConfig();
        _restClient = CreateRestClient();

    }
    private static RestClient CreateRestClient()
    {


        var client = new RestClient();
        var oAuth1 = OAuth1Authenticator.ForAccessToken(
                        consumerKey: _config.ClientId,
                        consumerSecret: _config.ClientSecret,
                        token: _config.TokenId,
                        tokenSecret: _config.TokenSecret,
                        OAuthSignatureMethod.HmacSha256);

        oAuth1.Realm = _config.AccountId;

        client.Authenticator = oAuth1;
        return client;
    }



    public virtual IActionResult Index()
    {
        return View();



    }


    public virtual IActionResult GetLatestBrands(string DateFrom = "2020")
    {

        var url = _config.ApiRoot + "&objType=41&objId=0&lastModifiedFrom=" + DateFrom + "-01-01T01:00:00&lastModifiedTo=2026-12-31T23:59:59";


        var httpRequest = new RestRequest(url, Method.GET);

        var httpResponse = _restClient.ExecuteAsync(httpRequest);
        var responseJson = httpResponse.Result.Content;


        var Items = JsonConvert.DeserializeObject<List<ItemBrandModel>>(responseJson);



        foreach (var item in Items.ToList())
        {
            try
            {
                MsSqlNopDataProvider msSqlNopDataProvider = new MsSqlNopDataProvider();
                DataParameter[] parameters = { new DataParameter(name: "@BrandId", value: item.ns_id), new DataParameter(name: "@Name", value: item.brand_name) };

                _ = msSqlNopDataProvider.ExecuteNonQueryAsync("EXEC [dbo].InsertBrand " + item.ns_id + "," + item.brand_name);


            }
            catch { }
        }
        return View(Items);


    }
    public virtual IActionResult GetLatestCategories()
    {

        var url = _config.ApiRoot + "&objType=51&objId=0";


        var httpRequest = new RestRequest(url, Method.GET);

        var httpResponse = _restClient.ExecuteAsync(httpRequest);
        var responseJson = httpResponse.Result.Content;


        var Items = JsonConvert.DeserializeObject<List<ItemCategoryModel>>(responseJson);



        foreach (var item in Items.ToList())
        {
            try
            {
                MsSqlNopDataProvider msSqlNopDataProvider = new MsSqlNopDataProvider();
                DataParameter[] parameters = { new DataParameter(name: "@CatId", value: item.ns_id), new DataParameter(name: "@Name", value: item.name) };

                //   _ = msSqlNopDataProvider.ExecuteNonQueryAsync("EXEC [dbo].InsertCategory " + item.ns_id + "," + item.name);

                _ = msSqlNopDataProvider.QueryProcAsync<object>("InsertCategory", parameters);


            }
            catch { }
        }
        return View(Items);


    }






    public virtual async Task<IActionResult> GetLatestProduct(string DateFrom = "2020", string To = "2026")
    {
        var url = _config.ApiRoot
            + "&objType=31&objId=0"
            + "&lastModifiedFrom=" + DateFrom + "-01-01T01:00:00"
            + "&lastModifiedTo=" + To + "-12-30T23:59:59";








        //   var url = _config.ApiRoot + "&objType=31&objId=0&lastModifiedFrom="+ DateFrom + "-01-01T01:00:00&lastModifiedTo="+To+"-12-30T23:59:59";


        var httpRequest = new RestRequest(url, Method.GET);

        var httpResponse = _restClient.ExecuteAsync(httpRequest);
        var responseJson = httpResponse.Result.Content;


        var Items = JsonConvert.DeserializeObject<List<ItemModel>>(responseJson);
        //var _Items = Items.Where(x => x.ns_id> 69501).ToList();
        // var _Items = Items.Skip(77671).ToList();
        foreach (var item in Items)
        {
            try
            {

                var url2 = _config.ApiRoot + "&objType=32&objId=" + item.ns_id.ToString();


                var httpRequest2 = new RestRequest(url2, Method.GET);

                var httpResponse2 = await _restClient.ExecuteAsync(httpRequest2);
                var responseJson2 = httpResponse2.Content;

                var Item = JsonConvert.DeserializeObject<List<ItemDetailsModel>>(responseJson2).FirstOrDefault();




                try
                {

                    MsSqlNopDataProvider msSqlNopDataProvider33 = new MsSqlNopDataProvider();
                    MsSqlNopDataProvider msSqlNopDataProvider34 = new MsSqlNopDataProvider();

                    DataParameter parameter1 = new DataParameter("@ProId", item.ns_id);
                    DataParameter parameter2 = new DataParameter("@BrandId", item.brand_name);

                    DataParameter parameter3 = new DataParameter("@Name", Item.part_no);
                    if (item.subcategory == null || item.subcategory == 0)
                    {
                        item.subcategory = item.category_id;


                    }
                    if (item.category_id == null)
                    {
                        item.category_id = 0;
                    }
                    DataParameter parameter4 = new DataParameter("@SupCategoryId", item.subcategory);
                    DataParameter parameter5 = new DataParameter("@CategoryId", item.category_id);
                    DataParameter parameter6 = new DataParameter("@Sku", item.itemid);
                    if (item.last_purchase_price == null)
                    {
                        item.last_purchase_price = decimal.Zero;
                    }
                    DataParameter parameter7 = new DataParameter("@Price", Item.basePrice ?? 0);
                    DataParameter parameter77 = new DataParameter("@ProductCost", Item.basePrice ?? 0);
                    DataParameter parameter8 = new DataParameter("@ParentGroupedProductId", 0);
                    DataParameter parameter9 = new DataParameter("@ProductTypeId", 5);
                    DataParameter parameter10 = new DataParameter("@ProductTemplateId", 1);

                    DataParameter parameter11 = new DataParameter("@ManufacturerPartNumber", item.part_no);
                    DataParameter parameter12 = new DataParameter("@StockQuantity", Item.stockQuantity ?? 0);
                    DataParameter parameter13 = new DataParameter("@ReservedQuantity", Item.reservedQuantity ?? 0);
                    //
                    DataParameter parameter14 = new DataParameter("@CategoryName", Item.category_name);
                    DataParameter parameter144 = new DataParameter("@SubCategoryName", Item.subcategory_name);
                    DataParameter parameter15 = new DataParameter("@origin_name", Item.origin_name);
                    DataParameter parameter16 = new DataParameter("@hs_code_name", Item.hs_code_name);
                    DataParameter parameter17 = new DataParameter("@line_of_business_name", Item.line_of_business_name);
                    DataParameter parameter18 = new DataParameter("@size_qty", Item.size_qty);
                    DataParameter parameter19 = new DataParameter("@Weight", Item.weight_in_kg ?? 0.0);
                    DataParameter parameter20 = new DataParameter("@inner_diameter_in_mm", Item.inner_diameter_in_mm);
                    DataParameter parameter21 = new DataParameter("@outer_diameter_in_mm", Item.outer_diameter_in_mm);
                    DataParameter parameter22 = new DataParameter("@thickness", Item.thickness);
                    DataParameter parameter23 = new DataParameter("@stock_type_name", Item.stock_type_name);
                    DataParameter parameter24 = new DataParameter("@carton_qty", Item.carton_qty);


                    //_ = msSqlNopDataProvider33.ExecuteNonQueryAsync("EXEC [dbo].[InsertProduct] @ProId,@BrandId,@Name,@SupCategoryId,@CategoryId,@Sku,@Price,@ProductCost,@ParentGroupedProductId,@ProductTypeId,@ProductTemplateId,@ManufacturerPartNumber,@StockQuantity,@ReservedQuantity,@CategoryName,@origin_name,@hs_code_name ,@line_of_business_name,@size_qty,@Weight,@inner_diameter_in_mm,@outer_diameter_in_mm,@thickness,@stock_type_name,@carton_qty", parameter1, parameter2, parameter3, parameter4, parameter5, parameter6, parameter7, parameter77, parameter8, parameter9, parameter10, parameter11, parameter12, parameter13, parameter14, parameter15, parameter16, parameter17, parameter18, parameter19, parameter20, parameter21, parameter22, parameter23, parameter24);

                    _ = msSqlNopDataProvider33.ExecuteNonQueryAsync(
    "EXEC [dbo].[InsertProduct] @ProId,@BrandId,@Name,@SupCategoryId,@CategoryId,@Sku,@Price,@ProductCost,@ParentGroupedProductId,@ProductTypeId,@ProductTemplateId,@ManufacturerPartNumber,@StockQuantity,@ReservedQuantity,@CategoryName,@SubCategoryName,@origin_name,@hs_code_name,@line_of_business_name,@size_qty,@Weight,@inner_diameter_in_mm,@outer_diameter_in_mm,@thickness,@stock_type_name,@carton_qty",
    parameter1, parameter2, parameter3, parameter4, parameter5, parameter6, parameter7, parameter77,
    parameter8, parameter9, parameter10, parameter11, parameter12, parameter13,
    parameter14, parameter144,
    parameter15, parameter16, parameter17, parameter18, parameter19, parameter20,
    parameter21, parameter22, parameter23, parameter24
);

                    string nsIds = null;

                    if (Item.substitutes?.Count > 0)
                        nsIds = string.Join(",", Item.substitutes.Select(x => x.ns_id));

                    string substitutesJson = null;
                    if (Item.substitutes?.Count > 0)
                        substitutesJson = JsonConvert.SerializeObject(Item.substitutes);

                    DataParameter parameter101 = new DataParameter("@NsIds", nsIds);
                    DataParameter parameter102 = new DataParameter("@SubstitutesJson", substitutesJson);

                    var results = await msSqlNopDataProvider33.QueryAsync<dynamic>(
       "EXEC InsertSimilarProducts @ProId = @ProId, @NsIds = @NsIds ,@SubstitutesJson=@SubstitutesJson",
       parameter1,
       parameter101, parameter102
   );

                }
                catch (Exception ex)
                {

                    _logger.Error("From GetLatestProduct 1 Product Id" + item.ns_id + ex.Message.ToString(), ex);
                }
            }


            catch (Exception ex)
            {

                _logger.Error("From GetLatestProduct 2 Product Id" + item.ns_id + ex.Message.ToString(), ex);
            }
        }



        return View(Items);
    }


    public virtual async Task<IActionResult> RegisterCustomerOTPAsync(string CodePhoneNumber = "", string PhoneNumber = "")

    {
        CodePhoneNumber = Regex.Replace(CodePhoneNumber, @"\s+", "");

        PhoneNumber = Regex.Replace(PhoneNumber, @"\s+", "");
        ViewBag.PhoneNumber = "";
        var isRegisterCustomer = await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync());
        if (isRegisterCustomer)
        {

            await _authenticationService.SignOutAsync();
        }
        if (PhoneNumber != "")
        {
            ViewBag.PhoneNumber = CodePhoneNumber + PhoneNumber;



            var currentcustomer = await _customerService.GetCustomerByUsernameAsync(CodePhoneNumber + PhoneNumber);



            if (currentcustomer == null)
            {


                var FullPhoneNumber = CodePhoneNumber + PhoneNumber;

                TempData["UserName"] = FullPhoneNumber;


                return RedirectToAction("CustomerOTP", new { UserName = FullPhoneNumber });



            }
            else
            {
                ViewBag.CheckUserName = "Username already exists";
            }

        }



        return View();
    }




public virtual async Task<IActionResult> CustomerOTP(string UserName)
{
  
    var currentCustomer = await _workContext.GetCurrentCustomerAsync();

    var otp = RandomOnlyNumber();  
    var otpStatus = "Failed";

    try
    {
        TempData["UserName"] = UserName;

       
        using var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Dexatel-Key", "218e25c2e5939f7b92654303f0a50b9d");
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

    
        var payload = new
        {
            data = new
            {
                channel = "SMS",
                sender = "Dexatel",   
                phone = UserName,                 
                template = "9c9dcaa2-990a-45c2-9efd-ae0bd4d63cbf", 
                code = otp                          
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(
            "https://api.dexatel.com/v1/verifications", content);

       
        if (response.StatusCode == HttpStatusCode.Created)
        {
            otpStatus = "Done";
        }
        else
        {
        
            var errorBody = await response.Content.ReadAsStringAsync();
          
            otpStatus = "Failed";
        }
    }
    catch (Exception ex)
    {
        
        otpStatus = "Failed";
    }

  
    var msSqlNopDataProvider = new MsSqlNopDataProvider();
    DataParameter[] parameters =
    {
        new DataParameter(name: "@CID",       value: currentCustomer.Id),
        new DataParameter(name: "@otp",       value: otp),
        new DataParameter(name: "@OTPStatus", value: otpStatus),
        new DataParameter(name: "@UserName",  value: UserName)
    };

    var lastSentOtpList = await msSqlNopDataProvider.QueryProcAsync<DateTime>(
        "CustomerSaveOTP", parameters);

    var lastSentOtp = lastSentOtpList.FirstOrDefault();

   
    ViewBag.CID = currentCustomer.Id;
    ViewBag.CustomerDBOTP = otp;
    ViewBag.NewPhoneNumber = UserName;
    ViewBag.LastSentOTP = lastSentOtp;

    return View();
}





public virtual async Task<string> ConfirmCustomerOTP(int CID, string OTP, string NewPhoneNumber = "")

    {
        // string CustomerOTP = ViewBag.CustomerDBOTP?? TempData["CustomerDBOTP"];

        string result = ConfirmOTP(CID, OTP).Result;

        TempData["UserName"] = NewPhoneNumber;

        return result;
    }
    public virtual async Task<string> ConfirmOTP(int CID, string OTP)

    {

        string result = "Fail";

        var currentcustomer = await _customerService.GetCustomerByIdAsync(CID);


        if (currentcustomer != null)
        {

            MsSqlNopDataProvider msSqlNopDataProvider = new MsSqlNopDataProvider();
            DataParameter[] parameters = { new DataParameter(name: "@CID", value: currentcustomer.Id), new DataParameter(name: "@otp", value: OTP) };

            var LastSentOTP = await msSqlNopDataProvider.QueryProcAsync<DateTime>("GetConfirmOTP", parameters);


            DateTime pastDate = DateTime.Now.AddMinutes(-3);
            if (LastSentOTP.Count() > 0)
            {


                if (pastDate <= LastSentOTP.FirstOrDefault())
                {

                    currentcustomer.Active = true;
                    currentcustomer.Deleted = false;

                    await _customerService.UpdateCustomerAsync(currentcustomer);
                    result = "Done";

                }
                else
                {
                    result = "Fail";
                }

            }




        }

        return result;
    }


    public string RandomOnlyNumber()
    {
        string chars = "0123456789";

        char[] strChars = new char[4];

        Random random = new();

        for (int i = 0; i < strChars.Length; i++)
        {
            strChars[i] = chars[random.Next(chars.Length)];
        }

        string finalString = new(strChars);

        return finalString;
    }

    public virtual async Task<IActionResult> CreditAndInvoices()
    {
        var model = new CreditAndInvoicesViewModel();

        try
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();


            var rawErpId = currentCustomer.ERPCustomerId;

            var erpCustomerId = rawErpId?.ToString()?.Trim();
            if (string.IsNullOrEmpty(erpCustomerId))
            {
                return null;
            }


            model.ErpCustomerId = erpCustomerId;


            var url = _config.ApiRoot + "&objType=89&objId=" + erpCustomerId;
            var request = new RestRequest(url, Method.GET);
            var response = await _restClient.ExecuteAsync(request);








            if (response.IsSuccessful
                && !string.IsNullOrEmpty(response.Content)
                && response.Content.TrimStart().StartsWith("{"))
            {
                CustomerCreditErpModel erpData = null;
                try
                {
                    erpData = JsonConvert.DeserializeObject<CustomerCreditErpModel>(response.Content);
                }
                catch (Exception exJson)
                {
                    _logger.Error($"[CreditAndInvoices] JSON parse failed: {exJson.Message} | Content: {response.Content}");
                }

                if (erpData != null)
                {
                    model.HasCredit = erpData.hasCredit;
                    model.AvailableCredit = erpData.availableCredit;
                    model.PaymentTerms = erpData.paymentTerms;
                    model.Balance = erpData.balance;
                    model.OverdueBalance = erpData.overdueBalance;
                    model.DaysOverdue = erpData.daysOverdue;

                    MsSqlNopDataProvider db = new MsSqlNopDataProvider();

                    DataParameter p1 = new DataParameter("@CustomerId", currentCustomer.Id);
                    DataParameter p2 = new DataParameter("@AvailableCredit", erpData.availableCredit);

                    await db.ExecuteNonQueryAsync(@"
                    IF EXISTS (SELECT 1 FROM [dbo].[NS_Wallet] WHERE [WalletCustomerId]=@CustomerId)
                        UPDATE [dbo].[NS_Wallet]
                           SET [AvailableCredit]=@AvailableCredit, [Active]=1
                         WHERE [WalletCustomerId]=@CustomerId
                    ELSE
                        INSERT INTO [dbo].[NS_Wallet]
                            ([WalletCustomerId],[CurrencyId],[Active],[CreditLimit],
                             [CreditUsed],[AvailableCredit],[AllowOverspend],[WarnUserForCreditBelow])
                        VALUES (@CustomerId,1,1,@AvailableCredit,0,@AvailableCredit,0,0)",
                        p1, p2);

                    DataParameter h1 = new DataParameter("@CustomerId", currentCustomer.Id);
                    DataParameter h2 = new DataParameter("@AvailableCredit", erpData.availableCredit);
                    DataParameter h3 = new DataParameter("@Balance", erpData.balance);
                    DataParameter h4 = new DataParameter("@Note",
                        $"Page visit sync | Balance:{erpData.balance} | Terms:{erpData.paymentTerms}");

                    await db.ExecuteNonQueryAsync(@"
                    INSERT INTO [dbo].[NS_Wallet_ActivityHistory]
                        ([WalletCustomerId],[ActivityTypeId],[PreviousTotalCreditUsed],
                         [CurrentTotalCreditUsed],[LastAvailableCredit],[CreatedByCustomerId],
                         [CreatedOnUtc],[Note])
                    SELECT @CustomerId,1,
                        ISNULL((SELECT TOP 1 [CurrentTotalCreditUsed]
                                FROM [dbo].[NS_Wallet_ActivityHistory]
                                WHERE [WalletCustomerId]=@CustomerId
                                ORDER BY [CreatedOnUtc] DESC),0),
                        @Balance,@AvailableCredit,@CustomerId,GETUTCDATE(),@Note",
                        h1, h2, h3, h4);

                    DataParameter lp = new DataParameter("@CustomerId", currentCustomer.Id);
                    var limitResult = await db.QueryAsync<decimal>(
                        "SELECT [CreditLimit] FROM [dbo].[NS_Wallet] WHERE [WalletCustomerId]=@CustomerId", lp);
                    model.CreditLimit = limitResult.FirstOrDefault();
                    model.CreditUsed = model.CreditLimit - model.AvailableCredit;
                }
            }
            else
            {
                _logger.Warning($"[CreditAndInvoices] Bad response — Status:{response.StatusCode} Body:{response.Content}");
            }


            MsSqlNopDataProvider dbInv = new MsSqlNopDataProvider();
            DataParameter op = new DataParameter("@CustomerId", currentCustomer.Id);
            var invoices = await dbInv.QueryAsync<InvoiceItem>(@"
            SELECT
                CAST([CustomOrderNumber] AS NVARCHAR(50)) AS InvoiceNumber,
                [OrderTotal]   AS Amount,
                [CreatedOnUtc] AS InvoiceDate,
                CASE
                    WHEN [PaymentStatusId]=30 THEN 'Paid'
                    WHEN [PaymentStatusId]=10 THEN 'Unpaid'
                    ELSE 'Expired'
                END AS Status,
                '' AS DueDate
            FROM [dbo].[Order]
            WHERE [CustomerId]=@CustomerId AND [Deleted]=0
            ORDER BY [CreatedOnUtc] DESC", op);

            model.OpenInvoices = invoices.Where(i => i.Status != "Paid").ToList();
            model.CompletedInvoices = invoices.Where(i => i.Status == "Paid").ToList();
        }
        catch (Exception ex)
        {
            _logger.Error("CreditAndInvoices error: " + ex.Message, ex);
        }

        return View(model);
    }
    [HttpPost]
    public virtual async Task<IActionResult> SendCreditRequest([FromBody] CreditRequestModel req)
    {
        try
        {
            if (req == null || req.Amount <= 0 || string.IsNullOrWhiteSpace(req.Reason))
                return Json(new { success = false, message = "Invalid request data" });

            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var customerFullName = await _customerService.GetCustomerFullNameAsync(currentCustomer);


            var subject = $"[Credit Request] {req.RequestType} — {customerFullName}";

            var body = $@"<div style='font-family:Segoe UI,sans-serif;max-width:600px;margin:auto;
                           border:1px solid #eee;border-radius:12px;overflow:hidden'>
            <div style='background:#1f278a;padding:24px 28px;color:#fff'>
                <h2 style='margin:0;font-size:20px'>{req.RequestType}</h2>
                <p  style='margin:6px 0 0;opacity:.8;font-size:13px'>Submitted via Rotat Website</p>
            </div>
            <div style='padding:28px'>
                <table style='width:100%;border-collapse:collapse;font-size:14px'>
                    <tr>
                        <td style='padding:10px 0;color:#888;width:170px'>Customer Name</td>
                        <td style='padding:10px 0;font-weight:600'>{customerFullName}</td>
                    </tr>
                    <tr style='border-top:1px solid #f0f0f0'>
                        <td style='padding:10px 0;color:#888'>Email</td>
                        <td style='padding:10px 0;font-weight:600'>{currentCustomer.Email}</td>
                    </tr>
                    <tr style='border-top:1px solid #f0f0f0'>
                        <td style='padding:10px 0;color:#888'>NopCommerce ID</td>
                        <td style='padding:10px 0;font-weight:600'>{currentCustomer.Id}</td>
                    </tr>
                    <tr style='border-top:1px solid #f0f0f0'>
                        <td style='padding:10px 0;color:#888'>ERP Customer ID</td>
                        <td style='padding:10px 0;font-weight:600'>{currentCustomer.ERPCustomerId}</td>
                    </tr>
                    <tr style='border-top:1px solid #f0f0f0'>
                        <td style='padding:10px 0;color:#888'>Request Type</td>
                        <td style='padding:10px 0;font-weight:600;color:#1f278a'>{req.RequestType}</td>
                    </tr>
                    <tr style='border-top:1px solid #f0f0f0'>
                        <td style='padding:10px 0;color:#888'>Requested Amount</td>
                        <td style='padding:10px 0;font-weight:700;font-size:18px'>{req.Amount:N2}</td>
                    </tr>
                    <tr style='border-top:1px solid #f0f0f0'>
                        <td style='padding:10px 0;color:#888;vertical-align:top'>Reason</td>
                        <td style='padding:10px 0;line-height:1.6'>{req.Reason}</td>
                    </tr>
                    <tr style='border-top:1px solid #f0f0f0'>
                        <td style='padding:10px 0;color:#888'>Submitted At</td>
                        <td style='padding:10px 0;color:#666'>{DateTime.UtcNow:dd MMM yyyy HH:mm} UTC</td>
                    </tr>
                </table>
            </div>
            <div style='background:#f8fafc;padding:16px 28px;font-size:12px;color:#aaa;border-top:1px solid #eee'>
                Auto-generated from Rotat e-commerce platform.
            </div>
        </div>";


            var emailAccount = (await _emailAccountService.GetAllEmailAccountsAsync())
                               .FirstOrDefault()
                               ?? throw new Exception("No email account configured in NopCommerce");

            await _emailSender.SendEmailAsync(
                emailAccount: emailAccount,
                subject: subject,
                body: body,
                fromAddress: emailAccount.Email,
                fromName: emailAccount.DisplayName,
                toAddress: "wafaaothman80@gmail.com",
                toName: "Rotat Sales Team"
            );


            MsSqlNopDataProvider db = new MsSqlNopDataProvider();


            await db.ExecuteNonQueryAsync(@"
            IF OBJECT_ID('dbo.NS_CreditRequests','U') IS NULL
            CREATE TABLE [dbo].[NS_CreditRequests] (
                [Id]           INT IDENTITY(1,1) PRIMARY KEY,
                [CustomerId]   INT NOT NULL,
                [RequestType]  NVARCHAR(100) NOT NULL,
                [Amount]       DECIMAL(18,4) NOT NULL,
                [Reason]       NVARCHAR(1000),
                [Status]       NVARCHAR(50)  NOT NULL DEFAULT 'Pending',
                [CreatedOnUtc] DATETIME      NOT NULL DEFAULT GETUTCDATE()
            )");

            // حفظ الطلب
            await db.ExecuteNonQueryAsync(@"
            INSERT INTO [dbo].[NS_CreditRequests]
                ([CustomerId],[RequestType],[Amount],[Reason])
            VALUES
                (@CustomerId, @RequestType, @Amount, @Reason)",
                new DataParameter("@CustomerId", currentCustomer.Id),
                new DataParameter("@RequestType", req.RequestType),
                new DataParameter("@Amount", req.Amount),
                new DataParameter("@Reason", req.Reason ?? ""));

            _logger.Information(
                $"[CreditRequest] Customer={currentCustomer.Email} | Type={req.RequestType} | Amount={req.Amount}");

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.Error("SendCreditRequest error: " + ex.Message, ex);
            return Json(new { success = false, message = "Failed to send request. Please try again." });
        }
    }


    public class CreditRequestModel
    {
        public string RequestType { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; }
    }

    public class CustomerCreditErpModel
    {
        public string erpCustomerId { get; set; }
        public bool hasCredit { get; set; }
        public decimal availableCredit { get; set; }
        public string paymentTerms { get; set; }
        public int dueDays { get; set; }
        public decimal balance { get; set; }
        public decimal overdueBalance { get; set; }
        public int daysOverdue { get; set; }
        public string message { get; set; }
    }
    public class CreditAndInvoicesViewModel
    {

        public decimal AvailableCredit { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CreditUsed { get; set; }
        public string PaymentTerms { get; set; }
        public decimal Balance { get; set; }
        public decimal OverdueBalance { get; set; }
        public int DaysOverdue { get; set; }
        public bool HasCredit { get; set; }
        public string ErpCustomerId { get; set; }


        public List<InvoiceItem> OpenInvoices { get; set; } = new();
        public List<InvoiceItem> CompletedInvoices { get; set; } = new();
    }

    public class InvoiceItem
    {
        public string InvoiceNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string Status { get; set; }
        public string DueDate { get; set; }
    }



}