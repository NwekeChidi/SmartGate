# API Reference

## Base Information
- **Base URL**: `https://localhost:5001` (Development)
- **API Version**: v1
- **Content-Type**: `application/json`
- **Authentication**: JWT Bearer Token (Production / Uat) / None (Development)

## Authentication

### Development Mode
No authentication required when `Auth:UseDevAuth = true`

### Production / Uat Mode
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

#### Success Scenario

**Request:**
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
    },
    {
      "type": "Collection", 
      "unitNumber": "DFDS789012"
    }
  ],
  "status": "PreRegistered",
  "idempotencyKey": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response:** `201 Created`
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
    },
    {
      "id": "789e0123-e89b-12d3-a456-426614174002",
      "type": "Collection",
      "unitNumber": "DFDS789012"
    }
  ],
  "createdBy": "user@company.com",
  "updatedBy": "user@company.com",
  "createdAtUtc": "2024-01-15T10:30:00.000Z",
  "updatedAtUtc": "2024-01-15T10:30:00.000Z"
}
```

#### Common Validation Errors

**Invalid Driver ID Format:**
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

**Invalid License Plate Length:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Validation failed",
  "status": "400",
  "detail": "One or more fields are invalid.",
  "instance": "/v1/Visits/create",
  "traceId": "0HNFPP3GVGK2G:00000004",
  "errors": [
    {
      "field": "truckLicensePlate",
      "message": "'Truck License Plate' must be 7 characters in length. You entered 5 characters."
    }
  ]
}
```

**Invalid Unit Number:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Validation failed",
  "status": "400",
  "detail": "One or more fields are invalid.",
  "instance": "/v1/Visits/create",
  "traceId": "0HNFPPCKRD62Q:00000001",
  "errors": [
    {
      "field": "activities[0].unitNumber",
      "message": "activity.unitNumber must match pattern DFDS<6 numeric characters>."
    },
    {
      "field": "activities[0].unitNumber",
      "message": "'Unit Number' must be 10 characters in length. You entered 7 characters."
    }
  ]
}
```

**Empty Activities:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Validation failed",
  "status": "400",
  "detail": "One or more fields are invalid.",
  "instance": "/v1/Visits/create",
  "traceId": "0HNFPOH18N8F4:00000001",
  "errors": [
    {
      "field": "activities",
      "message": "At least one activity is required"
    }
  ]
}
```

**Invalid Status for New Visit:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Validation failed",
  "status": "400",
  "detail": "One or more fields are invalid.",
  "instance": "/v1/Visits/create",
  "traceId": "0HNFPPM60V059:00000001",
  "errors": [
    {
      "field": "status",
      "message": "New visits must have status 'PreRegistered'"
    }
  ]
}
```

### 2. List Visits

**Endpoint:** `GET /v1/visits?page={page}&pageSize={pageSize}`  
**Authorization:** `visits:read` scope required

#### Success Scenario

**Request:** `GET /v1/visits?page=1&pageSize=2`

**Response:** `200 OK`
```json
{
  "page": 1,
  "pageSize": 2,
  "count": 150,
  "items": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "status": "AtGate",
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
      "updatedBy": "operator@company.com",
      "createdAtUtc": "2024-01-15T10:30:00.000Z",
      "updatedAtUtc": "2024-01-15T11:00:00.000Z"
    },
    {
      "id": "456e7890-e89b-12d3-a456-426614174001",
      "status": "Completed",
      "truckLicensePlate": "XYZ789E",
      "driverInformation": {
        "firstName": "Jane",
        "lastName": "Smith",
        "id": "DFDS-67890123456"
      },
      "activities": [
        {
          "id": "789e0123-e89b-12d3-a456-426614174003",
          "type": "Collection",
          "unitNumber": "DFDS789012"
        }
      ],
      "createdBy": "admin@company.com",
      "updatedBy": "operator@company.com",
      "createdAtUtc": "2024-01-15T09:00:00.000Z",
      "updatedAtUtc": "2024-01-15T12:00:00.000Z"
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
  "page": 100,
  "pageSize": 20,
  "count": 150,
  "items": []
}
```

### 3. Update Visit Status

**Endpoint:** `PATCH /v1/visits/status_update/{id}`  
**Authorization:** `visits:write` scope required

#### Success Scenario - Valid Transition

**Request:** `PATCH /v1/visits/status_update/123e4567-e89b-12d3-a456-426614174000`
```json
{
  "newStatus": "AtGate"
}
```

**Response:** `200 OK`
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "status": "AtGate",
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
  "updatedBy": "operator@company.com",
  "createdAtUtc": "2024-01-15T10:30:00.000Z",
  "updatedAtUtc": "2024-01-15T11:00:00.000Z"
}
```

#### Error Scenarios

**Visit Not Found:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Resource not found",
  "status": "404",
  "detail": "Visit not found.",
  "instance": "/v1/Visits/status_update/8dd55672-2bc3-4f51-9459-fa60b7070bc0",
  "traceId": "0HNFPOH18N8F6:00000008"
}
```

**Invalid Status Transition:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Domain rule violated",
  "status": "409",
  "detail": "Transition from PreRegistered to Completed is not allowed.",
  "instance": "/v1/Visits/status_update/8dd55672-2bc3-4f51-9459-fa60b7070bc3",
  "traceId": "0HNFPOH18N8F6:0000000A"
}
```

**Terminal State Violation:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Domain rule violated",
  "status": "409",
  "detail": "Visit is already Completed and cannot be changed.",
  "instance": "/v1/Visits/status_update/8dd55672-2bc3-4f51-9459-fa60b7070bc3",
  "traceId": "0HNFPOH18N8F6:0000000E"
}
```

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

### Authorization Error

**Response:** `403 Forbidden`
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "Insufficient permissions."
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
- **truckLicensePlate**: Minimum 7 characters, maximum 32 characters, required (normalized to exactly 7 characters)
- **driver.firstName**: Maximum 128 characters, required
- **driver.lastName**: Maximum 128 characters, required
- **driver.id**: Pattern `DFDS-[0-9]{1,11}`, exactly 16 characters total, required
- **activities**: At least 1 required
- **unitNumber**: Minimum 10 characters, maximum 32 characters, required (normalized to exactly 10 characters, must start with 'DFDS')
- **status**: Must be `PreRegistered` for new visits
- **idempotencyKey**: Valid GUID if provided

### Data Normalization
- **License plates** are normalized by removing non-alphanumeric characters and converting to uppercase
- **Unit numbers** are normalized by removing non-alphanumeric characters and converting to uppercase
- **Driver IDs** are case-insensitive but must follow the exact pattern