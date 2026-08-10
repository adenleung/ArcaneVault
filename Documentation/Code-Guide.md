# Arcane Vault Code Guide

This guide explains where each responsibility lives and how the important code works. Comments are placed around security, database relationships, ownership, filtering and formulas. Obvious syntax such as a closing brace is not commented because excessive line-by-line comments make code harder to read and defend.

## 1. Request flow

1. The browser requests a Razor Page from `ArcaneVault.Web`.
2. The PageModel validates display/query inputs and calls `ApiClient`.
3. `ApiClient` forwards the signed bearer token to `ArcaneVault.Api`.
4. The API validates the token and role again; it never trusts an editable username or role header.
5. A controller uses `ArcaneVaultDbContext` and Entity Framework Core.
6. The controller returns a DTO as JSON.
7. Razor renders HTML; the analytics page passes chart DTOs to `analytics.js`, which draws Google Charts.

This separation satisfies the requirement that Razor Pages consume a Web API rather than accessing SQLite directly.

## 2. Project map

### `ArcaneVault.Api`

- `Program.cs`: registers controllers, EF Core, token services, API error handling and database startup.
- `Data/ArcaneVaultDbContext.cs`: declares tables, keys, relationships, indexes and delete rules.
- `Data/DatabaseBootstrap.cs`: owns the single controlled schema creation/version process.
- `Data/DbSeeder.cs`: inserts clean demonstration roles, users, categories, items and acquisitions only when the database is empty.
- `Data/PasswordSecurity.cs`: hashes and verifies passwords with PBKDF2.
- `Data/ApiTokenService.cs`: creates and validates signed, expiring identity tokens.
- `Controllers/AccountController.cs`: registration/login and duplicate account checks.
- `Controllers/CollectionItemsController.cs`: collection CRUD, search, soft deletion and ownership checks.
- `Controllers/CategoriesController.cs`: category CRUD and in-use deletion protection.
- `Controllers/StaffAnalyticsController.cs`: staff-only formulas, grouping, filters, comparisons and rule-based insights.
- `DTOs`: defines safe request and response shapes; database entities are not accepted directly from the browser.
- `Models`: defines the EF Core database entities.

### `ArcaneVault.Web`

- `Program.cs`: configures Razor Pages, cookie authentication, Staff policy and friendly error routing.
- `Services/ApiClient.cs`: makes Web API requests and translates API errors into usable messages.
- `Pages/Account`: register, login and logout pages.
- `Pages/Collection`: collection list/create/details/edit/remove workflows.
- `Pages/Categories`: staff category workflows.
- `Pages/Staff/Analytics.cshtml.cs`: loads each analytical dataset from the API and safely handles a partial failure.
- `Pages/Staff/Analytics.cshtml`: renders filters, selectable KPIs, charts, definitions and insights.
- `wwwroot/js/analytics.js`: draws data returned by the API; it does not contain product values.
- `wwwroot/css/site.css` and `refinements.css`: core and final responsive presentation rules.

### `ArcaneVault.Tests`

- `DatabaseTests.cs`: keys, uniqueness, soft deletion and database behaviour.
- `SecurityTests.cs`: password hashing and signed-token tampering.
- `ValidationTests.cs`: impossible quantities, future dates, negative values, image schemes and empty analytics results.

## 3. Authentication and authorisation

1. Login receives email/username and password.
2. The API finds the account and calls `PasswordSecurity.Verify` against the stored hash.
3. The API returns a token whose username, role, expiry and nonce are protected by an HMAC signature.
4. The Web project places that token in an HTTP-only authentication cookie.
5. `ApiClient` sends the token using `Authorization: Bearer`.
6. `ApiControllerBase.CurrentIdentity()` validates the signature and expiry inside the API.
7. `RequireStaff()` returns an error unless the validated role is Staff.
8. Collection update/delete operations compare the validated username with the record owner. Staff may inspect all records; normal users may change only their own.

The navigation is only a convenience. API checks remain the actual security boundary.

## 4. Collection and category CRUD

`CollectionItemRequest` performs input validation before normal EF Core CRUD runs. Starting/current quantities and values cannot be negative, current quantity cannot exceed starting quantity, dates cannot be in the future, category must exist, and only safe image paths/URLs are accepted.

Deleting a collection item sets `IsDeleted` rather than destroying it. Normal queries exclude soft-deleted rows, while acquisition history remains available. Deleting a category returns a conflict response if any collection item still uses it.

## 5. Why `AcquisitionRecord` is separate

`CollectionItem` describes current state: current quantity and the user’s latest estimate. `AcquisitionRecord` describes a historical event: quantity bought, amount paid, purchase date, source and condition. If these were one table, editing the current collection could incorrectly rewrite the historical chart.

## 6. Analytics formulas

| Metric | Formula |
|---|---|
| Recorded acquisition value | sum of `UnitPrice × AcquisitionRecord.Quantity` |
| Items acquired | sum of `AcquisitionRecord.Quantity` |
| Active collectors | distinct acquisition usernames |
| Average unit price | recorded acquisition value ÷ items acquired |
| Estimated value of matching items | for each distinct matching item: `CurrentQuantity × EstimatedUnitValue` |
| Acquisition events | count of matching acquisition rows |
| Categories represented | distinct category codes among matching items |
| Leading source share | largest source event count ÷ all matching events × 100 |

The previous period has the same number of days immediately before the selected range. When its denominator is zero, the UI says **No comparable prior data** rather than showing a misleading `+100%`.

Estimated value is a current snapshot of items selected through acquisition filters. It is not a historical market valuation. All purchase and estimated values are user-entered prototype figures and are not externally verified.

## 7. Filtering, grouping and charts

`FilterAsync` applies inclusive date, category, source and product filters. Grouping maps dates to day, Monday-starting week, month, quarter or year. The API returns neutral `ChartPoint` DTOs. JavaScript chooses the requested metric and line/bar presentation, so changing a display choice never changes stored data.

The KPI customiser stores only card names in browser `localStorage`. It never stores or invents KPI values. All card values still come from the current API response.

## 8. Explainable insights

The insight endpoint calculates candidate observations for category/source concentration, estimated-versus-paid differences, current-versus-prior activity and participation. Each rule receives a priority. The API sorts candidates and returns the top three. Therefore the wording responds to the database and filters while remaining deterministic and explainable; no OpenAI or other AI API is used.

## 9. Clean demonstration data

`DbSeeder.SeedAsync` uses one `SeedItemSpec` per collectible. The same specification creates the current item, its category relationship and its acquisition event, preventing mismatched arrays. Purchase dates are relative to the day the clean database is created, so records remain useful and never appear in the future.

If an older local database still contains temporary records, stop both projects, delete the root `ArcaneVault.db`, and start the API again. The controlled bootstrap will create the clean database. Do this only when old demonstration data is no longer needed.

## 10. Marking explanation

- “Razor Pages always call the separate API through `ApiClient`.”
- “The API validates signed identity and ownership; navigation is not relied on for security.”
- “Acquisition history is separate so editing current stock does not rewrite historical events.”
- “The dashboard is database-backed. JavaScript only displays API DTOs.”
- “Estimated values are current user-entered estimates, not verified market prices.”
- “Insights are ranked transparent rules, so I can explain why every sentence appeared.”
- “Soft deletion keeps analytical history while hiding removed items from customer views.”
