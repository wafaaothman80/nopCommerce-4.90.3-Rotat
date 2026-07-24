# Customer Registration

## What it does

New customers register with their **mobile phone number**, verified by an SMS code:

1. The customer enters their mobile number.
2. They receive a **4-digit code by SMS** and must type it within **3 minutes**.
3. Only after the correct code do they reach the registration form — and their **phone number becomes their login username**.

One phone number can only ever register once. Customers log in with **phone number + password**.

## Example scenario

A buyer opens Register and enters his mobile number. He gets an SMS with code 4831, types the four digits, and the registration form opens with his phone number already fixed as the username. He completes his details, and from then on logs in with phone + password.

## What you need to do

### Review every new registration

1. Open **Customers → Customers** and open the new customer.
2. Choose their **customer type** and **COD country**.
3. Assign an **account manager**.
4. Create or link the customer in the **ERP**, then save.

### When a customer says the SMS never arrives

1. Ask them to press **Resend** (available after the countdown ends), or re-enter their number.
2. If several customers report it at once, the problem is at the SMS provider (Dexatel) — check the account balance and delivery status there.

## Settings

The SMS service (Dexatel) has two settings. Go to **Configuration → Settings → All settings (advanced)** and search for:

| Setting | What it is |
|---|---|
| dexatelsettings.apikey | The key of your Dexatel account. If Dexatel issues a new key, paste it here. |
| dexatelsettings.templateid | The SMS message template to use, from the Dexatel dashboard. |

Changes take effect immediately — no restart needed.

> ⚠️ **Warning:** if either setting is empty, **no SMS is sent at all** and nobody can register.

<figure>
  <img src="/docs-screens/otp-phone-entry.png" alt="The phone number step of registration" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> Registration step 1 — the customer enters a mobile number and receives the SMS code.</figcaption>
</figure>
