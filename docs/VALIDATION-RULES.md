# SmartGate Validation Rules

## Overview
This document outlines all validation rules, error messages, and data normalization policies in the SmartGate system.

## Data Normalization

### License Plates
- **Input**: Exactly 7 characters (after normalization)
- **Normalization**: Remove non-alphanumeric characters, convert to uppercase
- **Output**: Exactly 7 characters
- **Example**: `" ab-123cd "` → `AB123CD`

### Unit Numbers
- **Input**: Exactly 10 characters (after normalization)
- **Normalization**: Remove non-alphanumeric characters, convert to uppercase
- **Output**: Exactly 10 characters, must start with "DFDS"
- **Pattern**: `DFDS[0-9]{6}` (exactly 6 digits after DFDS)
- **Example**: `" dfds123456 "` → `DFDS123456`

### Driver IDs
- **Input**: Case-insensitive, must follow pattern
- **Normalization**: Convert to uppercase, preserve format
- **Output**: Exactly 16 characters, pattern `DFDS-[0-9]{11}` (exactly 11 digits)
- **Example**: `"dfds-12345678901"` → `DFDS-12345678901`

## Validation Rules

### Truck License Plate
- **Field**: `truckLicensePlate`
- **Required**: Yes
- **Min Length**: 7 characters (input)
- **Max Length**: 32 characters (input)
- **Normalized Length**: Exactly 7 characters
- **Error Messages**:
  - Empty: "'Truck License Plate' must not be empty."
  - Too short: "'Truck License Plate' must be 7 characters in length. You entered X characters."
  - Domain error: "truckLicensePlate must be exactly 7 characters long."

### Driver Information
#### First Name
- **Field**: `driver.firstName`
- **Required**: Yes
- **Max Length**: 128 characters
- **Error Messages**:
  - Empty: "'Driver First Name' must not be empty."
  - Too long: "firstName exceeds the allowed maximum length of 128."

#### Last Name
- **Field**: `driver.lastName`
- **Required**: Yes
- **Max Length**: 128 characters
- **Error Messages**:
  - Empty: "'Driver Last Name' must not be empty."
  - Too long: "lastName exceeds the allowed maximum length of 128."

#### Driver ID
- **Field**: `driver.id`
- **Required**: Yes
- **Pattern**: `^(?i)dfds-[0-9]{11}$` (exactly 11 digits)
- **Length**: Exactly 16 characters
- **Error Messages**:
  - Empty: "'Driver Id' must not be empty."
  - Invalid pattern: "driver.id must match pattern DFDS-<11 numeric characters>."
  - Wrong length: "'Driver Id' must be exactly 16 characters. You entered X characters."
  - Domain errors: "driver id must start with DFDS-.", "driver id must include 11 numeric characters after DFDS-.", "driver id suffix must be numeric (0–9)."

### Activities
#### Collection Validation
- **Field**: `activities`
- **Required**: Yes
- **Min Count**: 1
- **Error Messages**:
  - Empty: "At least one activity is required"
  - Domain error: "At least one activity is required."

#### Unit Number
- **Field**: `activities[].unitNumber`
- **Required**: Yes
- **Length**: Exactly 10 characters (input and after normalization)
- **Normalized Length**: Exactly 10 characters
- **Must Start With**: "DFDS"
- **Pattern**: `^(?i)dfds[0-9]{6}$` (exactly 6 digits after DFDS)
- **Error Messages**:
  - Empty: "'Unit Number' must not be empty."
  - Wrong length: "'Unit Number' must be exactly 10 characters. You entered X characters."
  - Invalid pattern: "activity.unitNumber must match pattern DFDS<6 numeric characters>."
  - Domain error: "unitNumber must be exactly 10 characters long."
  - Invalid prefix: "UnitNumber must start with 'DFDS'."

### Visit Status
- **Field**: `status`
- **Required**: Yes
- **Valid Values**: `PreRegistered`, `AtGate`, `OnSite`, `Completed`
- **New Visits**: Must be `PreRegistered`
- **Error Messages**:
  - Invalid for new visit: "New visits must have status 'PreRegistered'"
  - Invalid transition: "Transition from {from} to {to} is not allowed."
  - Terminal state: "Visit is already Completed and cannot be changed."
  - Invalid enum: "Invalid VisitStatus: '{value}'. Allowed: PreRegistered, AtGate, OnSite, Completed."

