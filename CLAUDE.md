# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

GBFS Quiz — a 60-second trivia game whose questions are generated dynamically from **live** bike-share GBFS feeds (Citi Bike NYC, Bluebikes Boston, Divvy Chicago). Scoring: +50 correct / −20 wrong; you win by finishing with a positive balance that never dropped to/below zero. .NET 10 / C# 14, Clean Architecture, EF Core 10 + PostgreSQL.

## Commands

```bash
# Build the whole solution
dotnet build GbfsQuiz.slnx

# Run the API (serves the SPA + API on the same origin; applies EF migrations on startup)
docker compose up -d db                       # Postgres must be running first
dotnet run --project src/GbfsQuiz.Web         # http://localhost:5023

# Full stack in containers
docker compose up --build                     # http://localhost:8080

# Tests
dotnet test --filter "Category!=Integration"  # fast, fully offline — use this by default
dotnet test                                   # also hits a LIVE GBFS feed (network required)
dotnet test --filter "FullyQualifiedName~AuthServiceTests"          # single class
dotnet test --filter "FullyQualifiedName~AuthServiceTests.Register" # single test

# EF Core migrations (Infrastructure owns the DbContext; Web is the startup project)
dotnet ef migrations add <Name> -p src/GbfsQuiz.Infrastructure -s src/GbfsQuiz.Web
```

Integration tests are tagged `[Trait("Category", "Integration")]` because they call real GBFS endpoints — exclude them when offline or in CI without network.

## Architecture

Clean Architecture sliced by **feature**, dependencies pointing inward: `Web → Infrastructure → Application → Domain`. Within each layer, code is organized under `Features/{Auth,Games,Quiz,Gbfs}/` rather than by technical type.

- **Domain** — entities + business rules only, no dependencies. `GameSession` is the heart: it owns all scoring constants and the win condition (`RecordAnswer`/`Complete`), with private setters and a parameterless ctor for EF materialization. Don't move scoring logic out of the entity.
- **Application** — feature slices each containing `Interfaces/`, services, `Requests/`, `Responses/` (with hand-written `*Mapper` classes), `Validators/`. Defines the abstractions (`IGbfsSnapshotProvider`, `IPlayerRepository`, etc.) that Infrastructure implements.
- **Infrastructure** — EF Core (`AppDbContext` + FluentConfigurations, **no Data Annotations**), the GBFS HTTP client, JWT issuance, PBKDF2 hashing. Implements Application interfaces.
- **Web** (`GbfsQuiz.Web`) — Minimal-API endpoints grouped per feature (`MapAuthEndpoints`, `MapGameEndpoints`), middleware, security, **and the Blazor + MudBlazor UI** under `Components/`.

### Blazor UI (`src/GbfsQuiz.Web/Components/`)

The front end is a **Blazor Web App, Interactive Server** render mode (prerendering disabled), styled with **MudBlazor**. Key points:

