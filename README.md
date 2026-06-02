# 🚲 GBFS Quiz

A time-boxed trivia game built on **live** bike-share data. Players get **60 seconds** to
answer as many multiple-choice questions as possible. Questions are generated dynamically
from real [GBFS](https://github.com/MobilityData/gbfs) feeds (Citi Bike NYC, Bluebikes
Boston, Divvy Chicago) — "which city has the most available bikes right now?", "which
station is closest to X?", and so on.

Built with **.NET 10**, Clean Architecture, EF Core 10 / PostgreSQL, and a **Blazor +
MudBlazor** front end (Interactive Server).

---

## Game rules

| Rule | Value |
|------|-------|
| Session length | **60 seconds** |
| Correct answer | **+50** |
| Wrong answer | **−20** |
| Win condition | Finish with a **positive balance that never dropped to/below zero** |

---

## Quick start

### Option A — Docker (everything, one command)

```bash
docker compose up --build
```

Then open **http://localhost:8080**. The API container applies EF Core migrations on
startup and serves the UI.

### Option B — Local dev (Postgres in Docker, API on host)

```bash
# 1. Start just the database
docker compose up -d db

# 2. Run the API (migrations apply automatically on startup)
dotnet run --project src/GbfsQuiz.Web
```

The launch profile serves the UI and API on the same origin; open the printed
`http://localhost:<port>` URL.

> **Secrets:** the JWT signing key and DB connection string are **not** in
> `appsettings.json`. Out of the box the app falls back to a dev-only signing key and a
> `localhost`/`postgres` connection (matching the Docker `db`). To use your own, set them
> via [.NET user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):
> ```bash
> dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;..." --project src/GbfsQuiz.Web
> dotnet user-secrets set "Jwt:SigningKey" "<a 32+ char key>" --project src/GbfsQuiz.Web
> ```

### Run the tests

```bash
dotnet test --filter "Category!=Integration"   # fast, fully offline
dotnet test                                    # also hits a live GBFS feed
```

---

## How to play

1. Open the app and **create an account** (username + password) or log in.
2. Press **Start game** — the clock starts immediately.
3. Tap an answer. Right = +50, wrong = −20. A new question appears instantly.
4. When the 60 seconds are up you see your result (**Won / Lost**), and the attempt is
   saved to your **history** on the lobby screen. Press **Play again** to retry.

---

## Architecture

Clean Architecture, organised by **feature slice**. Dependencies point inward
(`Web → Infrastructure → Application → Domain`).

```
src/
├── GbfsQuiz.Domain          # Entities + business rules (GameSession scoring, Player)
├── GbfsQuiz.Application      # Feature slices: interfaces, services, DTOs, validators
│   └── Features/{Gbfs,Quiz,Games,Auth}/{Requests,Responses,Validators,Interfaces}
├── GbfsQuiz.Infrastructure   # EF Core, FluentConfigurations, GBFS HTTP client, JWT, hashing
└── GbfsQuiz.Web              # Minimal-API endpoints, middleware, security, Blazor+MudBlazor UI (Components/)
tests/
└── GbfsQuiz.Tests            # xUnit + Moq + FluentAssertions + AutoFixture
```

### Key design decisions

- **Live data, cached briefly.** GBFS feeds refresh ~once a minute, so snapshots are
  fetched concurrently across providers and cached (~45 s). A round therefore sees a
  consistent world and the upstream feeds aren't hammered. If a provider is down it's
  skipped; the game continues as long as one provider responds.
- **The quiz engine is a set of strategies.** Each question type implements
  `IQuestionStrategy` (`CanGenerate` + `Generate`). Adding a new question = one new class.
  Strategies are pure functions of `(snapshots, Random)`, which makes them trivial to test.
- **Answers are graded server-side.** The correct choice never leaves the server: issued
  questions are remembered in a short-lived cache keyed by question id, and the client only
  ever receives choice ids. This prevents cheating via the network tab.
- **Result pattern, not exceptions.** Every service returns `FluentResults.Result<T>`.
  A single helper maps results to HTTP status codes (200/204/400/401/404/409/503); a
  `GlobalExceptionMiddleware` only ever fires for genuinely unexpected errors.
