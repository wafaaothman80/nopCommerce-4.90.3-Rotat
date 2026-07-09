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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Nop.Core.Infrastructure;
using Nop.Web.Controllers;

namespace NopAdvance.Plugin.Core.Controllers;

public abstract class NopAdvanceBasePublicController : BasePublicController
{
    #region Rendering

    protected async Task<string> RenderPartialViewToStringAsync(string viewName, object model, string htmlFieldPrefix)
    {
        //get Razor view engine
        var razorViewEngine = EngineContext.Current.Resolve<IRazorViewEngine>();

        //create action context
        var actionContext = new ActionContext(HttpContext, RouteData, ControllerContext.ActionDescriptor, ModelState);

        //set view name as action name in case if not passed
        if (string.IsNullOrEmpty(viewName))
            viewName = ControllerContext.ActionDescriptor.ActionName;

        //set model
        ViewData.Model = model;
        ViewData.TemplateInfo.HtmlFieldPrefix = htmlFieldPrefix;

        //try to get a view by the name
        var viewResult = razorViewEngine.FindView(actionContext, viewName, false);
        if (viewResult.View == null)
        {
            //or try to get a view by the path
            viewResult = razorViewEngine.GetView(null, viewName, false);
            if (viewResult.View == null)
                throw new ArgumentNullException($"{viewName} view was not found");
        }
        await using var stringWriter = new StringWriter();
        var viewContext = new ViewContext(actionContext, viewResult.View, ViewData, TempData, stringWriter, new HtmlHelperOptions());

        await viewResult.View.RenderAsync(viewContext);
        return stringWriter.GetStringBuilder().ToString();
    }

    #endregion
}
