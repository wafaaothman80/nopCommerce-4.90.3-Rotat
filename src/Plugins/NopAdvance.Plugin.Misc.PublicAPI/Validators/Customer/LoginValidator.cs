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
using Nop.Core.Domain.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;

namespace NopAdvance.Plugin.Misc.PublicAPI.Validators.Customer;

public partial class LoginValidator : BaseNopValidator<LoginRequest>
{
    public LoginValidator(ILocalizationService localizationService, CustomerSettings customerSettings)
    {
        if (!customerSettings.UsernamesEnabled)
        {
            //login by email
            RuleFor(x => x.UsernameOrEmail).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Login.Fields.Email.Required"));
            RuleFor(x => x.UsernameOrEmail).EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("Common.WrongEmail"));
        }
        else
        {
            //login by username
            RuleFor(x => x.UsernameOrEmail).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopAdvance.Plugin.PublicAPI.Login.Username.Required"));
        }

        RuleFor(x => x.Password).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopAdvance.Plugin.PublicAPI.Login.Password.Required"));
    }
}
