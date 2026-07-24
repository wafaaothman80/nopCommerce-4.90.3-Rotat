# Menu Customization — Hide Categories Without Products

> **Verified against source.** Every statement in this document was confirmed by comparing the Rotat codebase with pristine nopCommerce 4.90.3 (file-level diff). This customization consists of exactly **2 new files** and **0 modified core files**.

## Overview

The public mega menu (the "Product" dropdown rendered by the `MainMenu` view component) must show only categories that actually contain purchasable products. Categories whose entire subtree contains no published, individually-visible product are hidden from the storefront while remaining fully visible and manageable in the Admin panel.

The customization is implemented as a **decorated service**: a subclass of the stock `CategoryService` is registered in the DI container in place of the original, overriding a single query method. No core nopCommerce file was modified for this feature, which keeps it upgrade-safe.

## Business Requirement

The Rotat catalog is imported automatically from the NetSuite ERP (`HomeController.GetLatestCategories` / `GetLatestProduct` sync actions and the `InsertCategory` / `InsertProduct` stored procedures). The ERP contains many categories that have no sellable items on the web store at a given time. Showing empty categories:

- leads customers to empty category pages (bad UX, lost trust),
- inflates the mega menu with dead entries,
- would require constant manual publish/unpublish work in Admin after every ERP sync.

The business rule: **a category appears in the storefront menu only if it, or any of its descendants, contains at least one published product.**

## Files Modified

| Project | Folder | File | Status |
|---|---|---|---|
| Nop.Web | `Services/Catalog/` | `FilteredCategoryService.cs` | **New** |
| Nop.Web | `Infrastructure/` | `CustomNopStartup.cs` | **New** |

Files that participate in the feature but are **unmodified stock code** (listed because they form the execution path):

| Project | Folder | File | Role |
|---|---|---|---|
| Nop.Web | `Components/` | `MainMenuViewComponent.cs` | Renders the main menu |
| Nop.Web | `Factories/` | `MenuModelFactory.cs` | Builds the menu model; calls the overridden method |
| Nop.Services | `Catalog/` | `CategoryService.cs` | Base class being extended |
| Nop.Services | `Menus/` | `MenuService.cs` | Loads admin-defined menu items |
| Nop.Web | `Themes/Prisma/Views/Shared/Components/MainMenu/` | `Default.cshtml`, `_MenuItem.cshtml`, `_MenuItem.List.cshtml`, `_MenuItem.Grid.cshtml` | Theme rendering of the menu tree |

## Classes / Methods

### `FilteredCategoryService : CategoryService`
`src/Presentation/Nop.Web/Services/Catalog/FilteredCategoryService.cs`

| Member | Purpose |
|---|---|
| ctor | Passes all 11 dependencies straight to the base `CategoryService` — no new state is introduced. |
| `GetAllCategoriesByParentCategoryIdAsync(int parentCategoryId, bool showHidden = false)` (override) | The single point of customization. See workflow below. |

### `CustomNopStartup : INopStartup`
`src/Presentation/Nop.Web/Infrastructure/CustomNopStartup.cs`

| Member | Purpose |
|---|---|
| `ConfigureServices` | `services.AddScoped<ICategoryService, FilteredCategoryService>();` — replaces the stock registration. |
| `Order => 3001` | nopCommerce runs `INopStartup` implementations ordered ascending; core services register at Order ≤ 2000 (`NopStartup` classes), so 3001 guarantees this registration **wins** (last registration for an interface is the one resolved by default). |

### Inherited methods that matter

| Method (stock, inherited) | Role in this feature |
|---|---|
| `CategoryService.GetAllCategoriesByParentCategoryIdAsync` (base) | Supplies the pre-filtered list: published, not deleted, ACL-checked, store-mapped, sorted by `DisplayOrder`. Result is cached per parent (`NopCatalogDefaults.CategoriesByParentCategoryCacheKey`). |
| `CategoryService.GetChildCategoryIdsAsync(parentId, storeId)` | Recursively collects **all descendant** category IDs (stock recursive implementation, cached via `NopCatalogDefaults.ChildCategoryIdsCacheKey`). Used to evaluate the whole subtree. |

## Database Changes

**None.** No new tables, columns, procedures or migrations. The feature queries the existing `Product_Category_Mapping` and `Product` tables through the standard LINQ repositories (`IRepository<ProductCategory>`, `IRepository<Product>`).

## Configuration

**None.** There is no setting to toggle the behavior; it is always active for public (non-admin) callers. The DI swap in `CustomNopStartup` is the only wiring.

## Workflow

### How product existence is checked

For each candidate category the override runs one `EXISTS`-style query:

```csharp
var childIds    = await GetChildCategoryIdsAsync(category.Id, store.Id); // all descendants, recursive, cached
var subtreeIds  = childIds.Concat(new[] { category.Id }).ToList();      // subtree = self + descendants

var hasProducts = await (
    from pc in _productCategoryRepository.Table
    join p  in _productRepository.Table on pc.ProductId equals p.Id
    where subtreeIds.Contains(pc.CategoryId)
          && p.Published && !p.Deleted && p.VisibleIndividually
    select p.Id
).AnyAsync();
```

