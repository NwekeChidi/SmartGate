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
- Driver ID normalization preserves the "DFDS-" prefix format
- Applied at domain entity level to ensure data consistency

## Consequences
### Positive
- Consistent data storage regardless of input format
- Improved data quality and searchability
- Reduced duplicate entries due to formatting differences

### Negative
- Original formatting is lost
- May require additional validation for specific business rules
- Potential confusion if users expect exact input preservation

## Compliance
This decision supports data quality requirements and ensures consistent business rule application across the system.