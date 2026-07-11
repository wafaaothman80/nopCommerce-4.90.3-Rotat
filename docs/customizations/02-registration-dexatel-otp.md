# Registration Customization — Phone-Based Sign-Up with Dexatel OTP

> **Verified against source.** All behavior below was read directly from the implementation and confirmed by diffing against pristine nopCommerce 4.90.3. Stored procedures live in the SQL Server database (they are **not** in this repository); their observable inputs/outputs are documented from the calling code.

## Overview

Customer registration on rotat.com is phone-number-first. Before the standard nopCommerce registration form is ever shown, the visitor must:

1. enter a UAE mobile number (`/RegisterCustomerOTP`),
2. receive a 4-digit one-time password via SMS through the **Dexatel Verifications API**,
3. enter the OTP within 3 minutes (`/CustomerOTP`).

Only after successful verification is the visitor allowed onto the standard `/register` page, where the **verified phone number becomes the nopCommerce username** (usernames are enabled). Account activation state is toggled on the underlying customer record during OTP confirmation.

The feature is implemented in `HomeController` (public), two custom views, custom routes, two SQL Server stored procedures, and the intl-tel-input JS library added to the Prisma theme. The admin side adds a post-registration approval/ERP-onboarding workflow in the Admin `CustomerController`.

## Business Requirement

- Rotat is a B2B parts platform in the UAE; the phone number is the primary customer identity (used for WhatsApp/sales follow-up and as the ERP contact key).
- Prevent fake/duplicate sign-ups: a number can only register once (`Username = phone`, uniqueness enforced by username check).
- Verify real ownership of the number **before** collecting the rest of the registration data.
- After web registration, the customer must be reviewed by an admin and pushed to the NetSuite ERP as a customer record (see *Admin Changes*).

## Files Modified

| Project | Folder | File | Status | Role |
|---|---|---|---|---|
| Nop.Web | `Controllers/` | `HomeController.cs` | **Rewritten (custom)** | All OTP actions + Dexatel call |
| Nop.Web | `Views/Home/` | `RegisterCustomerOTP.cshtml` | **New** | Phone entry screen |
| Nop.Web | `Views/Home/` | `CustomerOTP.cshtml` | **New** | OTP entry screen |
| Nop.Web | `Infrastructure/` | `RouteProvider.cs` | **Modified** | Routes `RegisterCustomerOTP`, `CustomerOTP/{UserName}`, `ConfirmOTP/{CID}/{OTP}`, `SendCustomerOTP/...` |
| Nop.Web | `Controllers/` | `CustomerController.cs` | **Modified** | `Register` GET gate: no verified phone → redirect to OTP page |
| Nop.Web | `Themes/Prisma/Views/Customer/` | `Register.cshtml` | **New (theme override)** | Injects verified phone into `Model.Username` |
| Nop.Web | `Themes/Prisma/Content/scripts/` | `intlTelInput.js`, `intlTelInput.css`, `utils.js` | **New** | International phone input (restricted to AE) |
| Nop.Web | (root) | `NetSuiteApiConfig.cs`, `IApiConfig.cs` | **New** | ERP API credentials/config (admin flow dependency) |
| Nop.Web | `Areas/Admin/Controllers/` | `CustomerController.cs` | **Modified** | ERP status check, create/update ERP customer, account-manager mapping |
| Nop.Web | `Areas/Admin/Factories/` | `CustomerModelFactory.cs` | **Modified** | ERP fields, account-manager filter, COD/customer-type props |
| Nop.Web | `Areas/Admin/Models/Customers/` | `CustomerModel.cs`, `CustomerSearchModel.cs` | **Modified** | ERP/AM/COD/CustomerType model fields |
| Nop.Web | `Areas/Admin/Views/Customer/` | `List.cshtml`, `_CreateOrUpdate.Info.cshtml` | **Modified** | ERP columns/fields in admin UI |
| Nop.Core | `Domain/Customers/` | `Customer.cs` | **Modified** | `ERPCustomerId`, `ERPCustomerIdToUpdate`, `ERPRegisteredCIdsByPhone/Email`, `SelectedCODCountryId`, `SelectedCustomerTypeId` |
| Nop.Core | `Domain/Customers/` | `CustomerType.cs`, `CODFactors.cs` | **New** | Lookup entities used at admin approval |
| Nop.Services | `Customers/` | `CustomerService.cs`, `ICustomerService.cs` | **Modified** | `GetAllCODFactorsAsync`, `GetAllCustomerTypeAsync`, ERP/AM search filters |

