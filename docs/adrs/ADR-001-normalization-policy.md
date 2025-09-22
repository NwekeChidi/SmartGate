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
- Driver ID normalization preserves the "DFDS-" prefix format and validates the numeric suffix
- Applied at domain entity level to ensure data consistency
- Validation occurs before normalization to ensure input meets minimum requirements
- Error messages provide specific guidance on expected formats
- **License plates**: Must result in exactly 7 characters after normalization (input: 7-32 characters)
- **Unit numbers**: Must result in exactly 10 characters after normalization, start with "DFDS", and follow pattern DFDS<6 numeric characters> (input: 10-32 characters)
- **Driver IDs**: Must be exactly 16 characters total following pattern DFDS-[0-9]{1,11} (case-insensitive input, normalized to uppercase)

## Consequences
### Positive
- Consistent data storage regardless of input format
- Improved data quality and searchability
- Reduced duplicate entries due to formatting differences

### Negative
- Original formatting is lost
- May require additional validation for specific business rules
- Potential confusion if users expect exact input preservation
- Strict length requirements after normalization may reject some valid-looking inputs

## Compliance
This decision supports data quality requirements and ensures consistent business rule application across the system.

## Examples

### License Plate Normalization
- Input: `" ab-123cd "` → Output: `"AB123CD"` (7 characters)
- Input: `"ABC123D"` → Output: `"ABC123D"` (7 characters)

### Unit Number Normalization
- Input: `" dfds-123456 "` → Output: `"DFDS123456"` (10 characters)
- Input: `"DFDS123456"` → Output: `"DFDS123456"` (10 characters)

### Driver ID Normalization
- Input: `"dfds-12345678901"` → Output: `"DFDS-12345678901"` (16 characters)
- Input: `"DFDS-12345678901"` → Output: `"DFDS-12345678901"` (16 characters)

## Validation Error Messages
For complete validation error examples, see [API-CONTRACTS.md](../../API-CONTRACTS.md).

- License plate too short: "'Truck License Plate' must be 7 characters in length. You entered X characters."
- Unit number invalid pattern: "activity.unitNumber must match pattern DFDS<6 numeric characters>."
- Driver ID wrong length: "'Driver Id' must be 16 characters in length. You entered X characters."
- Driver ID invalid pattern: "driver.id must match pattern DFDS-<11 numeric characters>."