# PanoramicData.PostgresMigrator - Implementation Plan (Updated)

## Project Overview
A zero-downtime PostgreSQL database migration tool supporting multiple source databases (v15+) to destination databases (v18+) with real-time monitoring via Blazor UI using PanoramicData.Blazor library.

**Key Principles:**
- N:M instance-to-instance mappings
- Database-level logical replication (always)
- Stateless design with re-discovery on restart
- Fail-safe conflict handling
- Parallel sync with rate limiting
- **Cutover Ready** green light indicator

## Current State
- **Project Type**: Console application (.NET 9)
- **Status**: Initial scaffolding only (Hello World)
- **Target**: Convert to Blazor Server application with background services
- **License**: MIT (open source)

---

## Phase 1: Foundation & Architecture Setup (Week 1-2)

### 1.1 Project Structure Transformation
**Tasks:**
- Convert console app to Blazor Server application (update SDK and Program.cs)
- Add NuGet packages:
  - `Npgsql` (PostgreSQL .NET driver) - latest stable
  - `PanoramicData.Blazor` (UI components)
  - `Microsoft.Extensions.Hosting` (Background services)
  - `Microsoft.Extensions.Configuration.EnvironmentVariables`
  - `Serilog.AspNetCore` (Logging - stdout only initially)
  - `Polly` (Resilience and exponential backoff)

**Deliverables:**
- Updated `.csproj` with Blazor Server SDK
- Basic Blazor app structure (Pages, Components, wwwroot folders)
- Dependency injection setup in `Program.cs`
- Serilog configured for stdout

### 1.2 Configuration System
**Tasks:**
- Create `appsettings.json` template (for development)
- Create `appsettings.Development.json` example
- Implement environment variable override mechanism
- Design configuration model classes:
  - `PostgresInstanceConfig` (Server, Port, Username, Password)
- `MappingConfig` (source instance ? destination instance pairs)
  - `ReplicationConfig` (rate limiting, refresh intervals, timeouts)
  - `RoleConflictStrategy` enum (Merge/Skip/Fail, default: Merge)

**Configuration Format:**
```json
{
  "Instances": {
    "Source1": {
 "Server": "pg15-prod-1.example.com",
      "Port": 5432,
      "Username": "replication_user",
    "Password": "cleartext_password"
    },
 "Dest1": {
      "Server": "pg18-new-1.example.com",
      "Port": 5432,
      "Username": "admin_user",
      "Password": "cleartext_password"
    }
  },
  "Mappings": [
    {
      "SourceInstance": "Source1",
    "DestinationInstance": "Dest1",
      "RoleConflictStrategy": "Merge"
    }
  ],
  "Replication": {
    "RateLimitMBps": 100,
    "UIRefreshIntervalSeconds": 5,
    "WalLagThresholdMB": 1024,
 "WalLagThresholdMinutes": 5
  }
}
```

**Deliverables:**
- `Models/Configuration/` folder with all config classes
- `README-Configuration.md` documenting all environment variables
- Configuration validation on startup (fail-fast)
- Example appsettings with multiple mappings

### 1.3 Core Data Models
**Tasks:**
- Design domain models:
  - `MigrationMapping` (source ? destination instance relationship)
  - `DatabaseInfo` (discovered databases on instance)
  - `TableInfo` (table metadata, row counts)
  - `PartitionInfo` (partition hierarchy)
  - `SequenceInfo` (sequence name, current value)
  - `RoleInfo` (role properties, password hash)
  - `ExtensionInfo` (extension name, version)
  - `ReplicationSlotInfo` (WAL position, lag metrics)
  - `SyncStatus` enum (NotStarted, InProgress, Synced, CutoverReady, Problematic, Completed)
  
**Deliverables:**
- `Models/Domain/` folder with all entity classes
- Enums for states: `SyncStatus`, `MigrationPhase`, `ConflictType`, `RoleConflictStrategy`
- Models include timestamp tracking for state changes

---

## Phase 2: PostgreSQL Core Services (Week 3-4)

