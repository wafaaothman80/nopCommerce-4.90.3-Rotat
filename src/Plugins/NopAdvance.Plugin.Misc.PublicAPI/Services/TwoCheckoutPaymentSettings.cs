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

namespace NopAdvance.Plugin.Misc.PublicAPI.Services;

/// <summary>
/// Represents plugin settings
/// </summary>
public class TwoCheckoutPaymentSettings : ISettings
{
    /// <summary>
    /// Gets or sets an account number
    /// </summary>
    public string AccountNumber { get; set; }

    /// <summary>
    /// Gets or sets a secret word
    /// </summary>
    public string SecretWord { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use sandbox (testing environment)
    /// </summary>
    public bool UseSandbox { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use MD5 hashing
    /// </summary>
    public bool UseMd5Hashing { get; set; }

    /// <summary>
    /// Gets or sets an additional fee
    /// </summary>
    public decimal AdditionalFee { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value
    /// </summary>
    public bool AdditionalFeePercentage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to log IPN errors
    /// </summary>
    public bool LogIpnErrors { get; set; }
}