using Microsoft.AspNetCore.Mvc.Razor;

namespace Nop.Plugin.Shipping.ShipmentMergeDiscount.Infrastructure;

/// <summary>
/// Inserts the plugin's Views directory at the front of the Razor view search path.
/// This allows the plugin to override any core or theme view by placing a file at the
/// same relative path inside ~/Plugins/Shipping.ShipmentMergeDiscount/Views/.
///
/// Registered in <see cref="NopStartup.ConfigureServices"/> via
/// <c>services.Configure&lt;RazorViewEngineOptions&gt;</c>.
/// </summary>
public class PluginViewLocationExpander : IViewLocationExpander
{
    private const string PluginViewsRoot =
        "/Plugins/Shipping.ShipmentMergeDiscount/Views/";

    public void PopulateValues(ViewLocationExpanderContext context) { }

    public IEnumerable<string> ExpandViewLocations(
        ViewLocationExpanderContext context,
        IEnumerable<string> viewLocations)
    {
        // Prepend plugin-specific paths so they take precedence over core and theme views.
        var pluginLocations = new[]
        {
            PluginViewsRoot + "{1}/{0}.cshtml",
            PluginViewsRoot + "Shared/{0}.cshtml",
        };

        return pluginLocations.Concat(viewLocations);
    }
}
