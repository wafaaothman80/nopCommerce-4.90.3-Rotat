using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Recommendations.SimilarProducts.Components;
using Nop.Plugin.Recommendations.SimilarProducts.ScheduleTasks;
using Nop.Plugin.Recommendations.SimilarProducts.Services;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Recommendations.SimilarProducts
{
    public class SimilarProductsPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly IFeaturesConfigurationService _configurationService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ILocalizationService _localizationService;

        #endregion Fields

        #region Properties

        public bool HideInWidgetList => false;

        #endregion Properties

        #region Ctor

        public SimilarProductsPlugin(
            IFeaturesConfigurationService configurationService,
            IScheduleTaskService scheduleTaskService,
            ILocalizationService localizationService,
            IWebHelper webHelper)
            : base()
        {
            _configurationService = configurationService;
            _scheduleTaskService = scheduleTaskService;
            _localizationService = localizationService;
            _webHelper = webHelper;
        }

        #endregion Ctor

        #region Methods

        public override async Task InstallAsync()
        {
            await InsertLocaleStringResources();
            await InsertScheduleTasks();
            await _configurationService.SaveDefaultConfigurationAsync();
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Recommendations.SimilarProducts");
            await DeleteScheduleTasks();
            await base.UninstallAsync();
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/RecommendationsSimilarProducts/Configure";
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.ProductDetailsBeforeCollateral });
        }

        /// <summary>
        /// Gets the view component type for displaying the widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component type</returns>
        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(SimilarProductsViewComponent);
        }

        public async Task ManageSiteMapAsync(AdminMenuItem rootNode)
        {
            var pluginsNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Third party plugins")
                              ?? rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "ThirdPartyPlugins")
                              ?? rootNode; // fallback

            var myRoot = pluginsNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Recommendations.SimilarProducts.Root");
            if (myRoot == null)
            {
                myRoot = new AdminMenuItem
                {
                    SystemName = "Recommendations.SimilarProducts.Root",
                    Title = await _localizationService.GetResourceAsync("Admin.Recommendations.SimilarProducts.Menu.Root"),
                    Visible = true,
                    IconClass = "fa fa-th-large"
                };
                pluginsNode.ChildNodes.Add(myRoot);
            }

            var configureNode = new AdminMenuItem
            {
                SystemName = "Recommendations.SimilarProducts.Configure",
                Title = await _localizationService.GetResourceAsync("Admin.Recommendations.SimilarProducts.Menu.Configure"),
                Visible = true,
                IconClass = "fa fa-cog",
                Url = "~/Admin/RecommendationsSimilarProducts/Configure"
            };
            myRoot.ChildNodes.Add(configureNode);
        }

        #endregion Methods

        #region Private Methods

        private async Task InsertScheduleTasks()
        {
            var scheduleTask = new Core.Domain.ScheduleTasks.ScheduleTask
            {
                Name = DiscoverSimilarProductsScheduleTask.Name,
                Type = DiscoverSimilarProductsScheduleTask.Type,
                Seconds = DiscoverSimilarProductsScheduleTask.Seconds,
                Enabled = true,
                StopOnError = false
            };
            await _scheduleTaskService.InsertTaskAsync(scheduleTask);
        }

        private async Task DeleteScheduleTasks()
        {
            var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync(DiscoverSimilarProductsScheduleTask.Type);
            if (scheduleTask != null)
                await _scheduleTaskService.DeleteTaskAsync(scheduleTask);
        }

        private async Task InsertLocaleStringResources()
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Recommendations.SimilarProducts.SimilarProducts"] = "Similar Products",
                ["Plugins.Recommendations.SimilarProducts.CheckProductProperties"] = "Check product's properties to participate in comparison",
                ["Plugins.Recommendations.SimilarProducts.NumberOfSimilarProductsToFind"] = "Number of similar products to find",
                ["Plugins.Recommendations.SimilarProducts.NumberOfSimilarProductsToDisplay"] = "Number of similar products to display",
                ["Plugins.Recommendations.SimilarProducts.MinValueOfSimilarity"] = "Minimal value of similarity that is accepted",
                ["Plugins.Recommendations.SimilarProducts.TrainModel"] = "Train Model",
                ["Admin.Recommendations.SimilarProducts.Menu.Root"] = "Similar Products",
                ["Admin.Recommendations.SimilarProducts.Menu.Manual"] = "Manual Similarities",
                ["Admin.Recommendations.SimilarProducts.Menu.Configure"] = "Configure"
            });
        }

        #endregion
    }
}