## Classes / Methods

### `Nop.Web.Controllers.HomeController` (public)

| Method | Route | Purpose |
|---|---|---|
| `RegisterCustomerOTPAsync(string CodePhoneNumber, string PhoneNumber)` | `GET /{lang}/RegisterCustomerOTP` | Phone entry + duplicate check. Signs out an already-registered session. Strips whitespace. If `GetCustomerByUsernameAsync(code+number)` finds an existing customer → shows "Username already exists"; otherwise stores full number in `TempData["UserName"]` and redirects to `CustomerOTP`. *(The entry form posts back to this same action via HTTP GET — the `<form>` has no `method`, so query-string parameters.)* |
| `CustomerOTP(string UserName)` | `GET /{lang}/CustomerOTP/{UserName}` | Generates OTP, calls Dexatel, persists attempt via `CustomerSaveOTP` proc, renders OTP entry view. Re-hitting this URL (the *Resend* button reloads it with `?key=REconfirmOtp`) regenerates and resends a new OTP. |
| `ConfirmCustomerOTP(int CID, string OTP, string NewPhoneNumber)` | AJAX `GET Home/ConfirmCustomerOTP` | Thin wrapper: calls `ConfirmOTP(CID, OTP).Result` (sync-over-async) and re-stores `TempData["UserName"]` so `/register` can consume it. Returns `"Done"` / `"Fail"` as plain string. |
| `ConfirmOTP(int CID, string OTP)` | `GET /{lang}/ConfirmOTP/{CID}/{OTP}` (also routed directly) | Validation core: executes `GetConfirmOTP` proc with `(CID, OTP)`; if a row is returned and its datetime ≥ `DateTime.Now.AddMinutes(-3)`, sets `customer.Active = true; customer.Deleted = false;` via `UpdateCustomerAsync` and returns `"Done"`. |
| `RandomOnlyNumber()` | — | Generates the 4-digit OTP with `System.Random` over `"0123456789"` (length 4). |

### `Nop.Web.Controllers.CustomerController` (public, modified)

| Method | Change |
|---|---|
| `Register` (GET) | Added gate at the top: `if (TempData["UserName"] == null) return RedirectToRoute("RegisterCustomerOTP");` — the standard registration form is unreachable without a verified phone in TempData. The POST action is stock. |

### Dexatel integration (inline in `CustomerOTP`)

There is no separate integration class; the HTTP call is made inline with `IHttpClientFactory`:

- **Endpoint**: `POST https://api.dexatel.com/v1/verifications`
- **Auth header**: `X-Dexatel-Key: 218e25c2e5939f7b92654303f0a50b9d` *(hardcoded in source — see Risks)*
- **Payload** (serialized with `System.Text.Json`):

```json
{ "data": {
    "channel":  "SMS",
    "sender":   "Dexatel",
    "phone":    "<full phone incl. country code>",
    "template": "9c9dcaa2-990a-45c2-9efd-ae0bd4d63cbf",
    "code":     "<4-digit OTP>"
} }
```

- **Success criterion**: HTTP `201 Created` → `otpStatus = "Done"`; anything else (or exception) → `"Failed"`. The failure is recorded but **the user still lands on the OTP entry screen** (no error UI for SMS failure).

## Database Changes

No FluentMigrator migrations were added; schema objects were created directly in SQL Server. From the calling code:

