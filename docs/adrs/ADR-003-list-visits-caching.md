# ADR-003: List Visits Caching Implementation

## Status
Accepted

## Context
The `/v1/visits` endpoint is frequently accessed for monitoring and operational purposes. As the number of visits grows, repeated database queries for the same paginated results can impact performance and database load.

## Decision
Implement in-memory caching for the List Visits endpoint with the following characteristics:

### Cache Configuration
- **Cache Type**: In-memory cache using `IMemoryCache`
- **TTL**: 2 minutes
- **Cache Key Pattern**: `visits_list_{page}_{pageSize}`
- **Scope**: Application-level caching

### Cache Invalidation Strategy
- **Automatic Invalidation**: Cache is cleared when visits are created or updated
- **Invalidation Method**: Remove common cache keys for typical pagination patterns
- **Invalidated Keys**: `visits_list_1_20`, `visits_list_1_50`, `visits_list_1_100`, `visits_list_2_20`, `visits_list_2_50`, `visits_list_3_20`

### Implementation Details
- Cache implemented in `VisitService.ListVisitsAsync()`
- Cache invalidation in `CreateVisitAsync()` and `UpdateVisitStatusAsync()`
- Debug logging for cache hits and invalidations

## Consequences

### Positive
- **Improved Performance**: Reduced database load for repeated list requests
- **Better User Experience**: Faster response times for frequently accessed data
- **Scalability**: Reduced database pressure as traffic increases
- **Simple Implementation**: Minimal code changes with standard .NET caching

### Negative
- **Memory Usage**: Additional memory consumption for cached results
- **Data Freshness**: Up to 2-minute delay for new data visibility
- **Cache Invalidation Complexity**: Manual invalidation for common keys only

### Neutral
- **TTL Trade-off**: 2-minute TTL balances performance vs data freshness
- **Limited Scope**: Only applies to list endpoint, not individual visit queries

## Alternatives Considered

### 1. Distributed Cache (Redis)
- **Pros**: Shared across instances, persistent
- **Cons**: Additional infrastructure, complexity, network latency
- **Decision**: Not needed for current scale

### 2. Database Query Optimization
- **Pros**: No cache complexity, always fresh data
- **Cons**: Limited improvement potential, database load remains
- **Decision**: Complementary, not alternative

### 3. Longer Cache TTL
- **Pros**: Better performance
- **Cons**: Stale data issues for operational use
- **Decision**: 2 minutes balances needs

### 4. Event-Driven Cache Invalidation
- **Pros**: More precise invalidation
- **Cons**: Additional complexity, event infrastructure
- **Decision**: Simple invalidation sufficient for current needs

## Implementation Notes
- Cache dependency added to DI container in `Program.cs`
- Memory cache package added to Application layer
- Test coverage includes cache behavior verification
- Logging added for debugging and monitoring

## Monitoring Considerations
- Monitor memory usage in production
- Track cache hit rates through logs
- Consider cache size limits if memory becomes constrained
- Monitor response time improvements

## Future Considerations
- Evaluate distributed caching if multiple instances deployed
- Consider more sophisticated invalidation strategies
- Monitor for cache-related memory issues
- Evaluate cache effectiveness through metrics