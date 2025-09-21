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
- `Id`: DFDS driver identifier (format: DFDS-{1-11 digits})
- `FirstName`: Driver's first name (max 128 chars)
- `LastName`: Driver's last name (max 128 chars)

**Business Rules:**
- Driver ID must start with "DFDS-" prefix
- Driver ID suffix must be 1-11 numeric characters
- Names cannot be empty and are trimmed
- Driver ID is normalized to uppercase

### Truck
Represents truck information.

**Properties:**
- `LicensePlate`: Normalized license plate

**Business Rules:**
- License plate is normalized (uppercase, alphanumeric only)
- Must be between 6-32 characters after normalization

### Activity
Represents individual activities during a visit.

**Properties:**
- `Id`: Unique identifier (GUID)
- `Type`: Activity type (Delivery or Collection)
- `UnitNumber`: Unit identifier

**Business Rules:**
- Unit number must start with "DFDS"
- Unit number is normalized (uppercase, alphanumeric only)
- Maximum length of 32 characters

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

## Business Rules Summary

### Data Normalization
- License plates: Uppercase, alphanumeric only
- Unit numbers: Uppercase, alphanumeric only
- Driver IDs: Uppercase, preserve DFDS- prefix format

### Validation Rules
- **Driver ID**: Must match pattern `DFDS-[0-9]{1,11}`
- **License Plate**: 6-32 characters after normalization
- **Unit Number**: Must start with "DFDS", max 32 characters
- **Names**: Required, max 128 characters, trimmed
- **Activities**: At least one required per visit

### State Transitions
```
PreRegistered → AtGate → OnSite → Completed
```
- Only forward progression allowed
- Completed is terminal state
- Invalid transitions throw exceptions

## Domain Exceptions

- `ActivitiesRequiredException`: No activities provided
- `InvalidStatusTransitionException`: Invalid state transition attempted
- `CompletedIsTerminalException`: Attempt to modify completed visit
- `InvalidDriverIdException`: Driver ID format violation
- `MaxLengthExceededException`: Field exceeds maximum length
- `NullReferenceInAggregateException`: Required field is null
- `UnitNumberMustStartWithDFDSException`: Unit number format violation

## Aggregate Boundaries
- **Visit** is the aggregate root containing:
  - Truck information
  - Driver information
  - Collection of Activities
- Each aggregate maintains consistency within its boundary
- Cross-aggregate references use IDs only