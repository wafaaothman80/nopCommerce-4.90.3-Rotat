# Quote Cart Customizations — Algolia Add-to-Quote Search & Convert-to-Order

> **Verified against source.** Based on the deployed plugin views and the custom controller in this repository. The base plugin (`NopStation.Plugin.Misc.QuoteCart`) ships as a compiled DLL (`src/RefAssemblies/`, deployed under `Plugins/`); its stock behavior is not re-documented here — only Rotat's changes.

## Overview

Rotat uses the **Nop Station Quote Cart** plugin for its "Request a quote" flow and adds two customizations:

1. **Instant product search on the Quote Cart page** — an Algolia-powered autocomplete ("Add Products", min. 3 characters) that adds the chosen product to the quote cart via AJAX.
2. **Customer-side partial Convert-to-Order** — the customer selects items on the public quote-request details page and converts them into a real nopCommerce order at the quoted prices; per-item conversion state is tracked in the database and the request auto-completes when all items are converted.

## Business Requirement

- B2B buyers work from part-number lists; adding items to a quote must not require browsing the catalog page by page.
- Sales negotiates prices on the quote; once agreed, the buyer should be able to turn the quote (or part of it) into an order himself, at the agreed prices, without re-adding products to a cart.

## Files Modified

| Project / location | File | Status | Role |
|---|---|---|---|
| Nop.Web | `Controllers/CustomQuoteRequestController.cs` | **New** | `ConvertToOrder` (POST), `GetQuoteRequestItemsWithOrderId` (GET, debug) |
| Plugin (deployed) | `Plugins/NopStation.Plugin.Misc.QuoteCart/Views/QuoteCart/Cart.cshtml` | **Modified** | Algolia autocomplete + AJAX add-to-quote (lines ~890–1100) |
| Plugin (deployed) | `Plugins/NopStation.Plugin.Misc.QuoteCart/Views/QuoteRequest/Details.cshtml` | **Modified** | Item checkboxes, Select/Deselect All, hidden `SelectedQuoteRequestItemIds`, form posting to `/CustomQuoteRequest/ConvertToOrder` (lines ~660–760) |
| RefAssemblies | `NopStation.Plugin.Misc.QuoteCart.dll` | Reference | Compile-time reference for the custom controller (see RefAssemblies build setup) |

Backup copies (`Cart---.cshtml`, `Cart25-3.cshtml`, `Details24-2.cshtml`, …) are earlier revisions kept in the same folders.

## Key implementation points (`CustomQuoteRequestController.ConvertToOrder`)

1. Reads `Id` (quote request) and `SelectedQuoteRequestItemIds` (comma-separated item ids filled by view JS) from the form; redirects back to the referer (`/en/quoterequestdetails/{guid}`).
2. Loads the request via the plugin's `IQuoteRequestService`; filters its items to the selected ids.
3. **Creates an `Order` directly** (no checkout pipeline): `OrderStatus=Pending`, `PaymentStatus=Pending`, `ShippingStatus=NotYetShipped`, currency hardcoded `AED`, rate 1, tax display *excluding tax*. Inserts with a GUID placeholder `CustomOrderNumber`, then regenerates it with `ICustomNumberFormatter` after the Id exists.
4. Each selected quote item becomes an `OrderItem` with **`UnitPrice = quoteItem.DiscountedPrice`** (the negotiated price); subtotal/total = sum of items; shipping/tax/discount all 0. Adds an internal order note `Order created from quote request #N`.
5. Per-item tracking via stored procedures *(DB only)*:
   - `UpdateQuoteRequestItemsWithOrderId (@SelectedQuoteItems, @NopOrderId)` — stamps the created order id on the converted items;
   - `GetQuoteRequestItemsWithOrderId (@QuoteRequestId)` — returns `(Id, NopOrderId)` per item; if **all** items have an order id, the controller sets `QuoteRequest.RequestStatus = Complete` and `NopOrderId = order.Id`.
6. View JS keeps already-converted items unselectable and hides the convert form when nothing is left.

## Algolia add-to-quote search (`Cart.cshtml`)

- Loads `algoliasearch` v3 + `autocomplete.js` from jsDelivr CDN; client `algoliasearch('XOR92A46NK', '<search key>')`, index **`Products`**.
- Suggestions render image, name, SKU, manufacturer; selecting one calls the plugin's add-to-quote-cart AJAX endpoint and refreshes the cart grid.
- The admin-side flag **Allow Convert Order** (plugin request edit page) plus request status gate whether the convert UI is rendered for the customer.

## Database Changes

- Stored procedures `UpdateQuoteRequestItemsWithOrderId`, `GetQuoteRequestItemsWithOrderId` *(DB only — not in repo)*.
- A `NopOrderId` column on the plugin's quote-request-item table backing the per-item tracking *(DB only; the plugin's own `QuoteRequest.NopOrderId` is stock plugin schema)*.

## Risks

1. **🔴 No ownership/authorization check in `ConvertToOrder`** — it is a public POST taking an integer `Id`; any visitor who can post a valid id (ids are sequential) can convert someone's quote into an order on that customer's account. Should verify the current customer owns the request (or at least require the request GUID).
2. **🟠 Algolia credentials in the view** — app id + API key are embedded client-side. This is acceptable **only** if the key is a search-only key; verify it has no write ACLs.
3. **🟠 Orders bypass the checkout pipeline** — no payment method, shipping totals, tax, discounts, gift cards, or inventory adjustments; downstream processes that assume checkout-created orders (payment capture, shipment rates) must treat these as manual orders. Currency is hardcoded AED.
4. 🟡 `Referer`-based redirect after POST (open-redirect-ish; low impact since it only redirects the responder).
5. 🟡 Debug GET endpoint `GetQuoteRequestItemsWithOrderId` is publicly reachable and enumerates item/order ids.

## Future Improvements

- Add an ownership check (`quoteRequest.CustomerId == currentCustomer.Id`) and anti-forgery validation on `ConvertToOrder`.
- Move Algolia keys to settings; confirm search-only key.
- Consider calling the plugin's own order-conversion service (if present) or `IOrderProcessingService` to get taxes/shipping/inventory handled.
