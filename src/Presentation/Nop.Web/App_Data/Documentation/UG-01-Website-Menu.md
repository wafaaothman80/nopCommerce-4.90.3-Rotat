# Website Menu

## What it does

The Product menu on the website **fills itself automatically**. Any category that has at least one product for sale appears in the menu; a category with no products disappears from the menu on its own. A parent category stays visible as long as any of its subcategories still has products.

Customers never land on an empty category page.

## Example scenario

The ERP sync imports 15 new products into a category that was empty until today. Nobody touches the website — the category simply appears in the Product menu.

Months later, the last product of that category is sold out and unpublished. The category quietly disappears from the menu again.

## What you need to do

**Nothing, in daily work.** The menu takes care of itself. Only two situations need you:

### To hide a category that still has products

1. Go to **Catalog → Categories**.
2. Open the category and untick **Published**.
3. Save, clear the cache (gear icon, top-right), and refresh the website.

### To change the menu itself (rename, reorder, add a link)

1. Go to **Nop-Templates → Plugins → Mega Menu** and open the menu.
2. Drag the boxes to rearrange; use the left panel to add new links.
3. The **Product** item is set to "**All categories**" — leave it that way. Do **not** add categories one by one; the automatic behavior depends on this.
4. Save, clear the cache, refresh the website.

<figure>
  <img src="/docs-screens/menu-product-dropdown.png" alt="The Product menu on the website" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> The Product menu — only categories that contain products are listed.</figcaption>
</figure>

<figure>
  <img src="/docs-screens/megamenu-edit.png" alt="The menu editor in the admin panel" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> The menu editor (Nop-Templates → Plugins → Mega Menu) — drag the boxes to rearrange the menu.</figcaption>
</figure>
