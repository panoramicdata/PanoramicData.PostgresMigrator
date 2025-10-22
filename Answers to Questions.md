# Answers to Clarification Questions

## Configuration & Setup

### 1. Multi-Tenancy & Mappings
- **Q1.1:** N:M - Tool handles multiple, completely independent source-destination pairs
- **Q1.2:** Instance ? Instance level
- **Q1.3:** **Refuse and require manual intervention** (fail-safe approach)

### 2. Configuration Details
- **Q2.1:** Specify Server, Port, Username, Password separately (no connection strings). Cleartext password acceptable.
- **Q2.2:** No hot-reload needed, but handle cold-start configuration well
- **Q2.3:** Environment variables

## Replication Strategy

### 3. Replication Approach
- **Q3.1:** **Logical Replication** (table-to-table) - CONFIRMED
- **Q3.2:** Use `CREATE SUBSCRIPTION` if possible, fallback to most efficient alternative
- **Q3.3:** **Database-level ALWAYS**

### 4. Partitioned Tables
- **Q4.1:** Full partition replication - destination identical to source (definitions + contents)
- **Q4.2:** Detect issues and balk if any problems found

### 5. Write-Ahead Log (WAL) Management
- **Q5.1:** Retain until manual "mark complete" action, then cleanup. Tool provides cutover instructions.
- **Q5.2:** Monitor both WAL lag (MB) and time lag (minutes). Use sensible defaults.

## Schema & Objects

### 6. Schema Migration Details
- **Q6.1:** **Current version**: Tables, partitions, indexes ONLY
  - **Roadmap**: Views, materialized views, functions, stored procedures, triggers
- **Q6.2:** **YES** - Sync sequence definitions AND current values (critical)
- **Q6.3:** **Validate extensions** - refuse to sync if destination missing required extensions

### 7. Data Types & Custom Types
- **Q7.1:** No custom type migration needed
- **Q7.2:** No PostGIS support needed

### 8. Constraints & Indexes
- **Q8.1:** Apply constraints during sync (use deferred constraints if that means applying during sync)
- **Q8.2:** Create indexes as we go (during sync, not after)

## Roles & Security

### 9. Role Migration
- **Q9.1:** YES - migrate roles
- **Q9.2:** Allow config to specify merge/skip/fail, **default: merge**
- **Q9.3:** Copy password hashes if possible. If not, set configurable common password and alert user.

### 10. Permissions & Grants
- **Q10.1:** YES - replicate object-level, column-level, and default privileges (all levels)
- **Q10.2:** Log conflicting grants as informational

## Monitoring & UI

### 11. UI Requirements
- **Q11.1:** 5-second default refresh, **configurable**
- **Q11.2:** Display **BOTH** row count delta AND WAL bytes/lag
- **Q11.3:** High-level aggregation sufficient (X tables synced, Y in catchup)

### 12. Status Categories
- **Q12.1:** Make good architectural choices based on replication state
- **Q12.2:** **YES - CRITICAL!** "Cutover Ready" green light indicator

### 13. Alerting & Notifications
- **Q13.1:** **Roadmap item**: Microsoft Teams notifications
- **Q13.2:** No immediate notifications needed at this stage

## Operations & Control

### 14. Migration Lifecycle
- **Q14.1:** **NO** - Tool does NOT perform cutover. Tool provides instructions for manual cutover.
- **Q14.2:** **YES** - Support "mark mapping complete" action ? cleanup replication slots

### 15. Failure Handling
- **Q15.1:** **Continue with what can be synced** if individual table fails
- **Q15.2:** **Exponential backoff**

### 16. Rollback & Recovery
- **Q16.1:** Non-destructive to source, no rollback requirements
- **Q16.2:** **Stateless design** - re-discover status after restarts (desired state config only)

## Performance & Scale

### 17. Scale Expectations
- **Q17.1:** Don't worry about scale constraints for initial version
- **Q17.2:** Don't worry about bandwidth constraints

### 18. Performance Tuning
- **Q18.1:** **Parallel sync**: Databases in parallel, tables/partitions in parallel where possible
- **Q18.2:** Rate limiting to minimize source database impact (configurable)

## Development & Testing

### 19. Testing Strategy
- **Q19.1:** **YES** - Test Harness with Docker for ephemeral **destination** instances
  - **Existing source instances** available for testing (not ephemeral)
- **Q19.2:** TBD based on actual source database structures

### 20. Development Environment
- **Q20.1:** Existing PostgreSQL instances available
- **Q20.2:** No network restrictions

## Deployment

### 21. Kubernetes Deployment
- **Q21.1:** On-premises, no special ingress requirements
- **Q21.2:** **Single instance only** (one destination per source)

### 22. Authentication & Authorization
- **Q22.1:** **No authentication needed** for web UI
- **Q22.2:** N/A

## Miscellaneous

### 23. Logging & Debugging
- **Q23.1:** Stdout only for now, **use Serilog** for future flexibility
- **Q23.2:** No log retention (stdout only)

### 24. Metrics & Observability
- **Q24.1:** No Prometheus/health check endpoints needed at this stage

### 25. Licensing & Open Source
- **Q25.1:** **YES - MIT License**

---

## Key Decisions Summary

| Category | Decision |
|----------|----------|
| **Architecture** | N:M instance pairs, database-level replication, stateless design |
| **Replication** | Logical replication, CREATE SUBSCRIPTION, parallel sync with rate limiting |
| **Scope** | Tables, partitions, indexes, sequences, roles, permissions (v1) |
| **Extensions** | Validate required, refuse if missing |
| **Conflict Handling** | Fail-safe (refuse + manual intervention), merge roles by default |
| **UI** | Blazor Server, 5s refresh, both metrics, cutover ready indicator |
| **State** | Stateless (re-discover on restart) |
| **Failure** | Continue where possible, exponential backoff |
| **Cutover** | Manual (external), tool provides instructions |
| **Testing** | Docker for destination, real sources |
| **License** | MIT (open source) |

---

## Updated Roadmap Priorities

### Phase 1 (Current)
? Tables, partitions, indexes, sequences, roles, permissions  
? Cutover Ready indicator  
? Parallel sync with rate limiting  
? Stateless re-discovery  

### Future Phases
?? Views, materialized views, functions, procedures, triggers  
?? Microsoft Teams notifications  
?? Custom type migration  
?? PostGIS support  
?? Prometheus metrics  
?? Authentication for web UI  
