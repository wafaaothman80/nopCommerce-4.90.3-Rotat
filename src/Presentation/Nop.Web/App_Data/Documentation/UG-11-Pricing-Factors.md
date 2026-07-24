# Pricing Factors

## What it does

Prices on rotat.com are not one-price-for-everyone. A **custom pricing plugin built for Rotat** adjusts prices with percentage **factors** from five tables (admin sidebar → **Factors Menu**):

| Table | What the factor is attached to | Example from the live data |
|---|---|---|
| **COD Factors** | The customer's COD **country** | Albania 0.07, Åland Islands 0.03, American Samoa 0.2 |
| **Customer Type** | The customer's **business type** | AGENT −0.04, DEALER 0.02, FACTORY 0.28, SUPPLIER −0.02 |
| **Category Factors** | The product's **category** | AC Bearing 0.32, Adapter Sleeves 0.22 |
| **Brand Factors** | The product's **brand** | APZ 0, ARB 0 (currently neutral) |
| **Factors** | The internal **pricing tiers** (price roles) customers are mapped into | tiers from −0.05 up |

A factor is a fraction: **0.04 means +4%**, **−0.02 means −2%**, **0 means no change**.

How they work together:

- The **customer side** (customer type + COD country) decides which **pricing tier** a customer belongs to. This mapping happens automatically **when the admin saves the customer record** (after choosing the type and COD country during activation).
- The **product side** (category factor + brand factor) shapes each product's price level.
- The combination means: a FACTORY customer from Albania sees different prices than an AGENT from Kuwait — for the same product — without anyone maintaining separate price lists.

## Example scenario

Sales decides that AC bearings need a higher margin. You open **Category Factors**, search for the AC Bearing category, press **Edit Factor**, and raise 0.32 to 0.35. From the next catalog price refresh, every AC bearing is priced 3% higher — for all customers, with their personal tier still applied on top.

Later, a customer is reclassified from DEALER to WHOLESALER: you open the customer record, change **Customer Type**, and save — the system re-maps them to the right pricing tier immediately.

## What you need to do

### To change a factor

1. Open the right page under **Factors Menu** (COD / Category / Brand / Customer Type / Factors).
2. Use the search dropdown to find the row, press **Edit Factor**, enter the new value, save.

### To change how a specific customer is priced

Don't touch the factor tables — change **the customer**: open their record in **Customers → Customers**, set the right **Customer Type** and **COD Country**, and save. The pricing tier updates automatically.

## Good to know

- Factor changes on categories/brands affect prices at the next catalog price refresh (the ERP sync recalculates prices); the customer-side mapping refreshes whenever the customer record is saved.
- The **Factors** page (pricing tiers) is the engine's core — its values define the tiers themselves. Normally you never change these; work with the four other tables instead.

> ⚠️ **Warnings**
> - These numbers **change real selling prices**. Always agree changes with the pricing/sales team first, and double-check the decimal — 0.4 is +40%, not +4%.
> - Don't delete rows; set a factor to **0** to make it neutral.
> - After a change, verify one affected product's price on the storefront (impersonate a customer of the relevant type to see their tier).

<figure>
  <img src="/docs-screens/cod-factors.png" alt="COD factors per country" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> Factors Menu → COD Factors — one percentage per delivery country.</figcaption>
</figure>

<figure>
  <img src="/docs-screens/customer-type-factors.png" alt="Customer type factors" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> Factors Menu → Customer Type — the eight business types and their factors (AGENT −4% … FACTORY +28%).</figcaption>
</figure>

<figure>
  <img src="/docs-screens/category-factors.png" alt="Category factors" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> Factors Menu → Category Factors — a factor per product category.</figcaption>
</figure>

<figure>
  <img src="/docs-screens/brand-factors.png" alt="Brand factors" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> Factors Menu → Brand Factors — a factor per brand (currently all neutral at 0).</figcaption>
</figure>

<figure>
  <img src="/docs-screens/role-factors.png" alt="Pricing tier factors" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> Factors Menu → Factors — the internal pricing tiers customers are mapped into. Handle with care.</figcaption>
</figure>
