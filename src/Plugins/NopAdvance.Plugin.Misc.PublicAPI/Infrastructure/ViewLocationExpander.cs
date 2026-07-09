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
using Microsoft.AspNetCore.Mvc.Razor;
using NopAdvance.Plugin.Core.Helpers;

namespace NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;

public class ViewLocationExpander : BaseViewLocationExpander, IViewLocationExpander
{
    /// <summary>
    /// Invoked by a Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine to determine potential locations for a view.
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="viewLocations">View locations</param>
    /// <returns>View locations</returns>
    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        viewLocations = AddAdminLocation(context, viewLocations, PluginDefaults.SYSTEM_NAME);

        return viewLocations;
    }

    /// <summary>
    /// Invoked by a Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine to determine the
    /// values that would be consumed by this instance of Microsoft.AspNetCore.Mvc.Razor.IViewLocationExpander.
    /// The calculated values are used to determine if the view location has changed since the last time it was located.
    /// </summary>
    /// <param name="context">Context</param>
    public void PopulateValues(ViewLocationExpanderContext context)
    {
        //do nothing here
    }
}
