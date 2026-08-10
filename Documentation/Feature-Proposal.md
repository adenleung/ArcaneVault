# Propose-a-Feature Submission

## Feature name

Staff Acquisition and Market Insights Dashboard

## Purpose

The feature enables Arcane Vault staff to analyse anonymised acquisition activity across the prototype. It helps the company identify popular collectible categories, changes in recorded demand, average purchase prices, acquisition channels and collector-engagement patterns. These findings support Arcane Vault’s decision on whether a future marketplace may be feasible.

## Key functionality

- Users record acquisition price, date, quantity, source and condition when adding a collectible.
- Staff choose up to four cards from eight platform-wide metrics, with comparisons against the preceding equivalent period when comparison data exists.
- Staff filter all analysis by date, category and acquisition source.
- Staff can independently group the main trend by day, week, month, quarter or year; filter by product; switch between acquisition value, estimated collection value and quantity; and choose a line or bar presentation.
- Google Charts display value and volume trends, category share, source distribution and popular items.
- The API evaluates and ranks multiple rule-based signals, returning the three most relevant insights for the filtered data.
- The interface clearly labels user-entered values as prototype data rather than verified financial transactions.
- Both the Razor Pages folder and API endpoints restrict access to Staff accounts.

## Expected UI and API components

The responsive staff interface contains a filter bar, summary metric cards, Google Charts, empty states, comparison indicators and an automated-insights section. Dedicated Web API endpoints aggregate SQLite data through Entity Framework Core and LINQ rather than sending raw records to the browser.

## Why it is complex

This is more than template CRUD. It introduces an acquisition-history data model, relationships with users and collection items, several multidimensional aggregation queries, time-period comparison logic, interactive filtering, chart visualisation, staff authorisation and explanatory business insights. It directly addresses the assignment background’s requirement to investigate popular items, activity, market demand and collection trends.