| Object | Type | Called from | Contract (observed) |
|---|---|---|---|
| `dbo.CustomerSaveOTP` | Stored procedure | `HomeController.CustomerOTP` | IN: `@CID int, @otp nvarchar, @OTPStatus nvarchar ('Done'/'Failed'), @UserName nvarchar`. OUT: result set of `datetime` — the last-sent OTP timestamp. Persists the OTP attempt keyed to the **guest customer id**. |
| `dbo.GetConfirmOTP` | Stored procedure | `HomeController.ConfirmOTP` | IN: `@CID int, @otp nvarchar`. OUT: result set of `datetime` (non-empty ⇔ the OTP matches for this customer); caller enforces the 3-minute expiry against the returned timestamp. |
| `Customer` table | Modified | Linq2DB entity `Customer` | New columns backing `ERPCustomerId (int null)`, `ERPCustomerIdToUpdate (int)`, `ERPRegisteredCIdsByPhone (nvarchar)`, `ERPRegisteredCIdsByEmail (nvarchar)`, `SelectedCODCountryId (nvarchar)`, `SelectedCustomerTypeId (int)`. |
| `CustomerType`, `CODFactors` | New tables | `CustomerService.GetAllCustomerTypeAsync/GetAllCODFactorsAsync` | Lookup tables for the admin approval form. |
| `dbo.GetCustomerERPRegistered`, `dbo.CustomerSaveERPRegistered`, `dbo.UpdateFactorRoleMappingsToCustomer`, `dbo.UpsertAccountManagerCustomerMappingByCurrentCustomer` | Stored procedures | Admin `CustomerController` | ERP registration bookkeeping, price-factor role mapping, account-manager mapping (admin flow). |
| `Account_Manager`, `AccountManager_CustomerMapping`, `CountryRigionMapping`, `AccountManagerRigionMapping` | Custom tables | Admin `CustomerController` / `CustomerService` | Account-manager assignment and admin data-visibility restriction. |

> ⚠️ The stored procedure bodies and the OTP storage table are **only** in the database. They should be scripted into source control (see Future Improvements).

## Configuration

| Item | Where | Notes |
|---|---|---|
| Dexatel API key | Hardcoded in `HomeController.CustomerOTP` | Should move to `appsettings.json` / secrets. |
| Dexatel template id | Hardcoded (`9c9dcaa2-…`) | Message template configured in the Dexatel dashboard. |
| OTP expiry | Hardcoded `DateTime.Now.AddMinutes(-3)` in `ConfirmOTP`; mirrored client-side by a 180-second countdown in `CustomerOTP.cshtml`. |
| Allowed country | Hardcoded in `RegisterCustomerOTP.cshtml` — `intlTelInput` options `onlyCountries: ['AE'], initialCountry: 'AE'`. |
| nopCommerce settings assumed | `CustomerSettings.UsernamesEnabled = true` (phone as username); registration type Standard/AdminApproval as configured in Admin → Settings → Customer settings. |
| NetSuite API credentials | `NetSuiteApiConfig.cs` — hardcoded; **sandbox** values active, live values in comments. Used by the admin approval flow. |

## Workflow

### Complete execution flow (phone entry → account created)

