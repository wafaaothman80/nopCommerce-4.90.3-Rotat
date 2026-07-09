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
using Nop.Core.Domain.Common;
using NopAdvance.Plugin.Misc.PublicAPI.Models.Requests;

namespace NopAdvance.Plugin.Misc.PublicAPI.Infrastructure.Extensions;

public static class MappingExtensions
{
    #region Address mapping

    public static Address ToEntity(this AddressRequest model, bool trimFields = true)
    {
        if (model == null)
            return null;

        var entity = new Address();
        return ToEntity(model, entity, trimFields);
    }

    public static Address ToEntity(this AddressRequest model, Address destination, bool trimFields = true)
    {
        if (model == null)
            return destination;

        if (trimFields)
        {
            if (model.FirstName != null)
                model.FirstName = model.FirstName.Trim();
            if (model.LastName != null)
                model.LastName = model.LastName.Trim();
            if (model.Email != null)
                model.Email = model.Email.Trim();
            if (model.Company != null)
                model.Company = model.Company.Trim();
            if (model.County != null)
                model.County = model.County.Trim();
            if (model.City != null)
                model.City = model.City.Trim();
            if (model.Address1 != null)
                model.Address1 = model.Address1.Trim();
            if (model.Address2 != null)
                model.Address2 = model.Address2.Trim();
            if (model.ZipPostalCode != null)
                model.ZipPostalCode = model.ZipPostalCode.Trim();
            if (model.PhoneNumber != null)
                model.PhoneNumber = model.PhoneNumber.Trim();
            if (model.FaxNumber != null)
                model.FaxNumber = model.FaxNumber.Trim();
        }
        //destination.Id = model.Id;
        destination.FirstName = model.FirstName;
        destination.LastName = model.LastName;
        destination.Email = model.Email;
        destination.Company = model.Company;
        destination.CountryId = model.CountryId == 0 ? null : model.CountryId;
        destination.StateProvinceId = model.StateProvinceId == 0 ? null : model.StateProvinceId;
        destination.County = model.County;
        destination.City = model.City;
        destination.Address1 = model.Address1;
        destination.Address2 = model.Address2;
        destination.ZipPostalCode = model.ZipPostalCode;
        destination.PhoneNumber = model.PhoneNumber;
        destination.FaxNumber = model.FaxNumber;

        return destination;
    }

    #endregion
}
