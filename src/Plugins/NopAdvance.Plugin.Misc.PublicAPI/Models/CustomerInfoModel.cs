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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;

namespace NopAdvance.Plugin.Misc.PublicAPI.Models;

public partial record CustomerInfoModel
{
    /// <summary>
    /// Username (required if enabled from customer settings)
    /// </summary>
    [DefaultValue("John Smith")]
    public string Username { get; set; }

    /// <summary>
    /// Email address
    /// </summary>
    [DefaultValue("johnsmith@yourstore.com")]
    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    /// <summary>
    /// Time zone identifier (if AllowCustomersToSetTimeZone enabled from datetime settings)
    /// </summary>
    [DefaultValue("Central America Standard Time")]
    public string TimeZoneId { get; set; }

    /// <summary>
    /// Vat number (if EuVatEnabled enabled from tax settings)
    /// </summary>
    [DefaultValue("GB123456789")]
    public string VatNumber { get; set; }

    /// <summary>
    /// Gender (if GenderEnabled enabled from customer settings)
    /// </summary>
    [DefaultValue("M")]
    public string Gender { get; set; }

    /// <summary>
    /// First name (required if FirstNameEnabled &amp; FirstNameRequired enabled from customer settings)
    /// </summary>
    [DefaultValue("John")]
    public string FirstName { get; set; }

    /// <summary>
    /// Last name (required if LastNameEnabled &amp; LastNameRequired enabled from customer settings)
    /// </summary>
    [DefaultValue("Smith")]
    public string LastName { get; set; }

    /// <summary>
    /// Company name (required if CompanyEnabled &amp; CompanyRequired enabled from customer settings)
    /// </summary>
    [DefaultValue("Nop Solutions Ltd")]
    public string Company { get; set; }

    /// <summary>
    /// Address1 (required if StreetAddressEnabled &amp; StreetAddressRequired enabled from customer settings)
    /// </summary>
    [DefaultValue("21 West 52nd Street")]
    public string StreetAddress { get; set; }

    /// <summary>
    /// Address2 (required if StreetAddress2Enabled &amp; StreetAddress2Required enabled from customer settings)
    /// </summary>
    [DefaultValue("21 West 52nd Street")]
    public string StreetAddress2 { get; set; }

    /// <summary>
    /// Zip/postal code (required if ZipPostalCodeEnabled &amp; ZipPostalCodeRequired enabled from customer settings)
    /// </summary>
    [DefaultValue("10021")]
    public string ZipPostalCode { get; set; }

    /// <summary>
    /// City (required if CityEnabled &amp; CityRequired enabled from customer settings)
    /// </summary>
    [DefaultValue("New York")]
    public string City { get; set; }

    /// <summary>
    /// County (required if CountyEnabled &amp; CountyRequired enabled from customer settings)
    /// </summary>
    [DefaultValue("Bourbon")]
    public string County { get; set; }

    /// <summary>
    /// Country identifier (required if CountryEnabled &amp; CountryRequired enabled from customer settings)
    /// </summary>
    [DefaultValue(237)]
    public int CountryId { get; set; }

    /// <summary>
    /// State/province (required if CountryEnabled &amp; StateProvinceEnabled &amp; StateProvinceRequired enabled from customer settings)
    /// </summary>
    [DefaultValue(1708)]
    public int StateProvinceId { get; set; }

    /// <summary>
    /// Phone number (required if PhoneEnabled &amp; PhoneRequired enabled from customer settings)
    /// </summary>
    [DefaultValue("12345678")]
    [DataType(DataType.PhoneNumber)]
    public string Phone { get; set; }

    /// <summary>
    /// Fax number (required if FaxEnabled &amp; FaxRequired enabled from customer settings)
    /// </summary>
    [DefaultValue("12345678")]
    [DataType(DataType.PhoneNumber)]
    public string Fax { get; set; }

    /// <summary>
    /// Subscribe to newsletter? (if NewsletterEnabled enabled from customer settings)
    /// </summary>
    [DefaultValue(true)]
    public bool Newsletter { get; set; }

    /// <summary>
    /// Day of the birth date (required if DateOfBirthEnabled &amp; DateOfBirthRequired enabled from customer settings)
    /// </summary>
    [DefaultValue(1)]
    public int? DateOfBirthDay { get; set; }

    /// <summary>
    /// Month of the birth date (required if DateOfBirthEnabled &amp; DateOfBirthRequired enabled from customer settings)
    /// </summary>
    [DefaultValue(5)]
    public int? DateOfBirthMonth { get; set; }

    /// <summary>
    /// Year of the birth date (required if DateOfBirthEnabled &amp; DateOfBirthRequired enabled from customer settings)
    /// </summary>
    [DefaultValue(1998)]
    public int? DateOfBirthYear { get; set; }

    public DateTime? ParseDateOfBirth()
    {
        if (!DateOfBirthYear.HasValue || !DateOfBirthMonth.HasValue || !DateOfBirthDay.HasValue)
            return null;

        DateTime? dateOfBirth = null;
        try
        {
            dateOfBirth = new DateTime(DateOfBirthYear.Value, DateOfBirthMonth.Value, DateOfBirthDay.Value);
        }
        catch { }
        return dateOfBirth;
    }

    /// <summary>
    /// Additional info attributes
    /// <para><em><b>1. "customer_attribute_1": "Custom text"</b></em>  - customer_attribute_1(where 1 is customer attribute id), "Custom text" is value of attribute - <em>Control type is Textbox/Multiline text box</em></para>
    /// <para><em><b>2. "customer_attribute_2": "4,5"</b></em>       - customer_attribute_2(Where 2 is customer attribute id), "4,5" are customer attribute value ids(it can be single or multiple) - <em>Control type is Checkbox</em></para>
    /// <para><em><b>3. "customer_attribute_3": "1"</b></em>       - customer_attribute_3(Where 3 is customer attribute id), "1" is customer attribute value id</para>
    /// </summary>
    [DictionaryDefault("customer_attribute_1,Custom text,customer_attribute_3,1")]
    public IDictionary<string, string> CustomerAttributes { get; set; }

    /// <summary>
    /// Gdpr consents (if any &amp; GdprEnabled enabled from gdpr settings)
    /// <para><em><b>1. "consent1": "on"</b></em>  - "consent1" where "1" is gdpr consent id and "on" indicates gdpr conset is set to true</para>
    /// <para><em><b>2. "consent2": "on"</b></em></para>
    /// </summary>
    [DictionaryDefault("consent1,on,consent2,on")]
    public IDictionary<string, string> GdprConsents { get; set; }
}
