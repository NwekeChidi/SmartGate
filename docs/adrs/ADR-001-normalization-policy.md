# ADR-001: Data Normalization Policy

## Status
Accepted

## Context
The SmartGate system handles various user inputs including truck license plates, unit numbers, and driver IDs. These inputs come from different sources and may contain inconsistent formatting, special characters, and casing.

## Decision
Implement a consistent normalization policy for all string inputs:

### Normalization Rules
1. **Trim whitespace** from beginning and end
2. **Convert to uppercase** for consistency
3. **Remove non-alphanumeric characters** from plates and unit numbers
4. **Preserve specific formats** for structured identifiers (e.g., Driver IDs)

### Implementation
- `Normalization.NormalizePlateOrUnit()` method handles license plates and unit numbers
- Driver ID normalization preserves the "DFDS-" prefix format and validates exactly 11 numeric digits
- Applied at domain entity level to ensure data consistency
- Validation occurs at FluentValidation level for exact length requirements
- Domain validation ensures normalized output meets exact specifications
- Error messages provide specific guidance on expected formats
- **License plates**: Must result in exactly 7 characters after normalization (input must normalize to exactly 7 characters)
- **Unit numbers**: Must result in exactly 10 characters after normalization, start with "DFDS", and follow pattern DFDS[0-9]{6} (input must normalize to exactly 10 characters)
- **Driver IDs**: Must be exactly 16 characters total following pattern DFDS-[0-9]{11} (case-insensitive input, normalized to uppercase)

## Consequences
### Positive
- Consistent data storage regardless of input format
- Improved data quality and searchability
- Reduced duplicate entries due to formatting differences

### Negative
- Original formatting is lost
- May require additional validation for specific business rules
- Potential confusion if users expect exact input preservation
- Strict exact length requirements may reject inputs that don't normalize to precise lengths
- FluentValidation and domain validation must be kept in sync

## Compliance
This decision supports data quality requirements and ensures consistent business rule application across the system.

## Examples

### License Plate Normalization
- Input: `" ab-123cd "` → Output: `"AB123CD"` (7 characters)
- Input: `"ABC123D"` → Output: `"ABC123D"` (7 characters)

### Unit Number Normalization
- Input: `" dfds-123456 "` → Output: `"DFDS123456"` (10 characters)
- Input: `"DFDS123456"` → Output: `"DFDS123456"` (10 characters)
- Pattern: Must be exactly `DFDS` + 6 numeric digits

### Driver ID Normalization
- Input: `"dfds-12345678901"` → Output: `"DFDS-12345678901"` (16 characters)
- Input: `"DFDS-12345678901"` → Output: `"DFDS-12345678901"` (16 characters)
- Pattern: Must be exactly `DFDS-` + 11 numeric digits

## Validation Error Messages
For complete validation error examples, see [API-CONTRACTS.md](../../API-CONTRACTS.md).

- License plate wrong length: "'Truck License Plate' must be exactly 7 characters. You entered X characters."
- Unit number invalid pattern: "activity.unitNumber must match pattern DFDS<6 numeric characters>."
- Unit number wrong length: "'Unit Number' must be exactly 10 characters. You entered X characters."
- Driver ID wrong length: "'Driver Id' must be exactly 16 characters. You entered X characters."
- Driver ID invalid pattern: "driver.id must match pattern DFDS-<11 numeric characters>."
- Driver ID domain errors: "driver id must start with DFDS-.", "driver id must include 11 numeric characters after DFDS-."