- **Components call the application services (`IGameService`, `IAuthService`) directly via DI** — they do *not* go through the HTTP API. The HTTP API + JWT remain for programmatic clients and the test suite. So a gameplay change usually needs touching only the service + the component, and the same `Result<T>` returned by services is unwrapped in components (`result.IsFailed ? result.Errors[0].Message : result.Value`).
- **`Components/State/PlayerSession.cs`** is a scoped, per-circuit holder of the signed-in player (`PlayerId` + `DisplayName`) with a `Changed` event the layout/page subscribe to. The UI tracks identity only — it doesn't use the JWT. Login is persisted across refreshes via `ProtectedLocalStorage` (key `gbfs_session`); restore happens in `Home.OnAfterRenderAsync(firstRender)`.
- **`Components/Pages/Home.razor`** is the whole game as one stateful component (Loading → Auth → Lobby → Game → Result), mirroring the old SPA's single-screen flow. The 60-second countdown is a server-side `System.Timers.Timer` whose `Elapsed` marshals back via `InvokeAsync(...)`; on reaching 0 it calls `FinishGame` (guarded by `_finishing` against the answer path also finishing).
- **`Components/App.razor`** is the root document; it references MudBlazor's `_content/MudBlazor/*` bundle and `_framework/blazor.web.js` with plain (non-fingerprinted) paths so they pass the strict CSP.
- **CSP matters here.** `SecurityHeadersMiddleware` enforces `default-src 'self'` (+ `style-src 'unsafe-inline'` for MudBlazor's injected theme styles). Keep UI assets same-origin — don't add CDN scripts/fonts (MudBlazor icons are inline SVG, so no icon font is needed). `Program.cs` requires `app.UseAntiforgery()` and `MapRazorComponents<App>().AddInteractiveServerRenderMode()`.

### Cross-cutting conventions — follow these when adding code

- **Result pattern, never exceptions for control flow.** Every service returns `FluentResults.Result<T>`. Failures are typed errors in `Application/Common/Errors/` (`NotFoundError`, `ConflictError`, `UnauthorizedError`, `ExternalServiceError`). `ResultExtensions.ToHttpResult()` ([ResultExtensions.cs](src/GbfsQuiz.Web/Common/Http/ResultExtensions.cs)) is the single place that maps error types → HTTP status (404/409/401/503, else 400). Add a new error type → add a case there. `GlobalExceptionMiddleware` is only for genuinely unexpected exceptions.
- **The quiz engine is a strategy set.** Each question type implements `IQuestionStrategy` (`CanGenerate(snapshots)` + `Generate(snapshots, Random)`) and is a pure function of its inputs. `QuizService` picks a random *eligible* strategy. **Adding a question = one new class in `Application/Features/Quiz/Strategies/` + one `AddScoped<IQuestionStrategy, ...>` line in [DependencyInjection.cs](src/GbfsQuiz.Application/DependencyInjection.cs).**
- **Answers graded server-side.** The correct choice never leaves the server: issued questions are stored in `IIssuedQuestionStore` (in-memory, singleton) keyed by question id; clients receive only choice ids and `QuizService.Grade` looks the answer back up.
- **GBFS data is fetched concurrently and cached ~45s** (`CachedGbfsSnapshotProvider`). Providers come from `GbfsProviderCatalog` (three hard-coded defaults, overridable via the `Gbfs:Providers` config section). A down provider is skipped; the game continues as long as one responds.
- **IDs are UUIDv7** via `Guid.CreateVersion7()` for time-ordered, index-friendly keys.
- **Endpoints extract the player id from the JWT** via `user.TryGetId(out var playerId)` ([CurrentPlayer.cs](src/GbfsQuiz.Web/Common/Security/CurrentPlayer.cs)) and return `Results.Unauthorized()` if absent — see [GameEndpoints.cs](src/GbfsQuiz.Web/Features/Games/GameEndpoints.cs) for the pattern.
- **Validation** uses FluentValidation via `AddEndpointFilter<ValidationFilter<T>>()`; request DTOs are explicit white-listed records (no mass assignment).
- **DI composition roots:** `AddApplication()` and `AddInfrastructure(config)` — register new services there, not in `Program.cs`.

### DI lifetimes (deliberate, keep consistent)

Stores/catalogs/hashers/token issuers are **singletons** (`IIssuedQuestionStore`, `IGbfsProviderCatalog`, `IPasswordHasher`, `ITokenIssuer`); per-request services and repositories are **scoped** (`IAuthService`, `IGameService`, `IQuizService`, the question strategies, repositories, `IGbfsSnapshotProvider`).

## Testing conventions

xUnit + Moq + FluentAssertions + AutoFixture, AAA style. Shared test helpers live in `tests/GbfsQuiz.Tests/Common/` (`SnapshotBuilder` for GBFS fixtures, `StubHttpMessageHandler` for HTTP). Strategies test cleanly because they're pure functions of `(snapshots, Random)`.

## Notes

- The UI is Blazor + MudBlazor (see the "Blazor UI" section above). `Program.cs` exposes `public partial class Program` for `WebApplicationFactory` integration tests. `wwwroot` now holds only `app.css` (global tweaks + the Blazor error UI); the old vanilla-JS SPA was removed.
- Solution file is `GbfsQuiz.slnx` (the new XML solution format), not a `.sln`.
- `Directory.Build.props` applies `net10.0`, nullable + implicit usings, to all projects. `TreatWarningsAsErrors` is **off**.
- **Secrets** (JWT signing key, Postgres connection string) are kept out of `appsettings.json`. Local dev reads them from **.NET user secrets** (`dotnet user-secrets set "Jwt:SigningKey" ...` / `"ConnectionStrings:Default" ...`, scoped to `GbfsQuiz.Web`'s `UserSecretsId`); containers/CI supply them as env vars (`Jwt__SigningKey`, `ConnectionStrings__Default` — see `docker-compose.yml` / `render.yaml`). If neither is set, the code falls back to a labeled dev-only signing key (`JwtOptions`) and a `localhost`/`postgres` connection string (`AddInfrastructure`), so `docker compose up` still boots.