A product "counts" only when it is **Published**, **not Deleted**, and **VisibleIndividually** (i.e. grouped-product children that cannot be opened on their own do not keep a category alive).

### Step-by-step execution flow (public menu request)

```
Page request (any storefront page)
└─ _Root.cshtml → Component.InvokeAsync(MainMenuViewComponent)
   └─ MainMenuViewComponent.InvokeAsync()
      └─ IMenuModelFactory.PrepareMenuModelsAsync(MenuType.Main)
         ├─ static cache lookup: NopModelCacheDefaults.MenuByTypeModelKey
         │  (per menu type + customer roles + store + language) — HIT → done, no queries
         └─ MISS:
            ├─ IMenuService.GetAllMenusAsync(Main, storeId)
            ├─ MenuModelFactory.PrepareMenuItemModelsAsync(menu)
            │  ├─ IMenuService.GetAllMenuItemsAsync(...)          [admin-defined items]
            │  ├─ per item of type Category → PrepareSubMenuItemsAsync(...)
            │  │  └─ ICategoryService.GetAllCategoriesByParentCategoryIdAsync(entityId)
            │  │     ★ resolves to FilteredCategoryService override ★
            │  │     ├─ base call → published/ACL/store-filtered children (cached)
            │  │     ├─ showHidden == false → continue filtering
            │  │     └─ per child category:
            │  │        ├─ GetChildCategoryIdsAsync → full descendant id list (cached)
            │  │        └─ SQL EXISTS over Product_Category_Mapping ⋈ Product
            │  │           → keep category only if ≥1 published product in subtree
            │  │     (recursion: PrepareSubMenuItemsAsync calls itself for grid items,
            │  │      so every level of the tree is filtered the same way)
            │  └─ if menu.DisplayAllCategories → PrepareAllCategoriesMenuItemModelsAsync
            │     └─ same overridden method with parentCategoryId = 0 → root level filtered too
            └─ result cached in MenuByTypeModelKey
```

### Parent / child / hierarchy handling

- **Parent categories** are kept if *any descendant* has products — even if the parent itself has no directly-mapped product. This is guaranteed by evaluating the **subtree** (`self + GetChildCategoryIdsAsync`), not the single category.
- **Child categories** are filtered individually when their own level is prepared (the menu factory recurses level by level, and every level goes through the same override).
- **Hierarchy is preserved**: the override only removes entries from the flat list returned for one parent; it never re-parents or flattens. A hidden parent automatically hides its whole branch because the factory never recurses into a category that was filtered out.

### Admin isolation

The override starts with:

```csharp
if (showHidden)
    return categories;
```

Every Admin-area caller passes `showHidden: true` (e.g. category list, category tree, product mapping dialogs), so **Admin sees all categories** including empty ones. Only `showHidden == false` (public) callers get the filtered view.

### Scope — which storefront features are affected

`FilteredCategoryService` replaces `ICategoryService` globally, but only **one method** is overridden. Callers of *that method* get filtering:

| Caller | Filtered? |
|---|---|
| Main menu / mega menu (`MenuModelFactory.PrepareSubMenuItemsAsync`, `PrepareAllCategoriesMenuItemModelsAsync`) | ✅ Yes |
| Any other code path that calls `GetAllCategoriesByParentCategoryIdAsync` with `showHidden=false` (e.g. `CatalogModelFactory.PrepareSubCategoryModels` on category pages, homepage category blocks that use it) | ✅ Yes |
| Left-side category navigation (`CatalogModelFactory.PrepareCategorySimpleModelsAsync` → uses `GetAllCategoriesAsync`) | ❌ No — different method, not overridden |
| Sitemap / search / breadcrumbs (use `GetAllCategoriesAsync` / `GetCategoryBreadCrumbAsync`) | ❌ No |
| All Admin screens (`showHidden=true`) | ❌ No (by design) |

> **⚠ Update (July 2026) — actual menu renderer on the live site.** The Prisma layout (`Themes/Prisma/Views/Shared/_Root.cshtml`, lines ~133–150) renders the header menu in two steps: it first invokes widget zone **`theme_header_menu`**; if that returns HTML it is used, otherwise it falls back to `MainMenuViewComponent`. On the live site the **SevenSpikes (Nop-Templates) Mega Menu plugin** (`Plugins/SevenSpikes.Nop.Plugins.MegaMenu`, compiled only — no source in repo) fills that widget zone, so the *plugin*, not the core menu pipeline, produces the visible menu. Menu content (Home / Product / News / Brands / Contact Us and the category boxes under Product) is edited in admin at `/Admin/MegaMenuAdmin/MenuEdit/1` (see the User Guide, *Edit the Website Menu*). The menu's **Product** item is configured in the plugin as an **"All categories"** item (nothing listed manually), and the storefront behavior — only categories with products appear — is confirmed on the live site by the site owner. The plugin resolves categories through `ICategoryService`, which DI resolves to `FilteredCategoryService`, so the filter applies to the plugin's category listings in practice.

