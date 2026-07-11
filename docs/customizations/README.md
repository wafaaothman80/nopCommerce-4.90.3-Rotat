# Rotat Customizations — Technical Documentation

Documentation of all customizations made to the nopCommerce 4.90.3 platform for rotat.com.
The customization inventory below was produced by a **file-level diff against pristine nopCommerce 4.90.3** (official release source), so it is exhaustive for `Nop.Core`, `Nop.Data`, `Nop.Services`, `Nop.Web.Framework` and `Nop.Web` (excluding third-party plugins and the Prisma theme's purely visual overrides).

## Documented features

| # | Feature | Document |
|---|---|---|
| 1 | Main menu — hide categories without products | [01-menu-hide-empty-categories.md](01-menu-hide-empty-categories.md) |
| 2 | Registration — phone sign-up with Dexatel OTP (incl. admin ERP onboarding) | [02-registration-dexatel-otp.md](02-registration-dexatel-otp.md) |

## Customization inventory (pending documentation)

Identified via diff; each item below is a candidate for its own chapter:

| Area | Evidence (files) | Summary |
|---|---|---|
| NetSuite ERP catalog sync | `HomeController` (`GetLatestBrands/GetLatestCategories/GetLatestProduct`), `NetSuiteApiConfig.cs`, `SdaScheduleTask.cs`, procs `InsertBrand/InsertCategory/InsertProduct/InsertSimilarProducts` | Pulls brands, categories, products (incl. dimensions, substitutes, stock) from NetSuite RESTlet into nop tables via stored procedures. |
| Credit & invoices (customer wallet) | `HomeController.CreditAndInvoices`, `SendCreditRequest`, `Views/Home/CreditAndInvoices.cshtml`, tables `NS_Wallet`, `NS_Wallet_ActivityHistory`, `NS_CreditRequests` | Customer credit dashboard fed from ERP (`objType=89`), wallet sync, credit-request emails. |
| Dimension search | Route `DimensionSearch`, `DimensionSearchController` (plugin/controller), homepage "Search by Dimensions" UI | Search bearings by inner/outer diameter and thickness (range or exact). |
| Quote requests | `CustomQuoteRequestController.cs` (new), QuoteCart/Actions RefAssemblies | "Request a quote" flow (header button, quote list badge). |
| Catalog filters: in-stock / discount | `IProductService`/`ProductService` (`inStockOnly`, `discountOnly` params), `CatalogProductsCommand`, `CatalogModelFactory`, `_CatalogSelectors.cshtml`, `CatalogController` | Extra storefront product-list filters. |
| Account managers module | `Account_Manager`, `AccountManager_CustomerMapping`, `CountryRigionMapping`, `AccountManagerRigionMapping` tables; admin `CustomerController`/`CustomerModelFactory`/`CustomerService`; role 6 restrictions | AM assignment, primary AM, ERP sales-rep sync, and admin data-visibility scoping. |
| Customer pricing factors (COD / customer type) | `CustomerType.cs`, `CODFactors.cs`, `Nop.Plugin.Factors` plugin, proc `UpdateFactorRoleMappingsToCustomer` | Role/factor-based pricing driven by customer type and COD country. |
| Order flow customizations | `OrderService`, `ShipmentService`, `ShippingService`, `CheckoutController`, `ShoppingCartController`, `OrderPlacedEventConsumer .cs`, `shipping-datepicker.js`, admin `OrderController`/`OrderSearchModel` | Order placement side-effects, shipping date selection, admin order list changes. *(Needs analysis.)* |
| Filter levels (`FilterLevelValue`) | `FilterLevelValueSearch`/`CompatibleWithFilterLevelValues` components + factories, `SearchByFilterLevelValues.cshtml` | Vehicle/machine compatibility filtering. *(Part stock 4.90 feature, part theme customization — verify.)* |
| Cloudflare image import | `/cfimport/process-folder` (plugin), PowerShell `process-folder-with-map.ps1` | Bulk image import to Cloudflare CDN (see team memory/README). |
| Custom plugins | `Nop.Plugin.Factors`, `Nop.Plugin.Recommendations.SimilarProducts`, `Nop.Plugin.Misc.Brevo` (modified), QuoteCart/Actions (RefAssemblies) | Non-stock plugins present in `src/Plugins`. |
| Admin menu | `Nop.Web.Framework/Menu/AdminMenu.cs` (modified) | Custom admin menu entries. |
| Migrations | `Nop.Data/Migrations/MigrationManager.cs` (modified) | Behavior change in migration handling. *(Needs analysis.)* |
| Prisma theme | `Themes/Prisma/**` (entire theme is custom) | Rotat storefront design: header, homepage, product box, checkout views, RTL/Arabic strings. |

## Conventions used in these documents

- File paths are repo-relative; "stock" means identical to vanilla 4.90.3 (ignoring whitespace/line endings).
- Stored procedures marked *(DB only)* exist in SQL Server but not in this repository.
- Risk levels: 🔴 critical, 🟠 high, 🟡 moderate.
