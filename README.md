# SmartGate

A .NET 8 Web API for managing truck visits and gate operations, built with Clean Architecture principles.

## Overview

SmartGate is a visit management system designed for tracking truck visits through different stages of gate operations. It provides REST API endpoints for creating, updating, and querying visit records with proper authentication and authorization.

## Assumptions

* **Truck plates:** trim, uppercase, strip non-alphanumeric; must normalize to **exactly 7 chars**.
* **Unit numbers:** trim, uppercase, strip non-alphanumeric; must be **`DFDS` + 6 digits** (normalize to **10 chars**).
* **Driver ID:** case-insensitive; must be **`DFDS-` + 11 digits** (**16 chars total**); stored uppercased.
* **Driver names:** required, trimmed, max **128** chars each.
* **Activities:** at least **one** required per visit; only **Delivery** or **Collection** allowed.
* **Initial status:** new visits must start as **PreRegistered**.
* **Status flow:** linear **PreRegistered → AtGate → OnSite → Completed**; no skips/backwards; **Completed** is terminal; setting same status is a no-op.
* **Idempotency:** optional GUID; duplicates (same key) are rejected.
* **Pagination:** default **page=1**, **pageSize=20**; pageSize **capped at 200**; invalid values fall back to defaults.
* **Caching:** list endpoint cached **in-memory for 2 minutes**; only common keys invalidated → other pages can be stale up to 2 min.
* **Rate limiting:** **120 requests/min per client** (production: 300).
* **Auth:** dev mode may bypass auth; prod/UAT require **JWT** with scopes (**visits:read / visits:write / admin:manage**).
* **Driver reuse:** driver looked up by normalized ID; if missing, created with sanitized names.
* **Normalization policy:** original formatting (spaces, dashes) isn't stored—normalized values are persisted and returned.
* **Notification:** Assummed status update might require notification updates hence implemented a ready to integrate notification system for status update with services like SNS

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
│   │   ├── Common/
│   │   ├── Controllers/
│   │   ├── ErrorHandling/
│   │   ├── Extensions/
│   │   └── Properties/
│   ├── SmartGate.Application/
│   │   ├── Abstractions/
│   │   └── Visits/
│   │       ├── Dto/
│   │       ├── Ports/
│   │       └── Validators/
│   ├── SmartGate.Domain/
│   │   ├── Common/
│   │   └── Visits/
│   │       ├── Entities/
│   │       └── Events/
│   └── SmartGate.Infrastructure/
│       ├── Database/
│       │   └── Setup/
│       ├── Migrations/
│       └── Repositories/
├── tests/
│   ├── SmartGate.Api.Tests/
│   │   ├── Auth/
│   │   ├── Controllers/
│   │   └── ErrorHandling/
│   ├── SmartGate.Application.Tests/
│   │   ├── Dto/
│   │   └── Visits/
│   │       └── Validators/
│   └── SmartGate.Domain.Tests/
│       ├── Common/
│       └── Visits/
│           ├── Entities/
│           └── Events/
├── scripts/
├── docs/
    └── adrs/
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
- Various testing packages (xUnit with NSubstitute)

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
