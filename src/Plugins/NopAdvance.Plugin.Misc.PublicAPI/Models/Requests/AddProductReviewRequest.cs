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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;

public partial record AddProductReviewRequest
{
    public AddProductReviewRequest()
    {
        AdditionalProductReviewList = new List<AdditionalProductReviewRequest>();
    }

    /// <summary>
    /// Rating (From 1 to 5)
    /// </summary>
    [DefaultValue(5)]
    [Required]
    public int Rating { get; set; }

    /// <summary>
    /// Review title
    /// </summary>
    [DefaultValue("Some sample review")]
    [Required]
    public string Title { get; set; }

    /// <summary>
    /// Review text
    /// </summary>
    [DefaultValue("This sample review is for the Build your own computer. I've been waiting for this product to be available. It is priced just right.")]
    [Required]
    public string ReviewText { get; set; }

    /// <summary>
    /// Prepare product reviews?
    /// </summary>
    [DefaultValue(true)]
    public bool PrepareReviews { get; set; }

    /// <summary>
    /// Additional product reviews list
    /// </summary>
    public IList<AdditionalProductReviewRequest> AdditionalProductReviewList { get; set; }
}

public record AdditionalProductReviewRequest
{
    /// <summary>
    /// The review type identifier
    /// <para><em><b>1. "ReviewTypeId": "1"</b></em>  - where 1 is review type id - <em>Catalog setting => Review types(If configured)</em></para>
    /// </summary>
    [DefaultValue(1)]
    [Required]
    public int ReviewTypeId { get; set; }

    /// <summary>
    /// Rating (From 1 to 5)
    /// </summary>
    [DefaultValue(5)]
    [Required]
    public int Rating { get; set; }
}
