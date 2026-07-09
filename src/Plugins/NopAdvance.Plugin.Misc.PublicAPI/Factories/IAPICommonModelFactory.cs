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
using Nop.Web.Models.Checkout;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses.Payments;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;

namespace NopAdvance.Plugin.Misc.PublicAPI.Factories;

public partial interface IAPICommonModelFactory
{
    /// <summary>
    /// Prepare the language selector model
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the language response
    /// </returns>
    Task<LanguageResponse> PrepareLanguageSelectorModelAsync();

    /// <summary>
    /// Prepare the authorize net response
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the authorize net response
    /// </returns>
    Task<AuthorizeNetResponse> PrepareAuthorizeNetResponseAsync();

    /// <summary>
    /// Prepare the manual response
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the manual response
    /// </returns>
    Task<ManualResponse> PrepareManualResponseAsync();

    /// <summary>
    /// Prepare the check money order response
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the check money order response
    /// </returns>
    Task<CheckMoneyOrderResponse> PrepareCheckMoneyOrderResponseAsync();

    /// <summary>
    /// Prepare the brain tree response
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the brain tree response
    /// </returns>
    Task<BrainTreeResponse> PrepareBrainTreeResponseAsync();

    /// <summary>
    /// Prepare the purchase order response
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the purchase order response
    /// </returns>
    Task<PurchaseOrderResponse> PreparePurchaseOrderResponseAsync();

    /// <summary>
    /// Prepares the checkout pickup points model
    /// </summary>
    /// <param name="cart">Cart</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the checkout pickup points model
    /// </returns>
    Task<CheckoutPickupPointsModel> PrepareCheckoutPickupPointsModelAsync(IList<ShoppingCartItem> cart);
}
