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

public partial record AddToCartRequest
{
    /// <summary>
    /// Quantity of the item
    /// </summary>
    [DefaultValue(1)]
    [Required]
    public int Quantity { get; set; }

    /// <summary>
    /// Update cart item identifier (if in update mode)
    /// </summary>
    [DefaultValue(0)]
    public int UpdateCartItemId { get; set; }

    /// <summary>
    /// Product attributes
    /// <para><em><b>1. "product_attribute_1": "Custom text"</b></em>  - product_attribute_1(where 1 is product attribute id), "Custom text" is value of attribute - <em>Control type is Textbox/Multiline text box</em></para>
    /// <para><em><b>2. "product_attribute_2": "3"</b></em>       - product_attribute_2(Where 2 is product attribute id), "3" is product attribute value id</para>
    /// <para><em><b>3. "product_attribute_3": "4,5"</b></em>       - product_attribute_3(Where 3 is product attribute id), "4,5" are product attribute value ids(it can be single or multiple) - <em>Control type is Checkbox</em></para>
    /// <para><em><b>4. "product_attribute_14_day": "1"</b></em>       - product_attribute_14_day(Where 14 is product attribute id),"1" is date value - <em>Control type is Date picker</em></para>
    /// <para><em><b>5. "product_attribute_14_month": "10"</b></em>       - product_attribute_14_month(Where 14 is product attribute id),"10" is month value - <em>Control type is Date picker</em></para>
    /// <para><em><b>6. "product_attribute_14_year": "2001"</b></em>       - product_attribute_14_year(Where 14 is product attribute id),"2001" is year value - <em>Control type is Date picker</em></para>
    /// <para><em><b>7. "product_attribute_1": "39"</b></em>       - product_attribute_1(Where 1 is product attribute id),"39" is product attribute value id - <em>Attribute value type is associated to product(for more then 1 associated product qty, point 7 is required)</em></para>
    /// <para><em><b>8. "product_attribute_1_39_qty": "3"</b></em>       - product_attribute_1_39_qty(Where 1 is product attribute id, 39 is product attribute value id),"3" is associated product qty - <em>Attribute value type is associated to product</em></para>
    /// <para><em><b>9. "giftcard_1.RecipientName": "John"</b></em>       - giftcard_1.RecipientName(Where 1 is product attribute id),"John" is entered value(Recipient name) - <em>When product is gift card</em></para>
    /// <para><em><b>10. "giftcard_1.RecipientEmail": "john@yourstore.com"</b></em>       - giftcard_1.RecipientEmail(Where 1 is product attribute id),"john@yourstore.com" is entered value(Recipient email) - <em>When product is gift card</em></para>
    /// <para><em><b>11. "giftcard_1.SenderName": "Smith"</b></em>       - giftcard_1.SenderName(Where 1 is product attribute id),"Smith" is entered value(Sender name) - <em>When product is gift card</em></para>
    /// <para><em><b>12. "giftcard_1.SenderEmail": "smith@yourstore.com"</b></em>       - giftcard_1.SenderEmail(Where 1 is product attribute id),"smith@yourstore.com" is entered value(Sender email) - <em>When product is gift card</em></para>
    /// <para><em><b>13. "giftcard_1.Message": "This is giftcard sample message"</b></em>       - giftcard_1.Message(Where 1 is product attribute id),"This is giftcard sample message" is entered value(Gift card message) - <em>When product is gift card</em></para>
    /// <para><em><b>14. "product_attribute_5": "2BD9E2C1-700C-4E81-A019-1889B8A5C0D3"</b></em>       - product_attribute_5(Where 5 is product attribute id),"2BD9E2C1-700C-4E81-A019-1889B8A5C0D3" is download guid for uploaded file - <em>Control type is File upload</em></para>
    /// </summary>
    [DictionaryDefault("product_attribute_1,1,product_attribute_2,3")]
    public IDictionary<string, string> Attributes { get; set; }
}
