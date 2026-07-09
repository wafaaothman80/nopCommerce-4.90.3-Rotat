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
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NopAdvance.Plugin.Misc.PublicAPI.Models.Responses.Payments;

/// <summary>
/// Represents a payment info model
/// </summary>
public partial record BrainTreeResponse : PaymentInfoResponse
{
    #region Ctor

    public BrainTreeResponse()
    {
        ExpireMonths = new List<SelectListItem>();
        ExpireYears = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    public string CreditCardType { get; set; }

    public string CardholderName { get; set; }

    public string CardNumber { get; set; }

    public string ExpireMonth { get; set; }

    public string ExpireYear { get; set; }

    public IList<SelectListItem> ExpireMonths { get; set; }

    public IList<SelectListItem> ExpireYears { get; set; }

    public string CardCode { get; set; }

    public string Errors { get; set; }

    public string CardNonce { get; set; }

    public string ClientToken { get; set; }

    public decimal? OrderTotal { get; set; }

    #endregion
}