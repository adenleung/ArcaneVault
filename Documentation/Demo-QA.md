# Demonstration and Q&A Preparation

## Suggested demonstration order

1. Register a new User account and show duplicate-email validation.
2. Log in and explain the HTTP-only cookie and signed API bearer token.
3. Add a collectible with category and acquisition information.
4. Search using a category, value or quantity to demonstrate “matching any field.”
5. Edit the record, then open the removal confirmation page.
6. Log out and sign in as Staff.
7. Show that Category Management appears only for Staff.
8. Complete one category CRUD operation.
9. Open Market Intelligence, customise the four KPI cards, then apply date/category/source/product filters.
10. Switch day/week/month/quarter/year grouping, acquisition/estimated/quantity metrics, line/bar and prior-period comparison.
11. Explain how the top-items ranking follows the selected metric and how the API ranks several calculated insight rules.

## Likely questions

### Why use a separate AcquisitionRecords table?

`CollectionItems` represents the user’s current collection. Acquisition records represent events over time. Keeping them separate means an edited quantity does not rewrite historical trends and one item can eventually have multiple acquisitions.

### How is the composite primary key configured?

In `ArcaneVaultDbContext.OnModelCreating`, `CollectionItemCategory` is configured with `HasKey(x => new { x.ItemId, x.CategoryCode })`. This prevents the same category being assigned to the same item twice.

### How do you protect staff functionality?

The web project applies a `StaffOnly` authorization policy to the `/Categories` and `/Staff` folders. The API separately verifies a signed, expiring bearer token before category mutations or analytics responses. Role information from editable request headers is ignored. Navigation visibility is only a usability layer.

### Why use soft deletion for collection items?

Soft deletion hides an item without destroying its record. This preserves referential integrity and acquisition history, which the analytical feature depends on.

### How does “search any field” work?

The API first loads the authenticated user's small prototype collection with Entity Framework, then searches item name, description, username, current quantity, starting quantity, estimated value, category code and category name in memory. This deliberately avoids numeric `ToString()` expressions that SQLite may fail to translate.

### Are the analytics hard-coded?

No values are hard-coded. Metric cards, comparisons and chart points are calculated from filtered database records using LINQ. The insight engine has transparent predefined business rules, evaluates which rules apply, ranks their relevance and returns the three highest-priority observations. The wording templates are controlled, but their selection, products, categories, percentages and monetary values depend on the current filters and database.

### Why Google Charts?

It supports responsive line, area, donut, bar and column charts with accessible browser rendering. The API returns small aggregated datasets, keeping business logic away from the browser.

### What is one limitation?

Purchase values are entered by users and are not independently verified. The dashboard therefore labels them as directional prototype signals. A future system could add receipt verification or marketplace transaction data.

### How are passwords stored?

Passwords are never stored as plain text. PBKDF2 derives a hash using a unique random salt, 100,000 iterations and SHA-256. Login repeats the derivation and compares the result using a constant-time comparison.
