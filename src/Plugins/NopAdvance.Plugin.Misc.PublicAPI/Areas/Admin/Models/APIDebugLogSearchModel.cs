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
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;

namespace NopAdvance.Plugin.Misc.PublicAPI.Models.Admin;

public partial record APIDebugLogSearchModel : BaseSearchModel
{
    #region Ctor

    public APIDebugLogSearchModel()
    {
        AvailableStores = new List<SelectListItem>();
        AvailableApplications = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName(LocaleResourceDefaults.DEBUG_LOG_SEARCH_CREATED_ON_FROM)]
    [UIHint("DateNullable")]
    public DateTime? CreatedOnFrom { get; set; }

    [NopResourceDisplayName(LocaleResourceDefaults.DEBUG_LOG_SEARCH_CREATED_ON_TO)]
    [UIHint("DateNullable")]
    public DateTime? CreatedOnTo { get; set; }

    [NopResourceDisplayName(LocaleResourceDefaults.DEBUG_LOG_SEARCH_STORES)]
    public int StoreId { get; set; }
    public IList<SelectListItem> AvailableStores { get; set; }

    [NopResourceDisplayName(LocaleResourceDefaults.TOKEN_LIST_SEARCH_APPLICATION)]
    public int SearchApplicationId { get; set; }
    public IList<SelectListItem> AvailableApplications { get; set; }

    #endregion
}
