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
using Nop.Core.Domain.Tax;
using Nop.Web.Framework.Models;

namespace NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;

public partial record TaxTypeSelectorResponse
{
    public TaxTypeSelectorResponse()
    {
        AvailableTaxTypes = new List<TaxTypeResponse>();
    }

    /// <summary>
    /// Available tax types
    /// </summary>
    public IList<TaxTypeResponse> AvailableTaxTypes { get; set; }

    /// <summary>
    /// Current/Selected tax type
    /// </summary>
    public TaxDisplayType CurrentTaxType { get; set; }
}

public record TaxTypeResponse : BaseNopEntityModel
{
    /// <summary>
    /// Tax type
    /// </summary>
    public TaxDisplayType Name { get; set; }

    /// <summary>
    /// Tax type display text
    /// </summary>
    public string DisplayText { get; set; }
}
