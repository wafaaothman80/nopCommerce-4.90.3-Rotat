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

public partial record PaymentInfoRequest
{
    /// <summary>
    /// Payment informations(Can be vary with payment method selection)
    /// </summary>
    [DictionaryDefault("CreditCardType,Visa,CardholderName,xyz,CardNumber,4111111111111111,CardCode,123,ExpireMonth,12,ExpireYear,2025")]
    [Required]
    public IDictionary<string, string> PaymentInfo { get; set; } 

    /// <summary>
    /// Previously generated order guid identifer
    /// </summary>
    [DefaultValue("69215199-0bf3-46f2-ae40-65a48bffc4ec")]
    public Guid? PreviousOrderGuid { get; set; }

    /// <summary>
    /// Previously generated order guid identifer datetime in utc
    /// </summary>
    [DefaultValue("2024-01-09 08:49:55.198871")]
    public DateTime? PreviousOrderGuidGeneratedOnUtc { get; set; }
}
