# SmartGate

A .NET 8 Web API for managing truck visits and gate operations, built with Clean Architecture principles.

## Overview

SmartGate is a visit management system designed for tracking truck visits through different stages of gate operations. It provides REST API endpoints for creating, updating, and querying visit records with proper authentication and authorization.

## Technology Stack

- **.NET Version**: 8.0
- **C# Version**: 12.0 (implicit with .NET 8)
- **Database**: PostgreSQL 16
- **ORM**: Entity Framework Core 8.0.20
- **Authentication**: JWT Bearer tokens
- **Validation**: FluentValidation 11.*
- **Testing**: xUnit with NSubstitute for comprehensive unit tests
- **Documentation**: Swagger/OpenAPI

## Project Structure

```
smartgate/
├── src/
│   ├── SmartGate.Api/
│   │   ├── Auth/
│   │   │   ├── DevAuthHandler.cs
│   │   │   ├── Policies.cs
│   │   │   └── UserContext.cs
│   │   ├── Common/
│   │   │   └── AppConstants.cs
│   │   ├── Controllers/
│   │   │   └── VisitsController.cs
│   │   ├── ErrorHandling/
│   │   │   ├── FlatErrorsProblemDetailsFactory.cs
│   │   │   ├── ProblemDetailsExtensions.cs
│   │   │   └── StringTryParseConverterFactory.cs
│   │   ├── Extensions/
│   │   │   ├── ServiceCollectionExtensions.cs
│   │   │   └── WebApplicationExtensions.cs
│   │   ├── Properties/
│   │   │   └── launchSettings.json
│   │   ├── appsettings.Development.json
│   │   ├── appsettings.json
│   │   ├── appsettings.Production.json
│   │   ├── appsettings.Uat.json
│   │   ├── Program.cs
│   │   ├── SmartGate.Api.csproj
│   │   └── SmartGate.Api.http
│   ├── SmartGate.Application/
│   │   ├── Abstractions/
│   │   │   ├── DuplicateRequestException.cs
│   │   │   ├── IClock.cs
│   │   │   ├── IIdempotencyStore.cs
│   │   │   ├── IPiiPolicy.cs
│   │   │   ├── IUserContext.cs
│   │   │   └── IVisitService.cs
│   │   ├── Visits/
│   │   │   ├── Dto/
│   │   │   ├── Ports/
│   │   │   ├── Validators/
│   │   │   └── VisitService.cs
│   │   └── SmartGate.Application.csproj
│   ├── SmartGate.Domain/
│   │   ├── Common/
│   │   │   ├── AggregateRoot.cs
│   │   │   ├── DomainConstants.cs
│   │   │   ├── DomainException.cs
│   │   │   ├── IDomainEvent.cs
│   │   │   └── Normalization.cs
│   │   ├── Visits/
│   │   │   ├── Entities/
│   │   │   ├── Events/
│   │   │   ├── Enums.cs
│   │   │   └── Exceptions.cs
│   │   └── SmartGate.Domain.csproj
│   └── SmartGate.Infrastructure/
│       ├── Database/
│       │   ├── Setup/
│       │   ├── DesignTimeDBContextFactory.cs
│       │   └── SmartGateDBContext.cs
│       ├── Migrations/
│       ├── Repositories/
│       │   ├── DriverRepository.cs
│       │   ├── IdempotencyStore.cs
│       │   └── VisitRepository.cs
│       └── SmartGate.Infrastructure.csproj
├── tests/
│   ├── SmartGate.Api.Tests/
│   │   ├── Auth/
│   │   ├── Controllers/
│   │   ├── ErrorHandling/
│   │   └── SmartGate.Api.Tests.csproj
│   ├── SmartGate.Application.Tests/
│   │   ├── Dto/
│   │   ├── Visits/
│   │   ├── SmartGate.Application.Tests.csproj
│   │   └── TestHelpers.cs
│   └── SmartGate.Domain.Tests/
│       ├── Common/
│       ├── Visits/
│       ├── SmartGate.Domain.Tests.csproj
│       └── TestDataBuilders.cs
├── scripts/
│   ├── generate-token.ps1
│   ├── start-smartgate.ps1
│   └── test-coverage.ps1
├── docs/
│   ├── adrs/
│   ├── api-reference.md
│   ├── deployment-guide.md
│   ├── development-guide.md
│   ├── domain-model.md
│   ├── README.md
│   └── VALIDATION-RULES.md
├── coverage/
├── TestResults/
├── API-CONTRACTS.md
├── docker-compose.yml
├── README.md
└── SmartGate.sln
```

## Core Functionality

### Visit Management
- **Create Visits**: Register new truck visits with driver information and activities
- **Update Visit Status**: Progress visits through different stages (PreRegistered → AtGate → OnSite → Completed)
- **List Visits**: Paginated retrieval of visit records with in-memory caching (2-minute TTL)
- **Activity Tracking**: Support for Delivery and Collection activities

### Domain Entities
- **Visit**: Core aggregate containing truck, driver, and activity information
- **Driver**: Driver entity with DFDS-specific ID validation (format: DFDS-{11 digits}, exactly 16 characters)
- **Truck**: Truck information with license plate validation (exactly 7 characters after normalization)
- **Activity**: Individual activities (Delivery/Collection) with unit numbers (exactly 10 characters, pattern: DFDS<6 numeric characters>)

### Visit Status Flow
```
PreRegistered → AtGate → OnSite → Completed
```

### Authentication & Authorization
- **Development Mode**: Simplified auth for local development
- **Production Mode**: JWT Bearer token authentication
- **Scopes**: `visits:read`, `visits:write`, `admin:manage`

