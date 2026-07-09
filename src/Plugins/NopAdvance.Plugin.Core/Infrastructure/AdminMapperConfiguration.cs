// ***	 ** ****** ****** ****** ******* **     ** ****** ***   ** **** ****
// ****  ** **  ** **  ** **  **  **  **  **   **  **  ** ****  ** *    *  
// ** ** ** **  ** ****** ******  **  **   ** **   ****** ** ** ** *    ***
// **  **** **  ** **	  **  **  **  **    ***    **  ** **  **** *    *  
// **   *** ****** **	  **  ** *******     *     **  ** **   *** **** ****
// ***************************************************************************
// *                                                                         *
// *    NopAdvance Core Plugin by NopAdvance team                            *
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
using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Services.Plugins;
using NopAdvance.Plugin.Core.Models;

namespace NopAdvance.Plugin.Core.Infrastructure;

public class AdminMapperConfiguration : Profile, IOrderedMapperProfile
{
    #region Ctor

    public AdminMapperConfiguration()
    {
        CreatePluginsMaps();
    }

    #endregion

    #region Utilities

    protected virtual void CreatePluginsMaps()
    {
        CreateMap<PluginDescriptor, LicenseModel>()
            .ForMember(model => model.CanChangeEnabled, options => options.Ignore())
            .ForMember(model => model.IsEnabled, options => options.Ignore());
    }

    #endregion

    #region Properties

    /// <summary>
    /// Order of this mapper implementation
    /// </summary>
    public int Order => 0;

    #endregion
}
