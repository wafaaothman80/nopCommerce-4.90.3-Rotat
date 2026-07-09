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
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using System.Xml;

namespace NopAdvance.Plugin.Core.Helpers;

public class LocaleResourceHelper : ILocaleResourceHelper
{
    #region Fields

    private readonly INopFileProvider _fileProvider;
    private readonly ILocalizationService _localizationService;
    private readonly ILanguageService _languageService;
    private readonly LocalizationSettings _localizationSettings;

    #endregion

    #region Ctor

    public LocaleResourceHelper(INopFileProvider fileProvider,
        ILocalizationService localizationService,
        ILanguageService languageService,
        LocalizationSettings localizationSettings)
    {
        _fileProvider = fileProvider;
        _localizationService = localizationService;
        _languageService = languageService;
        _localizationSettings = localizationSettings;
    }

    #endregion

    #region Utilities

    private IDictionary<string, string> LoadLocaleResourcesFromStream(StreamReader xmlStreamReader)
    {
        var result = new Dictionary<string, string>();
        using (var xmlReader = XmlReader.Create(xmlStreamReader))
            while (xmlReader.ReadToFollowing("Language"))
            {
                if (xmlReader.NodeType != XmlNodeType.Element)
                    continue;

                using var languageReader = xmlReader.ReadSubtree();
                while (languageReader.ReadToFollowing("LocaleResource"))
                {
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.GetAttribute("Name") is string name)
                    {
                        using var lrReader = languageReader.ReadSubtree();
                        if (lrReader.ReadToFollowing("Value") && lrReader.NodeType == XmlNodeType.Element)
                        {
                            string loweredName = name.ToLowerInvariant();
                            if (result.ContainsKey(loweredName))
                                result[loweredName] = lrReader.ReadString();
                            else
                                result.Add(loweredName, lrReader.ReadString());
                        }
                    }
                }
                break;
            }

        return result;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Add or update locale resources
    /// </summary>
    /// <param name="pluginDirectory">Plugin's directory name</param>
    /// <param name="languageId">Language identifier; pass null to add the passed resources for all languages</param>
    /// <param name="deleteNotInUse">Delete not in use locale resources?/param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task AddOrUpdateLocaleResourcesAsync(string pluginDirectory, int? languageId = null,
        bool deleteNotInUse = true)
    {
        if (string.IsNullOrEmpty(pluginDirectory))
            return;

        var folderPath = _fileProvider.MapPath($"{NopPluginDefaults.Path}/{pluginDirectory}/Localization");

        if (_fileProvider.DirectoryExists(folderPath))
        {
            var resourceFiles = _fileProvider.GetFiles(folderPath, "*.xml");

            for (var i = 0; i < resourceFiles.Length; i++)
            {
                var resourceFile = resourceFiles[i];
                var resources = LoadLocaleResourcesFromStream(new StreamReader(resourceFile));
                await _localizationService.AddOrUpdateLocaleResourceAsync(resources, languageId);
            }
        }

        if (deleteNotInUse)
            await DeleteNotInUseLocaleResourcesAsync(pluginDirectory);
    }

    public async Task DeleteNotInUseLocaleResourcesAsync(string pluginDirectory)
    {
        if (string.IsNullOrEmpty(pluginDirectory))
            return;

        var folderPath = _fileProvider.MapPath($"{NopPluginDefaults.Path}/{pluginDirectory}/Localization");

        if (_fileProvider.DirectoryExists(folderPath))
        {
            if (_localizationSettings.LoadAllLocaleRecordsOnStartup)
            {
                var resourceFiles = _fileProvider.GetFiles(folderPath, "*.xml");
                var fileResources = new List<string>();
                for (var i = 0; i < resourceFiles.Length; i++)
                {
                    var resourceFile = resourceFiles[i];
                    var resources = LoadLocaleResourcesFromStream(new StreamReader(resourceFile));
                    fileResources.AddRange(resources.Select(x => x.Key));
                }
                fileResources = fileResources.Distinct().ToList();

                var pluginAllResources = new List<string>();
                var languages = await _languageService.GetAllLanguagesAsync();
                foreach (var language in languages)
                {
                    var resources = await _localizationService.GetAllResourceValuesAsync(language.Id, true);
                    pluginAllResources.AddRange(resources.Where(x => x.Key.StartsWith(pluginDirectory.ToLowerInvariant())).Select(x => x.Key));
                }
                pluginAllResources = pluginAllResources.Distinct().ToList();

                var toDelete = pluginAllResources.Except(fileResources).ToList();
                await _localizationService.DeleteLocaleResourcesAsync(toDelete);
            }
        }
    }

    #endregion
}

public interface ILocaleResourceHelper
{
    /// <summary>
    /// Add or update locale resources
    /// </summary>
    /// <param name="pluginDirectory">Plugin's directory name</param>
    /// <param name="languageId">Language identifier; pass null to add the passed resources for all languages</param>
    /// <param name="deleteNotInUse">Delete not in use locale resources?/param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task AddOrUpdateLocaleResourcesAsync(string pluginDirectory, int? languageId = null,
        bool deleteNotInUse = true);

    Task DeleteNotInUseLocaleResourcesAsync(string pluginDirectory);
}
