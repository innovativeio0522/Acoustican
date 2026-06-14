# Plan A (Subscriptions for public learners) — Implementation Blueprint

This document describes how to implement subscription gating for the public learner side using your existing catalog model:
- `PricingTier` (Name, Price, BillingPeriod, IsPublished)
- `PricingFeature` (Feature key, IsIncluded)

## Assumption
AdminPanel authentication is currently JWT-based for `AdminUser`.
Plan A requires public learners to have JWTs too, or for the frontend to authenticate against the same backend.

## Step 0 — Entitlement model
Treat `PricingFeature.Feature` as a capability key.
Example keys (you will create these in admin pricing UI):
- `COURSE_ACCESS`
- `MODULE_ACCESS`
- `LESSON_ACCESS`
- `VIDEO_ACCESS`
- `FILE_DOWNLOAD` (optional)

Rule:
If a learner has an active subscription for `PricingTier X`, they are granted all `PricingTier.Features` where `IsIncluded=true`.

## Step 1 — Add subscription persistence
Create table/model for learners’ subscriptions.
Suggested new entity:
- `UserSubscription`
  - `Id`
  - `UserEmail` (or `UserId` if you have a stable public user PK)
  - `PricingTierId`
  - `Status` (trialing/active/past_due/canceled)
  - `Provider` (e.g., Stripe)
  - `StripeCustomerId`
  - `StripeSubscriptionId`
  - `CurrentPeriodEndUtc`
  - `CreatedAtUtc`, `UpdatedAtUtc`

Add `DbSet<UserSubscription>` to `ApplicationDbContext`.

## Step 2 — Stripe workflow endpoints
### 2.1 Checkout start
`POST /api/subscription/checkout`
- Auth required (public learner)
- Body: `{ pricingTierId }`
- Server:
  - load tier
  - create Stripe Checkout Session for subscription
  - store a pending mapping (optional, depends on Stripe flow)
  - return `checkoutUrl`

### 2.2 Webhook
`POST /api/subscription/webhook`
- Allow anonymous
- Verify Stripe signature header with webhook secret
- On events:
  - `checkout.session.completed`
  - `customer.subscription.updated`
  - `customer.subscription.deleted`
- Update `UserSubscription` row idempotently (key on StripeSubscriptionId).

### 2.3 Subscription status
`GET /api/subscription/status`
- Auth required
- Returns: `{ status, pricingTierId, currentPeriodEndUtc }` plus feature keys (optional).

## Step 3 — Entitlement checker service
Create `IEntitlementService`:
- `Task<bool> HasFeatureAsync(string userEmail, string featureKey)`
- `Task<HashSet<string>> GetActiveFeatureKeysAsync(string userEmail)`

Implementation:
- find active subscription(s)
- load tier + included features
- return set of feature keys

## Step 4 — Enforce entitlements
Update premium endpoints so they do server-side checks.

Key points in your current repo:
- `VideosController.GetVideoOtp` currently allows anonymous previews.
  - keep anonymous preview for free preview videos
  - for paid content, require `VIDEO_ACCESS`
  - you can also keep watermark behavior for authenticated users

Courses/modules/lessons:
- right now `CoursesController` endpoints are anonymous for reads.
  - change plan A behavior:
    - keep course list/preview anonymous (only published free/preview data)
    - add protected endpoints for “paid course details / modules / lessons”
    - or guard existing endpoints and return only entitlements

## Step 5 — AdminPanel UI support
AdminPanel should:
- list pricing tiers/features
- allow configuring feature keys for each tier
- add a read-only page showing active subscriptions and learner emails/status

## Step 6 — Testing checklist
- User without subscription hitting paid endpoint => 403
- User with active subscription => 200
- Stripe webhook updates DB => entitlement changes immediately after webhook
- Cancel subscription => access remains until period end (or immediately, depending on rule)

---

