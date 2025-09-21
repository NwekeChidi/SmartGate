# Development Guide

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Docker Desktop
- PowerShell 7+ (for scripts)
- IDE (Visual Studio, VS Code, Rider)

### Initial Setup
```bash
git clone <repository-url>
cd smartgate
.\scripts\start-smartgate.ps1 -Env dev
```

## Architecture

### Clean Architecture Layers
```
┌─────────────────────────────────────┐
│           SmartGate.Api             │  ← Controllers, Auth, Error Handling
├─────────────────────────────────────┤
│        SmartGate.Application        │  ← Services, DTOs, Validators
├─────────────────────────────────────┤
│          SmartGate.Domain           │  ← Entities, Business Rules, Events
├─────────────────────────────────────┤
│       SmartGate.Infrastructure      │  ← Database, Repositories, External
└─────────────────────────────────────┘
```

### Dependency Rules
- **Inward Dependencies Only**: Outer layers depend on inner layers
- **Domain Independence**: Domain layer has no external dependencies
- **Interface Segregation**: Use abstractions for cross-layer communication

## Development Workflow

### 1. Feature Development
```bash
# Create feature branch
git checkout -b feature/new-feature

# Run tests continuously
dotnet watch test

# Make changes following TDD approach
# 1. Write failing test
# 2. Implement minimum code to pass
# 3. Refactor
```

### 2. Testing Strategy
```bash
# Run all tests with coverage
.\scripts\test-coverage.ps1

# Run specific test project
dotnet test tests/SmartGate.Domain.Tests/

# Run tests with filter
dotnet test --filter "Category=Unit"
```

### 3. Database Changes
```bash
# Add migration
dotnet ef migrations add <MigrationName> --project src/SmartGate.Infrastructure

# Update database
dotnet ef database update --project src/SmartGate.Infrastructure

# Remove last migration (if not applied)
dotnet ef migrations remove --project src/SmartGate.Infrastructure
```

## Code Standards

### Naming Conventions
- **Classes**: PascalCase (`VisitService`)
- **Methods**: PascalCase (`CreateVisitAsync`)
- **Properties**: PascalCase (`FirstName`)
- **Fields**: camelCase with underscore (`_visitRepository`)
- **Constants**: PascalCase (`MaxNameLength`)

### File Organization
```
Feature/
├── Entities/           # Domain entities
├── Events/            # Domain events  
├── Exceptions/        # Domain exceptions
├── Dto/              # Data transfer objects
├── Validators/       # Input validation
├── Ports/           # Interfaces/abstractions
└── Services/        # Application services
```

### Error Handling
- **Domain Exceptions**: For business rule violations
- **Validation**: Use FluentValidation for input validation
- **Problem Details**: Consistent error responses via RFC 7807

## Testing Guidelines

### Test Structure
```csharp
[Fact]
public void Should_ThrowException_When_InvalidInput()
{
    // Arrange
    var invalidInput = "invalid";
    
    // Act & Assert
    var exception = Assert.Throws<DomainException>(() => 
        new Driver("John", "Doe", invalidInput));
    
    Assert.Contains("DFDS-", exception.Message);
}
```

### Test Categories
- **Unit Tests**: Domain logic, validators, services
- **Integration Tests**: Database operations, API endpoints
- **Contract Tests**: API request/response validation

### Test Data Builders
```csharp
public class VisitBuilder
{
    private string _licensePlate = "ABC123";
    private Driver _driver = new("John", "Doe", "DFDS-12345");
    
    public VisitBuilder WithLicensePlate(string plate)
    {
        _licensePlate = plate;
        return this;
    }
    
    public Visit Build() => new(_truck, _driver, _activities, "test-user");
}
```

## Database Development

### Entity Configuration
```csharp
public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Status).HasConversion<string>();
        builder.OwnsOne(v => v.Truck);
        builder.OwnsOne(v => v.Driver);
    }
}
```

### Repository Pattern
```csharp
public interface IVisitRepository
{
    Task<Visit?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PaginatedResult<Visit>> ListAsync(int page, int pageSize, CancellationToken ct = default);
    Task SaveAsync(Visit visit, CancellationToken ct = default);
}
```

## API Development

### Controller Guidelines
- Use `IResult` return types
- Handle exceptions with `ToProblem()` extension
- Apply authorization policies
- Use cancellation tokens

### Request/Response Models
```csharp
public sealed record CreateVisitRequest(
    string TruckLicensePlate,
    DriverDto Driver,
    IReadOnlyList<ActivityDto> Activities,
    VisitStatus Status,
    Guid? IdempotencyKey
);
```

### Validation
```csharp
public class CreateVisitRequestValidator : AbstractValidator<CreateVisitRequest>
{
    public CreateVisitRequestValidator()
    {
        RuleFor(x => x.TruckLicensePlate)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(32);
    }
}
```

## Debugging

### Local Development
- Use `appsettings.Development.json` for dev settings
- Enable detailed errors and sensitive data logging
- Use development authentication (no JWT required)

### Database Debugging
```bash
# Connect to local PostgreSQL
docker exec -it smartgate-db psql -U postgres -d smartgate

# View tables
\dt

# Query visits
SELECT * FROM "Visits" LIMIT 10;
```

### Logging
```csharp
private readonly ILogger<VisitService> _logger;

public async Task<VisitResponse> CreateVisitAsync(CreateVisitRequest request, CancellationToken ct)
{
    _logger.LogInformation("Creating visit for truck {LicensePlate}", request.TruckLicensePlate);
    // Implementation
}
```

## Performance Considerations

### Database
- Use indexes for frequently queried columns
- Implement pagination for large result sets
- Use `AsNoTracking()` for read-only queries
- Consider connection pooling

### API
- Implement rate limiting
- Use caching for static data
- Optimize serialization
- Monitor response times

### Caching
- **List Visits Caching**: In-memory cache with 2-minute TTL
- **Cache Invalidation**: Automatic on data mutations (create/update)
- **Cache Keys**: Pattern-based (`visits_list_{page}_{pageSize}`)
- **Memory Usage**: Monitor cache size in production

## Security

### Authentication
- Development: `UseDevAuth = true`
- Production: JWT Bearer tokens with proper validation

### Authorization
- Scope-based permissions (`visits:read`, `visits:write`)
- Policy-based authorization
- Principle of least privilege

### Input Validation
- Server-side validation always required
- Sanitize inputs through normalization
- Validate business rules in domain layer

## Troubleshooting

### Common Issues
1. **Migration Errors**: Check database connection and permissions
2. **Test Failures**: Ensure test database is clean
3. **JWT Issues**: Verify signing key and token format
4. **Docker Issues**: Check container status and logs

### Debug Commands
```bash
# Check Docker containers
docker ps -a

# View application logs
docker logs smartgate-api

# Check database connectivity
docker exec smartgate-db pg_isready -U postgres

# View EF migrations
dotnet ef migrations list --project src/SmartGate.Infrastructure
```