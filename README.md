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
├── src/                           # Source code
│   ├── SmartGate.Api/            # Web API layer
│   │   ├── Auth/                 # Authentication handlers and policies
│   │   ├── Controllers/          # API controllers
│   │   ├── ErrorHandling/        # Error handling and problem details
│   │   └── Program.cs            # Application entry point
│   ├── SmartGate.Application/    # Application layer
│   │   ├── Abstractions/         # Interfaces and contracts
│   │   ├── Visits/              # Visit-related services and DTOs
│   │   │   ├── Dto/             # Data transfer objects
│   │   │   ├── Validators/      # Request validators
│   │   │   └── VisitService.cs  # Core business logic
│   ├── SmartGate.Domain/         # Domain layer
│   │   ├── Common/              # Shared domain concepts
│   │   └── Visits/              # Visit domain entities and logic
│   │       ├── Entities/        # Domain entities (Visit, Driver, etc.)
│   │       ├── Events/          # Domain events
│   │       ├── Enums.cs         # Domain enumerations
│   │       └── Exceptions.cs    # Domain exceptions
│   └── SmartGate.Infrastructure/ # Infrastructure layer
│       ├── Database/            # EF Core DbContext and configuration
│       ├── Migrations/          # Database migrations
│       └── Repositories/        # Data access implementations
├── tests/                        # Test projects
│   ├── SmartGate.Api.Tests/     # API integration tests
│   ├── SmartGate.Application.Tests/ # Application layer tests
│   └── SmartGate.Domain.Tests/  # Domain unit tests
├── scripts/                      # PowerShell automation scripts
├── docs/                        # Documentation
├── coverage/                    # Test coverage reports
└── docker-compose.yml          # PostgreSQL container setup
```

## Core Functionality

### Visit Management
- **Create Visits**: Register new truck visits with driver information and activities
- **Update Visit Status**: Progress visits through different stages (PreRegistered → AtGate → OnSite → Completed)
- **List Visits**: Paginated retrieval of visit records with in-memory caching (2-minute TTL)
- **Activity Tracking**: Support for Delivery and Collection activities

### Domain Entities
- **Visit**: Core aggregate containing truck, driver, and activity information
- **Driver**: Driver entity with DFDS-specific ID validation (format: DFDS-{1-11 digits})
- **Truck**: Truck information with license plate validation
- **Activity**: Individual activities (Delivery/Collection) with unit numbers

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
- Docker (for PostgreSQL)
- PowerShell (for scripts)

### Quick Start

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd smartgate
   ```

2. **Start the application (Development)**
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

### Visits Controller (`/v1/visits`)

- **POST** `/v1/visits/create` - Create new visit
- **GET** `/v1/visits` - List visits (paginated)
- **PATCH** `/v1/visits/status_update/{id}` - Update visit status

### Health Check
- **GET** `/health` - Application health status

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

### Production Configuration
- Set `Auth:UseDevAuth` to `false`
- Configure `Jwt:SigningKey`, `Jwt:Authority`, `Jwt:Audience`
- Update connection strings for production database

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