### 2.1 Connection Management
**Tasks:**
- Create `IPostgresConnectionFactory` interface
- Implement connection pooling per instance (Npgsql's built-in pooling)
- Add connection health checks
- Implement retry logic with Polly (exponential backoff)
- Support both source (read-only) and destination (admin) connections

**Deliverables:**
- `Services/PostgresConnectionFactory.cs`
- `Services/ConnectionHealthCheck.cs`
- Unit tests for connection management (mocked Npgsql)

### 2.2 Schema Discovery & Analysis
**Tasks:**
- Implement `ISchemaDiscoveryService`:
  - Query `information_schema` for tables, columns, constraints
  - Extract primary keys, foreign keys, indexes
  - Discover partitions and partition strategies (`pg_partitioned_table`, `pg_inherits`)
  - List sequences and current values (`pg_sequences`, `currval`)
  - List roles and users (`pg_roles`, `pg_authid`)
  - List extensions (`pg_extension`)
- Implement schema comparison logic
- Handle version-specific differences (PG 15+ vs 18+)
- **Focus:** Tables, partitions, indexes, sequences, roles (v1 scope)

**Deliverables:**
- `Services/SchemaDiscoveryService.cs`
- `Services/SchemaComparisonService.cs`
- `Models/Schema/` with schema metadata models
- Version compatibility layer

### 2.3 Conflict Detection
**Tasks:**
- Implement `IConflictDetectionService`:
  - Detect database name conflicts across mappings
  - Detect role/username conflicts
  - Validate destination databases not in active use (check for connections)
  - Identify missing extensions on destination
- Define conflict resolution policies (fail-safe: refuse + require intervention)
- Generate actionable conflict reports

**Deliverables:**
- `Services/ConflictDetectionService.cs`
- `Models/Conflicts/` with conflict types and resolution suggestions
- Conflict reporting in UI

### 2.4 Extension Validation
**Tasks:**
- Implement `IExtensionValidationService`:
  - Compare source extensions with destination
  - **Refuse to proceed** if destination missing required extensions
  - Provide clear error messages listing missing extensions

**Deliverables:**
- `Services/ExtensionValidationService.cs`
- Pre-flight check integration

---

## Phase 3: Native Replication Implementation (Week 5-7)

### 3.1 Replication Slot Management
**Tasks:**
- Implement `IReplicationSlotService`:
  - Create logical replication slots on source using `pg_create_logical_replication_slot()`
  - Monitor WAL position and lag (`pg_replication_slots`)
  - Calculate lag in both MB and time
  - Clean up slots on mapping completion (when user marks complete)
- Handle slot lifecycle: create ? monitor ? cleanup

**Deliverables:**
- `Services/ReplicationSlotService.cs`
- Replication slot lifecycle management
- WAL lag monitoring (both metrics)
- Cleanup on "mark complete" action

### 3.2 Publication & Subscription Setup
**Tasks:**
- Implement `IReplicationService`:
  - Create publications on source databases (all tables in database)
  - Use `CREATE SUBSCRIPTION` on destination (includes initial data copy)
  - Fallback mechanism if CREATE SUBSCRIPTION not optimal
  - Configure subscription for continuous sync
  - Handle partitioned table replication (parent + children)
  - Monitor subscription status (`pg_subscription`, `pg_stat_subscription`)
- Support **database-level replication** (always)

**Deliverables:**
- `Services/ReplicationService.cs`
- `Services/PublicationManager.cs`
- `Services/SubscriptionManager.cs`
- Partitioned table replication support

### 3.3 Schema Migration (In-Process)
**Tasks:**
- Implement `ISchemaMigrationService`:
  - Extract schema DDL from source (replace pg_dump)
  - Query `pg_catalog` and `information_schema` for table definitions
  - Generate `CREATE TABLE` statements (including partition definitions)
  - Generate `CREATE INDEX` statements (create as we go)
  - Generate constraint definitions (apply during sync)
  - Apply schema to destination (DDL execution)
  - Validate schema integrity post-creation
- **Scope:** Tables, partitions, indexes (constraints created during sync)

**Deliverables:**
- `Services/SchemaMigrationService.cs`
- `Services/DdlGenerator.cs`
- Schema validation checks
- Partition hierarchy preservation

### 3.4 Sequence Synchronization
**Tasks:**
- Implement `ISequenceSyncService`:
  - Discover sequences on source
  - Extract sequence definitions and **current values**
  - Create sequences on destination
  - Set current values on destination to match source
  - Continuously sync sequence values during replication

**Deliverables:**
- `Services/SequenceSyncService.cs`
- Sequence value tracking
- Current value synchronization

### 3.5 Role & Permission Migration
**Tasks:**
- Implement `IRoleMigrationService`:
  - Extract roles from source (`pg_roles`, `pg_authid`)
  - Attempt to copy password hashes (if accessible)
  - If password copy fails, set configurable common password and alert user
  - Extract grants and permissions (object-level, column-level, default privileges)
  - Apply roles to destination with configurable conflict strategy (merge/skip/fail)
  - Default: **merge** (use existing role if exists)
  - Log conflicting grants as informational

**Deliverables:**
- `Services/RoleMigrationService.cs`
- `Services/PermissionMigrationService.cs`
- Role conflict handling (configurable strategy)
- Password fallback mechanism with user notification
- Permission replication at all levels

---

## Phase 4: Orchestration & State Management (Week 8-9)

### 4.1 Migration Orchestrator
**Tasks:**
- Implement `IMigrationOrchestrator` (IHostedService background service):
  - Process all mappings **in parallel** (N:M support)
  - Process tables/partitions **in parallel within each mapping** (where possible)
  - Coordinate migration phases per mapping:
    1. **Pre-flight checks** (connections, conflicts, extensions)
    2. **Schema migration** (tables, partitions, indexes)
    3. **Sequence migration** (definitions + current values)
    4. **Role migration** (with configured conflict strategy)
    5. **Permission replication**
    6. **Replication setup** (publications, subscriptions)
    7. **Initial sync** (handled by CREATE SUBSCRIPTION)
    8. **Continuous catchup** (monitor lag)
    9. **Cutover ready** (green light when lag < thresholds)
    10. **Manual cutover** (external, tool provides instructions)
    11. **Mark complete** (user action ? cleanup slots)
  - Handle failures: continue with what can be synced
  - Implement **rate limiting** (configurable MB/s throttle)
  - Support graceful shutdown

**Deliverables:**
- `Services/MigrationOrchestrator.cs` (hosted service)
- State machine for migration phases
- Parallel processing with rate limiting
- Graceful shutdown handling
- Failure isolation (per table/database)

### 4.2 State Re-Discovery (Stateless Design)
**Tasks:**
- Implement `IStateDiscoveryService`:
  - On startup/restart, query source and destination to determine current state
  - Detect existing publications, subscriptions, replication slots
  - Determine which phase each mapping is in
  - Calculate current sync status (lag, row counts, etc.)
  - No persistent state storage required (desired state = config only)

**Deliverables:**
- `Services/StateDiscoveryService.cs`
- Stateless restart capability
- Current state calculation from live database queries

### 4.3 Monitoring & Health Checks
**Tasks:**
- Implement `IMonitoringService`:
  - Calculate row count deltas (source vs destination per table)
  - Monitor replication lag (WAL position in MB + time lag in minutes)
  - Detect stalled replications
  - Determine **Cutover Ready** status:
    - All tables synced
    - Lag < configured thresholds (both MB and time)
    - No problematic tables
    - Stable for X seconds
  - Health status aggregation per mapping
  - Expose real-time metrics for UI consumption

**Deliverables:**
- `Services/MonitoringService.cs`
- `Services/DeltaCalculator.cs`
- `Services/CutoverReadinessService.cs` (GREEN LIGHT logic)
- Real-time metrics collection (5-second default refresh)

### 4.4 Cutover Instruction Generator
**Tasks:**
- Implement `ICutoverInstructionService`:
  - Generate step-by-step cutover instructions per mapping
  - Example output:
    ```
    Cutover Instructions for Mapping: Source1 ? Dest1
    ================================================================
    1. Shut down ALL applications using source database
    2. Verify no active connections: 
       SELECT * FROM pg_stat_activity WHERE datname IN ('db1', 'db2');
    3. Run this SQL on destination to finalize sequences:
       [Generated SQL script]
  4. Update application connection strings to point to destination
 5. Start applications
    6. Mark this mapping as COMPLETE in the UI to cleanup replication slots
    ```

**Deliverables:**
- `Services/CutoverInstructionService.cs`
- Dynamic instruction generation based on mapping state
- Display in UI when Cutover Ready

---

## Phase 5: Blazor UI Implementation (Week 10-11)

### 5.1 Dashboard Page (Main View)
**Tasks:**
- Create main dashboard (`Pages/Index.razor`):
  - Show all migration mappings in `PDGrid`
  - Columns:
    - Source Instance ? Destination Instance
    - Overall Status (badge with color)
    - **Cutover Ready** indicator (large green checkmark/button when ready)
    - Row count delta (aggregate across all databases)
    - WAL lag (both MB and minutes)
    - Last updated timestamp
- Real-time updates using `@onrender` with timer (5-second default, configurable)
  - Click row to drill into mapping details
- Use PanoramicData.Blazor components:
  - `PDGrid` for mapping list
  - `PDProgressBar` for sync progress
  - `PDAlert` for errors/warnings
  - `PDBadge` for status indicators

**Deliverables:**
- `Pages/Index.razor`
- `Services/UiStateService.cs` (state provider for UI, injected)
- Auto-refresh mechanism (configurable interval)
- Color-coded status badges

### 5.2 Mapping Detail View
**Tasks:**
- Create drill-down page (`Pages/MappingDetail.razor?mappingId=X`):
  - Header with mapping summary (source ? destination, overall status)
  - **Cutover Ready** green light prominent display
  - Tabs or sections for:
    - **Databases**: High-level list (X tables synced, Y in catchup, Z problematic)
    - **Tables**: Aggregated table status (not individual rows, just counts/status)
    - **Partitions**: Partition sync status
    - **Roles**: Role migration status
    - **Metrics**: 
      - Row count delta (aggregate)
      - WAL lag (MB and minutes, both displayed)
    - Replication slot info
 - **Logs**: Recent errors/warnings for this mapping
  - **Cutover Instructions**: Display when Cutover Ready (collapsible panel)
  - Use `PDTable` for structured data display
  - "Mark Complete" button (triggers cleanup, only enabled when safe)

**Deliverables:**
- `Pages/MappingDetail.razor`
- `Components/DatabaseStatusSummary.razor`
- `Components/MetricsDashboard.razor`
- `Components/CutoverInstructionsPanel.razor`
- `Components/MarkCompleteButton.razor` (with confirmation dialog)

### 5.3 Navigation & Layout
**Tasks:**
- Create layout with navigation
- Add configuration display (read-only, for verification)
- Add global status indicators (how many mappings total, how many cutover ready)
- Implement error notifications using `PDAlert` or `PDToast`
- No authentication required (network-level security assumed)

**Deliverables:**
- `Shared/MainLayout.razor`
- `Shared/NavMenu.razor`
- `Components/GlobalStatusBar.razor`
- `Pages/Configuration.razor` (read-only config display)

---

## Phase 6: Testing & Validation (Week 12-13)

### 6.1 Unit Tests
**Tasks:**
- Test configuration loading and validation
- Test schema discovery and comparison
- Test conflict detection logic
- Test DDL generation
- Test state re-discovery logic
- Mock Npgsql for isolated tests

**Deliverables:**
- Test project: `PanoramicData.PostgresMigrator.Tests`
- NUnit or xUnit test suite with 70%+ coverage
- Mocking infrastructure for Npgsql

### 6.2 Integration Tests & Test Harness
**Tasks:**
- Create **Test Harness** for automated testing
- Set up Docker Compose for **destination PostgreSQL 18** instances (ephemeral)
- Use **existing source instances** for testing (not ephemeral)
- Test scenarios:
  - Full database replication
  - Partition replication
  - Sequence synchronization
  - Role migration with conflicts
  - Extension validation (missing extension scenario)
  - Stateless restart (kill and restart tool, verify re-discovery)
  - "Mark complete" cleanup
  - Rate limiting effectiveness
- Test failure scenarios:
  - Network interruption (exponential backoff)
  - Partial table failure (continue with others)
  - Missing extensions (refuse to proceed)

**Deliverables:**
- `docker-compose.test.yml` for destination databases (PG 18)
- `TestHarness/` project with automated test scenarios
- Integration test suite
- Test documentation

### 6.3 Load & Performance Testing
**Tasks:**
- Test with multiple large databases simultaneously
- Test concurrent migrations (N:M mappings)
- Verify rate limiting works as expected
- Measure catchup performance

**Deliverables:**
- Performance test results document
- Rate limiting tuning recommendations
- Scalability notes

---

## Phase 7: Deployment & Documentation (Week 14)

### 7.1 Docker Support
**Tasks:**
- Update Dockerfile for Blazor Server
- Create docker-compose.yml example
- Document Kubernetes deployment
- Ensure stdout logging works in container environment

**Deliverables:**
- Production-ready Dockerfile
- Kubernetes YAML examples (single instance deployment)
- Environment variable documentation for K8s/Flux

### 7.2 Documentation
**Tasks:**
- Write comprehensive README.md
- Document all environment variables
- Create architecture diagrams (Mermaid or similar)
- Write troubleshooting guide
- Create user guide with screenshots
- Document cutover process
- MIT License file

**Deliverables:**
- `README.md`
- `docs/Architecture.md`
- `docs/Configuration.md`
- `docs/Troubleshooting.md`
- `docs/UserGuide.md`
- `docs/CutoverProcess.md`
- `LICENSE` (MIT)

### 7.3 CI/CD Pipeline
**Tasks:**
- Set up GitHub Actions workflow
- Automated build and test
- Docker image publishing (GitHub Container Registry or Docker Hub)
- Release automation

**Deliverables:**
- `.github/workflows/ci.yml`
- `.github/workflows/release.yml`
- Automated versioning

---

## Phase 8: Future Enhancements (Roadmap)

### 8.1 Additional Schema Objects
- Views, materialized views
- Functions, stored procedures
- Triggers
- Custom types (ENUMs, composite types, domains)

### 8.2 Notifications & Alerting
- Microsoft Teams notifications
- Webhook support for custom alerting
- Email notifications

### 8.3 Advanced Features
- Prometheus metrics export
- PostGIS support
- Advanced rate limiting (per-table, adaptive)
- Authentication for web UI (Azure AD)
- Multi-replica support (HA)

---

## Technology Stack Summary

| Component | Technology | Notes |
|-----------|-----------|-------|
| Framework | .NET 9, Blazor Server | |
| Database Driver | Npgsql 8.x | Latest stable |
| UI Library | PanoramicData.Blazor | |
| Configuration | Microsoft.Extensions.Configuration | Env vars + appsettings.json |
| Logging | Serilog | Stdout only initially |
| Background Services | IHostedService | MigrationOrchestrator |
| Resilience | Polly | Exponential backoff |
| Testing | xUnit or NUnit | Docker for destinations |
| Containerization | Docker, Kubernetes | On-premises, single instance |
| License | MIT | Open source |

---

## Key Architectural Decisions

| Decision | Rationale |
|----------|-----------|
| **Logical Replication** | Required for version upgrade (15?18), table-level granularity |
| **CREATE SUBSCRIPTION** | Most efficient for initial sync + continuous replication |
| **Database-level replication** | Simplifies config, ensures complete database consistency |
| **Stateless design** | Re-discover state on restart, no persistent storage needed |
| **Parallel sync** | Maximize throughput, databases and tables in parallel |
| **Rate limiting** | Minimize source database impact during migration |
| **Fail-safe conflicts** | Refuse destructive operations, require manual intervention |
| **Cutover Ready indicator** | Critical UX feature, clear green light for production switch |
| **No automatic cutover** | Safety: manual process with tool-provided instructions |
| **Serilog stdout** | Kubernetes-friendly, centralized log aggregation |
| **No authentication (v1)** | Network-level security, simplify initial implementation |

---

## Key Risks & Mitigations

| Risk | Mitigation |
|------|-----------|
| **Replication lag too high** | Monitor both MB and time lag, rate limiting, alert if thresholds exceeded |
| **Conflicting objects** | Fail-safe approach: refuse to overwrite, provide clear conflict reports |
| **Source database impact** | Rate limiting, parallel sync throttling, monitoring |
| **Partition handling complexity** | Test extensively, balk if issues detected |
| **Large database initial sync** | CREATE SUBSCRIPTION handles efficiently, document timelines |
| **PostgreSQL version differences** | Abstract version-specific queries, test with PG 15 and 18 |
| **Missing extensions** | Pre-flight check, refuse to proceed with clear error message |
| **Password migration failure** | Fallback to common password, alert user to change passwords |
| **Sequence value drift** | Continuous sync during replication, finalize in cutover script |

---

## Success Criteria

1. ? Zero data loss during migration
2. ? Zero downtime on source databases
3. ? All primary keys, foreign keys, and constraints correctly replicated
4. ? Partition definitions and contents identical
5. ? Sequence values synchronized
6. ? Roles and permissions replicated (with configurable conflict strategy)
7. ? Real-time UI updates showing accurate sync status (5-second refresh)
8. ? **Cutover Ready** green light indicator functional
9. ? Automatic conflict detection prevents destructive operations
10. ? Support for multiple concurrent migrations (N:M mappings)
11. ? Parallel sync with rate limiting
12. ? Stateless restart capability (re-discover state)
13. ? Kubernetes/Flux-friendly configuration via environment variables
14. ? Comprehensive logging (Serilog stdout)
15. ? Clean separation of concerns (testable services)
16. ? Production-ready Docker image
17. ? MIT License (open source)
18. ? Test harness with Docker for automated testing

---

## Next Steps

1. ? Review and approve this updated implementation plan
2. Set up development environment:
   - PostgreSQL 15+ source instances (existing)
   - PostgreSQL 18 destination instances (Docker for testing)
3. Begin Phase 1 implementation
4. Establish weekly checkpoint reviews
5. Create GitHub repository structure
6. Set up CI/CD pipeline early
