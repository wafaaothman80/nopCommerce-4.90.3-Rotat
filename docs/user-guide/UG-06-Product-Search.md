# Product Search

## What it does

The homepage search box has two modes:

| Mode | How the customer uses it |
|---|---|
| **Search by item** | Types a part number — instant suggestions appear as they type. |
| **Search by Dimensions** | Enters inner diameter, outer diameter and thickness in millimeters — each as a range (from–to) or an exact value. |

Dimension results show exact matches **and verified equivalent parts** (substitutes from other brands that fit the same dimensions).

## Example scenario

A mechanic has an unmarked bearing. He measures it: inner 30 mm, outer 62 mm, thickness 16 mm. He enters the three values and searches — the site lists the matching part plus verified equivalents from other brands.

## What you need to do

**Nothing** — the dimensions and the equivalence links come from the ERP together with the product data.

If a product does not appear in dimension search, its measurements are missing in the ERP record: fix them **in the ERP** and they arrive with the next sync.

<figure>
  <img src="/docs-screens/dimension-search.png" alt="The dimension search on the homepage" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> Search by Dimensions — inner Ø, outer Ø and thickness, each as a range or exact value.</figcaption>
</figure>
