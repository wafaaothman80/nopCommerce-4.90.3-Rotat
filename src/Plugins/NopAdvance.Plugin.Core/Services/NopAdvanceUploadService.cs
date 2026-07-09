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
using Nop.Core.Infrastructure;
using Nop.Core;
using Nop.Services.Plugins;
using Nop.Services.Themes;
using Newtonsoft.Json;
using System.IO.Compression;
using NopAdvance.Plugin.Core.Infrastructure;

namespace NopAdvance.Plugin.Core.Services;

public class NopAdvanceUploadService : UploadService
{
    #region Fields

    private readonly INopFileProvider _fileProvider;

    #endregion

    #region Ctor

    public NopAdvanceUploadService(INopFileProvider fileProvider,
        IStoreContext storeContext,
        IThemeProvider themeProvider) : base (fileProvider, storeContext, themeProvider)
    {
        _fileProvider = fileProvider;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Get information about the uploaded items in the archive
    /// </summary>
    /// <param name="archivePath">Path to the archive</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the list of uploaded items
    /// </returns>
    protected override async Task<IList<UploadedItem>> GetUploadedItemsAsync(string archivePath)
    {
        using var archive = ZipFile.OpenRead(archivePath);
        //try to get the entry containing information about the uploaded items 
        var uploadedItemsFileEntry = archive.Entries
            .FirstOrDefault(entry => entry.Name.Equals(NopPluginDefaults.UploadedItemsFileName, StringComparison.InvariantCultureIgnoreCase)
                && string.IsNullOrEmpty(_fileProvider.GetDirectoryName(entry.FullName)));
        if (uploadedItemsFileEntry == null)
            return null;

        //read the content of this entry if exists
        await using var unzippedEntryStream = uploadedItemsFileEntry.Open();
        using var reader = new StreamReader(unzippedEntryStream);
        var content = await reader.ReadToEndAsync();

        var uploadedItems = JsonConvert.DeserializeObject<IList<UploadedItem>>(content);

        var corePluginUploadedItem = uploadedItems.Where(x => x.SystemName == CoreDefaults.SYSTEM_NAME &&
            (x.SupportedVersions?.Contains(NopVersion.CURRENT_VERSION) ?? false)).FirstOrDefault();

        if (corePluginUploadedItem != null)
        {
            var existingCorePluginJsonFile = _fileProvider.Combine(_fileProvider.MapPath(NopPluginDefaults.Path), CoreDefaults.SYSTEM_NAME, NopPluginDefaults.DescriptionFileName);
            if (_fileProvider.FileExists(existingCorePluginJsonFile))
            {
                var uploadedCorePluginJsonFileEntry = archive.Entries
                    .FirstOrDefault(entry => entry.FullName.Equals($"Plugins/{CoreDefaults.SYSTEM_NAME}/{NopPluginDefaults.DescriptionFileName}", StringComparison.InvariantCultureIgnoreCase));
                if (uploadedCorePluginJsonFileEntry != null)
                {
                    await using var unzippedCorePluginEntryStream = uploadedCorePluginJsonFileEntry.Open();
                    using var corePluginReader = new StreamReader(unzippedCorePluginEntryStream);
                    var corePluginJsonContent = await corePluginReader.ReadToEndAsync();
                    var corePluginJson = JsonConvert.DeserializeObject<PluginDescriptor>(corePluginJsonContent);

                    using var streamReader = new StreamReader(existingCorePluginJsonFile);
                    var descriptor = JsonConvert.DeserializeObject<PluginDescriptor>(streamReader.ReadToEnd());

                    if (corePluginJson.Version == descriptor.Version)
                        uploadedItems.Remove(corePluginUploadedItem);
                }
            }
        }

        return uploadedItems;
    }

    #endregion
}
