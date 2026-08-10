# Analytics Feature Defence

## Why acquisition history is separate

`CollectionItems` is the user's current state: what they own now, its estimated value and remaining quantity. `AcquisitionRecords` is event history: when units were acquired, at what recorded price, through which source and in what condition. Separating them prevents an ordinary edit to current stock from rewriting historical trends and allows future support for restocking or multiple purchases of one item.

## KPI formulas

| KPI | Formula | Meaning |
|---|---|---|
| Recorded acquisition value | `Σ(UnitPrice × Quantity)` | User-recorded spend represented by matching acquisition events. |
| Items acquired | `Σ(Quantity)` | Units in matching acquisition events. |
| Active collectors | `COUNT(DISTINCT UserName)` | Distinct users with at least one matching acquisition. |
| Average unit price | `Σ(UnitPrice × Quantity) ÷ Σ(Quantity)` | Quantity-weighted mean, avoiding an incorrect average of averages. |
| Estimated collection value | `Σ(CurrentQuantity × EstimatedUnitValue)` for distinct matching items | Current estimated value of active items represented by the selected acquisition filters. |
| Acquisition events | `COUNT(AcquisitionRecords)` | Number of matching recorded acquisition events. |
| Categories represented | `COUNT(DISTINCT CategoryCode)` | Number of categories represented by matching items. |
| Leading source share | `Largest source event count ÷ all event count × 100` | Dependence on the most frequently recorded acquisition source. |
| Previous-period change | `(Current − Previous) ÷ Previous × 100` | Comparison with the immediately preceding date window of equal length. |

If the previous value is zero, the interface displays “No comparable prior data” instead of inventing a percentage or dividing by zero.

Staff may choose between one and four KPI cards from eight available metrics. The preference is saved only in that browser using `localStorage`; it does not change database data or other Staff accounts. The default selection is estimated collection value, recorded acquisition value, items acquired and active collectors.

## Filtering, grouping and charts

Date, category, source and product filters are applied by the API before every summary, trend, breakdown, ranking and insight calculation. Day uses calendar date; week begins Monday; month begins on day 1; quarter begins in January, April, July or October; year begins January 1. The prior-period line uses an equal-length window immediately before the selected range. Acquisition value, estimated value and quantity switches change the chosen metric; line/bar changes presentation without changing source records.

The top-items endpoint returns every matching product. The browser sorts by the selected metric and displays the highest eight, so a high-estimated-value product is not excluded merely because its quantity is low. Selecting a specific product filters the entire dashboard to that product.

## Calculated insight selection

The API evaluates multiple transparent rules for category concentration, source dependency, estimated-value concentration, estimated value versus acquisition cost, period activity, average price and collector participation. Each applicable observation receives a documented priority; the three highest-priority cards are returned. Titles and outcomes therefore change with the current data and filters. This is local deterministic analysis, not generative AI, and it sends no data to an external service.

All analytical grouping happens after a simple Entity Framework materialisation step. This is intentional for the small academic data set and prevents complex SQLite date/group queries from failing translation. A production-scale system should aggregate in a warehouse or database-native reporting layer.

## Soft deletion and access control

Deleting a collection item sets `IsDeleted`; analytics exclude it from active analysis while preserving its related history and referential integrity. Razor Pages protect staff routes, and the API independently verifies a signed bearer token and Staff role. Collection mutations additionally enforce item ownership.

## Limitations and future work

- Purchase values are entered by users and are not verified transactions.
- Currency is assumed to be Singapore dollars; exchange-rate handling is not implemented.
- The first acquisition is recorded automatically when an item is created; a future restocking workflow should add later events explicitly.
- The prototype compares aligned grouped buckets by sequence; a production dashboard should use a complete date spine for missing periods.
- Future work could add receipt verification, audit logs, token revocation, pagination, exports and database-side aggregation.
