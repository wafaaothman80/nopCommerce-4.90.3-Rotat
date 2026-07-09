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
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopAdvance.Plugin.Core.Domain;
using NopAdvance.Plugin.Core.Infrastructure;

namespace NopAdvance.Plugin.Core.Helpers;

public class GenericHelper : IGenericHelper
{
    #region Fields

    private readonly INopFileProvider _fileProvider;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly ICustomerService _customerService;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly NopAdvanceCorePluginSettings _widgetZoneSettings;

    #endregion

    #region Ctor

    public GenericHelper(INopFileProvider fileProvider,
        IWorkContext workContext,
        IStoreContext storeContext,
        ICustomerService customerService,
        ILocalizationService localizationService,
        ISettingService settingService,
        NopAdvanceCorePluginSettings widgetZoneSettings)
    {
        _fileProvider = fileProvider;
        _workContext = workContext;
        _storeContext = storeContext;
        _customerService = customerService;
        _localizationService = localizationService;
        _settingService = settingService;
        _widgetZoneSettings = widgetZoneSettings;
    }

		#endregion

	#region Methods

	/// <summary>
	/// Gets plugin configure view page layout
	/// </summary>
	/// <returns>path</returns>
	public string GetPluginConfigureLayout()
    {
        return $"{NopPluginDefaults.Path}/{CoreDefaults.SYSTEM_NAME}/Areas/{AreaNames.ADMIN}/Views/_ConfigurePlugin.cshtml";
    }

    /// <summary>
    /// Gets theme configure view page layout
    /// </summary>
    /// <returns>path</returns>
    public string GetThemePluginConfigureLayout()
    {
        return $"{NopPluginDefaults.Path}/{CoreDefaults.SYSTEM_NAME}/Areas/{AreaNames.ADMIN}/Views/_ConfigureThemePlugin.cshtml";
    }

    public IQueryable<TEntity> ApplyDateRangeFilter<TEntity>(IQueryable<TEntity> query) where TEntity : BaseEntity, IDateRangeSupported
    {
        var now = DateTime.UtcNow;
        return query.Where(p =>
                (!p.StartDateUtc.HasValue || p.StartDateUtc.Value < now) &&
                (!p.EndDateUtc.HasValue || p.EndDateUtc.Value > now));

    }

    /// <summary>
    /// Get a plugin descriptor
    /// </summary>
    /// <param name="pluginDirectory">Plugin directory name</param>
    /// <returns>Plugin descriptor</returns>
    public NopAdvancePluginDescriptor GetPluginDescriptor(string pluginDirectory)
    {
        var filePath = _fileProvider.Combine(_fileProvider.MapPath($"{NopPluginDefaults.Path}/{pluginDirectory}"), NopPluginDefaults.DescriptionFileName);
        using var streamReader = new StreamReader(filePath);
        var descriptor = JsonConvert.DeserializeObject<NopAdvancePluginDescriptor>(streamReader.ReadToEnd());
        return descriptor;
    }

    public async Task<string> GetContactSupportLinkAsync(string pluginName)
    {
        var link = $"mailto:support@nopadvance.com?subject=Support request from {(await _workContext.GetCurrentCustomerAsync()).Email} for {pluginName}";
        link += $"&body=Hello,%0D%0A%0D%0AI am looking to get some support for your plugin {pluginName}.";
        link += $" The website in which I am using this plugin is {(await _storeContext.GetCurrentStoreAsync()).Url}.";
        link += $"%0D%0A%0D%0A";
        link += $"-- Please replace this block with the description of your request --";

        return link;
    }

    public async Task<string> GetContactSalesLinkAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var customerFullName = await _customerService.GetCustomerFullNameAsync(customer);
        var link = $"mailto:sales@nopadvance.com?subject=Sales query from {customerFullName} - {customer.Email}";
        link += $"&body=Hello,%0D%0A%0D%0A";
        link += "I am looking to hire your team for some development services.";
        link += $"%0D%0A%0D%0A";
        link += $"-- Please replace this block with the description of your request --";

