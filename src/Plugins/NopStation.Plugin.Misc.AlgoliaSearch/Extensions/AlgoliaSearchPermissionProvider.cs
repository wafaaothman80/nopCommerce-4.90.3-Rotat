using Nop.Core.Domain.Customers;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Extensions;

public class AlgoliaSearchPermissionProvider
{
    public const string MANAGE_CONFIGURATION = "ManageAlgoliaConfiguration";
    public const string MANAGE_UPLOAD_PRODUCTS = "ManageAlgoliaUploadProducts";
}

public class AlgoliaSearchPermissionConfigManager : IPermissionConfigManager
{
    public IList<PermissionConfig> AllConfigs => new List<PermissionConfig>
    {
        new("Algolia search. Configuration", AlgoliaSearchPermissionProvider.MANAGE_CONFIGURATION, "NopStation", NopCustomerDefaults.AdministratorsRoleName),
        new("Algolia search. Manage Upload Products", AlgoliaSearchPermissionProvider.MANAGE_UPLOAD_PRODUCTS, "NopStation", NopCustomerDefaults.AdministratorsRoleName)
    };
}
