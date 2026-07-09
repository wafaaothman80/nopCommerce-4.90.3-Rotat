using System.Text;
using Nop.Core;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Data;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using NopStation.Plugin.Misc.AlgoliaSearch.Extensions;
using NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.AlgoliaSearch;

public class AlgoliaSearchPlugin : BasePlugin, IMiscPlugin, INopStationPlugin, IWidgetPlugin
{
    #region Fields

    private readonly IPermissionService _permissionService;
    private readonly ILocalizationService _localizationService;
    private readonly IWebHelper _webHelper;
    private readonly IScheduleTaskService _scheduleTaskService;
    private readonly ISettingService _settingService;
    private readonly INopDataProvider _dataProvider;
    private readonly AlgoliaSearchSettings _algoliaSearchSettings;

   

    #endregion

    #region Ctor

    public AlgoliaSearchPlugin(IPermissionService permissionService,
        ILocalizationService localizationService,
        IWebHelper webHelper,
        IScheduleTaskService scheduleTaskService,
        ISettingService settingService,
        INopDataProvider dataProvider,
        AlgoliaSearchSettings algoliaSearchSettings)
    {
        _permissionService = permissionService;
        _localizationService = localizationService;
        _webHelper = webHelper;
        _scheduleTaskService = scheduleTaskService;
        _settingService = settingService;
        _dataProvider = dataProvider;
        _algoliaSearchSettings = algoliaSearchSettings;
    }


    #endregion

    #region Utilities

    protected virtual string ReadNextStatementFromStream(StreamReader reader)
    {
        var sb = new StringBuilder();
        while (true)
        {
            var lineOfText = reader.ReadLine();
            if (lineOfText == null)
            {
                if (sb.Length > 0)
                {
                    return sb.ToString();
                }
                return null;
            }
            if (lineOfText.TrimEnd(Array.Empty<char>()).ToUpper() == "GO")
            {
                return sb.ToString();
            }
            sb.Append(lineOfText + Environment.NewLine);
        }
    }

    protected virtual async Task ExecuteSqlFileAsync(string path)
    {
        var statements = new List<string>();

        using (var stream = File.OpenRead(path))
        using (var reader = new StreamReader(stream))
        {
            var statement = "";
            while ((statement = ReadNextStatementFromStream(reader)) != null)
                statements.Add(statement);
        }

        foreach (var stmt in statements)
            await _dataProvider.ExecuteNonQueryAsync(stmt);
    }

    #endregion

