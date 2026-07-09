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
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;

namespace NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;

public partial record CatalogRequest : BasePageableRequest
{
    /// <summary>
    /// Price ('min-max' format)
    /// </summary>
    public string Price { get; set; }

    /// <summary>
    /// Specification attribute option identifiers
    /// </summary>
    [FromQuery(Name = "specs")]
    public List<int> SpecificationOptionIds { get; set; }

    /// <summary>
    /// Manufacturer identifiers
    /// </summary>
    [FromQuery(Name = "ms")]
    public List<int> ManufacturerIds { get; set; }

    /// <summary>
    /// Order by (sorting)
    /// </summary>
    public ProductSortingEnum? OrderBy { get; set; }
}
