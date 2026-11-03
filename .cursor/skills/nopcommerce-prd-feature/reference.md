# Reference: Wishlist Price-Drop & Back-in-Stock Alerts PRD

Sample PRD insights (Akshay Kumar, Draft, Nov 3, 2026). Use as the worked example for `nopcommerce-prd-feature`.

## Problem insight

nopCommerce already has **per-product** back-in-stock subscriptions. It does **not** have price-drop alerts or wishlist-scoped alerting. The recoverable opportunity is wishlisted intent that goes cold.

## Goals vs non-goals

| In | Out (later) |
|----|-------------|
| Email when wishlisted item cheapens or returns to stock | SMS / push |
| Admin settings + reports, no custom plugin required for core path | Price prediction |
| Reuse schedule tasks, message templates, queued email | Guest shoppers (Phase 2) |

## User stories → FR map

| Story | FR |
|-------|-----|
| Per-item “Notify me of price drops” | FR1 |
| One email when drop ≥ store % | FR3, FR4, settings |
| Email when OOS wishlisted item is available | FR4 (+ stock path) |
| Unsubscribe from email or My Account | messaging + account UI |
| Admin: enable, min drop % (default 5%), weekly cap | settings; FR6 |
| Admin report: sent / opened / orders | FR7 |

## Functional requirements

| ID | Requirement | Priority |
|----|-------------|----------|
| FR1 | Price-drop toggle per wishlist item (default on; setting-controlled) | Must |
| FR2 | Persist baseline price at wishlist time | Must |
| FR3 | Hourly scheduled compare of current price (discounts + tier prices) vs baseline | Must |
| FR4 | Templates `Wishlist.PriceDrop` and `Wishlist.BackInStock` with tokens | Must |
| FR5 | Multi-store: store, language, currency | Must |
| FR6 | Weekly email cap + 72-hour cooldown per product | Should |
| FR7 | Admin Reports + CSV export | Should |
| FR8 | Web API for headless storefronts | Could |

## Technical notes (PRD)

- Table: `WishlistAlertSubscription` — `CustomerId`, `ProductId`, `StoreId`, `BaselinePrice`, `LastNotifiedOn`, `Type`
- Hooks: `IScheduleTaskService`, `IWorkflowMessageService`
- Extensibility: `IEventPublisher`
- Batch size: 500

## Codebase touchpoints (start here)

| Concern | Existing anchor |
|---------|-----------------|
| Back-in-stock entity | `src/Libraries/Nop.Core/Domain/Catalog/BackInStockSubscription.cs` |
| Subscription service + notify/delete loop | `src/Libraries/Nop.Services/Catalog/BackInStockSubscriptionService.cs` |
| Public subscribe UI | `src/Presentation/Nop.Web/Controllers/BackInStockSubscriptionController.cs` |
| Message send | `IWorkflowMessageService.SendBackInStockNotificationAsync` |
| Product flag | `Product.AllowBackInStockSubscriptions` |
| Schedule task plumbing | `src/Libraries/Nop.Services/ScheduleTasks/` |
| Wishlist cart behavior | `IShoppingCartService` / wishlist cart type usage in Web factories |

Price-drop is **additive** (baseline + threshold + wishlist toggle). Wishlist back-in-stock should **align with** existing back-in-stock flows to avoid duplicate emails for the same customer/product/store when both are subscribed.

## Success metrics

- ≥ 8% of alert emails → order within 7 days
- Unsubscribe rate &lt; 2% of alert emails
- Scheduled task &lt; 60s for 100k wishlist items

## Rollout

1. Phase 1: setting off by default; community beta
2. Phase 2: guest email capture; SMS via plugin
3. Phase 3: default on in next major release

## Risks / open questions to force decisions on

1. **Sale-day email storms** — mitigate with FR6 cap + cooldown
2. **GDPR** — separate opt-in checkbox for alerts?
3. **Call for price** products — disable alerts?

## Review verdict template (this PRD)

```markdown
## Verdict
Ready for Phase 1 Must slice after GDPR + Call-for-price decisions

## Insights
- Reuse back-in-stock messaging/task patterns; do not replace them
- Baseline price at wishlist time is the core price-drop invariant
- Multi-store/language/currency is Must, not polish
- FR6/FR7/FR8 and guests are explicitly deferrable

## Proposed Phase 1 slice
- FR1–FR5 + unsubscribe + off-by-default setting
- Optionally include FR6 early if sale-volume risk is high
```