```
1. Visitor opens /RegisterCustomerOTP                       [HomeController.RegisterCustomerOTPAsync GET]
   ├─ if current session is a registered customer → SignOutAsync()
   └─ View: RegisterCustomerOTP.cshtml
      ├─ intl-tel-input, UAE only, dial code shown separately
      ├─ JS guards: digits only, leading zeros stripped, submit enabled at ≥9 digits,
      │  MutationObserver copies the dial code into hidden input #CodePhoneNumber
      └─ submit (GET, same action) with CodePhoneNumber + PhoneNumber

2. Duplicate check                                          [same action]
   ├─ full = CodePhoneNumber + PhoneNumber (whitespace stripped)
   ├─ ICustomerService.GetCustomerByUsernameAsync(full)
   │  ├─ found  → ViewBag.CheckUserName → "Username already exists" shown, stay on page
   │  └─ null   → TempData["UserName"] = full
   └─ RedirectToAction CustomerOTP(UserName = full)

3. OTP generation & sending                                 [HomeController.CustomerOTP GET]
   ├─ otp = RandomOnlyNumber()            // 4 digits, System.Random
   ├─ POST https://api.dexatel.com/v1/verifications  (X-Dexatel-Key header)
   │  ├─ 201 Created → otpStatus = "Done"
   │  └─ else / exception → otpStatus = "Failed"   (user still proceeds to entry screen)
   ├─ EXEC CustomerSaveOTP @CID=<guest customer id>, @otp, @OTPStatus, @UserName
   │  └─ returns LastSentOTP datetime
   └─ View: CustomerOTP.cshtml  (ViewBag: CID, CustomerDBOTP, NewPhoneNumber, LastSentOTP)

4. OTP entry                                                [CustomerOTP.cshtml JS]
   ├─ 4 single-digit boxes, auto-advance, numeric-only, auto-submit when all filled
   ├─ 3:00 countdown; on expiry shows "OTP is not valid Or Expired." and enables Resend
   ├─ Resend → reload CustomerOTP?key=REconfirmOtp → step 3 repeats (new OTP)
   ├─ "Re-enter phone number" → back to /RegisterCustomerOTP
   └─ Confirm → $.ajax GET Home/ConfirmCustomerOTP { CID, OTP, NewPhoneNumber }

5. OTP validation                                           [ConfirmCustomerOTP → ConfirmOTP]
   ├─ customer = GetCustomerByIdAsync(CID)      // the guest session customer
   ├─ EXEC GetConfirmOTP @CID, @otp → LastSentOTP rows
   ├─ rows empty → "Fail"                        (wrong code)
   ├─ LastSentOTP < Now-3min → "Fail"            (expired)
   └─ else: customer.Active = true; customer.Deleted = false;
            UpdateCustomerAsync(customer) → "Done"
            TempData["UserName"] = NewPhoneNumber (re-armed for /register)

6. Browser: data == "Done" → window.open(host + "/register", "_self")

7. Registration form                                        [CustomerController.Register GET]
   ├─ TempData["UserName"] == null → redirect back to /RegisterCustomerOTP  (gate)
   └─ Prisma Register.cshtml: Model.Username = TempData["UserName"]  (phone pre-filled
      as username; view re-stores it in TempData to survive validation round-trips)

8. Form submit                                              [CustomerController.Register POST — stock]
   ├─ CustomerRegistrationService.RegisterCustomerAsync
   │  └─ username = phone (UsernamesEnabled), password hashed per settings,
   │     approval per UserRegistrationType
   └─ standard nop events, newsletter, notifications, redirect to result page

9. Admin approval & ERP onboarding                          [Admin CustomerController — see Admin Changes]
```

### Method call hierarchy

```
RegisterCustomerOTPAsync
├── ICustomerService.IsRegisteredAsync / IAuthenticationService.SignOutAsync
└── ICustomerService.GetCustomerByUsernameAsync

CustomerOTP
├── IWorkContext.GetCurrentCustomerAsync
├── RandomOnlyNumber
├── IHttpClientFactory.CreateClient → HttpClient.PostAsync (Dexatel)
└── MsSqlNopDataProvider.QueryProcAsync<DateTime>("CustomerSaveOTP")

ConfirmCustomerOTP
└── ConfirmOTP
    ├── ICustomerService.GetCustomerByIdAsync
    ├── MsSqlNopDataProvider.QueryProcAsync<DateTime>("GetConfirmOTP")
    └── ICustomerService.UpdateCustomerAsync   (Active=true, Deleted=false)
```

### Class dependency diagram (text)

```
RegisterCustomerOTP.cshtml ──GET──▶ HomeController ──HTTP──▶ Dexatel Verifications API
CustomerOTP.cshtml ────AJAX GET──▶ HomeController ──proc──▶ SQL Server (CustomerSaveOTP / GetConfirmOTP)
HomeController ─────────────────▶ ICustomerService / IWorkContext / IAuthenticationService
CustomerController.Register ◀──TempData["UserName"]── HomeController / Register.cshtml (Prisma)
Admin CustomerController ──REST──▶ NetSuite RESTlet (NetSuiteApiConfig)  [post-registration]
```

### Validation logic

