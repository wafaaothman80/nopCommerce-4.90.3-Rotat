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
using Nop.Core.Configuration;
using NopAdvance.Plugin.Misc.PublicAPI.Domain.Payments;

namespace NopAdvance.Plugin.Misc.PublicAPI.Services;

/// <summary>
/// Represents plugin settings
/// </summary>
public class SkrillSettings : ISettings
{
    /// <summary>
    /// Gets or sets the email address of merchant account
    /// </summary>
    public string MerchantEmail { get; set; }

    /// <summary>
    /// Gets or sets the secret word submitted in the settings section of merchant account
    /// </summary>
    public string SecretWord { get; set; }

    /// <summary>
    /// Gets or sets the password required to request services
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Gets or sets the payment flow type
    /// </summary>
    public SkrillPaymentFlowType PaymentFlowType { get; set; }

    /// <summary>
    /// Gets or sets a period (in seconds) before the request times out
    /// </summary>
    public int? RequestTimeout { get; set; }
}