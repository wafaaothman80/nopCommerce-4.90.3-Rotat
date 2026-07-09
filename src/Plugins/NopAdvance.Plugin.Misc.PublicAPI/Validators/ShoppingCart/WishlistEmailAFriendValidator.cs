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
using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;

namespace NopAdvance.Plugin.Misc.PublicAPI.Validators.ShoppingCart;

public partial class WishlistEmailAFriendValidator : BaseNopValidator<EmailWishlistRequest>
{
    public WishlistEmailAFriendValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.FriendEmail).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Wishlist.EmailAFriend.FriendEmail.Required"));
        RuleFor(x => x.FriendEmail).EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("Common.WrongEmail"));

        RuleFor(x => x.YourEmailAddress).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Wishlist.EmailAFriend.YourEmailAddress.Required"));
        RuleFor(x => x.YourEmailAddress).EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("Common.WrongEmail"));
    }
}