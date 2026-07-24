# SDA Orders

## What it does

An **SDA** is the delivery document in the ERP that groups a customer's paid orders for shipment. On rotat.com this is fully automated by the scheduled task **"Create SDA for paid orders"** (System → Schedule tasks), which runs **every hour**:

- It collects every order that is **paid** and in **Processing**, and that already exists in the ERP.
- Orders are grouped **per customer** by their requested shipping date. At checkout the customer chooses either *Ship as soon as possible* or *Choose a shipping date* (a date picker) — that choice drives the grouping.
- When a group's shipping date comes **within 7 days**, the group is **locked** and an **SDA is created in the ERP** containing all its orders.
- If the customer pays **more orders while an SDA is active**, those orders are **appended to the same SDA** instead of opening a new one — shipments are combined automatically.
- If a customer accidentally ends up with more than one open group, the task **merges them into one** (within a 30-day window).
- Every attempt is recorded per order (SDA number, date, status, and the ERP's response message), so an order is never sent to the ERP twice.

## Example scenario

A customer pays order **A** on Monday and chooses shipping date next week. The task creates a shipment group for that date. On Wednesday he pays order **B** with *Ship as soon as possible* — it joins the same group. When the shipping date is 7 days away, the group locks and one **SDA** is created in the ERP with both orders.

On Thursday he pays order **C**. The SDA is still active, so **C is appended to the same SDA** — one delivery, three orders. Only when the SDA's window has passed will a future order start a fresh group and a new SDA.

## What you need to do

Normally **nothing** — the task runs every hour on its own.

### To run it immediately

1. Go to **System → Schedule tasks**.
2. Find **Create SDA for paid orders** and press **Run now** (useful after fixing an ERP issue, or when a customer is waiting).

### When an order doesn't get an SDA, check in this order

1. **Is the order Paid and in Processing?** Unpaid or pending orders are ignored by design.
2. **Does the order exist in the ERP?** An order without an ERP number is skipped (the task logs a warning). This usually means the order-to-ERP sync hasn't run for it yet.
3. **Did the ERP reject it?** Open **System → Log** and search for "SdaScheduleTask" — every ERP request and response is logged, including the ERP's rejection message per order.

## Good to know

- The 7-day rule works in both directions: an SDA is created when the ship date is within 7 days, and it stays "active" for appending for about 7 days after creation.
- Orders where the customer picked a date **more than 30 days away** wait in their group until the date approaches.
- The hourly schedule (3600 seconds) can be changed with **Edit** on the task row — coordinate with the ERP team before changing it.

<figure>
  <img src="/docs-screens/sda-schedule-task.png" alt="The schedule tasks list with Create SDA for paid orders" style="max-width:100%;border:1px solid #d2d6de;border-radius:6px;">
  <figcaption><strong>Screen:</strong> System → Schedule tasks — "Create SDA for paid orders" runs every hour; Run now executes it immediately.</figcaption>
</figure>