| Layer | Rule |
|---|---|
| Client (phone page) | UAE dial code enforced by intl-tel-input; digits only; leading zeros stripped; ≥9 digits to enable submit; custom Arabic validity messages. |
| Server (phone page) | Whitespace stripped; duplicate username (= phone) rejected. |
| Client (OTP page) | Exactly 4 numeric digits; auto-submit; empty-field warning; 180s countdown gates the resend button. |
| Server (OTP) | Proc `GetConfirmOTP` must return a row for `(CID, OTP)`; row timestamp must be within the last 3 minutes. |
| Registration POST | Stock nopCommerce validators (captcha/honeypot attributes present, GDPR consents, customer attributes). |

### Error scenarios

| Scenario | Behavior |
|---|---|
| Phone already registered | Message *"Username already exists (+phone)"* on the phone page. |
| Dexatel returns non-201 / network error | `OTPStatus='Failed'` stored; **user still sees the OTP page** and will fail entry unless they resend; no user-facing SMS error. |
| Wrong OTP | AJAX returns `"Fail"` → "OTP is wrong", inputs cleared, focus reset. Unlimited retries (see Risks). |
| OTP older than 3 min | Same `"Fail"` path; client countdown also labels it expired and offers resend. |
| Direct navigation to `/register` | Redirected to `/RegisterCustomerOTP` (TempData gate). |
| TempData lost (cookie cleared / different browser) | Same redirect — user restarts phone verification. |

## Frontend Changes

- **Views**: `Views/Home/RegisterCustomerOTP.cshtml`, `Views/Home/CustomerOTP.cshtml`, Prisma `Views/Customer/Register.cshtml` (username injection), all with inline CSS (Rotat blue `#1f278a`) and Arabic localized strings via `@T(...)`.
- **JavaScript**: intl-tel-input v-bundle (`intlTelInput.js`, `utils.js`) under `Themes/Prisma/Content/scripts/`; extensive inline JS in both views (input masking, MutationObserver for dial code, OTP auto-advance/auto-submit, countdown timer, resend redirect). jQuery is loaded from the public CDN (`code.jquery.com`) on both pages.
- **AJAX**: single call — `GET Home/ConfirmCustomerOTP?CID=&OTP=&NewPhoneNumber=` returning a plain string.
- **APIs**: Dexatel Verifications (server-side only; nothing Dexatel-related runs in the browser).

## Backend Changes

- `HomeController` — OTP actions (detailed above); registered `HttpClient`/`IHttpClientFactory` usage.
- `CustomerController` (public) — TempData gate on `Register` GET.
- `RouteProvider` — 4 OTP-related routes (plus `CreditAndInvoices`, `DimensionSearch`, `GetLatestCategories` from other features). **Note:** route `SendCustomerOTP/{CurrentCustomerId}/{UserName}` points to a `Home.SendCustomerOTP` action that **does not exist** — dead route.
- No custom DI registrations are needed (controller resolves stock services; `MsSqlNopDataProvider` is instantiated with `new`, bypassing DI — see Risks).
- No events/consumers involved in the OTP flow itself.

## Admin Changes

The admin side completes the registration lifecycle (review → activate → push to ERP). All in `Areas/Admin/Controllers/CustomerController.cs` unless noted:

1. **ERP registration status** (`Edit` GET + `CheckCustomerERPStatus` AJAX): reads `GetCustomerERPRegistered` proc, and live-queries NetSuite (`objType=22`) by phone (`+Username`) and by email; persists findings via `CustomerSaveERPRegistered`; model shows *IsRegisteredPhone/IsRegisteredEmail* flags with the matching NetSuite ids.
2. **ERP action on save** (`Edit` POST): dropdown `ERPAction` — *Create new ERP customer* (`objType=23`, POST with full JSON body incl. addresses and `sales_rep` array) or *Update existing* (`objType=26`), with returned `ns_id` stored as `ERPCustomerId`.
3. **Account managers**: multi-select + primary flag, sourced from `Account_Manager` table, ERP sales-rep list (`objType=88`), or country→region mapping; saved to `AccountManager_CustomerMapping`. Customer list can be filtered by AM and by `ERPCustomerId`.
4. **Access restriction**: users in role 6 (*Account Manager*) can view/edit **only** customers mapped to them (checks in `Edit` GET/POST and in `CustomerService.GetAllCustomersAsync`); role 1 (*Admin*) unrestricted.
5. **Pricing classification**: required `SelectedCustomerTypeId` (from `CustomerType`) and `SelectedCODCountryId` (from `CODFactors`); on save runs `UpdateFactorRoleMappingsToCustomer` proc which maps the customer into price-factor roles.
6. UI: `_CreateOrUpdate.Info.cshtml` and `List.cshtml` extended with the fields/columns above.

