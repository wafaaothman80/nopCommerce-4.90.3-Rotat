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
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Services.Plugins;
using Nop.Web.Framework.Themes;
using NopAdvance.Plugin.Core.Infrastructure;

namespace NopAdvance.Plugin.Core.Helpers;

public class ThemeHelper : IThemeHelper
{
    #region Fields

    private readonly INopFileProvider _fileProvider;
    private readonly IThemeContext _themeContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly MediaSettings _mediaSettings;
    private readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public ThemeHelper(INopFileProvider fileProvider,
        IThemeContext themeContext,
        IHttpContextAccessor httpContextAccessor,
        MediaSettings mediaSettings,
        IWebHelper webHelper)
    {
        _fileProvider = fileProvider;
        _themeContext = themeContext;
        _httpContextAccessor = httpContextAccessor;
        _mediaSettings = mediaSettings;
        _webHelper = webHelper;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get plugin image path of the current theme
    /// </summary>
    /// <param name="pluginDirectory">Output directory name of the plugin</param>
    /// <returns>Path of the image files for the current theme</returns>
    public async Task<string> GetThemeImagePathAsync(string pluginDirectory)
    {
        var pathBase = _httpContextAccessor.HttpContext.Request.PathBase.Value ?? string.Empty;
        var imagePathUrl = _mediaSettings.UseAbsoluteImagePath ? _webHelper.GetStoreLocation() : $"{pathBase}/";
        imagePathUrl += "Plugins/" + pluginDirectory + "/Themes/{0}/Content/images";

        if (!string.IsNullOrEmpty(pluginDirectory))
        {
            var currentThemeName = await _themeContext.GetWorkingThemeNameAsync();
            var themeDirectoryPath = _fileProvider.MapPath(@"Plugins/" + pluginDirectory + "/Themes/" + currentThemeName);

            if (_fileProvider.DirectoryExists(themeDirectoryPath))
                return string.Format(imagePathUrl, currentThemeName);
        }
        return string.Format(imagePathUrl, CorePluginDefaults.DEFAULT_THEME);
    }

    /// <summary>
    /// Get plugin css path of the current theme
    /// </summary>
    /// <param name="pluginDirectory">Output directory name of the plugin</param>
    /// <returns>Path of the css files for the current theme</returns>
    public async Task<string> GetThemeCssPathAsync(string pluginDirectory)
    {
        string cssPath = $"{NopPluginDefaults.Path}/{pluginDirectory}/Themes/{{0}}/Content/css";

        if (!string.IsNullOrEmpty(pluginDirectory))
        {
            var currentThemeName = await _themeContext.GetWorkingThemeNameAsync();
            var themeDirectoryPath = _fileProvider.MapPath(@"Plugins/" + pluginDirectory + "/Themes/" + currentThemeName);

            if (_fileProvider.DirectoryExists(themeDirectoryPath))
                return string.Format(cssPath, currentThemeName);
        }
        return string.Format(cssPath, CorePluginDefaults.DEFAULT_THEME);
    }

    /// <summary>
    /// Get plugin js path of the current theme
    /// </summary>
    /// <param name="pluginDirectory">Output directory name of the plugin</param>
    /// <returns>Path of the js files for the current theme</returns>
    public async Task<string> GetThemeJsPathAsync(string pluginDirectory)
    {
        string jsPath = $"{NopPluginDefaults.Path}/{pluginDirectory}/Themes/{{0}}/Content/js";

        if (!string.IsNullOrEmpty(pluginDirectory))
        {
            var currentThemeName = await _themeContext.GetWorkingThemeNameAsync();
            var themeDirectoryPath = _fileProvider.MapPath(@"Plugins/" + pluginDirectory + "/Themes/" + currentThemeName);

            if (_fileProvider.DirectoryExists(themeDirectoryPath))
                return string.Format(jsPath, currentThemeName);
        }
        return string.Format(jsPath, CorePluginDefaults.DEFAULT_THEME);
    }

    /// <summary>
    /// Get plugin viewcomponent view path of the current theme
    /// </summary>
    /// <param name="pluginDirectory">Output directory name of the plugin</param>
    /// <returns>Path of the viewcomponent view files for the current theme</returns>
    public async Task<string> GetThemeViewComponentViewPathAsync(string pluginDirectory, string viewComponent)
    {
        var currentThemeName = await _themeContext.GetWorkingThemeNameAsync();
        var themeViewPath = $"{NopPluginDefaults.Path}/{pluginDirectory}/Themes/{currentThemeName}/Views/Shared/Components/{viewComponent}/Default.cshtml";
        if (_fileProvider.FileExists(_fileProvider.MapPath(themeViewPath)))
            return themeViewPath;
        else
            return $"{NopPluginDefaults.Path}/{pluginDirectory}/Views/Shared/Components/{viewComponent}/Default.cshtml";
    }

    #endregion
}

public interface IThemeHelper
{
    /// <summary>
    /// Get plugin image path of the current theme
    /// </summary>
    /// <param name="pluginDirectory">Output directory name of the plugin</param>
    /// <returns>Path of the image files for the current theme</returns>
    Task<string> GetThemeImagePathAsync(string pluginDirectory);

    /// <summary>
    /// Get plugin css path of the current theme
    /// </summary>
    /// <param name="pluginDirectory">Output directory name of the plugin</param>
    /// <returns>Path of the css files for the current theme</returns>
    Task<string> GetThemeCssPathAsync(string pluginDirectory);

    /// <summary>
    /// Get plugin js path of the current theme
    /// </summary>
    /// <param name="pluginDirectory">Output directory name of the plugin</param>
    /// <returns>Path of the js files for the current theme</returns>
    Task<string> GetThemeJsPathAsync(string pluginDirectory);

    /// <summary>
    /// Get plugin viewcomponent view path of the current theme
    /// </summary>
    /// <param name="pluginDirectory">Output directory name of the plugin</param>
    /// <param name="viewComponent">ViewComponent name</param>
    /// <returns>Path of the viewcomponent view files for the current theme</returns>
    Task<string> GetThemeViewComponentViewPathAsync(string pluginDirectory, string viewComponent);
}
