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
using Nop.Core.Domain.Orders;

namespace NopAdvance.Plugin.Misc.PublicAPI.Services;

public interface IPluginPaymentService
{
    /// <summary>
    /// Generate order guid
    /// </summary>
    /// <param name="previousOrderGuid">previousOrderGuid</param>
    /// <param name="previousOrderGuidGeneratedOnUtc">previousOrderGuidGeneratedOnUtc</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the order guid and order guid generated date-time utc
    /// </returns>
    (Guid, DateTime) GenerateOrderGuid(Guid? previousOrderGuid, DateTime? previousOrderGuidGeneratedOnUtc);

    /// <summary>
    /// Get PayPalStandard redirection Url
    /// </summary>
    /// <param name="order">order</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the paypal standard redirect url
    /// </returns>
    Task<string> GetPayPalStandardRedirectionUrl(Order order);

    /// <summary>
    /// Get skrill redirection Url
    /// </summary>
    /// <param name="order">order</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the skrill redirect url
    /// </returns>
    Task<string> GetSkrillRedirectionUrl(Order order);

    /// <summary>
    /// Get two checkout redirection Url
    /// </summary>
    /// <param name="order">order</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the two checkout redirect url
    /// </returns>
    Task<string> GetTwoCheckoutRedirectionUrl(Order order);
}