### Idempotency Key
- **Field**: `idempotencyKey`
- **Required**: No
- **Type**: Valid GUID
- **Error Messages**:
  - Invalid GUID: "'Idempotency Key' must be a valid GUID."
  - Duplicate: "A request with IdempotencyKey '{key}' already exists."

## Domain Exceptions

### Core Exceptions
- `ActivitiesRequiredException`: "At least one activity is required."
- `InvalidIdentifierException`: "Please provide a valid {field}."
- `InvalidIdentifierLengthException`: "{field} must be exactly {requiredLength} characters long."
- `MaxLengthExceededException`: "{field} exceeds the allowed maximum length of {max}."
- `NullReferenceInAggregateException`: "{field} cannot be null."

### Visit-Specific Exceptions
- `InvalidStatusTransitionException`: "Transition from {from} to {to} is not allowed."
- `CompletedIsTerminalException`: "Visit is already Completed and cannot be changed."
- `UnitNumberMustStartWithDFDSException`: "UnitNumber must start with 'DFDS'."
- `InvalidDriverIdException`: Custom message based on validation failure

## Status Transitions

### Valid Transitions
```
PreRegistered → AtGate → OnSite → Completed
```

### Business Rules
- Only forward progression allowed
- `Completed` is terminal (no further changes)
- Transitions to same status are idempotent (no error, no timestamp update)
- Invalid transitions throw `InvalidStatusTransitionException`
- Attempts to modify completed visits throw `CompletedIsTerminalException`

## Example Valid Data

### Minimal Valid Request
```json
{
  "truckLicensePlate": "ABC123D",
  "driver": {
    "firstName": "John",
    "lastName": "Doe", 
    "id": "DFDS-12345678901"
  },
  "activities": [
    {
      "type": "Delivery",
      "unitNumber": "DFDS123456"
    }
  ],
  "status": "PreRegistered"
}
```

### After Normalization (Response)
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "status": "PreRegistered",
  "truckLicensePlate": "ABC123D",
  "driverInformation": {
    "firstName": "John",
    "lastName": "Doe",
    "id": "DFDS-12345678901"
  },
  "activities": [
    {
      "id": "789e0123-e89b-12d3-a456-426614174001",
      "type": "Delivery", 
      "unitNumber": "DFDS123456"
    }
  ],
  "createdBy": "user@company.com",
  "updatedBy": "user@company.com",
  "createdAtUtc": "2024-01-15T10:30:00.000Z",
  "updatedAtUtc": "2024-01-15T10:30:00.000Z"
}
```

### Common Validation Error Response Format
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Validation failed",
  "status": "400",
  "detail": "One or more fields are invalid.",
  "instance": "/v1/Visits/create",
  "traceId": "0HNFPP3GVGK2G:00000002",
  "errors": [
    {
      "field": "driver.id",
      "message": "driver.id must match pattern DFDS-<11 numeric characters>."
    },
    {
      "field": "driver.id",
      "message": "'Driver Id' must be 16 characters in length. You entered 10 characters."
    }
  ]
}
```

## Testing Guidelines

### Valid Test Data
- License plates: Use 7+ character inputs that normalize to exactly 7 chars (e.g., "ABC123D")
- Unit numbers: Use 10+ character inputs that normalize to exactly 10 chars starting with "DFDS" (e.g., "DFDS-123456")
- Driver IDs: Use exactly 16 character format "DFDS-{11 digits}" (e.g., "DFDS-12345678901")

### Invalid Test Data Examples
- License plate too short: `"ABC12"` (5 chars) → "'Truck License Plate' must be 7 characters in length. You entered 5 characters."
- Unit number too short: `"DFDS123"` (7 chars) → "'Unit Number' must be 10 characters in length. You entered 7 characters."
- Invalid unit number pattern: `"DF9S123"` → "activity.unitNumber must match pattern DFDS<6 numeric characters>."
- Invalid driver ID: `"INVALID-ID"` (wrong pattern) → "driver.id must match pattern DFDS-<11 numeric characters>."
- Driver ID wrong length: `"dfds-1234567890"` (15 chars) → "'Driver Id' must be 16 characters in length. You entered 15 characters."
- Wrong status: `"AtGate"` for new visits → "New visits must have status 'PreRegistered'"
- Invalid activity type: `"Return"` → "Invalid ActivityType: 'Return'. Allowed: Delivery, Collection."
- Invalid status enum: `"Pending"` → "Invalid VisitStatus: 'Pending'. Allowed: PreRegistered, AtGate, OnSite, Completed."