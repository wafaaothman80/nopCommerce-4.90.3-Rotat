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

public partial record RegisterRequest : CustomerInfoModel
{
    /// <summary>
    /// Login after registration?
    /// </summary>
    [DefaultValue(true)]
    public bool LoginAfterRegistration { get; set; }

    /// <summary>
    /// Password
    /// </summary>
    [DefaultValue("Abc12@#D")]
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    /// <summary>
    /// Confirm password
    /// </summary>
    [DefaultValue("Abc12@#D")]
    [Required]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }

    /// <summary>
    /// Confirm email (required if EnteringEmailTwice enabled from customer settings)
    /// </summary>
    [DefaultValue("johnsmith@yourstore.com")]
    [DataType(DataType.EmailAddress)]
    public string ConfirmEmail { get; set; }
}
