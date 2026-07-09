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
using System.ComponentModel;

namespace NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;

public partial record ReturnRequestRequest
{
    /// <summary>
    /// Uploaded file guid identifer(Works only when orderSettings => ReturnRequestsAllowFiles is true)
    /// </summary>
    [DefaultValue("2BD9E2C1-700C-4E81-A019-1889B8A5C0D3")]
    public Guid UploadedFileGuid { get; set; }

    /// <summary>
    /// Return request reason identifer
    /// </summary>
    [DefaultValue(1)]
    public int ReturnRequestReasonId { get; set; }

    /// <summary>
    /// Return request action identifer
    /// </summary>
    [DefaultValue(2)]
    public int ReturnRequestActionId { get; set; }

    /// <summary>
    /// Comments
    /// </summary>
    [DefaultValue("This is return request customer comment")]
    public string Comments { get; set; }

    /// <summary>
    /// Order item identifer with quantity (collection)
    /// <para><em><b>1. "22": "1"</b></em>  - where 22 is order item id, 1 is order item quantity</para>
    /// <para><em><b>1. "23": "3"</b></em>  - where 23 is order item id, 3 is order item quantity</para>
    /// </summary>
    [DictionaryDefault("22,1,23,3")]
    public IDictionary<int, int> OrderItemsQuantity { get; set; }
}
