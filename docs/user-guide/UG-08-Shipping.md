# Shipping

## What it does

Shipping prices on rotat.com come from **two shipping providers**, both active (Configuration → Shipping → Shipping providers):

| Provider | Used for |
|---|---|
| **Manual (Fixed or By Weight and By Total)** | Land Shipping to GCC countries — rates you define yourself per country (customized for Rotat). |
| **DHL** | Express / worldwide shipping — rates come live from DHL, limited to the DHL services that fit the business. |

On top of both, a Rotat customization adds a **weight overhead factor**: the order's shipping weight is automatically increased by a percentage (currently **+25%**) before rates are calculated — this covers packaging and handling weight, so quoted shipping prices are realistic.

## Example scenario

A customer in Kuwait checks out with an order weighing 20 kg. The system first raises the calculated weight by 25% (to 25 kg), then offers: **Land Shipping** at the Kuwait rate you defined (250 AED fixed + 6% of the order subtotal), and the active **DHL** services with live DHL prices for 25 kg. The customer picks one, and that price becomes the order's shipping cost.

## What you need to do

### Land Shipping rates (Manual provider)

1. Go to **Configuration → Shipping → Shipping providers** and press **Configure** on *Manual (Fixed or By Weight and By Total)*.
2. The table lists one rate row per country (Bahrain, Kuwait, Oman, …) for the *Land Shipping* method. The formula at the top shows how the price is built: **fixed cost + weight-based part + percentage of the order subtotal**.
3. To add a rate: press **Add record** and fill the popup — country, shipping method, weight and subtotal ranges, **Additional fixed cost**, **Rate per weight unit**, **Charge percentage (of subtotal)**, and **Transit days**. Save.
4. To change a country's price: **Edit** its row (for example, the current GCC rows use 250 AED fixed + 6% of subtotal).

### DHL

- **Services** (**Nop Station → DHL shipping → Services**): the list of DHL products (Express Domestic, Economy Select, Express Worldwide…). Only the ones marked **Active** are offered to customers — keep active only the services compatible with the business; deactivate the rest (e.g., Medical Express is off).
- **Settings** (**Nop Station → DHL shipping → Configuration**): the DHL account connection — URL, site id, password, account number — plus the company/pickup details (company name, address, country AE, city Dubai). Change these only when DHL issues new credentials or the pickup address changes.

### The weight overhead factor (Rotat customization)

- To view or change it: **Configuration → Settings → All settings (advanced)**, search for **shippingsettings.weightoverheadfactor**.
- The value is a fraction: **0.25 means +25%** added to every order's shipping weight. Set 0.3 for +30%, or 0 to disable.
- The change applies immediately to **both** providers (Land Shipping weight ranges and DHL quotes).

> ⚠️ **Warning:** this factor affects every shipping price on the site. Change it only after agreeing with the business, and test a checkout afterwards.

<figure>
  <img src="/docs-screens/shipping-providers.png" alt="The two active shipping providers" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> Configuration → Shipping → Shipping providers — DHL and the Manual (Fixed or By Weight and By Total) provider, both active.</figcaption>
</figure>

<figure>
  <img src="/docs-screens/shipping-rates-manual.png" alt="Land shipping rates per country" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> The Manual provider's rate table — one Land Shipping row per GCC country (250 AED fixed + 6% of subtotal), with the price formula shown above the table.</figcaption>
</figure>

<figure>
  <img src="/docs-screens/shipping-rate-add.png" alt="Adding a shipping rate" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> Adding a rate — country, method, weight/subtotal ranges, fixed cost, rate per weight unit, charge percentage and transit days.</figcaption>
</figure>

<figure>
  <img src="/docs-screens/dhl-services.png" alt="The DHL services list" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> DHL Services — only the services marked Active are offered to customers.</figcaption>
</figure>

<figure>
  <img src="/docs-screens/dhl-settings.png" alt="The DHL connection settings" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> DHL settings — account connection and company/pickup details.</figcaption>
</figure>

<figure>
  <img src="/docs-screens/shipping-weight-factor.png" alt="The weight overhead factor setting" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> All settings (advanced) — shippingsettings.weightoverheadfactor, currently 0.25 (+25% shipping weight).</figcaption>
</figure>