## Frontend Changes

None. Views, JavaScript and CSS of the menu are the standard Prisma-theme menu files; they simply receive a smaller `MenuModel`. (The Prisma `MainMenu/Default.cshtml` adds mobile plus-button/back-button behavior, but that is theme styling, not part of this feature.)

## Backend Changes

- **Service**: `FilteredCategoryService` (new, subclass of `CategoryService`).
- **Dependency Injection**: `CustomNopStartup` (Order 3001) re-registers `ICategoryService` → `FilteredCategoryService` (scoped).
- No controllers, events, repositories or plugins involved.

## Admin Changes

No admin *code* changes belong to this customization; admin category screens are deliberately untouched via the `showHidden` guard.

Operationally, the storefront menu **content** is managed in admin through the Mega Menu plugin: **Nop-Templates → Plugins → Mega Menu** (`/Admin/MegaMenuAdmin/MenuEdit/1`) — items can be added from Pages / Categories / Manufacturers / Vendors / Topics / Product Tags / Custom Links and rearranged by drag & drop. Administrator instructions live in the User Guide (*UG-20 — Edit the Website Menu*).

## Website Changes

Before vs. after:

| | Before (stock) | After (Rotat) |
|---|---|---|
| Category with 0 products in its whole subtree | Shown in menu; leads to an empty category page | Hidden from menu |
| Parent with empty direct mapping but products in a grandchild | Shown | Shown (subtree check) |
| Category with only unpublished / deleted / not-individually-visible products | Shown | Hidden |
| Admin category tree & lists | All categories | All categories (unchanged) |

## Screens

<figure>
  <img src="/docs-screens/homepage-header.png" alt="rotat.com homepage header with the main menu" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Caption:</strong> The storefront header on rotat.com. The main menu (Home · Product · News · Brands · Contact Us) is rendered by <code>MainMenuViewComponent</code>, whose category items come from the filtered category service documented on this page.</figcaption>
</figure>

<figure>
  <img src="/docs-screens/menu-product-dropdown.png" alt="Product menu expanded, showing only categories that contain products" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Caption:</strong> The <em>Product</em> menu expanded. Only categories whose subtree contains at least one published product appear (Agriculture parts, Wheel Speed Sensors, Mechanical seals, Automotive Bearings, Industrial Bearings), and the flyout shows a child category with products (RASP BAR under Agriculture parts). Categories with no products stay hidden even when they are published in the admin panel.</figcaption>
</figure>

## Performance Considerations

- **Query cost**: the filter runs 1 `EXISTS` SQL query **per candidate category per level** (N+1 pattern). For a catalog with ~5 root categories and a few dozen children this is negligible; for very deep/wide trees it grows linearly with category count.
- **Mitigations already in place**:
  - The **final menu model is cached** (`MenuByTypeModelKey`, per customer-role set + store + language), so the EXISTS queries run only on cache miss, not per request.
  - `base.GetAllCategoriesByParentCategoryIdAsync` results are cached per parent.
  - `GetChildCategoryIdsAsync` descendant lists are cached.
- **Not cached**: the boolean outcome of the product-existence check itself. On every menu-model rebuild all EXISTS queries re-run.

## Risks

1. **Stale menu after product changes.** The menu model cache is invalidated by category/menu entity events, but publishing or unpublishing a *product* does not clear `MenuByTypeModelKey`. A category whose last product was just unpublished may keep appearing in the menu until the cache entry expires (default short-term cache time) or is otherwise evicted. Conversely, a newly stocked category can take equally long to appear.
2. **Inconsistent surfaces.** Because only `GetAllCategoriesByParentCategoryIdAsync` is overridden, other category surfaces (left navigation built from `GetAllCategoriesAsync`, sitemap, search filters) can still show empty categories. If a customer reaches an empty category via URL or sitemap, the page renders with no products.
3. **DI replacement fragility.** Any plugin that also re-registers `ICategoryService` with an `INopStartup.Order` above 3001 would silently disable this feature.
4. **Per-role cache multiplication.** Filtering happens after the role-aware base call; combined with the role-keyed model cache this is correct, but it means the EXISTS workload repeats for every distinct customer-role/store/language combination.

## Future Improvements

- Cache the per-category "has products" boolean with its own cache key, invalidated by `ProductCategory`/`Product` cache event consumers — removes the N+1 on every rebuild and fixes staleness in one step.
- Replace per-category EXISTS with **one grouped query** per level (`GROUP BY CategoryId` over the subtree id set) to cut round-trips.
- Extend the same rule to `GetAllCategoriesAsync` (left menu, sitemap) for consistency, or intentionally document that those surfaces show all categories.
- Consider a `CatalogSettings`-style toggle so the behavior can be switched off without a deployment.
