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
using Nop.Core.Domain.Shipping;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;

namespace NopAdvance.Plugin.Misc.PublicAPI.Validators.Catalog;

public partial class EstimateShippingRequestValidator : BaseNopValidator<EstimateShippingRequest>
{
    public EstimateShippingRequestValidator(ILocalizationService localizationService,
        ShippingSettings shippingSettings)
    {
        RuleFor(x => x.CountryId).NotNull().NotEqual(0).WithMessageAwait(localizationService.GetResourceAsync("Shipping.EstimateShipping.Country.Required"));

        if (!shippingSettings.EstimateShippingCityNameEnabled)
        {
            RuleFor(x => x.ZipPostalCode).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Shipping.EstimateShipping.ZipPostalCode.Required"));
        }

        if (shippingSettings.EstimateShippingCityNameEnabled)
        {
            RuleFor(x => x.City).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Shipping.EstimateShipping.City.Required"));
        }
    }
}
