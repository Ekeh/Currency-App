# Currency Exchange Rate Dashboard

A full-stack application: a **.NET 9 Clean Architecture API** (JWT auth, validation, caching, rate limiting) and a **React + TypeScript client** (`client/`) that shows live currency exchange rates and converts amounts between currencies. The API also ships server-rendered MVC views.

| Layer | Stack |
|-------|-------|
| API | ASP.NET Core 9, EF Core, ASP.NET Identity, JWT, FluentValidation, SQL Server |
| Client | React 18, TypeScript, Vite, React Router, Vitest + Testing Library |

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Web (Controllers)                         │
│  • Lean controllers - receive request, call service, return │
└─────────────────────────────┬───────────────────────────────┘
                              │
┌─────────────────────────────▼───────────────────────────────┐
│                Application (Services)                        │
│  • ALL business logic                                        │
│  • FluentValidation                                          │
│  • Orchestrates repositories via UnitOfWork                  │
└─────────────────────────────┬───────────────────────────────┘
                              │
┌─────────────────────────────▼───────────────────────────────┐
│              Infrastructure (Repositories)                   │
│  • Generic Repository + Unit of Work                         │
│  • DbContext & Migrations                                    │
│  • External API clients                                      │
└─────────────────────────────┬───────────────────────────────┘
                              │
┌─────────────────────────────▼───────────────────────────────┐
│                    Core (Domain)                             │
│  • Entities & Interfaces                                     │
│  • DTOs (Request/Response)                                   │
│  • No external dependencies                                  │
└─────────────────────────────────────────────────────────────┘
```

## Project Structure

```
src/
├── CurrencyExchangeApp.Core/           # Domain Layer
│   ├── Entities/                        # Domain entities
│   ├── Interfaces/                      # IRepository, IUnitOfWork, IServices
│   └── DTOs/                            # Request & Response models
│
├── CurrencyExchangeApp.Application/    # Application Layer
│   ├── Services/                        # Business logic (ExchangeRateService)
│   └── Validators/                      # FluentValidation validators
│
├── CurrencyExchangeApp.Infrastructure/ # Infrastructure Layer
│   ├── Data/                            # DbContext, Migrations
│   ├── Repositories/                    # Repository & UnitOfWork implementations
│   └── External/                        # External API client
│
└── CurrencyExchangeApp.Web/            # Presentation Layer
    ├── Controllers/                     # Lean MVC & API controllers
    ├── Models/ViewModels/               # View-specific models
    └── Views/                           # Razor views
```

## Features

- **Clean Architecture** with separation of concerns
- **Unit of Work** pattern for transaction management
- **Generic Repository** pattern for data access
- **FluentValidation** for request validation
- **Lean Controllers** - no business logic, just orchestration
- **ASP.NET Core Identity** for authentication
- **Real-time exchange rates** from ExchangeRate-API
- **NGN (Nigerian Naira)** as default base currency
- **Swagger/OpenAPI** documentation

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (Docker or local)
- Free API key from [ExchangeRate-API](https://www.exchangerate-api.com/)

## Quick Start

1. **Clone and restore**
   ```bash
   git clone <repository-url>
   cd Currency-App
   dotnet restore
   ```

2. **Configure database** (appsettings.json in Web project)
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=CurrencyExchangeDb;User Id=sa;Password=YourPassword;TrustServerCertificate=true"
   }
   ```

3. **Configure API key**
   ```json
   "ExchangeRateApi": {
     "ApiKey": "your-api-key-here"
   }
   ```

4. **Configure the JWT signing key.** The API refuses to start without a real key (no built-in default). Keep it out of source control:
   ```bash
   dotnet user-secrets init --project src/CurrencyExchangeApp.Web
   dotnet user-secrets set "Jwt:SecretKey" "<random string, 32+ characters>" --project src/CurrencyExchangeApp.Web
   ```
   In other environments set the `Jwt__SecretKey` environment variable.

5. **Run migrations**
   ```bash
   dotnet ef database update \
     --project src/CurrencyExchangeApp.Infrastructure \
     --startup-project src/CurrencyExchangeApp.Web
   ```

6. **Run the application** (the `http` profile serves on port 5283, which the client proxies to)
   ```bash
   dotnet run --project src/CurrencyExchangeApp.Web --launch-profile http
   ```

7. **Open in browser**
   - Web App: http://localhost:5283
   - Swagger: http://localhost:5283/swagger

## React Client (`client/`)

A single-page app that consumes the JSON API with JWT authentication.

**Features**
- Register / sign in, with client-side checks that mirror the server's validation rules (the server's messages are still shown)
- Protected dashboard: pick a base currency, filter and sort the rates table, refresh on demand
- Converter: amount + currency pair, swap button, uses the API's pair endpoint
- Loading, empty and error states; accessible labels, `aria-live` results and sortable column headers
- Session kept in `sessionStorage` and dropped when expired; any `401` from an authenticated call signs the user out

**Run it** (API running on http://localhost:5283 with the `http` profile)
```bash
cd client
npm install
npm run dev          # http://localhost:5173, proxies /api to the API (no CORS setup needed)
```
Set `VITE_API_PROXY_TARGET` (see `client/.env.example`) if the API listens elsewhere.

**Quality checks**
```bash
npm run typecheck
npm test             # Vitest + Testing Library
npm run build
```

**Design notes**
- `src/api/client.ts` is the only place that calls `fetch`. It attaches the bearer token, normalises the API's `{ errors: [...] }` / `{ error: "..." }` bodies into one `ApiError`, and gives friendly messages for `429` and network failures.
- Stale responses are discarded with `AbortController`, so switching the base currency quickly cannot show out-of-date rates.
- The token lives in `sessionStorage` for simplicity. A hardened production setup would use an `httpOnly` cookie instead to remove XSS exposure.

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/ExchangeRatesApi` | Get all rates (default: NGN) |
| GET | `/api/ExchangeRatesApi?baseCurrency=USD` | Get rates for specific base |
| GET | `/api/ExchangeRatesApi/{from}/{to}` | Get specific currency pair |
| GET | `/api/ExchangeRatesApi/currencies` | List supported currencies |
| POST | `/api/Auth/login` | Sign in, returns a JWT |
| POST | `/api/Auth/register` | Create an account, returns a JWT |
| GET | `/api/Auth/me` | Current user (requires JWT) |

Rate-limited endpoints return `429 Too Many Requests` once the configured window is exceeded (see `RateLimiting` in `appsettings.json`).

## Supported Currencies

NGN, USD, EUR, GBP, JPY, CAD, AUD, CHF

## Technologies

- ASP.NET Core 9.0 MVC
- Entity Framework Core 9.0
- ASP.NET Core Identity
- FluentValidation 11
- Swashbuckle (Swagger)
- SQL Server
- React 18, TypeScript, Vite, Vitest (client)

## License

MIT License
