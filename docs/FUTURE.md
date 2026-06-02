# Future improvements

Things I would change or add with more than the ~6-hour budget.

## Authentication & configuration
- **Passkeys (WebAuthn)** via the .NET 10 Identity scaffolding for passwordless login,
  replacing the simple username/password flow used here.
- **Database-backed configuration**: move provider lists, cache windows, scoring and JWT
  settings into a settings table so they're tunable without a redeploy.
- Refresh tokens + token revocation; right now tokens are short-lived and stateless only.

## Front end
- The UI is **Blazor + MudBlazor** (Interactive Server). Next steps: move to
  `InteractiveAuto` rendering with `[PersistentState]` for seamless circuit reconnection,
  and wire `FluentValidationValidator` on the forms so the UI reuses the same validators
  as the API instead of the lightweight inline checks it has now.
- A **map view** (Leaflet/MapLibre) for "which station is closest?" questions so players
  can see the geography.

## Game & quiz engine
- More question types: distance between two specific bikes, cheapest pricing plan,
  e-bike vs. classic availability, busiest station.
- **Server-authoritative timer**: persist `EndsAtUtc` and reject answers past it on every
  call (partially done) plus a background sweep that auto-finalises abandoned sessions.
- Difficulty scaling and per-category weighting.
- **Multiplayer leaderboard** and head-to-head rounds on the same question set.

## Quality & ops
- Integration tests with `WebApplicationFactory` + **Testcontainers** for a real Postgres,
  plus contract tests against recorded GBFS fixtures.
- CI pipeline (build, test, container publish) and online deployment.
- OpenTelemetry traces/metrics alongside the existing Serilog structured logs.
- Score/history **CSV export** and a user avatar upload (bonus features).

## Data resilience
- Cache the last good GBFS snapshot to disk so a cold start during a provider outage can
  still serve a game.
- Backoff + circuit breaker (Polly) around the GBFS HTTP client.
