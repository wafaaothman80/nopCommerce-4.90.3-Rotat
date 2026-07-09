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
using FluentMigrator;
using Nop.Data;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Services.Configuration;
using NopAdvance.Plugin.Core.Helpers;
using NopAdvance.Plugin.Misc.PublicAPI.Domain;
using NopAdvance.Plugin.Misc.PublicAPI.Infrastructure;

namespace NopAdvance.Plugin.Misc.PublicAPI.Data.Migrations;

[NopMigration("2024/02/21 19:22:18:6543528", PluginDefaults.DATA_MIGRATION, MigrationProcessType.Update)]
public class PublicAPIMigration : Migration
{
    #region Fields

    private readonly INopDataProvider _dataProvider;
    private readonly ILicenseHelper _licenseHelper;
    private readonly ISettingService _settingService;

    #endregion

    #region Ctor

    public PublicAPIMigration(INopDataProvider dataProvider,
        ILicenseHelper licenseHelper,
        ISettingService settingService)
    {
        _dataProvider = dataProvider;
        _licenseHelper = licenseHelper;
        _settingService = settingService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        var message = _licenseHelper.UpdatePluginAsync(PluginDefaults.SYSTEM_NAME,
            nameof(NopAdvanceAPISettings)).Result;

        if (!string.IsNullOrEmpty(message))
            return;

        var refreshTokenTableName = NameCompatibilityManager.GetTableName(typeof(APIRefreshToken));
        var refreshTokenExpirationAPIRefreshTokenColumnName = "RefreshTokenExpiration";
        var refreshTokenExpirationDurationTypeIdAPIRefreshTokenColumnName = "RefreshTokenExpirationDurationTypeId";
        var expiryInUtcAPIRefreshTokenColumnName = nameof(APIRefreshToken.ExpiryInUtc);
       
        if (Schema.Table(refreshTokenTableName).Column(refreshTokenExpirationAPIRefreshTokenColumnName).Exists() &&
                Schema.Table(refreshTokenTableName).Column(refreshTokenExpirationDurationTypeIdAPIRefreshTokenColumnName).Exists())
        {
            var refreshTokens = _dataProvider.QueryAsync<dynamic>($"SELECT * FROM {refreshTokenTableName}").Result;
            foreach (var refreshToken in refreshTokens)
            {
                var refreshTokenExpirationTimeSpan = new TimeSpan();
                switch ((DurationType)refreshToken.RefreshTokenExpirationDurationTypeId)
                {
                    case DurationType.Seconds:
                        refreshTokenExpirationTimeSpan = new TimeSpan(0, 0, 0, refreshToken.RefreshTokenExpiration);
                        break;
                    case DurationType.Minutes:
                        refreshTokenExpirationTimeSpan = new TimeSpan(0, 0, refreshToken.RefreshTokenExpiration, 0);
                        break;
                    case DurationType.Hours:
                        refreshTokenExpirationTimeSpan = new TimeSpan(0, refreshToken.RefreshTokenExpiration, 0, 0);
                        break;
                    case DurationType.Days:
                        refreshTokenExpirationTimeSpan = new TimeSpan(refreshToken.RefreshTokenExpiration, 0, 0, 0);
                        break;
                }
                try
                {
                    DateTime createdOnUtc = DateTime.Parse(refreshToken.CreatedOnUtc);
                    var expiryInUtc = createdOnUtc.Add(refreshTokenExpirationTimeSpan);
                    _dataProvider.ExecuteNonQueryAsync($"UPDATE {refreshTokenTableName} SET {expiryInUtcAPIRefreshTokenColumnName} = '{expiryInUtc}'").Wait();
                }
                catch (Exception)
                {
                }

            }
        }

        if (Schema.Table(refreshTokenTableName).Column(refreshTokenExpirationAPIRefreshTokenColumnName).Exists())
            Delete.Column(refreshTokenExpirationAPIRefreshTokenColumnName).FromTable(refreshTokenTableName);

        if (Schema.Table(refreshTokenTableName).Column(refreshTokenExpirationDurationTypeIdAPIRefreshTokenColumnName).Exists())
            Delete.Column(refreshTokenExpirationDurationTypeIdAPIRefreshTokenColumnName).FromTable(refreshTokenTableName);

        var apiSettingAllStore = _settingService.GetSettingAsync("nopadvanceapisettings.isapienabled", 0).Result;
        if (!string.IsNullOrEmpty(apiSettingAllStore.Value) && Convert.ToBoolean(apiSettingAllStore.Value))
            _settingService.SetSettingAsync(nameof(NopAdvanceAPISettings.Enabled), true, 0).Wait();
    }
   
    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
    
    #endregion
}
