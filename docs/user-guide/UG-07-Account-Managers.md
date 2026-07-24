# Account Managers

## What it does

Every business customer at Rotat has a personal **account manager** (contact person from the sales team). This is a plugin built from scratch for Rotat. It manages:

- the **list of account managers** — kept in sync with the ERP;
- **regions** — groups of countries (e.g., *Africa = Egypt, Germany*);
- the **assignment** of account managers to customers — manually by the admin, or automatically by the customer's country;
- what the customer sees: after logging in, the homepage greets them and shows **"Your contact person"** with their account manager's name.

An account manager can cover **many regions**, and a region can have **many account managers**.

## Example scenario

A new customer from Egypt registers and is activated. Egypt belongs to the *Africa* region, and two account managers cover Africa — so the customer is **automatically** linked to them. Later the sales lead decides SANJAR should personally handle this customer: he opens the customer in admin, adds SANJAR to the account managers and marks him as **primary** (★). From then on, when the customer logs in to rotat.com, the homepage shows *"Your contact person: SANJAR"*.

## What you need to do

### Keep the account manager list up to date

1. Go to **Our Plugins → Account Managers**.
2. Press **Sync from ERP** — the list updates from the ERP system (new sales reps appear, data refreshes).
3. You can also **Add new** manually, or **Edit** one (name, e-mail, phone, regions, active).
4. Use the search boxes (e-mail, name, regions) to find a manager quickly.

### Manage regions

1. Go to **Our Plugins → Rigions**.
2. **Add new**: give the region a name, pick its **countries** from the list (multi-select), set the display order, and tick Active.
3. A country should normally belong to one region — that keeps automatic assignment predictable.

### Assign account managers to a customer

**Automatically:** when a customer registers and their country matches a country inside a region, the account managers of that region are linked to the customer — no action needed.

**Manually (or to adjust):**
1. Open the customer in **Customers → Customers**.
2. In the **selected account managers** box, add or remove managers (you can pick several).
3. Choose the **Primary Account Manager** — the ★ in the list marks the primary one; this is the name the customer sees as their contact person.
4. Save.

## Good to know

- Staff members who log in to the admin panel with the **Account Manager role** can see **only the customers assigned to them** — a manager never sees another manager's customers. Full admins see everyone.
- The customer's homepage (after login) also shows their recent orders and current daily order value next to the contact person card.

<figure>
  <img src="/docs-screens/account-managers-list.png" alt="The account managers list with Sync from ERP" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> Our Plugins → Account Managers — the list with search and the Sync from ERP button.</figcaption>
</figure>

<figure>
  <img src="/docs-screens/regions-list.png" alt="The regions list" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> Our Plugins → Rigions — regions with their countries (e.g., Africa = Egypt, Germany).</figcaption>
</figure>

<figure>
  <img src="/docs-screens/region-edit.png" alt="Adding a region with its countries" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> Adding a region — name, the countries it contains, display order and Active.</figcaption>
</figure>

<figure>
  <img src="/docs-screens/customer-account-managers.png" alt="Assigning account managers on the customer page" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> The customer record — several account managers assigned; the ★ marks the primary one the customer sees.</figcaption>
</figure>

<figure>
  <img src="/docs-screens/contact-person-homepage.png" alt="The customer's homepage with their contact person" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> What the customer sees after login — a greeting, "Your contact person" with the primary account manager, and recent orders.</figcaption>
</figure>