        return link;
    }

    public IList<SelectListItem> PrepareWidgetZones(IList<SelectListItem> items, bool withSpecialDefaultItem = false, string defaultItemText = null, string defaultItemValue = "")
    {
        var customWidgetZoneExists = !string.IsNullOrWhiteSpace(_widgetZoneSettings.CustomWidgetZones);
        var props = typeof(PublicWidgetZones).GetProperties();

        if (!customWidgetZoneExists)
        {
            foreach (var prop in props)
            {
                items.Add(new SelectListItem
                {
                    Value = (string)prop.GetValue(null),
                    Text = string.Concat(prop.Name.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ')
                });
            }
        }
        else
        {
            var widgetZones = new Dictionary<string, string>();
            foreach (var prop in props)
            {
                widgetZones.Add(string.Concat(prop.Name.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' '), (string)prop.GetValue(null));
            }

            foreach (var widgetZone in _widgetZoneSettings.CustomWidgetZones.Split(','))
                widgetZones.Add(widgetZone, widgetZone);

            foreach (var widgetZone in widgetZones.OrderBy(key => key.Key))
                items.Add(new SelectListItem { Value = widgetZone.Value, Text = widgetZone.Key });
        }

        if (withSpecialDefaultItem)
            //insert this default item at first
            items.Insert(0, new SelectListItem { Text = defaultItemText, Value = defaultItemValue });

        return items;
    }

    public async Task<AdminMenuItem> GetHelpMenuItemAsync(string systemName)
    {
        var pluginDescriptor = GetPluginDescriptor(systemName);
        var helpMenuItem = new AdminMenuItem()
        {
            SystemName = $"{systemName}.Help",
            Title = await _localizationService.GetResourceAsync(CoreLocaleResourceDefaults.HELP_MENU),
            Visible = true,
            IconClass = CoreIconClassDefaults.QUESTION,
            Url = pluginDescriptor.HelpUrl,
            OpenUrlInNewTab = true
        };

        return helpMenuItem;
    }

    public async Task AddCustomWidgetZonesAsync(string[] widgetZones, int storeId = 0)
    {
        var widgetZoneSettings = await _settingService.LoadSettingAsync<NopAdvanceCorePluginSettings>(storeId);
        var allWidgetZones = widgetZoneSettings.CustomWidgetZones.Split(",").ToList();
        allWidgetZones.AddRange(widgetZones);
        widgetZoneSettings.CustomWidgetZones = string.Join(',', allWidgetZones.Distinct());

        await _settingService.SaveSettingAsync(widgetZoneSettings, storeId);
    }

    #endregion
}

public interface IGenericHelper
{
    /// <summary>
    /// Gets plugin configure view page layout
    /// </summary>
    /// <returns>path</returns>
    string GetPluginConfigureLayout();

    /// <summary>
    /// Gets theme configure view page layout
    /// </summary>
    /// <returns>path</returns>
    string GetThemePluginConfigureLayout();

    IQueryable<TEntity> ApplyDateRangeFilter<TEntity>(IQueryable<TEntity> query) where TEntity : BaseEntity, IDateRangeSupported;

    /// <summary>
    /// Get a plugin descriptor
    /// </summary>
    /// <param name="pluginDirectory">Plugin directory name</param>
    /// <returns>Plugin descriptor</returns>
    NopAdvancePluginDescriptor GetPluginDescriptor(string pluginDirectory);

    Task<string> GetContactSupportLinkAsync(string pluginName);

    Task<string> GetContactSalesLinkAsync();

    IList<SelectListItem> PrepareWidgetZones(IList<SelectListItem> items, bool withSpecialDefaultItem = false, string defaultItemText = null, string defaultItemValue = "");

    Task<AdminMenuItem> GetHelpMenuItemAsync(string systemName);

    Task AddCustomWidgetZonesAsync(string[] widgetZones, int storeId = 0);
}
