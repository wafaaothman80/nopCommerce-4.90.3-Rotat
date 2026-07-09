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
using System.Linq;

namespace NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;

public class DictionaryDefaultAttribute : DefaultValueAttribute
{
    public DictionaryDefaultAttribute(string dataDictonaryKeyValue) : base(GetDataDictionary(dataDictonaryKeyValue))
    {
    }

    private static Dictionary<string, string> GetDataDictionary(string dataDictonaryKeyValue)
    {
        var dataDictonary = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(dataDictonaryKeyValue))
        {
            var dataDictonaryKeyValueList = dataDictonaryKeyValue.Split(",").ToList();
            for (int i = 0; i < dataDictonaryKeyValueList.Count; i++)
            {
                dataDictonary.Add(dataDictonaryKeyValueList[i], dataDictonaryKeyValueList[i + 1]);
                i++;
            }
        }
        
        return dataDictonary;
    }
}
