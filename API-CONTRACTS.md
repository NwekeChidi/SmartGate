# SmartGate API Contracts

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

#### Validation Error Scenarios

**Invalid Driver ID Format:**
```json
{
  "truckLicensePlate": "ABC123D",
  "driver": {
    "firstName": "John",
    "lastName": "Doe",
    "id": "INVALID-ID"
  },
  "activities": [{"type": "Delivery", "unitNumber": "DFDS123456"}],
  "status": "PreRegistered"
}
```

**Response:** `400 Bad Request`
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

**Invalid Drvier.Id Length:**
```json
{
  "truckLicensePlate": "ac6-2347",
  "driver": {
    "firstName": "Grace",
    "lastName": "Mercy",
    "id": "dfds-1234567890"
  },
  "activities": [
    {
      "type": "Delivery",
      "unitNumber": "dfds123456"
    }
  ],
  "status": "PreRegistered"
}
```

**Response** `400 Bad Request`
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Validation failed",
  "status": "400",
  "detail": "One or more fields are invalid.",
  "instance": "/v1/Visits/create",
  "traceId": "0HNFPOH18N8F0:00000003",
  "errors": [
    {
      "field": "driver.id",
      "message": "'Driver Id' must be 16 characters in length. You entered 15 characters."
    }
  ]
}
```

**Invalid License Plate Length:**
```json
{
  "truckLicensePlate": "ABC12",
  "driver": {
    "firstName": "John",
    "lastName": "Doe",
    "id": "DFDS-12345678901"
  },
  "activities": [{"type": "Delivery", "unitNumber": "DFDS-123456"}],
  "status": "PreRegistered"
}
```

**Response:** `400 Bad Request`
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
  "truckLicensePlate": "ABC123D",
  "driver": {
    "firstName": "John",
    "lastName": "Doe",
    "id": "DFDS-12345678901"
  },
  "activities": [{"type": "Delivery", "unitNumber": "DF9S123"}],
  "status": "PreRegistered"
}
```

**Response:**
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

**Invalid Unit Number Length:**
```json
{
  "truckLicensePlate": "ABC123D",
  "driver": {
    "firstName": "John",
    "lastName": "Doe",
    "id": "DFDS-12345678901"
  },
  "activities": [{"type": "Delivery", "unitNumber": "DFDS123"}],
  "status": "PreRegistered"
}
```

**Response:** `400 Bad Request`
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Validation failed",
  "status": "400",
  "detail": "One or more fields are invalid.",
  "instance": "/v1/Visits/create",
  "traceId": "0HNFPOH18N8F2:00000001",
  "errors": [
    {
      "field": "activities[0].unitNumber",
      "message": "'Unit Number' must be 10 characters in length. You entered 7 characters."
    }
  ]
}
```

**Missing Required Fields:**
```json
{
  "truckLicensePlate": "",
  "driver": {
    "firstName": "",
    "lastName": "Doe",
    "id": "DFDS-12345678901"
  },
  "activities": [{"type": "Delivery", "unitNumber": "DFDS-123456"}],
  "status": "PreRegistered"
}
```

**Response:** `400 Bad Request`
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Validation failed",
  "status": "400",
  "detail": "One or more fields are invalid.",
  "instance": "/v1/Visits/create",
  "traceId": "0HNFPPCKRD62R:00000001",
  "errors": [
    {
      "field": "truckLicensePlate",
      "message": "'Truck License Plate' must not be empty."
    },
    {
      "field": "truckLicensePlate",
      "message": "'Truck License Plate' must be 7 characters in length. You entered 0 characters."
    },
    {
      "field": "driver.firstName",
      "message": "'Driver First Name' must not be empty."
    },
    {
      "field": "activities[0].unitNumber",
      "message": "activity.unitNumber must match pattern DFDS<6 numeric characters>."
    },
    {
      "field": "activities[0].unitNumber",
      "message": "'Unit Number' must be 10 characters in length. You entered 11 characters."
    }
  ]
}
```

**Creating With Empty Activities**
```json
{
  "truckLicensePlate": "ABC123D",
  "driver": {
    "firstName": "John",
    "lastName": "Doe",
    "id": "DFDS-12345678901"
  },
  "activities": [],
  "status": "PreRegistered"
}
```

**Response:** `400 Bad Request`
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

**Invalid Status:**
```json
{
  "truckLicensePlate": "ABC123D",
  "driver": {
    "firstName": "John",
    "lastName": "Doe",
    "id": "DFDS-12345678901"
  },
  "activities": [{"type": "Delivery", "unitNumber": "DFDS123456"}],
  "status": "AtGate"
}
```

**Response:** `400 Bad Request`
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

**Invalid Activities Enum:**
```json
{
  "truckLicensePlate": "ABC123D",
  "driver": {
    "firstName": "John",
    "lastName": "Doe",
    "id": "DFDS-12345678901"
  },
  "activities": [{"type": "Return", "unitNumber": "DFDS123456"}],
  "status": "PreRegistered"
}
```

**Response:** `400 Bad Request`
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": "400",
  "instance": "/v1/Visits/create",
  "errors": [
    {
      "field": "activities[0].type",
      "message": "Invalid ActivityType: 'Return'. Allowed: Delivery, Collection."
    }
  ],
  "traceId": "00-3ed7a60bd00903ff19251d5fc64484bd-28328ccd823fdccf-00"
}
```

**Invalid Status Enum:**
```json
{
  "truckLicensePlate": "ABC123D",
  "driver": {
    "firstName": "John",
    "lastName": "Doe",
    "id": "DFDS-12345678901"
  },
  "activities": [{"type": "Collection", "unitNumber": "DFDS123456"}],
  "status": "Pending"
}
```

**Response:** `400 Bad Request`
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": "400",
  "instance": "/v1/Visits/create",
  "errors": [
    {
      "field": "status",
      "message": "Invalid VisitStatus: 'Pending'. Allowed: PreRegistered, AtGate, OnSite, Completed."
    }
  ],
  "traceId": "00-cfa36cdb4a05464f8dc6b2c50af7a757-4f9c7a0c8dbc0179-00"
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

#### Success Scenario - Idempotent Update

**Request:** `PATCH /v1/visits/status_update/123e4567-e89b-12d3-a456-426614174000`
```json
{
  "newStatus": "AtGate"
}
```

**Response:** `200 OK` (Same response, no timestamp change)

#### Error Scenario - Visit Not Found

**Request:** `PATCH /v1/visits/status_update/999e9999-e89b-12d3-a456-426614174999`
```json
{
  "newStatus": "AtGate"
}
```

**Response:** `404 Not Found`
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

#### Error Scenario - Invalid Status Transition

**Request:** `PATCH /v1/visits/status_update/123e4567-e89b-12d3-a456-426614174000`
```json
{
  "newStatus": "Completed"
}
```

**Response:** `400 Bad Request`
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

#### Error Scenario - Terminal State Violation

**Request:** `PATCH /v1/visits/status_update/456e7890-e89b-12d3-a456-426614174001`
```json
{
  "newStatus": "OnSite"
}
```

**Response:** `400 Bad Request`
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