- **UUIDv7 primary keys** (`Guid.CreateVersion7()`) for index-friendly, time-ordered ids.
- **EF Core via FluentConfigurations only** (no Data Annotations); `metadata` is a
  PostgreSQL `jsonb` column; reads use `AsNoTracking()`.
- **Security**: JWT bearer auth, per-feature rate limiting, white-listed request DTOs
  (no mass assignment), and CSP / HSTS / nosniff response headers. The Blazor UI is
  CSP-compliant (all assets same-origin, no inline scripts).

### API surface

| Method & route | Auth | Purpose |
|---|---|---|
| `POST /api/auth/register` | — | Create account, returns JWT |
| `POST /api/auth/login` | — | Log in, returns JWT |
| `POST /api/games/start` | ✅ | Begin a 60s session, returns first question |
| `POST /api/games/{id}/answers` | ✅ | Submit an answer, returns score + next question |
| `POST /api/games/{id}/finish` | ✅ | Finalise the session, returns Won/Lost summary |
| `GET  /api/games/history` | ✅ | List the player's past attempts |
| `GET  /api/games/history/export` | ✅ | Download history as CSV |
| `GET  /api/leaderboard` | — | Public multiplayer leaderboard (top players) |
| `POST /api/players/me/avatar` | ✅ | Upload an avatar image (PNG/JPEG/WebP/GIF ≤512 KB) |
| `GET  /api/players/{id}/avatar` | — | Serve a player's avatar |

---

## Bonus features

All of the assignment's bonus items are implemented:

- **Automated tests** — 36 unit tests + a live GBFS integration test (`dotnet test`).
- **Multiplayer leaderboard** — public, ranked by best score then wins; shown in the lobby and result screens with avatars.
- **Avatar upload** — players upload an image (validated type/size, stored as Postgres `bytea`), shown on their profile and the leaderboard.
- **History export** — one-click CSV download in the UI (and `GET /api/games/history/export` for API clients).
- **Reveal-on-wrong** — a missed question highlights the correct answer (teaching UX), graded server-side so the answer never leaks early.

## Deploy online

A [`render.yaml`](render.yaml) Render Blueprint provisions the web service (from the existing Dockerfile) **and** a managed Postgres in one click:

1. Push this repo to GitHub.
2. In Render: **New → Blueprint**, point at the repo.
3. Render builds the container, provisions Postgres, injects the connection string and a generated JWT key, and applies EF migrations on startup.

Managed hosts (Render/Railway/Heroku/Fly) expose Postgres as a `postgres://` URI; `PostgresConnectionString.Normalize` converts it to Npgsql's form, so the same config works locally and in the cloud.

---

## Tech stack & rationale

| Concern | Choice | Why |
|---|---|---|
| Runtime | .NET 10 / C# 14 | Modern, fast, `Guid.CreateVersion7()` built in |
| Persistence | EF Core 10 + PostgreSQL | Relational history + `jsonb` flexibility |
| Results | FluentResults | Explicit success/failure without exceptions for control flow |
| Validation | FluentValidation | White-listing input, composable rules |
| Logging | Serilog | Structured, templated logs |
| Auth | JWT bearer + PBKDF2 hashing | Simple, stateless, standard |
| Tests | xUnit, Moq, FluentAssertions, AutoFixture | Readable AAA tests |
| UI | Blazor (Interactive Server) + MudBlazor | Component model, same-origin; UI calls the app services directly |

---

## What I'd change with more time

See [docs/FUTURE.md](docs/FUTURE.md) for the full list. Highlights:

- **Passkeys (WebAuthn)** instead of passwords, and move app settings into the database.
- Move the Blazor UI to **`InteractiveAuto`** rendering (it currently runs Interactive
  Server) and add `[PersistentState]` for seamless circuit reconnection.
- A **multiplayer leaderboard**, map-based "nearest station" questions, and richer
  question types (distance between bikes, pricing, e-bike availability).
- Persist a server-authoritative timer so the clock can't be gamed by a paused client.
- Integration tests with `WebApplicationFactory` + Testcontainers, and CI.
# UmobTask
