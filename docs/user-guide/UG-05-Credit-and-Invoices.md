# Credit and Invoices

## What it does

Every business customer has a **My Invoices & Credit** page inside their account showing:

- their **credit limit**, **available credit** and credit **status**;
- their **payment terms**, **outstanding balance**, **overdue balance** and **days overdue**;
- their invoices in two groups: **Open** (unpaid) and **Completed** (paid).

All figures come **live from the ERP** each time the page opens — the website never invents them.

Customers without credit see an **Apply for Credit** button; customers with credit see **Request Additional Credit**. Both send a request to the sales team.

## Example scenario

A customer with a 150,000 AED credit limit opens the page: it shows how much is used, terms of *1% 10 Net 30*, and that 109,721.98 AED is overdue by 106 days. He presses **Request Additional Credit** and submits an amount and a reason.

Sales receives the request by e-mail, checks the customer externally, and decides in the ERP. The next time he opens the page, the new limit shows.

## What you need to do

### Credit requests

They arrive **by e-mail to the sales team**. Approval happens outside the website (in the ERP). Nothing changes on the customer's account until the ERP is updated.

### To see the page as a specific customer sees it

1. Open the customer in **Customers → Customers**.
2. Use **Impersonate**.
3. On the website, open the account menu → **My Invoices** (or Wallet Details).
4. Finish the impersonated session when done (link at the top of the page).

### If a customer's figures look wrong

The cause is almost always the **ERP data** or the customer's **ERP link** on their admin record — not the website page. Check both before anything else.

<figure>
  <img src="/docs-screens/credit-dashboard.png" alt="The credit dashboard of a customer" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> A customer's credit dashboard — limit, available credit, terms, balances and overdue days, live from the ERP.</figcaption>
</figure>

<figure>
  <img src="/docs-screens/credit-invoices-open.png" alt="The customer's invoice list" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> The invoice list — open invoices with red Unpaid marks, and the Apply for Credit button.</figcaption>
</figure>
