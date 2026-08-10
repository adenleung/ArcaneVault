# Arcane Vault Quality Assurance Checklist

## Compulsory requirements

| Requirement | Implementation evidence | Status |
|---|---|---|
| SQLite file | Versioned bootstrap; clean EF schema with timestamped legacy backup | Complete |
| Five required tables | Entities and `ArcaneVaultDbContext` | Complete |
| Required relationships | Fluent configuration and foreign keys | Complete |
| EF Core in Web API | SQLite provider and generated entities | Complete |
| Registration | Razor Page, API, model and validation | Complete |
| Duplicate email | Unique index and API conflict response | Complete |
| Login/logout | PBKDF2 verification and cookie authentication | Complete |
| Category CRUD | Five Razor Pages and five API operations | Complete |
| Collection CRUD | Five Razor Pages and five API operations | Complete |
| Search any field | Provider-safe name, description, owner, quantity, value and category search | Complete |
| Staff navigation | Role-aware Categories and Market Intelligence links | Complete |
| User navigation | Authenticated My Collection and Add Item links | Complete |
| API identity | Signed expiring bearer token; editable identity headers ignored | Complete |
| Ownership | API restricts item read/update/delete to owner unless Staff | Complete |
| Error handling | Friendly Razor error page and structured API problem response | Complete |
| Student comments | Included in all `.cs` and `.cshtml` files | Complete |

## Custom feature

| Area | Verification |
|---|---|
| Data model | Acquisition history stores price, quantity, date, source and condition independently from current collection state. |
| Authorisation | Staff pages use a `StaffOnly` policy and analytics API endpoints require the Staff role. |
| Filters | Date, category, source and product affect every chart; grouping supports day/week/month/quarter/year. |
| Visualisation | Google Charts supports trend, category, channel and popular-item views. |
| Analysis | Summary comparisons and three rule-based insights are computed from filtered data. |
| Accuracy | Weighted average uses total recorded value divided by total acquired quantity. |
| Limitations | The interface labels user-entered values as directional prototype data. |

## Verification completed in the preparation environment

- JSON configuration parses successfully.
- Project XML parses successfully.
- JavaScript passes Node syntax checks.
- SQLite contains all required table definitions.
- All local image references and product assets are present.
- ZIP archive integrity passes.
- Student-information comments are present.
- Automated test project covers security, validation, data constraints, soft deletion, search fields and empty results.

## Required final Visual Studio verification

The preparation environment does not contain the .NET SDK. Before submission, run these on the Windows/Visual Studio machine:

1. Build the complete solution with zero errors.
2. Run all tests with Test Explorer or `dotnet test ArcaneVault.sln`.
3. Start both API and Web projects with the supplied multi-project launch profile.
4. Test registration, duplicate email, login and logout.
5. Test every Category and Collection CRUD operation.
6. Test ordinary-user access denial for staff URLs.
7. Test search using text, category, quantity and value.
8. Test every analytics filter and empty state.
9. Check the browser console and API Output window for errors.
10. Test at 1440 px, 1024 px, 768 px and 390 px widths; verify grid/list layout and failed-image placeholders.

## Final demonstration data check

- Confirm the database contains one Staff account, two normal User accounts, six categories and twelve realistic collectibles.
- Confirm there are no temporary names such as `hihi` or `skibiditoilet`, unrelated landscape images or unexplained S$100,000 entries.
- Confirm every item image is square, relevant to its category, has useful alternative text and falls back to the placeholder if loading fails.
- Confirm acquisition dates are not in the future and include enough earlier rows for a previous-period comparison.

## Manual workflow evidence sheet

Record Pass/Fail and, if required by the tutor, one screenshot for each case:

| Case | Expected result |
|---|---|
| Register unique user | Account created confirmation; login available |
| Duplicate username | Clear conflict message; no technical page |
| Duplicate email | Clear conflict message; no technical page |
| Login/logout | Session begins and ends correctly |
| User CRUD own item | Add/view/edit/remove confirmations appear |
| Edit another user's ID | 403/friendly access denied; data unchanged |
| User opens Staff URL/API | 403/friendly access denied |
| Staff category CRUD | All operations work; confirmations appear |
| Delete category in use | Clear conflict; category remains |
| Invalid item ID | Friendly not-found page; no exception screen |
| Analytics filters | Active-filter summary and every chart/KPI respond |
| Empty analytics result | Recovery message and Reset filters action appear |
| KPI customiser | One–four cards; choice remains after refresh |
| Estimated/acquisition labels | Definitions and unverified-data warning visible |
| Responsive widths | Navigation, controls, labels and actions remain usable |

## Dependency vulnerability review

Before submission, use Visual Studio's NuGet manager or run `dotnet list package --vulnerable --include-transitive`. Review safe patch-level updates for any reported transitive package. Do not make an untested major framework upgrade immediately before submission; after any package change, rebuild and run every test again.

## API endpoint release matrix

Test unauthenticated, ordinary User, other-owner User and Staff where applicable: registration/login; category GET/POST/PUT/DELETE; collection GET/POST/PUT/DELETE; analytics summary/trend/categories/sources/top-items/products/insights. Confirm 400 validation, 401 missing/invalid token, 403 wrong role/owner, 404 missing record, 409 duplicate or in-use category, and successful responses. Test search by name, description, owner, category code/name, starting/current quantity and estimated value. Test analytics with an empty database and filters yielding no rows.
