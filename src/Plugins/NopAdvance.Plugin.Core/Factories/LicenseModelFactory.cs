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
using NopAdvance.Plugin.Core.Models;
using Nop.Services.Plugins;
using NopAdvance.Plugin.Core.Infrastructure;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Services.Configuration;

namespace NopAdvance.Plugin.Core.Factories;

public class LicenseModelFactory : ILicenseModelFactory
{
    #region Fields

    private readonly IPluginService _pluginService;
    private readonly ISettingService _settingService;

    #endregion

    #region Ctor

    public LicenseModelFactory(IPluginService pluginService,
        ISettingService settingService)
    {
        _pluginService = pluginService;
        _settingService = settingService;
    }

    #endregion

    #region Methods

    public Task<LicenseSearchModel> PrepareLicenseSearchModelAsync(LicenseSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        searchModel.SystemName = CorePluginDefaults.LICENSE_SYSTEM_NAME;
        searchModel.ControllerName = CorePluginDefaults.LICENSE_CONTROLLER_NAME;

        //prepare page parameters
        searchModel.SetGridPageSize();

        return Task.FromResult(searchModel);
    }

    public async Task<LicenseListModel> PrepareLicenseListModelAsync(LicenseSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //filter visible plugins
        var plugins = (await _pluginService.GetPluginDescriptorsAsync<IPlugin>(group: CoreDefaults.ROOT_MENU_SYSTEM_NAME, loadMode: LoadPluginsMode.InstalledOnly, dependsOnSystemName: CoreDefaults.SYSTEM_NAME))
            .Where(p => p.ShowInPluginsList).ToList()
            .ToPagedList(searchModel);

        //prepare list model
        var model = await new LicenseListModel().PrepareToGridAsync(searchModel, plugins, () =>
        {
            return plugins.SelectAwait(async pluginDescriptor =>
            {

                //fill in model values from the entity
                var licenseModel = pluginDescriptor.ToPluginModel<LicenseModel>();

                //fill in additional values (not existing in the entity)
                licenseModel.LicenseKey = await _settingService.GetSettingByKeyAsync($"{pluginDescriptor.SystemName.ToLowerInvariant()}.{CorePluginDefaults.LICENSE_KEY_SETTING}", "");
                licenseModel.IsActivated = !string.IsNullOrEmpty(licenseModel.LicenseKey);
                licenseModel.LogoUrl = await _pluginService.GetPluginLogoUrlAsync(pluginDescriptor);

                return licenseModel;
            });
        });

        return model;
    }

    #endregion
}
