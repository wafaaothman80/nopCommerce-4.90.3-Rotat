# Card Payments

## What it does

Online card payment on rotat.com runs through a **custom plugin for the Mastercard Payment Gateway** (the bank's hosted payment page).

How a card payment works:

1. At checkout the customer selects card payment and confirms the order.
2. The site opens a **secure payment page hosted by Mastercard** — the customer types the card details **there**, never on rotat.com. (No card numbers are ever stored on the website.)
3. After a successful payment the customer returns to the site and the order is automatically marked **Paid**.
4. If the payment fails or the customer cancels, the order stays **Pending** — no money is taken, and the customer can try again or choose another payment method.

Once the order is Paid, it enters the normal flow — including the automatic **SDA grouping** (see the *SDA Orders* section).

## Example scenario

A customer confirms an order of 1,200 AED and chooses card payment. The Mastercard page opens, he enters his card and confirms. The bank approves, he lands back on rotat.com with the order confirmation, and in admin the order already shows **Paid**. Later that hour, the SDA task picks the order up for delivery grouping.

## What you need to do

### Settings

**Configuration → Payment methods → MastercardGateway → Configure**:

| Setting | Meaning |
|---|---|
| **Use Sandbox** | Test mode. Keep ticked only while testing with the bank's TEST merchant account — test cards work, no real money moves. |
| **Merchant Id** | Your merchant number at the gateway. Test accounts start with "TE…/TEST…"; the live one comes from the bank. |
| **Api Password** | The API password issued by the bank for this merchant id. |

Press **Save** after changes — they apply immediately.

> ⚠️ **Warnings**
> - **Going live** means: untick *Use Sandbox* AND replace both the Merchant Id and Api Password with the production values from the bank. Doing only one of the two breaks card payment for every customer.
> - If the bank rotates (changes) the API password, update it here the same day — with a wrong password customers cannot pay by card at all.
> - After any change, place a small test order to confirm the payment page opens and the order comes back as Paid.

### Refunds

The plugin does **not** process refunds from the admin panel. Refund the customer through the **bank's gateway portal**, then record it on the order in admin (order → refund offline) so the website's numbers match the bank.

### If customers report card payment problems

1. Try the checkout yourself — does the Mastercard page open? If not, the credentials/settings above are the first suspect.
2. Check **System → Log** for gateway errors around the time of the attempt.
3. If the page opens but payments are declined, the issue is on the bank/card side — the gateway's decline reason appears in the log entry.

<figure>
  <img src="/docs-screens/mastercard-settings.png" alt="The Mastercard gateway configuration page" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> Configuration → Payment methods → MastercardGateway → Configure — sandbox switch, merchant id and API password.</figcaption>
</figure>
