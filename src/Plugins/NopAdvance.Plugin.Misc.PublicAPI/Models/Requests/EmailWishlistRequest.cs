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
using System.ComponentModel.DataAnnotations;

namespace NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;

public partial record EmailWishlistRequest
{
    /// <summary>
    /// Friend's(To) email address
    /// </summary>
    [DefaultValue("stevegates@yourstore.com")]
    [DataType(DataType.EmailAddress)]
    [Required]
    public string FriendEmail { get; set; }

    /// <summary>
    /// Your(From) email address
    /// </summary>
    [DefaultValue("johnsmith@yourstore.com")]
    [DataType(DataType.EmailAddress)]
    [Required]
    public string YourEmailAddress { get; set; }

    /// <summary>
    /// Personal message
    /// </summary>
    [DefaultValue("This is personal message for email wishlist to friend")]
    public string PersonalMessage { get; set; }
}
