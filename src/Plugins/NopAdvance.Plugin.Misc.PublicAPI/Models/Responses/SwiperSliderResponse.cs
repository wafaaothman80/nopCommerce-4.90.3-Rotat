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
using Nop.Web.Framework.Models;

namespace NopAdvance.Plugin.Misc.PublicAPI.Models.Responses;

public partial record SwiperSliderResponse : BaseNopModel
{
    public bool ShowNavigation { get; set; }
    public bool ShowPagination { get; set; }
    public bool Autoplay { get; set; }
    public int AutoplayDelay { get; set; }
    public List<PublicSlideModel> Slides { get; set; } = new();
}

public class SwiperSlide
{
    #region Properties

    /// <summary>
    /// Picture identifier
    /// </summary>
    public int PictureId { get; set; }

    /// <summary>
    /// Title attribute for image
    /// </summary>
    public string TitleText { get; set; }

    /// <summary>
    /// Link URL 
    /// </summary>
    public string LinkUrl { get; set; }

    /// <summary>
    /// Image alternate text
    /// </summary>
    public string AltText { get; set; }

    #endregion
}

public record PublicSlideModel : BaseNopModel
{
    #region Properties

    public int PictureId { get; set; }
    public string PictureUrl { get; set; }
    public string TitleText { get; set; }
    public string LinkUrl { get; set; }
    public string AltText { get; set; }
    public bool LazyLoading { get; set; }

    #endregion
}
