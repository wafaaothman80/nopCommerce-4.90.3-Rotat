using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Web.Services.Catalog;

namespace Nop.Web.Infrastructure;

/// <summary>
/// Re-registers services that need custom overrides.
/// Must run after all other INopStartup registrations (Order > 3000).
/// </summary>
public class CustomNopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Replace the default CategoryService with our filtered version so that the
        // MegaMenu (and any other caller) only sees categories that have products.
        services.AddScoped<ICategoryService, FilteredCategoryService>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 3001;
}