    #region Methods

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/AlgoliaSearch/Configure";
    }

    public override async Task InstallAsync()
    {
        var settings = new AlgoliaSearchSettings()
        {
            AllowCategoryFilter = true,
            AllowCustomersToSelectPageSize = true,
            AllowedSortingOptions = new List<int>()
            {
                (int)AlgoliaSortingEnum.Position,
                (int)AlgoliaSortingEnum.NameAsc,
                (int)AlgoliaSortingEnum.NameDesc,
                (int)AlgoliaSortingEnum.PriceAsc,
                (int)AlgoliaSortingEnum.PriceDesc,
                (int)AlgoliaSortingEnum.CreatedOn
            },
            AllowManufacturerFilter = true,
            AllowPriceRangeFilter = true,
            AllowRatingFilter = true,
            AllowSpecificationFilter = true,
            AllowProductSorting = true,
            AllowProductViewModeChanging = true,
            AllowVendorFilter = true,
            AutoCompleteListSize = 3,
            EnableAutoComplete = true,
            EnableMultilingualSearch = false,
            HidePoweredByAlgolia = false,
            MaximumCategoriesShowInFilter = 10,
            MaximumAttributesShowInFilter = 10,
            MaximumManufacturersShowInFilter = 10,
            MaximumSpecificationsShowInFilter = 10,
            MaximumVendorsShowInFilter = 10,
            ShowProductsCount = true,
            SearchPagePageSizeOptions = "6,12,18,30",
            AllowAttributeFilter = false,
            SearchPageProductsPerPage = 12,
            SearchTermMinimumLength = 3,
            ShowProductImagesInSearchAutoComplete = true,
            DefaultViewMode = "grid",
            WidgetZones = ""
        };
        await _settingService.SaveSettingAsync(settings);

        await this.InstallPluginAsync();

        var task = await _scheduleTaskService.GetTaskByTypeAsync(AlgoliaDefaults.ScheduleTaskType);
        if (task == null)
        {
            await _scheduleTaskService.InsertTaskAsync(new ScheduleTask()
            {
                Enabled = true,
                Name = "Update algolia items",
                Seconds = 3600,
                Type = AlgoliaDefaults.ScheduleTaskType,
                StopOnError = false
            });
        }

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        if (await _scheduleTaskService.GetTaskByTypeAsync(AlgoliaDefaults.ScheduleTaskType) is ScheduleTask scheduleTask)
            await _scheduleTaskService.DeleteTaskAsync(scheduleTask);

        await this.UninstallPluginAsync();
        await base.UninstallAsync();
    }

    public override async Task UpdateAsync(string currentVersion, string targetVersion)
    {
        await _localizationService.AddOrUpdateLocaleResourceAsync(GetPluginResources());
        await base.UpdateAsync(currentVersion, targetVersion);
    }

    public IDictionary<string, string> GetPluginResources()
    {
        var list = new Dictionary<string, string>
        {
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.EnableAutoComplete"] = "Enabled auto complete",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.EnableAutoComplete.Hint"] = "Check to enable auto complete in search box.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AutoCompleteListSize"] = "Auto complete list size",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AutoCompleteListSize.Hint"] = "Enter the number of products that will be showm in auto complete list.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.ShowProductImagesInSearchAutoComplete"] = "Show product images in search auto complete",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.ShowProductImagesInSearchAutoComplete.Hint"] = "Check to show product images in search auto complete.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.ApplicationId"] = "Application id",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.ApplicationId.Hint"] = "The algolia application id. Click 'Update index' if the value of this property is changed.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchOnlyKey"] = "Search only key",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchOnlyKey.Hint"] = "The algolia search only key. Click 'Update index' if the value of this property is changed.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AdminKey"] = "Admin key",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AdminKey.Hint"] = "The algolia admin key. Click 'Update index' if the value of this property is changed.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.MonitoringKey"] = "Monitoring key",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.MonitoringKey.Hint"] = "The algolia aonitoring key. Click 'Update index' if the value of this property is changed.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.UsageKey"] = "Usage key",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.UsageKey.Hint"] = "The algolia usage key. Click 'Update index' if the value of this property is changed.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchTermMinimumLength"] = "Search term minimum length",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchTermMinimumLength.Hint"] = "The search term minimum query length.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowCustomersToSelectPageSize"] = "Allow customers to select page size",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowCustomersToSelectPageSize.Hint"] = "Check to allow customers to select page size.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchPagePageSizeOptions"] = "Search page age size options",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchPagePageSizeOptions.Hint"] = "The search page page size options.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchPageProductsPerPage"] = "Search page products per page",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchPageProductsPerPage.Hint"] = "The page size of search page.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowProductViewModeChanging"] = "Allow product view mode changing",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowProductViewModeChanging.Hint"] = "Check to allow customers to change product view mode.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.DefaultViewMode"] = "Default view mode",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.DefaultViewMode.Hint"] = "Select search page default product view mode.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowCategoryFilter"] = "Allow category filter",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowCategoryFilter.Hint"] = "Check to allow customers to filter by product categories.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumCategoriesShowInFilter"] = "Maximum categories show in filter",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumCategoriesShowInFilter.Hint"] = "Maximum number of categories show in filter.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowVendorFilter"] = "Allow vendor filter",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowVendorFilter.Hint"] = "Check to allow customers to filter by product vendors.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumVendorsShowInFilter"] = "Maximum vendors show in filter",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumVendorsShowInFilter.Hint"] = "Maximum number of vendors show in filter.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowManufacturerFilter"] = "Allow manufacturer filter",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowManufacturerFilter.Hint"] = "Check to allow customers to filter by product manufacturers.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumManufacturersShowInFilter"] = "Maximum manufacturers show in filter",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumManufacturersShowInFilter.Hint"] = "Maximum number of manufacturers show in filter.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowSpecificationFilter"] = "Allow specification filter",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowSpecificationFilter.Hint"] = "Check to allow customers to filter by product specifications.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumSpecificationsShowInFilter"] = "Maximum specifications show in filter",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumSpecificationsShowInFilter.Hint"] = "Maximum number of specifications show in filter.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowAttributeFilter"] = "Allow attribute filter",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowAttributeFilter.Hint"] = "Check to allow customers to filter by product attributes.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumAttributesShowInFilter"] = "Maximum attributes show in filter",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumAttributesShowInFilter.Hint"] = "Maximum number of attributes show in filter.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowProductSorting"] = "Allow product sorting",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowProductSorting.Hint"] = "Check to allow customers to sort products. Click 'Update index' if the value of this property is changed.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowedSortingOptions"] = "Allowed sorting options",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowedSortingOptions.Hint"] = "Allowed sorting options. Click 'Update index' if the value of this property is changed.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowPriceRangeFilter"] = "Allow price range filter",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowPriceRangeFilter.Hint"] = "Check to allow customers to filter by product price range.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowRatingFilter"] = "Allow rating filter",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowRatingFilter.Hint"] = "Check to allow customers to filter by product rating.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.ShowProductsCount"] = "Show products count",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.ShowProductsCount.Hint"] = "Check to show products count on product search page.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.HidePoweredByAlgolia"] = "Hide powered by algolia",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.HidePoweredByAlgolia.Hint"] = "Check to hide powered by algolia from search result footer. Please don't hide it when you are using community plan.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.EnableMultilingualSearch"] = "Enable Multilingual Search",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.EnableMultilingualSearch.Hint"] = "Enable this if you want to search on different languages.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.EnableMultilingualSearch.Warnings"] = "If you enable this make sure your languages are supported by the algolia. For more informatio <a href=\"https://www.algolia.com/doc/guides/managing-results/optimize-search-results/handling-natural-languages-nlp/in-depth/supported-languages/\" >click Here.</a>",

            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.ApplicationId.Required"] = "The 'Application id' is required.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchOnlyKey.Required"] = "The 'Search only key' is required.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AdminKey.Required"] = "The 'Admin key' is required.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.UsageKey.Required"] = "The 'Usage key' is required.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.MonitoringKey.Required"] = "The 'Monitoring key' is required.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.MinimumQueryLength.GreaterThanZero"] = "'Minimum query length' must be greater than '0'.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.DefaultViewMode.Required"] = "The 'Default view mode' is required.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchPagePageSizeOptions.Required"] = "The 'Search page page size options' is required.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchPagePageSizeOptions.InvalidPageSizeOptions"] = "Invalid page size options.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchPageProductsPerPage.GreaterThanZero"] = "'Page size' must be greater than '0'.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumCategoriesShowInFilter.GreaterThanZero"] = "'Maximum categories show in filter' must be greater than '0'.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumVendorsShowInFilter.GreaterThanZero"] = "'Maximum vendors show in filter' must be greater than '0'.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumManufacturersShowInFilter.GreaterThanZero"] = "'Maximum manufacturers show in filter' must be greater than '0'.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumSpecificationsShowInFilter.GreaterThanZero"] = "'Maximum specifications show in filter' must be greater than '0'.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowedSortOptions.Required"] = "The 'Allowed sorting options' is required.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.AutoCompleteListSize.GreaterThanZero"] = "'Auto complete list size' must be greater than '0'.",

            ["Admin.NopStation.AlgoliaSearch.UploadProduct.FromId"] = "From id",
            ["Admin.NopStation.AlgoliaSearch.UploadProduct.FromId.Hint"] = "The from product id for upload to algolia.",
            ["Admin.NopStation.AlgoliaSearch.UploadProduct.ToId"] = "To id",
            ["Admin.NopStation.AlgoliaSearch.UploadProduct.ToId.Hint"] = "The to product id for upload to algolia.",

            ["Admin.NopStation.AlgoliaSearch.Menu.AlgoliaSearch"] = "Algolia search",
            ["Admin.NopStation.AlgoliaSearch.Menu.Configuration"] = "Configuration",
            ["Admin.NopStation.AlgoliaSearch.Menu.UpdatableItems"] = "Updatable items",
            ["Admin.NopStation.AlgoliaSearch.Menu.UploadProducts"] = "Upload products",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Title"] = "Algolia settings",
            ["Admin.NopStation.AlgoliaSearch.Configuration.IndexCleared"] = "Algolia index has been cleared successfully.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.IndexUpdated"] = "Algolia index has been updated successfully.",
            ["Admin.NopStation.AlgoliaSearch.Configuration.ClearIndex"] = "Clear index",
            ["Admin.NopStation.AlgoliaSearch.Configuration.UpdateIndex"] = "Update index",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Update"] = "Update",
            ["Admin.NopStation.AlgoliaSearch.Configuration.UpdateIndexTitle"] = "Update algolia index settings",
            ["Admin.NopStation.AlgoliaSearch.Configuration.ClearIndexWarning"] = "Are you sure want to clear algolia index?",
            ["Admin.NopStation.AlgoliaSearch.Configuration.BlockTitle.Credential"] = "Algolia credential",
            ["Admin.NopStation.AlgoliaSearch.Configuration.BlockTitle.Search"] = "Product search",
            ["Admin.NopStation.AlgoliaSearch.Configuration.BlockTitle.Filter"] = "Product filter",
            ["Admin.NopStation.AlgoliaSearch.Configuration.AlgoliaIndexUpdateFailed"] = "Failed to update algolia index. Please check error message in system log.",

            ["Admin.NopStation.AlgoliaSearch.UploadProduct.Title"] = "Upload products to algolia",
            ["Admin.NopStation.AlgoliaSearch.UploadProduct.Upload"] = "Upload",
            ["Admin.NopStation.AlgoliaSearch.UploadProduct.IndexCleared"] = "Algolia index has been cleared successfully.",
            ["Admin.NopStation.AlgoliaSearch.UploadProduct.UploadCompleted"] = "Product upload has been completed.",

            ["Admin.NopStation.AlgoliaSearch.UpdatableItem.Name"] = "Name",
            ["Admin.NopStation.AlgoliaSearch.UpdatableItem.UpdatedBy"] = "Updated by",
            ["Admin.NopStation.AlgoliaSearch.UpdatableItem.UpdatedOn"] = "Updated on",
            ["Admin.NopStation.AlgoliaSearch.UpdatableItem.Title"] = "Updatable items",
            ["Admin.NopStation.AlgoliaSearch.UpdatableItem.UpdateAll"] = "Update all",
            ["Admin.NopStation.AlgoliaSearch.UpdatableItem.BlockTitle.Product"] = "Product",
            ["Admin.NopStation.AlgoliaSearch.UpdatableItem.BlockTitle.Category"] = "Category",
            ["Admin.NopStation.AlgoliaSearch.UpdatableItem.BlockTitle.Manufacturer"] = "Manufacturer",
            ["Admin.NopStation.AlgoliaSearch.UpdatableItem.BlockTitle.Vendor"] = "Vendor",
            ["Admin.NopStation.AlgoliaSearch.UpdatableItem.Product.Error"] = "The product (ID = {0}) has an issue and was not updated.\n {1}",

            ["Admin.NopStation.AlgoliaSearch.UpdateIndices.ResetSearchableAttributeSettings"] = "Reset searchable attribute settings",
            ["Admin.NopStation.AlgoliaSearch.UpdateIndices.ResetSearchableAttributeSettings.Hint"] = "Reset searchable attribute settings of algolia index.",
            ["Admin.NopStation.AlgoliaSearch.UpdateIndices.ResetFacetedAttributeSettings"] = "Reset faceted attribute settings",
            ["Admin.NopStation.AlgoliaSearch.UpdateIndices.ResetFacetedAttributeSettings.Hint"] = "Reset faceted attribute settings of algolia index.",

            ["Enums.NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure.AlgoliaSortingEnum.CreatedOn"] = "Created on",
            ["Enums.NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure.AlgoliaSortingEnum.NameAsc"] = "Name: A to Z",
            ["Enums.NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure.AlgoliaSortingEnum.NameDesc"] = "Name: Z to A",
            ["Enums.NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure.AlgoliaSortingEnum.Position"] = "Position",
            ["Enums.NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure.AlgoliaSortingEnum.PriceAsc"] = "Price: Low to High",
            ["Enums.NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure.AlgoliaSortingEnum.PriceDesc"] = "Price: High to Low",

            ["NopStation.AlgoliaSearch.Filterings.Categories"] = "Categories",
            ["NopStation.AlgoliaSearch.Filterings.Manufacturers"] = "Manufacturers",
            ["NopStation.AlgoliaSearch.Filterings.Vendors"] = "Vendors",
            ["NopStation.AlgoliaSearch.Filterings.Ratings"] = "Ratings",
            ["NopStation.AlgoliaSearch.Filterings.Price"] = "Price",

            ["NopStation.AlgoliaSearch.Filterings.Ratings.OneStar"] = "One star",
            ["NopStation.AlgoliaSearch.Filterings.Ratings.TwoStar"] = "Two star",
            ["NopStation.AlgoliaSearch.Filterings.Ratings.ThreeStar"] = "Three star",
            ["NopStation.AlgoliaSearch.Filterings.Ratings.FourStar"] = "Four star",
            ["NopStation.AlgoliaSearch.Filterings.Ratings.FiveStar"] = "Five star",

            ["Admin.NopStation.AlgoliaSearch.UploadProduct.UpdatedAllItems"] = "Items updated successfully.",
            ["NopStation.AlgoliaSearch.EnterSearchMinimumLength"] = "Enter minimum {0} character(s).",
            ["NopStation.AlgoliaSearch.NoSearchResultFor"] = "No search result for",
            ["Admin.NopStation.AlgoliaSearch.UpdatableItem.Product.Inforamtion.Start"] = "Started uploading products to Algolia from product ID ",
            ["Admin.NopStation.AlgoliaSearch.UpdatableItem.Product.Inforamtion.Finish"] = "Finished uploading products to Algolia from product ID ",

            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.WidgetZones"] = "Widget zones",
            ["Admin.NopStation.AlgoliaSearch.Configuration.Fields.WidgetZones.Hint"] = "Widget zone(s) where the search box will appear. Separate multiple zones with a comma (e.g. 'home_page_top'). Leave empty to disable. Requires plugin reinstall or cache clear after change."
        };

        return list;
    }


    #region IWidgetPlugin

    /// <summary>
    /// Gets widget zones where this component should be rendered.
    /// Configure the "WidgetZones" setting in Admin → AlgoliaSearch → Configuration.
    /// Example values: "home_page_top", "home_page_before_categories", your custom theme zone.
    /// </summary>
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        var zones = (_algoliaSearchSettings.WidgetZones ?? "")
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(z => z.Trim())
            .Where(z => !string.IsNullOrEmpty(z))
            .ToList<string>();

        return Task.FromResult<IList<string>>(zones);
    }

    /// <summary>
    /// Returns the view component name used to render in the widget zone.
    /// </summary>
    public string GetWidgetViewComponentName(string widgetZone)
    {
        return "AlgoliaSearch";
    }

  
    public bool HideInWidgetList => false;

            public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(NopStation.Plugin.Misc.AlgoliaSearch.Components.AlgoliaSearchViewComponent);
    }
    #endregion

    #endregion
}