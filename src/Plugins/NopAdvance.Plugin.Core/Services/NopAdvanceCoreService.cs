// ***	 ** ****** ****** ****** ******* **     ** ****** ***   ** **** ****
// ****  ** **  ** **  ** **  **  **  **  **   **  **  ** ****  ** *    *  
// ** ** ** **  ** ****** ******  **  **   ** **   ****** ** ** ** *    ***
// **  **** **  ** **	  **  **  **  **    ***    **  ** **  **** *    *  
// **   *** ****** **	  **  ** *******     *     **  ** **   *** **** ****
// ***************************************************************************
// *                                                                         *
// *    NopAdvance Core Plugin by NopAdvance team                            *
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
using Nop.Core.Domain.Security;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;

namespace NopAdvance.Plugin.Core.Services;
public class NopAdvanceCoreService : INopAdvanceCoreService
{
    #region Fields

    private readonly IRepository<PermissionRecord> _permissionRecordRepository;
    private readonly ICustomerService _customerService;
    private readonly IPermissionService _permissionService;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public NopAdvanceCoreService(IRepository<PermissionRecord> permissionRecordRepository,
        ICustomerService customerService,
        IPermissionService permissionService,
        ILocalizationService localizationService)
    {
        _permissionRecordRepository = permissionRecordRepository;
        _customerService = customerService;
        _permissionService = permissionService;
        _localizationService = localizationService;
    }

    #endregion

    #region Utilities

    private async Task<PermissionRecord> GetPermissionRecordBySystemNameAsync(string systemName)
    {
        if (string.IsNullOrWhiteSpace(systemName))
            return null;

        var query = from pr in _permissionRecordRepository.Table
                    where pr.SystemName == systemName
                    orderby pr.Id
                    select pr;

        var permissionRecord = await query.FirstOrDefaultAsync();
        return permissionRecord;
    }

    #endregion

    #region Methods

    public async Task UninstallPermissionsAsync(IList<PermissionConfig> allConfigs)
    {
        //uninstall permissions
        foreach (var config in allConfigs)
        {
            var permissionRecord = await GetPermissionRecordBySystemNameAsync(config.SystemName);
            if (permissionRecord == null)
                continue;

            //clear permission record customer role mapping
            foreach (var defaultCustomerRole in config.DefaultCustomerRoles)
            {
                var customerRole = await _customerService.GetCustomerRoleBySystemNameAsync(defaultCustomerRole);
                await _permissionService.DeletePermissionRecordCustomerRoleMappingAsync(permissionRecord.Id, customerRole.Id);
            }

            //delete permission
            await _permissionService.DeletePermissionRecordAsync(permissionRecord);

            //delete localization
            await _localizationService.DeleteLocalizedPermissionNameAsync(permissionRecord);
        }
    }

    #endregion
}

public interface INopAdvanceCoreService
{
    Task UninstallPermissionsAsync(IList<PermissionConfig> allConfigs);
}
