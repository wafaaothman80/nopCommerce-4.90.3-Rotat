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
using Nop.Core.Domain.Configuration;
using Nop.Data;

namespace NopAdvance.Plugin.Misc.PublicAPI.Services;

public class PublicSettingService : IPublicSettingService
{
    #region fields

    private readonly IRepository<Setting> _settingRepository;

    #endregion

    #region Ctor

    public PublicSettingService(IRepository<Setting> settingRepository)
    {
        _settingRepository = settingRepository;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get settings by name
    /// </summary>
    /// <param name="names">names</param>
    /// <param name="storeId">storeId</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the setting name
    /// </returns>
    public virtual async Task<IList<Setting>> GetSettingsByNameAsync(string[] names, int storeId = 0)
    {
        var settings = await _settingRepository.GetAllAsync(query =>
        {
            query = from s in query
                    where names.Contains(s.Name) && s.StoreId == storeId
                    select s;

            var defaultSettingsNames = new List<string>();
            if (query.Any())
                defaultSettingsNames = names.Where(n => query.Any(s => !n.Contains(s.Name))).Select(n => n).ToList();
            else
                defaultSettingsNames = names.ToList();


            var query1 = from s in _settingRepository.Table
                         where defaultSettingsNames.Contains(s.Name) && s.StoreId == 0
                         select s;

            query1 = query1.Union(from s in query
                                  select s);

            return query1;

        });

        return settings;
    }

    #endregion
}
