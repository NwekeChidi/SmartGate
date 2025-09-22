# Deployment Guide

## Prerequisites

### System Requirements
- .NET 8.0 Runtime
- PostgreSQL 16+
- Reverse proxy (nginx/IIS) for production
- SSL certificate for HTTPS

### Environment Variables
```bash
# Database
ConnectionStrings__Postgres="Host=<host>;Port=<port>;Database=<db>;Username=<user>;Password=<pass>"

# Authentication (Production)
Auth__UseDevAuth=false
Jwt__Authority=<issuer>
Jwt__Audience=SmartGate.Api
Jwt__SigningKey=<256-bit-key>

# Optional
RateLimiting__PermitPerMinute=120
ASPNETCORE_ENVIRONMENT=Production
```

## Database Setup

### Create Database
```sql
CREATE DATABASE smartgate_prod;
CREATE USER smartgate_user WITH PASSWORD '<secure-password>';
GRANT ALL PRIVILEGES ON DATABASE smartgate_prod TO smartgate_user;
```

### Apply Migrations
```bash
dotnet ef database update --project src/SmartGate.Infrastructure --connection "<connection-string>"
```

## Deployment Options

### Option 1: Docker Deployment

**Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/SmartGate.Api/SmartGate.Api.csproj", "src/SmartGate.Api/"]
COPY ["src/SmartGate.Application/SmartGate.Application.csproj", "src/SmartGate.Application/"]
COPY ["src/SmartGate.Domain/SmartGate.Domain.csproj", "src/SmartGate.Domain/"]
COPY ["src/SmartGate.Infrastructure/SmartGate.Infrastructure.csproj", "src/SmartGate.Infrastructure/"]
RUN dotnet restore "src/SmartGate.Api/SmartGate.Api.csproj"
COPY . .
WORKDIR "/src/src/SmartGate.Api"
RUN dotnet build "SmartGate.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SmartGate.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartGate.Api.dll"]
```

**Build and Run:**
```bash
docker build -t smartgate-api .
docker run -d -p 8080:80 --name smartgate-api \
  -e ConnectionStrings__Postgres="<connection-string>" \
  -e Auth__UseDevAuth=false \
  -e Jwt__SigningKey="<signing-key>" \
  smartgate-api
```

### Option 2: Direct Deployment

**Build:**
```bash
dotnet publish src/SmartGate.Api/SmartGate.Api.csproj -c Release -o ./publish
```

**Run:**
```bash
cd publish
dotnet SmartGate.Api.dll
```

### Option 3: IIS Deployment (Windows)

1. Install ASP.NET Core Hosting Bundle
2. Create IIS application
3. Copy published files to wwwroot
4. Configure application pool (.NET CLR Version: No Managed Code)
5. Set environment variables in web.config

## Configuration

### Production appsettings.json
```json
{
  "ConnectionStrings": {
    "Postgres": ""
  },
  "Jwt": {
    "Authority": "SmartGate",
    "Audience": "SmartGate.Api", 
    "SigningKey": ""
  },
  "Auth": {
    "UseDevAuth": false
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "SmartGate": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "RateLimiting": {
    "PermitPerMinute": 120
  },
  "AllowedHosts": "*"
}
```

### Reverse Proxy (nginx)
```nginx
server {
    listen 80;
    server_name smartgate.company.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name smartgate.company.com;
    
    ssl_certificate /path/to/certificate.crt;
    ssl_certificate_key /path/to/private.key;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

## Security Considerations

### JWT Configuration
- Use strong 256-bit signing key
- Set appropriate token expiration
- Validate issuer and audience
- Use HTTPS in production

### Database Security
- Use dedicated database user with minimal privileges
- Enable SSL connections
- Regular security updates
- Backup encryption

### Application Security
- Enable HTTPS redirection
- Configure CORS appropriately
- Set secure headers
- Regular dependency updates

## Monitoring

### Health Checks
- Endpoint: `/health`
- Monitor database connectivity
- Set up automated health monitoring

### Logging
- Configure structured logging
- Use log aggregation (ELK, Splunk)
- Monitor error rates and performance
- Set up alerts for critical errors

### Metrics
- Response times
- Request rates
- Error rates
- Database performance
- Memory and CPU usage

## Backup Strategy

### Database Backups
```bash
# Daily backup
pg_dump -h <host> -U <user> -d smartgate_prod > backup_$(date +%Y%m%d).sql

# Restore
psql -h <host> -U <user> -d smartgate_prod < backup_20240101.sql
```

### Application Backups
- Configuration files
- SSL certificates
- Application binaries (if not using CI/CD)

## Troubleshooting

### Common Issues
1. **Database Connection**: Check connection string and network connectivity
2. **JWT Validation**: Verify signing key and token format
3. **Migration Errors**: Ensure database user has sufficient privileges
4. **Performance**: Check database indexes and query performance

### Log Analysis
- Check application logs for exceptions
- Monitor database logs for slow queries
- Review reverse proxy logs for request patterns