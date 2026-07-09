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

public partial record AddressRequest
{
    /// <summary>
    /// First name
    /// </summary>
    [DefaultValue("John")]
    [Required]
    public string FirstName { get; set; }

    /// <summary>
    /// Last name
    /// </summary>
    [DefaultValue("Smith")]
    [Required]
    public string LastName { get; set; }

    /// <summary>
    /// Email address
    /// </summary>
    [DefaultValue("johnsmith@yourstore.com")]
    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    /// <summary>
    /// Company name
    /// </summary>
    [DefaultValue("Nop Solutions Ltd")]
    public string Company { get; set; }

    /// <summary>
    /// Country identifier
    /// </summary>
    [DefaultValue(237)]
    public int? CountryId { get; set; }

    /// <summary>
    /// State/province identifier
    /// </summary>
    [DefaultValue(1708)]
    public int? StateProvinceId { get; set; }

    /// <summary>
    /// County
    /// </summary>
    [DefaultValue("Bourbon")]
    public string County { get; set; }

    /// <summary>
    /// City
    /// </summary>
    [DefaultValue("New York")]
    public string City { get; set; }

    /// <summary>
    /// Address 1
    /// </summary>
    [DefaultValue("21 West 52nd Street")]
    public string Address1 { get; set; }

    /// <summary>
    /// Address 2
    /// </summary>
    [DefaultValue("21 West 52nd Street")]
    public string Address2 { get; set; }

    /// <summary>
    /// Zip/postal code
    /// </summary>
    [DefaultValue("10021")]
    public string ZipPostalCode { get; set; }

    /// <summary>
    /// Phone number
    /// </summary>
    [DefaultValue("12345678")]
    [DataType(DataType.PhoneNumber)]
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Fax number
    /// </summary>
    [DefaultValue("12345678")]
    [DataType(DataType.PhoneNumber)]
    public string FaxNumber { get; set; }

    /// <summary>
    /// Custom address attributes
    /// <para><em><b>1. "address_attribute_1": "Custom text"</b></em>  - address_attribute_1(where 1 is address attribute id), "Custom text" is value of attribute - <em>Control type is Textbox/Multiline text box</em></para>
    /// <para><em><b>2. "address_attribute_2": "4,5"</b></em>       - address_attribute_2(Where 2 is address attribute id), "4,5" are address attribute value ids(it can be single or multiple) - <em>Control type is Checkbox</em></para>
    /// <para><em><b>3. "address_attribute_3": "1"</b></em>       - address_attribute_3(Where 3 is address attribute id), "1" is address attribute value id</para>
    /// </summary>
    [DictionaryDefault("address_attribute_1,Custom text,address_attribute_3,1")]
    public IDictionary<string, string> CustomAddressAttributes { get; set; }
}
