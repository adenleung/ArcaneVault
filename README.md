# Arcane Vault

Arcane Vault is an ASP.NET Core prototype for collectible collection management. It includes a Razor Pages web application, a separate Web API, Entity Framework Core, SQLite, role-based access, full CRUD workflows, search and a staff acquisition-analytics feature powered by Google Charts.

## Included features

- User registration with required-field, type and duplicate-email validation
- PBKDF2 password hashing, cookie sign-in and logout, plus signed eight-hour API bearer tokens
- User and Staff roles
- Staff-only category list, create, details, update and delete pages
- Collection item list, create, details, update and soft-delete pages
- Search across item name, description, owner, category, quantity and value
- Ownership checks for user collection records
- Acquisition records containing date, price, quantity, source and condition
- Staff-only analytical API endpoints
- Staff dashboard with acquisition value, estimated collection value, demand mix, channel analysis and metric-aware top items
- Eight database-backed KPI choices; Staff can display up to four and the browser remembers the selection
- Ranked rule-based insights selected from current filters and live SQLite data
- Responsive customer collection with summary cards, filters, sorting and grid/list layouts
- Friendly website error handling and structured API error responses
- Original, brand-free collectible imagery
- AI Smart Add with image recognition, confidence disclosure and catalogue-confirmation workflow
- Searchable `CollectibleCatalog` master data that prevents AI suggestions from being saved without review
- User-only Vault Assistant grounded in the authenticated user's collection summary
- Manual fallbacks when AI recognition is uncertain or temporarily unavailable

## Technology

- ASP.NET Core 10 Razor Pages
- ASP.NET Core Web API
- Entity Framework Core 10
- SQLite
- Cookie authentication
- Google Charts
- HTML, CSS and JavaScript

## Run in Visual Studio 2026

1. Open `ArcaneVault.sln`.
2. Allow Visual Studio to restore NuGet packages.
3. Select the included **Arcane Vault - Web and API** multi-project launch profile. If Visual Studio does not show it, right-click the solution, choose **Configure Startup Projects**, select **Multiple startup projects**, and set both `ArcaneVault.Api` and `ArcaneVault.Web` to **Start**.
4. Start without debugging or press `Ctrl+F5`.
5. The API runs at `http://localhost:5165` and the website at `http://localhost:5265`.

On the first API start, one controlled bootstrap checks schema version 5, creates the Entity Framework schema and inserts demonstration records. If an older incompatible database is present, it is copied to a timestamped `.backup-*` file before a clean schema is created. This replaces fragile per-column repair code.

### AI configuration

Copy `.env.example` to `.env.local` at the solution or workspace root and set `OPENAI_API_KEY`. The API project searches parent folders for this ignored file at startup. Never place the key in JavaScript, `appsettings.json`, screenshots or source control. Smart Add and Vault Assistant keep working with clear fallback messages if the AI service is unavailable.

## Demonstration accounts

| Role | Email | Password |
|---|---|---|
| Staff | `staff@arcanevault.test` | `Staff123!` |
| User | `aden@arcanevault.test` | `Aden123!` |
| User | `collector@arcanevault.test` | `Collect123!` |

These credentials are demonstration data only and should be changed before any non-academic deployment.

## Important routes

| Area | Route |
|---|---|
| Website | `http://localhost:5265` |
| API documentation | `http://localhost:5165/swagger` |
| Collection | `/Collection` |
| Category management | `/Categories` |
| Staff analytics | `/Staff/Analytics` |
| Smart Add | `/Collection/Create` |

## Architecture and security

The browser uses Razor Pages only. Razor Pages call the separate Web API through `ApiClient`; controllers use Entity Framework Core for normal CRUD against SQLite. Login returns a signed HMAC token containing the validated username, role and expiry. The web project stores that token inside its HTTP-only authentication cookie and sends it as `Authorization: Bearer` to the API. Editable identity headers are not trusted.

The web project protects staff folders for good UX, while API controllers independently verify the signed token and Staff role. Collection API operations also compare the authenticated username with each item's owner. A user therefore cannot gain staff access by changing a URL or modify another user's item by changing an ID.

Set `ApiTokenSecret` through a protected environment-specific configuration source for any real deployment; the packaged value exists only so the academic prototype runs without external secrets.

## Tests

Run `dotnet test ArcaneVault.sln`. The test project covers password hashing, signed-token tampering, item validation, duplicate database email enforcement, soft deletion, safe searchable fields and empty analytical results. The manual release checklist in `Documentation/Quality-Assurance.md` covers full endpoint, authorization and responsive-browser scenarios.

For a file-by-file explanation, request flow, security walkthrough and every analytics formula, read `Documentation/Code-Guide.md`.

## Database behaviour

- Collection items use soft deletion through `IsDeleted`.
- A category cannot be deleted while it is used by collection items.
- `CollectionItemCategories` uses the required composite key of `ItemId` and `CategoryCode`.
- Acquisition history is kept separate from the current collection record so trend analysis remains historically meaningful.
- Estimated collection value is computed as `CurrentQuantity × EstimatedUnitValue`.
- Analytics use acquisition events, not the editable current-stock record; see `Documentation/Analytics-Defence.md` for formulas and limitations.

## Academic integrity

Review, understand and be able to explain every submitted section. Add or update the student-information comment if any submission details change, and acknowledge permitted AI assistance according to your tutor’s instructions and NYP policy.
