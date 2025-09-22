# Domain Model

## Overview
The SmartGate domain model represents the core business concepts for managing truck visits through gate operations.

## Entities

### Visit (Aggregate Root)
The central entity representing a truck visit.

**Properties:**
- `Id`: Unique identifier (GUID)
- `Status`: Current visit status (enum)
- `Truck`: Associated truck information
- `Driver`: Associated driver information
- `Activities`: Collection of activities to perform
- `CreatedAtUTC`: Creation timestamp
- `UpdatedAtUTC`: Last update timestamp
- `CreatedBy`: User who created the visit
- `UpdatedBy`: User who last updated the visit

**Business Rules:**
- Must have at least one activity
- Status follows linear progression: PreRegistered → AtGate → OnSite → Completed
- Completed status is terminal (no further changes allowed)
- Status updates are idempotent

### Driver
Represents truck driver information.

**Properties:**
- `Id`: DFDS driver identifier (format: DFDS-{11 digits})
- `FirstName`: Driver's first name (max 128 chars)
- `LastName`: Driver's last name (max 128 chars)

**Business Rules:**
- Driver ID must start with "DFDS-" prefix
- Driver ID suffix must be 1-11 numeric characters
- Total driver ID length must be exactly 16 characters
- Names cannot be empty and are trimmed
- Driver ID is normalized to uppercase

### Truck
Represents truck information.

**Properties:**
- `LicensePlate`: Normalized license plate

**Business Rules:**
- License plate is normalized (uppercase, alphanumeric only)
- Must be exactly 7 characters after normalization
- Input can be 7-32 characters before normalization

### Activity
Represents individual activities during a visit.

**Properties:**
- `Id`: Unique identifier (GUID)
- `Type`: Activity type (Delivery or Collection)
- `UnitNumber`: Unit identifier

**Business Rules:**
- Unit number must start with "DFDS"
- Unit number must follow pattern: DFDS<6 numeric characters>
- Unit number is normalized (uppercase, alphanumeric only)
- Must be exactly 10 characters after normalization
- Input can be 10-32 characters before normalization

## Enumerations

### VisitStatus
```csharp
public enum VisitStatus
{
    PreRegistered = 0,  // Initial state
    AtGate = 1,         // Truck arrived at gate
    OnSite = 2,         // Truck on site performing activities
    Completed = 3       // All activities completed (terminal)
}
```

### ActivityType
```csharp
public enum ActivityType
{
    Delivery = 0,    // Delivering goods
    Collection = 1   // Collecting goods
}
```

## Domain Events

### VisitStatusChanged
Raised when a visit's status is updated.

**Properties:**
- `VisitId`: ID of the visit
- `OldStatus`: Previous status
- `NewStatus`: New status
- `ChangedAt`: Timestamp of change
- `ChangedBy`: User who made the change

**Business Rules:**
- Event is raised only for actual status changes (not idempotent updates)
- Contains audit information for tracking purposes
- Used for cache invalidation and external system notifications

## Business Rules Summary

### Data Normalization
- License plates: Uppercase, alphanumeric only
- Unit numbers: Uppercase, alphanumeric only
- Driver IDs: Uppercase, preserve DFDS- prefix format

### Validation Rules
- **Driver ID**: Must match pattern `DFDS-[0-9]{1,11}`, exactly 16 characters total
- **License Plate**: Exactly 7 characters after normalization (7-32 characters input)
- **Unit Number**: Must match pattern `DFDS<6 numeric characters>`, exactly 10 characters after normalization (10-32 characters input)
- **Names**: Required, max 128 characters, trimmed
- **Activities**: At least one required per visit
- **Status**: New visits must have status 'PreRegistered'
- **Idempotency Key**: Valid GUID if provided

### State Transitions
```
PreRegistered → AtGate → OnSite → Completed
```
- Only forward progression allowed
- Completed is terminal state
- Invalid transitions throw exceptions

## Domain Exceptions

- `ActivitiesRequiredException`: "At least one activity is required."
- `InvalidStatusTransitionException`: "Transition from {from} to {to} is not allowed."
- `CompletedIsTerminalException`: "Visit is already Completed and cannot be changed."
- `InvalidDriverIdException`: Driver ID format violation
- `InvalidIdentifierException`: "Please provide a valid {field}."
- `InvalidIdentifierLengthException`: "{field} must be exactly {requiredLength} characters long."
- `MaxLengthExceededException`: "{field} exceeds the allowed maximum length of {max}."
- `NullReferenceInAggregateException`: "{field} cannot be null."
- `UnitNumberMustStartWithDFDSException`: "UnitNumber must start with 'DFDS'."

## Aggregate Boundaries
- **Visit** is the aggregate root containing:
  - Truck information
  - Driver information
  - Collection of Activities
- Each aggregate maintains consistency within its boundary
- Cross-aggregate references use IDs only

## API Integration

For complete API request/response examples and detailed validation scenarios, see [API-CONTRACTS.md](../API-CONTRACTS.md).

### Common Validation Error Patterns
- Driver ID length: "'Driver Id' must be 16 characters in length. You entered X characters."
- Driver ID pattern: "driver.id must match pattern DFDS-<11 numeric characters>."
- License plate length: "'Truck License Plate' must be 7 characters in length. You entered X characters."
- Unit number length: "'Unit Number' must be 10 characters in length. You entered X characters."
- Unit number pattern: "activity.unitNumber must match pattern DFDS<6 numeric characters>."
- Empty activities: "At least one activity is required"
- Invalid status for new visits: "New visits must have status 'PreRegistered'"
- Invalid enum values: "Invalid {EnumType}: '{value}'. Allowed: {allowedValues}."