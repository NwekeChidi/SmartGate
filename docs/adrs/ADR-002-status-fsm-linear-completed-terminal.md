# ADR-002: Visit Status Finite State Machine - Linear Flow with Terminal State

## Status
Accepted

## Context
Visit management requires a clear progression through operational stages. The system needs to prevent invalid state transitions while allowing operators to track visit progress through gate operations.

## Decision
Implement a linear finite state machine for visit status with the following characteristics:

### State Flow
```
PreRegistered → AtGate → OnSite → Completed
```

### Rules
1. **Linear Progression**: Visits can only advance to the next sequential state
2. **Terminal State**: `Completed` is final - no further transitions allowed
3. **Idempotent Updates**: Setting the same status is a no-op
4. **Validation**: Invalid transitions throw `InvalidStatusTransitionException`

### Implementation
- `Visit.UpdateStatus()` enforces state machine rules
- `_linearNext` dictionary defines valid transitions
- `CompletedIsTerminalException` prevents changes to completed visits
- Domain events raised on successful status changes

## Consequences
### Positive
- Clear, predictable visit lifecycle
- Prevents data corruption from invalid state changes
- Audit trail through domain events
- Simple to understand and implement

### Negative
- No backward transitions (e.g., cannot undo completion)
- No parallel or branching workflows
- May require manual intervention for error correction

## Alternatives Considered
- **Non-linear FSM**: Rejected due to complexity
- **Reversible states**: Rejected to maintain data integrity
- **Status history**: Implemented through domain events instead

## Compliance
This decision ensures operational consistency and supports audit requirements for visit tracking.