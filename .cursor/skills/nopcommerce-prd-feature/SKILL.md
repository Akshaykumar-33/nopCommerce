---
name: nopcommerce-prd-feature
description: Review nopCommerce feature PRDs and implement them by reusing platform systems (schedule tasks, message templates, queued email, events, multi-store). Use when working from a PRD, designing wishlist/price-drop/back-in-stock alerts, or turning product requirements into nopCommerce code.
---

# nopCommerce Feature from PRD

## When to use

Apply this skill when the user provides or references a feature PRD for nopCommerce, asks to implement wishlist/price-drop/stock alerts, or asks to review a PRD against platform conventions.

## PRD review checklist

Score the PRD before coding. Flag gaps; do not invent silent scope.

- [ ] **Problem** names the existing gap (what nopCommerce already does vs missing)
- [ ] **Goals** and explicit **Not in scope**
- [ ] **Users** cover shopper and admin (or explain why not)
- [ ] **User stories** map to settings, notifications, unsubscribe, and reporting
- [ ] **FR table** with IDs and MoSCoW (Must / Should / Could)
- [ ] **Technical notes** name concrete nopCommerce services/tables to reuse
- [ ] **Success metrics** are measurable (conversion, unsubscribe, task SLA)
- [ ] **Rollout** is phased (off-by-default → beta → default on)
- [ ] **Risks / open questions** cover email volume, consent/GDPR, edge products

If Must items lack technical notes or reuse targets, stop and list questions before implementing.

## Implementation principles

1. **Reuse before inventing** — Prefer `IScheduleTaskService` / `IScheduleTask`, `IWorkflowMessageService`, queued email, `IEventPublisher`, existing settings and admin report patterns.
2. **Mirror closest feature** — For alerts, start from `BackInStockSubscription` + `BackInStockSubscriptionService` + `SendBackInStockNotificationAsync`; extend; do not fork a parallel notification stack.
3. **Multi-store is Must** — Respect store, language, and currency on every price/message path.
4. **Ship Must first** — Defer Should/Could (caps, CSV, Web API, guests, SMS) unless the user asks otherwise.
5. **Off by default** — New engagement features ship behind a store setting until rollout says otherwise.
6. **Batch large work** — Scheduled price/stock scans use bounded batches (PRD default: 500) and stay under the stated SLA.
7. **Publish events** — Raise domain events so plugins can extend without core forks.
8. **Resolve open questions** — GDPR opt-in and “Call for price” handling need an explicit decision before coding those paths.

## Alert-feature domain rules

When the PRD is wishlist / price-drop / back-in-stock style:

| Rule | Default from Wishlist Alerts PRD |
|------|----------------------------------|
| Baseline price | Store price at wishlist (or subscribe) time |
| Compare price | Current price including discounts and tier prices |
| Price-drop threshold | Store setting; default 5% |
| Schedule | Hourly task unless PRD says otherwise |
| Templates | New message templates with product, old/new price, link tokens |
| Rate limits | Weekly per-customer cap + per-product cooldown (e.g. 72h) |
| Guests | Out of Phase 1 unless PRD includes them |
| Channels | Email only unless PRD includes SMS/push via plugin |

Entity shape to prefer: subscription rows with `CustomerId`, `ProductId`, `StoreId`, `BaselinePrice`, `LastNotifiedOn`, and alert `Type` (price-drop vs back-in-stock).

## Workflow

Copy and track:

```
PRD → Code:
- [ ] Review PRD with checklist; list gaps / open questions
- [ ] Map Must FRs to existing types/services (cite paths)
- [ ] Design entity + settings + message templates
- [ ] Implement service + schedule task (batched)
- [ ] Wire public toggle / My Account unsubscribe
- [ ] Wire admin settings (+ report if Should is in scope)
- [ ] Raise IEventPublisher events
- [ ] Tests for threshold, cooldown/cap, multi-store price, template send
- [ ] Feature setting off by default; document rollout phase
```

**Step 1 — Map, don’t redesign.** Search for back-in-stock, wishlist cart type, workflow messages, and schedule tasks. Cite concrete files in the plan.

**Step 2 — Data + settings.** Add entity builder, migration/installation seed for schedule task and message templates, store-scoped settings.

**Step 3 — Detection task.** Implement `IScheduleTask`: load subscriptions in pages of 500; compare; enqueue messages; update `LastNotifiedOn`; enforce cap/cooldown.

**Step 4 — Messaging.** Add `Wishlist.PriceDrop` / `Wishlist.BackInStock` (or PRD names) through `IWorkflowMessageService` + queued email; one-click unsubscribe.

**Step 5 — Verify metrics hooks.** Ensure sends are countable for conversion / unsubscribe reporting even if the admin report is Phase later.

## Output format

When reviewing a PRD, respond with:

```markdown
## Verdict
[Ready / Needs decisions / Not implementable yet]

## Insights
- ...

## FR → code map
| ID | Priority | Approach | Touchpoints |

## Open questions
- ...

## Proposed Phase 1 slice
- ...
```

When implementing, keep commits aligned to Must FRs; note deferred Should/Could in the PR description.

## Additional resources

- Worked example (Wishlist Price-Drop & Back-in-Stock Alerts): [reference.md](reference.md)
