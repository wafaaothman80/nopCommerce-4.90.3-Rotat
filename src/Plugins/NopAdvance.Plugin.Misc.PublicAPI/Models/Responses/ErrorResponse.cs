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
namespace NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;

public partial record ErrorResponse
{
    public ErrorResponse()
    {
        Errors = new List<string>();
    }

    public ErrorResponse(string error)
    {
        Errors = new List<string>() { error };
    }

    public string Error
    {
        set => AddError(value);
    }

    /// <summary>
    /// Error messages collection
    /// </summary>
    public IList<string> Errors { get; set; }

    /// <summary>
    /// Add error
    /// </summary>
    /// <param name="error">Error</param>
    public void AddError(string error)
    {
        Errors.Add(error);
    }

    public void AddErrors(IList<string> errors)
    {
        foreach (var error in errors)
            AddError(error);
    }
}
