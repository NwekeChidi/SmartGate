# SmartGate API Contracts

## Base Information
- **Base URL**: `https://localhost:5001` (Development)
- **API Version**: v1
- **Content-Type**: `application/json`
- **Authentication**: JWT Bearer Token (Production) / None (Development)

## Authentication

### Development Mode
No authentication required when `Auth:UseDevAuth = true`

### Production Mode
```http
Authorization: Bearer <jwt-token>
```

**Required Scopes:**
- `visits:read` - Read operations
- `visits:write` - Create/Update operations

---

## Endpoints

### 1. Create Visit

**Endpoint:** `POST /v1/visits/create`  
**Authorization:** `visits:write` scope required  
**Anti-Forgery:** Required

#### Success Scenario

**Request:**
```json
{
  "TruckLicensePlate": "ABC123",
  "Driver": {
    "FirstName": "John",
    "LastName": "Doe",
    "Id": "DFDS-12345"
  },
  "Activities": [
    {
      "Type": "Delivery",
      "UnitNumber": "DFDS001"
    },
    {
      "Type": "Collection", 
      "UnitNumber": "DFDS002"
    }
  ],
  "Status": "PreRegistered",
  "IdempotencyKey": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response:** `201 Created`
```json
{
  "Id": "123e4567-e89b-12d3-a456-426614174000",
  "Status": "PreRegistered",
  "TruckLicensePlate": "ABC123",
  "DriverInformation": {
    "FirstName": "John",
    "LastName": "Doe",
    "Id": "DFDS-12345"
  },
  "Activities": [
    {
      "Id": "789e0123-e89b-12d3-a456-426614174001",
      "Type": "Delivery",
      "UnitNumber": "DFDS001"
    },
    {
      "Id": "789e0123-e89b-12d3-a456-426614174002",
      "Type": "Collection",
      "UnitNumber": "DFDS002"
    }
  ],
  "CreatedBy": "user@company.com",
  "UpdatedBy": "user@company.com",
  "CreatedAtUtc": "2024-01-15T10:30:00.000Z",
  "UpdatedAtUtc": "2024-01-15T10:30:00.000Z"
}
```

#### Validation Error Scenarios

**Invalid Driver ID Format:**
```json
{
  "TruckLicensePlate": "ABC123",
  "Driver": {
    "FirstName": "John",
    "LastName": "Doe",
    "Id": "INVALID-ID"
  },
  "Activities": [{"Type": "Delivery", "UnitNumber": "DFDS001"}],
  "Status": "PreRegistered"
}
```

**Response:** `400 Bad Request`
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Driver.Id": ["Driver.Id must match pattern DFDS-<1..11 numeric characters>."]
  }
}
```

**Missing Required Fields:**
```json
{
  "TruckLicensePlate": "",
  "Driver": {
    "FirstName": "",
    "LastName": "Doe",
    "Id": "DFDS-12345"
  },
  "Activities": [],
  "Status": "PreRegistered"
}
```

**Response:** `400 Bad Request`
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "TruckLicensePlate": ["'Truck License Plate' must not be empty."],
    "Driver.FirstName": ["'First Name' must not be empty."],
    "Activities": ["At least one activity is required"]
  }
}
```

**Invalid Status:**
```json
{
  "TruckLicensePlate": "ABC123",
  "Driver": {
    "FirstName": "John",
    "LastName": "Doe",
    "Id": "DFDS-12345"
  },
  "Activities": [{"Type": "Delivery", "UnitNumber": "DFDS001"}],
  "Status": "AtGate"
}
```

**Response:** `400 Bad Request`
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Status": ["New visits must have status 'PreRegistered'"]
  }
}
```

#### Authorization Error

**Request without proper scope:**

**Response:** `403 Forbidden`
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "Insufficient permissions."
}
```

---

### 2. List Visits

**Endpoint:** `GET /v1/visits?page={page}&pageSize={pageSize}`  
**Authorization:** `visits:read` scope required

#### Success Scenario

**Request:** `GET /v1/visits?page=1&pageSize=2`

**Response:** `200 OK`
```json
{
  "Page": 1,
  "PageSize": 2,
  "Count": 150,
  "Items": [
    {
      "Id": "123e4567-e89b-12d3-a456-426614174000",
      "Status": "AtGate",
      "TruckLicensePlate": "ABC123",
      "DriverInformation": {
        "FirstName": "John",
        "LastName": "Doe",
        "Id": "DFDS-12345"
      },
      "Activities": [
        {
          "Id": "789e0123-e89b-12d3-a456-426614174001",
          "Type": "Delivery",
          "UnitNumber": "DFDS001"
        }
      ],
      "CreatedBy": "user@company.com",
      "UpdatedBy": "operator@company.com",
      "CreatedAtUtc": "2024-01-15T10:30:00.000Z",
      "UpdatedAtUtc": "2024-01-15T11:00:00.000Z"
    },
    {
      "Id": "456e7890-e89b-12d3-a456-426614174001",
      "Status": "Completed",
      "TruckLicensePlate": "XYZ789",
      "DriverInformation": {
        "FirstName": "Jane",
        "LastName": "Smith",
        "Id": "DFDS-67890"
      },
      "Activities": [
        {
          "Id": "789e0123-e89b-12d3-a456-426614174003",
          "Type": "Collection",
          "UnitNumber": "DFDS003"
        }
      ],
      "CreatedBy": "admin@company.com",
      "UpdatedBy": "operator@company.com",
      "CreatedAtUtc": "2024-01-15T09:00:00.000Z",
      "UpdatedAtUtc": "2024-01-15T12:00:00.000Z"
    }
  ]
}
```

#### Default Parameters

**Request:** `GET /v1/visits`

**Response:** `200 OK` (Same structure with page=1, pageSize=20)

#### Empty Results

**Request:** `GET /v1/visits?page=100&pageSize=20`

**Response:** `200 OK`
```json
{
  "Page": 100,
  "PageSize": 20,
  "Count": 150,
  "Items": []
}
```

#### Authorization Error

**Request without proper scope:**

**Response:** `403 Forbidden`
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "Insufficient permissions."
}
```

