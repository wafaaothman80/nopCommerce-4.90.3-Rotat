// ***	 ** ****** ****** ****** ******* **     ** ****** ***   ** **** ****
// ****  ** **  ** **  ** **  **  **  **  **   **  **  ** ****  ** *    *  
// ** ** ** **  ** ****** ******  **  **   ** **   ****** ** ** ** *    ***
// **  **** **  ** **	  **  **  **  **    ***    **  ** **  **** *    *  
// **   *** ****** **	  **  ** *******     *     **  ** **   *** **** ****
// ***************************************************************************
// *                                                                         *
// *    NopCommerce Public RESTful API Plugin by NopAdvance team             *
// *    Copyright (c) NopAdvance LLP. All Rights Reserved.                   *
// *                                                                         *
// ***************************************************************************
// *                                                                         *
// *    This software is licensed for use under the terms accepted during    *
// *    the purchase of this product. A non-exclusive, non-transferable      *
// *    right is granted to use this product on the website for which it was *
// *    licensed.                                                            *
// *                                                                         *
// *    Companies purchasing this product for their customers are permitted, *
// *    provided the use complies with the terms outlined in the EULA:       *
// *    https://store.nopadvance.com/eula.                                   *
// *                                                                         *
// *    You may not reverse engineer, decompile, modify, or distribute this  *
// *    software without explicit permission from NopAdvance LLP. Any        *
// *    violation will result in the termination of your license and may     *
// *    lead to legal action.                                                *
// *                                                                         *
// ***************************************************************************
// *    Contact: contact@nopadvance.com                                      *
// *    Website: https://nopadvance.com                                      *
// ***************************************************************************
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;

namespace NopAdvance.Plugin.Misc.PublicAPI.Models.Admin;

public partial record APIDebugLogModel : BaseNopEntityModel
{
    #region Properties

    [NopResourceDisplayName(LocaleResourceDefaults.DEBUG_LOG_CUSTOMER_ID)]
    public int CustomerId { get; set; }
    public string Customer { get; set; }

    [NopResourceDisplayName(LocaleResourceDefaults.DEBUG_LOG_STORE_ID)]
    public int StoreId { get; set; }
    public string Store { get; set; }

    [NopResourceDisplayName(LocaleResourceDefaults.DEBUG_LOG_STATUS_CODE)]
    public int StatusCode { get; set; }

    [NopResourceDisplayName(LocaleResourceDefaults.DEBUG_LOG_METHOD)]
    public string Method { get; set; }

    [NopResourceDisplayName(LocaleResourceDefaults.DEBUG_LOG_HEADERS)]
    public string Headers { get; set; }

    [NopResourceDisplayName(LocaleResourceDefaults.DEBUG_LOG_REQUEST_BODY)]
    public string RequestBody { get; set; }

    [NopResourceDisplayName(LocaleResourceDefaults.DEBUG_LOG_QUERY_STRING)]
    public string QueryString { get; set; }

    [NopResourceDisplayName(LocaleResourceDefaults.DEBUG_LOG_RESPONSE_BODY)]
    public string ResponseBody { get; set; }

    [NopResourceDisplayName(LocaleResourceDefaults.DEBUG_LOG_RESPONSE_TIME)]
    public long ResponseTime { get; set; }

    [NopResourceDisplayName(LocaleResourceDefaults.DEBUG_LOG_PATH)]
    public string Path { get; set; }

    [NopResourceDisplayName(LocaleResourceDefaults.DEBUG_LOG_CREATED_ON_UTC)]
    public DateTime CreatedOnUtc { get; set; }

    [NopResourceDisplayName(LocaleResourceDefaults.APPLICATION_NAME)]
    public string ApplicationName { get; set; }

    #endregion
}
