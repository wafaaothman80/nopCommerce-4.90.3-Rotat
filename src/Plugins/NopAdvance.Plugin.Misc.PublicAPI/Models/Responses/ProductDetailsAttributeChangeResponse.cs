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
namespace NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;

public partial record ProductDetailsAttributeChangeResponse
{
    /// <summary>
    /// The product identifier
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// The Global Trade Item Number (GTIN)
    /// </summary>
    public string Gtin { get; set; }

    /// <summary>
    /// Manufacturer part number
    /// </summary>
    public string Mpn { get; set; }

    /// <summary>
    /// Stock keeping unit
    /// </summary>
    public string Sku { get; set; }

    /// <summary>
    /// Product price
    /// </summary>
    public string Price { get; set; }

    /// <summary>
    /// Base price
    /// </summary>
    public string Basepricepangv { get; set; }

    /// <summary>
    /// Stock availability message
    /// </summary>
    public string StockAvailability { get; set; }

    /// <summary>
    /// Enabled attribute mapping identifiers
    /// </summary>
    public int[] Enabledattributemappingids { get; set; }

    /// <summary>
    /// Disabled attribute mapping identifiers
    /// </summary>
    public int[] Disabledattributemappingids { get; set; }

    /// <summary>
    /// Product full size picture url
    /// </summary>
    public string PictureFullSizeUrl { get; set; }

    /// <summary>
    /// Product default picture url
    /// </summary>
    public string PictureDefaultSizeUrl { get; set; }

    /// <summary>
    /// Is free shippping
    /// </summary>
    public bool IsFreeShipping { get; set; }

    /// <summary>
    /// Error messages if any
    /// </summary>
    public string[] Message { get; set; }
}
