# SmartGate Documentation

This directory contains comprehensive documentation for the SmartGate project.

## Documentation Structure

### Architecture Decision Records (ADRs)
- [ADR-001: Data Normalization Policy](adrs/ADR-001-normalization-policy.md)
- [ADR-002: Visit Status Finite State Machine](adrs/ADR-002-status-fsm-linear-completed-terminal.md)
- [ADR-003: List Visits Caching Implementation](adrs/ADR-003-list-visits-caching.md)

### API Documentation
- [API Reference](api-reference.md) - Complete API endpoint documentation with examples

### Domain Documentation
- [Domain Model](domain-model.md) - Business entities, rules, and domain logic

### Development Documentation
- [Development Guide](development-guide.md) - Setup, workflow, standards, and troubleshooting
- [Deployment Guide](deployment-guide.md) - Production deployment, configuration, and monitoring

## Quick Links

### For Developers
- [Getting Started](development-guide.md#getting-started)
- [Architecture Overview](development-guide.md#architecture)
- [Testing Guidelines](development-guide.md#testing-guidelines)

### For API Consumers
- [Authentication](api-reference.md#authentication)
- [Endpoints](api-reference.md#endpoints)
- [Error Handling](api-reference.md#error-responses)

### For Operations
- [Deployment Options](deployment-guide.md#deployment-options)
- [Configuration](deployment-guide.md#configuration)
- [Monitoring](deployment-guide.md#monitoring)

### For Business Stakeholders
- [Domain Model](domain-model.md#overview)
- [Business Rules](domain-model.md#business-rules-summary)
- [Visit Workflow](domain-model.md#enumerations)

## Contributing to Documentation

When making changes to the system:

1. **Update ADRs** for architectural decisions
2. **Update API Reference** for endpoint changes
3. **Update Domain Model** for business rule changes
4. **Update Development Guide** for process changes
5. **Update Deployment Guide** for infrastructure changes

## Documentation Standards

- Use Markdown format
- Include code examples where applicable
- Keep examples up-to-date with implementation
- Use clear, concise language
- Include diagrams for complex concepts