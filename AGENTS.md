# AGENTS.md

This repository is a .NET 9 solution following Clean Architecture.
Use this guide for consistent commands, style, and conventions.

## Quick Context
- Solution: `CurrencyExchangeApp.sln`
- Layers: Core (domain), Application (business), Infrastructure (data), Web (API/UI)
- Tests: `src/CurrencyExchangeApp.Tests`
- Nullable reference types: enabled
- Implicit usings: enabled

## Build / Run / Test Commands
Run commands from the repo root unless stated otherwise.

### Restore
- `dotnet restore`

### Build
- `dotnet build`
- `dotnet build CurrencyExchangeApp.sln`

### Run Web App
- `dotnet run --project src/CurrencyExchangeApp.Web`

### Database Migrations
- `dotnet ef database update --project src/CurrencyExchangeApp.Infrastructure --startup-project src/CurrencyExchangeApp.Web`

### Test (All)
- `dotnet test`
- `dotnet test CurrencyExchangeApp.sln`

### Test (Single Project)
- `dotnet test src/CurrencyExchangeApp.Tests/CurrencyExchangeApp.Tests.csproj`

### Test (Single Class)
- `dotnet test src/CurrencyExchangeApp.Tests/CurrencyExchangeApp.Tests.csproj --filter "FullyQualifiedName~ExchangeRateServiceTests"`

### Test (Single Test Method)
- `dotnet test src/CurrencyExchangeApp.Tests/CurrencyExchangeApp.Tests.csproj --filter "FullyQualifiedName=CurrencyExchangeApp.Tests.Application.Services.ExchangeRateServiceTests.GetExchangeRatesAsync_WithValidRequest_ReturnsCachedRates"`

### Test (Trait/Category)
- `dotnet test src/CurrencyExchangeApp.Tests/CurrencyExchangeApp.Tests.csproj --filter "Category=Integration"`
  - Only if traits are present in tests.

### Coverage
- Coverage is enabled via Coverlet in the test project.
- Use `dotnet test` as usual; coverage output defaults to `cobertura`.

### Lint / Format
- No explicit linting tool is configured.
- If `dotnet format` is available locally, use `dotnet format`.
- Do not add new tooling unless requested.

## Cursor / Copilot Rules
- No `.cursor/rules`, `.cursorrules`, or `.github/copilot-instructions.md` found.
- If any are added later, follow them.

## Code Style Guidelines
Follow existing style in the repo; keep diffs minimal.

### General C# Conventions
- Use file-scoped namespaces (`namespace Foo.Bar;`).
- Prefer `var` when the type is obvious.
- Use `readonly` fields and constructor injection for dependencies.
- Keep constructors simple; use expression-bodied members for trivial assignments.
- Keep methods small and focused; avoid unrelated changes.
- Avoid adding comments unless explicitly requested.

### Naming
- Public types/members: `PascalCase`.
- Private fields: `_camelCase` with underscore prefix.
- Local variables/parameters: `camelCase`.
- Async methods: suffix with `Async`.
- DTOs and request/response models: `PascalCase` + `Request`/`Response` suffix.

### Imports / Usings
- Use standard `using` directives at top of file.
- Rely on implicit usings where appropriate.
- Group by framework first, then third-party, then local namespaces.
- Remove unused `using` directives.

### Formatting
- Indentation: 4 spaces.
- One class per file.
- Brace style: opening brace on new line.
- Keep argument lists aligned with existing patterns.
- Keep object initializers and LINQ chains readable.

### Types & Nullability
- Nullable reference types are enabled; respect nullability warnings.
- Use `string.Empty` for default strings.
- Use nullable annotations (`?`) when values may be absent.
- Prefer explicit types when nullability could be ambiguous.

### Error Handling
- Prefer returning `ServiceResult<T>` from services.
- Use FluentValidation for request validation and surface validation errors.
- For API controllers, return `BadRequest` with validation errors and `NotFound` when appropriate.
- Log meaningful events with `ILogger` (information for expected paths, error for failures).

### Async / Await
- Use async all the way; avoid `.Result` or `.Wait()`.
- Prefer `Task`/`Task<T>` return types.
- When adding new async methods, ensure naming and signatures are consistent.

### Dependency Injection
- Register services in `src/CurrencyExchangeApp.Web/Program.cs`.
- Prefer interface-based injections (e.g., `IUnitOfWork`).
- Keep DI registrations grouped by layer (Infrastructure, Application, Web).

### API / Controllers
- Controllers should stay lean and delegate to services.
- Use `ApiController` attributes for API controllers.
- Prefer `FromQuery`/`FromRoute` as in existing endpoints.
- Keep responses consistent with existing patterns.

### Data Access
- Use repositories and `UnitOfWork` abstractions.
- Keep EF Core entity configuration in `ApplicationDbContext`.
- Avoid direct DbContext usage in higher layers.

### Testing
- Use xUnit + FluentAssertions + Moq.
- Arrange/Act/Assert formatting like existing tests.
- Use in-memory EF Core for repository tests when needed.
- Favor deterministic tests; avoid external API calls.

## Repo Layout Reference
- `src/CurrencyExchangeApp.Core`: domain entities, interfaces, DTOs
- `src/CurrencyExchangeApp.Application`: services, validators
- `src/CurrencyExchangeApp.Infrastructure`: data access, repositories, external clients
- `src/CurrencyExchangeApp.Web`: MVC/API presentation layer
- `src/CurrencyExchangeApp.Tests`: xUnit test suite

## Guardrails for Agents
- Keep changes scoped to the requested task.
- Update or add tests only when needed.
- Do not introduce new dependencies without approval.
- Respect existing architecture boundaries.
- Prefer small, reviewable diffs.
