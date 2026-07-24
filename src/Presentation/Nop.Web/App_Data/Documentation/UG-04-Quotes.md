# Quotes

## What it does

Customers collect products into a **quote cart** (separate from the shopping cart) and send it to you as a **quote request**. You review it, agree prices and terms, and — when you allow it — the customer **converts the quote into a real order himself**, at the agreed prices. No payment is taken online.

## Example scenario

A workshop owner has a list of part numbers to price. On the Quote Cart page he types each part number into the **Add Products** box — after three characters, matching products appear and one click adds them to his quote. He submits the quote.

Your team prices it and ticks *Allow Convert Order*. He opens his quote, ticks the items he wants now, and presses **Convert to Order**. An order is created at the quoted prices; the items he didn't tick stay on the quote for later.

## What you need to do

### Handling a quote request, step by step

1. Open **Nop Station → Quote cart → Quote requests**. New requests arrive as **Pending**.
2. Open a request and set its status to **Processing** while you work on it.
3. Agree prices with the customer — you can message each other in the **Conversation** box on the request (the customer sees it on his side).
4. Enter the commercial terms on the request: **payment method** and any fee, **shipping company**, **shipping option** and **shipping cost**.
5. When the price is final, tick **Allow Convert Order** and save. Only from this moment can the customer convert.
6. When the customer converts, a normal order appears under **Sales → Orders** as *Pending*, at the quoted prices. Process it like any other order.

When every item of a quote has been converted, the quote closes itself as **Complete**.

### Useful extras

- **Share Link** on a request copies a public link to the quote — handy for e-mail or WhatsApp.
- You can search requests by date, status, customer e-mail, or jump directly to a request number.

> ⚠️ **Important:** only tick *Allow Convert Order* after the prices on the quote are final — the customer orders at exactly those prices.

<figure>
  <img src="/docs-screens/quotecart-search.png" alt="The quote cart with the product search box" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> The customer's quote cart — typing a part number adds products directly to the quote.</figcaption>
</figure>

<figure>
  <img src="/docs-screens/quote-requests-admin.png" alt="The quote requests list in admin" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> Nop Station → Quote cart → Quote requests — all customer quotes with their statuses.</figcaption>
</figure>

<figure>
  <img src="/docs-screens/quote-request-edit-admin.png" alt="Editing a quote request in admin" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> Inside a quote request — status, terms, and the Allow Convert Order switch.</figcaption>
</figure>

<figure>
  <img src="/docs-screens/quote-request-details.png" alt="The customer's view of a quote with Convert to Order" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> What the customer sees — item tick-boxes, the Convert to Order button, and the conversation with your team.</figcaption>
</figure>
