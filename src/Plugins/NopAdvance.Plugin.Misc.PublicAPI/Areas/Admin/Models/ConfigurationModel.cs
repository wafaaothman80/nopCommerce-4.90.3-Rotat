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
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopAdvance.Plugin.Core.Models;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;

namespace NopAdvance.Plugin.Misc.PublicAPI.Models.Admin;

public record ConfigurationModel : BaseNopAdvanceConfigurationModel
{
    #region Properties

    [NopResourceDisplayName(LocaleResourceDefaults.CONFIGURE_ENABLE_SWAGGER)]
    public bool EnableSwagger { get; set; }
    public bool EnableSwagger_OverrideForStore { get; set; }

    [NopResourceDisplayName(LocaleResourceDefaults.CONFIGURE_IS_DEVELOPMENT)]
    public bool IsDevelopment { get; set; }
    public bool IsDevelopment_OverrideForStore { get; set; }

    [NopResourceDisplayName(LocaleResourceDefaults.CONFIGURE_SECRET_KEY)]
    public string SecretKey { get; set; }
    public bool SecretKey_OverrideForStore { get; set; }

    [NopResourceDisplayName(LocaleResourceDefaults.CONFIGURE_SECURITY_ALGORITHM_TYPE)]
    public int SecurityAlgorithmTypeId { get; set; }
    public SelectList AvailableSecurityAlgorithmTypes { get; set; }
    public bool SecurityAlgorithmTypeId_OverrideForStore { get; set; }

    [NopResourceDisplayName(LocaleResourceDefaults.CONFIGURE_ACCESS_TOKEN_EXPIRATION)]
    public int AccessTokenExpiration { get; set; }
    public int AccessTokenExpirationDurationId { get; set; }
    public SelectList AvailableAccessTokenExpirationDurations { get; set; }
    public bool AccessTokenExpiration_OverrideForStore { get; set; }

    [NopResourceDisplayName(LocaleResourceDefaults.CONFIGURE_REFRESH_TOKEN_EXPIRATION)]
    public int RefreshTokenExpiration { get; set; }
    public int RefreshTokenExpirationDurationId { get; set; }
    public SelectList AvailableRefreshTokenExpirationDurations { get; set; }
    public bool RefreshTokenExpiration_OverrideForStore { get; set; }

    [NopResourceDisplayName(LocaleResourceDefaults.CONFIGURE_ENABLE_DEBUGGING)]
    public bool EnableDebugging { get; set; }
    public bool EnableDebugging_OverrideForStore { get; set; }

    #endregion
}
