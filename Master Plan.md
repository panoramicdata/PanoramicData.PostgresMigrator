# PanoramicData.PostgresMigrator - Master Plan

## Project Overview

This project is designed to handle the seamless migration of multiple Production Postgres database servers on a lower version (e.g. 15) to later versions (e.g. 18) live and with zero downtime.

## Core Requirements

### Multi-Instance Migration (N:M)
- Handle multiple completely independent source-destination instance pairs
- Support N:M mapping (multiple sources to multiple destinations)
- Instance-to-instance level mapping (not database-specific)

### Zero-Downtime Migration
- Live replication using PostgreSQL 18's native logical replication
- Write-Ahead Log (WAL) based approach
- No schema changes permitted during migration on either source or destination

### Web UI (Blazor)
- Real-time, constantly-updating web interface
- Built with Blazor Server and PanoramicData.Blazor UI library
- Main dashboard showing current delta for each mapping
- Drill-down views showing status per table, partition, role

### Configuration
- Environment variables for Kubernetes/Flux compatibility
- Standard .NET configuration mechanism (appsettings.json for development)
- Server, Port, Username, Password (cleartext acceptable)
- Cold-start configuration loading (no hot-reload required)

### Conflict Handling
- Detect username, role, and database name conflicts
- **Refuse** to perform destructive operations on destination
- Require manual intervention for conflicts (fail-safe approach)
- Configurable merge/skip/fail for role conflicts (default: merge)

### Replication Scope
- **Database-level replication** (always)
- Tables, partitions, and indexes
- Sequences (including current values)
- Roles and permissions (all levels)
- Extensions validation (must exist on destination)

### Key Integrity
- Perfect replication of primary keys, foreign keys, constraints
- Partition definitions and contents must be identical

### In-Process Operations
- No external processes (e.g., pg_dump)
- All schema discovery and DDL generation handled in-system
- Tool has full admin permissions on both instances

### Destination Requirements
- PostgreSQL 18+ only
- Extensions must be pre-installed (tool validates and refuses to proceed if missing)

### Migration Lifecycle
1. Pre-flight checks (connections, conflicts, extensions)
2. Schema migration (tables, partitions, indexes)
3. Role migration (with password copying or common password fallback)
4. Permission replication (all levels)
5. Sequence synchronization
6. Data replication (using CREATE SUBSCRIPTION if possible)
7. Continuous sync with monitoring
8. **Cutover Ready** indicator (green light when ready)
9. Manual cutover (external to tool, following tool-provided instructions)
10. Post-cutover cleanup (mark mapping complete → cleanup replication slots)

### Status Indicators
- **Synced**: Replication up-to-date
- **In Catchup**: Initial sync or lagging
- **Problematic**: Errors detected
- **Cutover Ready**: GREEN LIGHT - ready for production switch
- Per-table/partition status tracking

### Parallelization
- Databases sync in parallel
- Tables and partitions sync in parallel (where possible)
- Rate limiting to minimize source database impact

### Failure Handling
- Continue with what can be synced if individual table fails
- Exponential backoff for transient connection failures
- Non-destructive to source (no rollback requirements)

### State Management
- Stateless design (desired state config only)
- Re-discover status following restarts
- No state persistence required

### Monitoring
- Real-time UI updates (5-second default, configurable)
- Display both row count delta and WAL lag metrics
- High-level aggregated status (X tables synced, Y in catchup)

### Roadmap Items (Future)
- Views, materialized views, functions, stored procedures, triggers
- Microsoft Teams notifications
- Custom type migration
- PostGIS support

## License
MIT License (open source)

---

## Next Steps
See `Implementation Plan.md` for detailed phased implementation approach.
