using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Web.Services.Catalog;

/// <summary>
/// Extends CategoryService to exclude categories (and their subtrees) that have no published products.
/// This affects the public-facing mega menu and any other caller that passes showHidden=false.
/// Admin calls (showHidden=true) are unaffected.
/// </summary>
public class FilteredCategoryService : CategoryService
{
    public FilteredCategoryService(
        IAclService aclService,
        ICustomerService customerService,
        ILocalizationService localizationService,
        IRepository<Category> categoryRepository,
        IRepository<DiscountCategoryMapping> discountCategoryMappingRepository,
        IRepository<Product> productRepository,
        IRepository<ProductCategory> productCategoryRepository,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        IStoreMappingService storeMappingService,
        IWorkContext workContext)
        : base(aclService, customerService, localizationService, categoryRepository,
              discountCategoryMappingRepository, productRepository, productCategoryRepository,
              staticCacheManager, storeContext, storeMappingService, workContext)
    {
    }

    public override async Task<IList<Category>> GetAllCategoriesByParentCategoryIdAsync(
        int parentCategoryId, bool showHidden = false)
    {
        var categories = await base.GetAllCategoriesByParentCategoryIdAsync(parentCategoryId, showHidden);

        // Only filter on the public-facing side; admin needs to see all categories.
        if (showHidden)
            return categories;

        var store = await _storeContext.GetCurrentStoreAsync();
        var result = new List<Category>();

        foreach (var category in categories)
        {
            // Build the full subtree: this category + all descendants.
            var childIds = await GetChildCategoryIdsAsync(category.Id, store.Id);
            var subtreeIds = childIds.Concat(new[] { category.Id }).ToList();

            // Check whether any published, non-deleted product is mapped to any category in the subtree.
            var hasProducts = await (
                from pc in _productCategoryRepository.Table
                join p in _productRepository.Table on pc.ProductId equals p.Id
                where subtreeIds.Contains(pc.CategoryId) && p.Published && !p.Deleted && p.VisibleIndividually
                select p.Id
            ).AnyAsync();

            if (hasProducts)
                result.Add(category);
        }

        return result;
    }
}