## Website Changes

- New public pages `/RegisterCustomerOTP` and `/CustomerOTP/{phone}`; `/register` is now gated behind them.
- Registration username field arrives pre-filled with the verified phone.
- Login (stock) works with username = phone + password.

## Security Considerations / Risks

1. **🔴 OTP is rendered in the page** — `CustomerOTP.cshtml` line 10: `<div class="REN">Your OTP @ViewBag.CustomerDBOTP</div>`. Anyone can read the code on screen/HTML source, defeating SMS verification entirely. This is debug leftover and must be removed.
2. **🔴 Hardcoded secrets in source**: Dexatel API key in `HomeController`; NetSuite consumer/token secrets in `NetSuiteApiConfig.cs`. Anyone with repo access can send SMS on the account / hit the ERP. Move to configuration/secret storage and rotate the keys.
3. **🟠 Brute-forceable confirmation**: `ConfirmCustomerOTP` is an unauthenticated GET with no rate limiting or attempt counter; a 4-digit space (10,000 codes) inside a 3-minute window is scriptable. Add attempt limits and/or lengthen the code.
4. **🟠 Weak OTP generation**: `System.Random` is not cryptographically secure; use `RandomNumberGenerator`.
5. **🟠 `ConfirmOTP` flips `Active`/`Deleted` on an arbitrary `CID`** supplied by the client. The OTP check constrains it, but combined with (3) this allows reactivating/activating arbitrary customer records.
6. **🟡 TempData as the only gate** for `/register`: cookie-based, single-read semantics; losing it bounces legitimate users; it does not cryptographically bind the verified phone to the browser session.
7. **🟡 Sync-over-async** (`ConfirmOTP(...).Result`) risks thread-pool starvation under load.
8. **🟡 `new MsSqlNopDataProvider()`** bypasses DI/data-settings abstraction; ties the site to SQL Server and complicates testing.
9. **🟡 SMS failure is silent** to the user (`otpStatus='Failed'` but the OTP page still shows); users wait for an SMS that never comes until the countdown offers resend.
10. **🟡 Dead route** `SendCustomerOTP` (no matching action) — returns 404 if ever linked.
11. **🟡 jQuery from public CDN** on both OTP pages — availability/supply-chain concern; the rest of the site serves assets locally.
12. **🟡 Duplicated legacy JS**: `focusNextInput` is defined 4 times and `window.onload` reassigned 3 times in `CustomerOTP.cshtml`; last definition wins — functional today but brittle.

## Future Improvements

- Remove the on-screen OTP output (single-line fix, highest priority).
- Move Dexatel/NetSuite credentials to `appsettings.json` + secret store; rotate exposed keys; add a typed `DexatelSettings` (`ISettingService`) with an Admin configuration page.
- Extract a `IDexatelSmsService` (typed `HttpClient` via `AddHttpClient`) and DTO classes; add retry + explicit user-facing error when the SMS send fails.
- Replace `System.Random` with `RandomNumberGenerator.GetInt32`; add per-phone and per-IP rate limiting and max 3–5 verification attempts per OTP.
- Script `CustomerSaveOTP`, `GetConfirmOTP`, ERP procs and the custom tables into source control (FluentMigrator migrations or `/db` folder).
- Convert `ConfirmCustomerOTP` to POST returning JSON; remove the dead `SendCustomerOTP` route or implement the action.
- Bundle jQuery locally; consolidate the duplicated inline JS.
- Consider binding verification to the session server-side (e.g., a signed verification token) instead of `TempData` alone.
