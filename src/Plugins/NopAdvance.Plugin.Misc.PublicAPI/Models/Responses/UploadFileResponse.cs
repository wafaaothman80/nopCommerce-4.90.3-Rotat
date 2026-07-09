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
using System;

namespace NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;

public class TopMenuResponse
{
    public IList<CategoryNodeDto> Categories { get; set; } = new List<CategoryNodeDto>();
}

    public class UploadFileResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Guid UploadedFileGuid { get; set; }
    }

public class NewsletterBoxResponse
{
    public bool IsEnabled { get; set; }
    public bool AllowToUnsubscribe { get; set; }
    public bool IsGuest { get; set; }
    public string Email { get; set; }
}
public class CategoryNodeDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string SeName { get; set; }
    public IList<CategoryNodeDto> Children { get; set; } = new List<CategoryNodeDto>();
}
