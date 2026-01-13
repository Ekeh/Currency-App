# Currency Exchange Rate Dashboard

A .NET 9 web application built with **Clean Architecture** that displays real-time currency exchange rates with user authentication.

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

4. **Run migrations**
   ```bash
   dotnet ef database update \
     --project src/CurrencyExchangeApp.Infrastructure \
     --startup-project src/CurrencyExchangeApp.Web
   ```

5. **Run the application**
   ```bash
   dotnet run --project src/CurrencyExchangeApp.Web
   ```

6. **Open in browser**
   - Web App: http://localhost:5283
   - Swagger: http://localhost:5283/swagger

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/ExchangeRatesApi` | Get all rates (default: NGN) |
| GET | `/api/ExchangeRatesApi?baseCurrency=USD` | Get rates for specific base |
| GET | `/api/ExchangeRatesApi/{from}/{to}` | Get specific currency pair |
| GET | `/api/ExchangeRatesApi/currencies` | List supported currencies |

## Supported Currencies

NGN, USD, EUR, GBP, JPY, CAD, AUD, CHF

## Technologies

- ASP.NET Core 9.0 MVC
- Entity Framework Core 9.0
- ASP.NET Core Identity
- FluentValidation 11
- Swashbuckle (Swagger)
- SQL Server

## License

MIT License