### Performance Features
- **List Caching**: Visit list results cached for 2 minutes to improve response times
- **Cache Invalidation**: Automatic cache clearing when visits are created or updated
- **Rate Limiting**: 120 requests per minute per client

## Dependencies

### Core Dependencies
- `Microsoft.AspNetCore.OpenApi` (8.0.20)
- `Microsoft.EntityFrameworkCore` (8.0.20)
- `Npgsql.EntityFrameworkCore.PostgreSQL` (8.*)
- `Microsoft.AspNetCore.Authentication.JwtBearer` (8.*)
- `Microsoft.Extensions.Caching.Memory` (8.*)
- `FluentValidation` (11.*)
- `Swashbuckle.AspNetCore` (6.*)

### Development Dependencies
- `Microsoft.EntityFrameworkCore.Design` (8.0.20)
- Various testing packages (xUnit, Moq, etc.)

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Docker (for PostgreSQL) - **Docker daemon must be running**
- PowerShell (for scripts)

### Quick Start

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd smartgate
   ```

2. **Ensure Docker is running**
   - Start Docker Desktop or Docker daemon
   - Verify with: `docker --version`

3. **Start the application (Development)**
   ```powershell
   .\scripts\start-smartgate.ps1 -Env dev
   ```

3. **Access the API**
   - API: `https://localhost:5001`
   - Swagger UI: `https://localhost:5001/swagger`

### Manual Setup

1. **Start PostgreSQL**
   ```bash
   docker compose up -d
   ```

2. **Apply database migrations**
   ```bash
   dotnet ef database update --project src/SmartGate.Infrastructure
   ```

3. **Run the API**
   ```bash
   dotnet run --project src/SmartGate.Api
   ```

## Scripts Usage

### `start-smartgate.ps1`
Comprehensive startup script that handles environment setup, testing, database, and API startup.

```powershell
# Development environment (default)
.\scripts\start-smartgate.ps1

# UAT environment
.\scripts\start-smartgate.ps1 -Env uat

# Production environment
.\scripts\start-smartgate.ps1 -Env production
```

**Features:**
- Validates prerequisites (Docker, .NET SDK, dotnet-ef)
- Runs test coverage
- Starts PostgreSQL container
- Applies database migrations
- Configures environment-specific settings
- Starts API with hot reload (dev) or standard mode (uat/prod)

### `generate-token.ps1`
Generates JWT tokens for API authentication.

```powershell
# Generate read-only token
.\scripts\generate-token.ps1 -Permission read -Principal "user@company.com"

# Generate write token
.\scripts\generate-token.ps1 -Permission write -Principal "admin@company.com"

# Generate admin token
.\scripts\generate-token.ps1 -Permission admin -Principal "super.admin@company.com"
```

### `test-coverage.ps1`
Runs tests and generates coverage reports.

```powershell
# Run with default settings
.\scripts\test-coverage.ps1

# Custom output directories
.\scripts\test-coverage.ps1 -ResultsDir "MyResults" -CoverageOutDir "MyCoverage"
```

## API Endpoints

For detailed API contracts with comprehensive request/response examples and validation scenarios, see [API-CONTRACTS.md](API-CONTRACTS.md).

### Visits Controller (`/v1/visits`)

- **POST** `/v1/visits/create` - Create new visit (requires `visits:write` scope)
- **GET** `/v1/visits` - List visits with pagination (requires `visits:read` scope)
- **PATCH** `/v1/visits/status_update/{id}` - Update visit status (requires `visits:write` scope)

### Health Check
- **GET** `/health` - Application health status (no authentication required)

### Authentication & Authorization
- **Development Mode**: No authentication required when `Auth:UseDevAuth = true`
- **Production/UAT Mode**: JWT Bearer token authentication with scope-based authorization
- **Scopes**: `visits:read` for read operations, `visits:write` for create/update operations

## Configuration

### Development (`appsettings.Development.json`)
```json
{
  "Auth": {
    "UseDevAuth": true
  },
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=15432;Database=smartgate;Username=postgres;Password=postgres"
  }
}
```

### Production/UAT Configuration
- Set `Auth:UseDevAuth` to `false`
- Configure `Jwt:SigningKey`, `Jwt:Authority`, `Jwt:Audience`
- Update connection strings for production database
- Enable rate limiting (120 requests per minute per client)
- Configure proper logging levels

## Testing

The project includes comprehensive test coverage across all layers:

- **Domain Tests**: Unit tests for business logic and domain rules
- **Application Tests**: Service layer and validation tests
- **API Tests**: Integration tests for controllers and endpoints

Run tests with coverage:
```powershell
.\scripts\test-coverage.ps1
```

View coverage report: `coverage/index.html`

## Development

### Adding New Features
1. Start with domain entities and business rules
2. Add application services and DTOs
3. Implement infrastructure (repositories, database)
4. Create API controllers and endpoints
5. Add comprehensive tests at each layer

### Database Migrations
```bash
# Add new migration
dotnet ef migrations add <MigrationName> --project src/SmartGate.Infrastructure

# Update database
dotnet ef database update --project src/SmartGate.Infrastructure
```

## Architecture

The project follows Clean Architecture principles:

- **Domain Layer**: Core business logic, entities, and rules
- **Application Layer**: Use cases, services, and DTOs
- **Infrastructure Layer**: Data access, external services
- **API Layer**: Controllers, authentication, and presentation logic

This ensures separation of concerns, testability, and maintainability.