---

### 3. Update Visit Status

**Endpoint:** `PATCH /v1/visits/status_update/{id}`  
**Authorization:** `visits:write` scope required  
**Anti-Forgery:** Required

#### Success Scenario - Valid Transition

**Request:** `PATCH /v1/visits/status_update/123e4567-e89b-12d3-a456-426614174000`
```json
{
  "NewStatus": "AtGate"
}
```

**Response:** `200 OK`
```json
{
  "Id": "123e4567-e89b-12d3-a456-426614174000",
  "Status": "AtGate",
  "TruckLicensePlate": "ABC123",
  "DriverInformation": {
    "FirstName": "John",
    "LastName": "Doe",
    "Id": "DFDS-12345"
  },
  "Activities": [
    {
      "Id": "789e0123-e89b-12d3-a456-426614174001",
      "Type": "Delivery",
      "UnitNumber": "DFDS001"
    }
  ],
  "CreatedBy": "user@company.com",
  "UpdatedBy": "operator@company.com",
  "CreatedAtUtc": "2024-01-15T10:30:00.000Z",
  "UpdatedAtUtc": "2024-01-15T11:00:00.000Z"
}
```

#### Success Scenario - Idempotent Update

**Request:** `PATCH /v1/visits/status_update/123e4567-e89b-12d3-a456-426614174000`
```json
{
  "NewStatus": "AtGate"
}
```

**Response:** `200 OK` (Same response, no timestamp change)

#### Error Scenario - Visit Not Found

**Request:** `PATCH /v1/visits/status_update/999e9999-e89b-12d3-a456-426614174999`
```json
{
  "NewStatus": "AtGate"
}
```

**Response:** `404 Not Found`
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Visit not found."
}
```

#### Error Scenario - Invalid Status Transition

**Request:** `PATCH /v1/visits/status_update/123e4567-e89b-12d3-a456-426614174000`
```json
{
  "NewStatus": "Completed"
}
```

**Response:** `400 Bad Request`
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Domain Error",
  "status": 400,
  "detail": "Transition from PreRegistered to Completed is not allowed."
}
```

#### Error Scenario - Terminal State Violation

**Request:** `PATCH /v1/visits/status_update/456e7890-e89b-12d3-a456-426614174001`
```json
{
  "NewStatus": "OnSite"
}
```

**Response:** `400 Bad Request`
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Domain Error",
  "status": 400,
  "detail": "Visit is already Completed and cannot be changed."
}
```

#### Error Scenario - Invalid GUID Format

**Request:** `PATCH /v1/visits/status_update/invalid-guid`
```json
{
  "NewStatus": "AtGate"
}
```

**Response:** `400 Bad Request`
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "id": ["The value 'invalid-guid' is not valid."]
  }
}
```

---

### 4. Health Check

**Endpoint:** `GET /health`  
**Authorization:** None required

#### Success Scenario

**Request:** `GET /health`

**Response:** `200 OK`
```json
{
  "status": "Healthy"
}
```

#### Failure Scenario

**Response:** `503 Service Unavailable`
```json
{
  "status": "Unhealthy",
  "results": {
    "database": {
      "status": "Unhealthy",
      "description": "Database connection failed"
    }
  }
}
```

---

## Common Error Responses

### Rate Limiting

**Response:** `429 Too Many Requests`
```json
{
  "type": "https://tools.ietf.org/html/rfc6585#section-4",
  "title": "Too Many Requests",
  "status": 429,
  "detail": "Rate limit exceeded. Try again later."
}
```

### Unauthorized

**Response:** `401 Unauthorized`
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication required."
}
```

### Internal Server Error

**Response:** `500 Internal Server Error`
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500
}
```

---

## Data Types Reference

### VisitStatus Enum
- `PreRegistered` (0)
- `AtGate` (1)
- `OnSite` (2)
- `Completed` (3)

### ActivityType Enum
- `Delivery` (0)
- `Collection` (1)

### Valid Status Transitions
- `PreRegistered` → `AtGate`
- `AtGate` → `OnSite`
- `OnSite` → `Completed`
- `Completed` → (Terminal - no transitions allowed)

### Validation Rules
- **TruckLicensePlate**: 6-32 characters, required
- **Driver.FirstName**: Max 128 characters, required
- **Driver.LastName**: Max 128 characters, required
- **Driver.Id**: Pattern `DFDS-[0-9]{1,11}`, required
- **Activities**: At least 1 required
- **UnitNumber**: Max 32 characters, required
- **Status**: Must be `PreRegistered` for new visits
- **IdempotencyKey**: Valid GUID if provided