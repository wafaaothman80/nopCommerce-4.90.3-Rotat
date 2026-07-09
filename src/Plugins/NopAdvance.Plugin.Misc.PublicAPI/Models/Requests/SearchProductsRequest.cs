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

namespace NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;

public partial record SearchProductsRequest : CatalogRequest
{
    /// <summary>
    /// Search term
    /// </summary>
    [Required]
    public string q { get; set; }

    /// <summary>
    /// Category identifier
    /// </summary>
    public int cid { get; set; }

    /// <summary>
    /// A value indicating whether to include sub categories
    /// </summary>
    public bool isc { get; set; }

    /// <summary>
    /// Manufacturer identifier
    /// </summary>
    public int mid { get; set; }

    /// <summary>
    /// Vendor identifier
    /// </summary>
    public int vid { get; set; }

    /// <summary>
    /// A value indicating whether to search in descriptions
    /// </summary>
    public bool sid { get; set; }

    /// <summary>
    /// A value indicating whether "advanced search" is enabled
    /// </summary>
    public bool advs { get; set; }

    /// <summary>
    /// A value indicating whether "allow search by vendor" is enabled
    /// </summary>
    public bool asv { get; set; }
}
