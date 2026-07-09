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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;

public partial record EstimateShippingRequest
{
    /// <summary>
    /// Zip/postal code
    /// </summary>
    [DefaultValue("10021")]
    [Required]
    public string ZipPostalCode { get; set; }

    /// <summary>
    /// City
    /// </summary>
    [DefaultValue("New York")]
    [Required]
    public string City { get; set; }

    /// <summary>
    /// Country identifier
    /// </summary>
    [DefaultValue(237)]
    [Required]
    public int CountryId { get; set; }

    /// <summary>
    /// State/province identifier
    /// </summary>
    [DefaultValue(1708)]
    public int? StateProvinceId { get; set; }

    /// <summary>
    /// Product attributes collection
    /// <para><em><b>1. "addtocart_1.CustomerEnteredPrice": "1000"</b></em>  - addtocart_1.CustomerEnteredPrice(where 1 is product id), "1000" is customer entered price - <em>Product => Customer enters price is true</em></para>
    /// <para><em><b>2. "addtocart_1.EnteredQuantity": "5"</b></em>       - addtocart_1.EnteredQuantity(Where 1 is product id), "5" is product quantity</para>
    /// <para><em><b>3. "product_attribute_1": "Custom text"</b></em>  - product_attribute_1(where 1 is product attribute id), "Custom text" is value of attribute - <em>Control type is Textbox/Multiline text box</em></para>
    /// <para><em><b>4. "product_attribute_2": "3"</b></em>       - product_attribute_2(Where 2 is product attribute id), "3" is product attribute value id</para>
    /// <para><em><b>5. "product_attribute_3": "4,5"</b></em>       - product_attribute_3(Where 3 is product attribute id), "4,5" are product attribute value ids(it can be single or multiple) - <em>Control type is Checkbox</em></para>
    /// <para><em><b>6. "product_attribute_14_day": "1"</b></em>       - product_attribute_14_day(Where 14 is product attribute id),"1" is date value - <em>Control type is Date picker</em></para>
    /// <para><em><b>7. "product_attribute_14_month": "10"</b></em>       - product_attribute_14_month(Where 14 is product attribute id),"10" is month value - <em>Control type is Date picker</em></para>
    /// <para><em><b>8. "product_attribute_14_year": "2001"</b></em>       - product_attribute_14_year(Where 14 is product attribute id),"2001" is year value - <em>Control type is Date picker</em></para>
    /// </summary>
    [DictionaryDefault("addtocart_1.CustomerEnteredPrice,1000,addtocart_1.EnteredQuantity,5,product_attribute_1,Custom text,product_attribute_2,3")]
    public IDictionary<string, string> Attributes { get; set; }
}
