# PanoramicData.PostgresMigrator

A zero-downtime PostgreSQL database migration tool for migrating multiple production databases from PostgreSQL 15+ to PostgreSQL 18+ with real-time monitoring and replication status tracking.

## ?? Purpose

This tool enables live migration of PostgreSQL databases across major version upgrades without downtime, using PostgreSQL's native logical replication capabilities. It supports N:M instance-to-instance mappings and provides a real-time web UI to monitor migration progress.

## ? Key Features

- **Zero Downtime**: Live replication using PostgreSQL logical replication
- **Multi-Instance Support**: Handle N:M source-to-destination instance pairs simultaneously
- **Real-Time Monitoring**: Blazor-based web UI with live status updates
- **Cutover Ready Indicator**: Clear green light when migration is ready for production switch
- **Stateless Design**: Re-discovers state on restart, no persistent storage required
- **Fail-Safe**: Refuses destructive operations, requires manual intervention for conflicts
- **Parallel Processing**: Databases, tables, and partitions sync in parallel with rate limiting
- **Kubernetes-Ready**: Configuration via environment variables for cloud-native deployments

## ?? Documentation

- **[Master Plan](Master%20Plan.md)** - High-level requirements and core principles
- **[Implementation Plan](Implementation%20Plan.md)** - Detailed 8-phase development roadmap
- **[Answers to Questions](Answers%20to%20Questions.md)** - Architectural decisions and clarifications

## ?? Technology Stack

- **.NET 9** - Blazor Server application
- **Npgsql** - PostgreSQL .NET driver
- **PanoramicData.Blazor** - UI component library
- **Serilog** - Structured logging
- **Polly** - Resilience and retry policies
- **Docker** - Containerization
- **Kubernetes** - Orchestration-ready

## ?? Current Status

**Phase**: Planning Complete ?  
**Next**: Phase 1 - Foundation & Architecture Setup

## ?? What Gets Migrated

### Version 1 (Current Scope)
- ? Tables (including partitioned tables)
- ? Partitions (definitions and data)
- ? Indexes
- ? Sequences (definitions and current values)
- ? Roles and users
- ? Permissions (all levels)
- ? Primary keys, foreign keys, constraints

### Future Roadmap
- ?? Views and materialized views
- ?? Functions and stored procedures
- ?? Triggers
- ?? Custom types
- ?? PostGIS support
- ?? Microsoft Teams notifications

## ?? How It Works

1. **Configure** - Define source and destination instance mappings via environment variables or appsettings.json
2. **Pre-Flight** - Tool validates connections, detects conflicts, checks for required extensions
3. **Schema Migration** - Replicates table structures, partitions, and indexes
4. **Replication Setup** - Creates logical replication publications and subscriptions
5. **Initial Sync** - PostgreSQL handles initial data copy via CREATE SUBSCRIPTION
6. **Continuous Sync** - Tool monitors WAL lag and replication progress
7. **Cutover Ready** - Green light appears when lag is minimal and stable
8. **Manual Cutover** - Follow tool-provided instructions to switch production traffic
9. **Cleanup** - Mark mapping complete to cleanup replication slots

## ?? Safety Features

- **Non-Destructive**: Never modifies source databases (read-only access)
- **Conflict Detection**: Refuses to proceed if conflicts detected (database names, roles, etc.)
- **Extension Validation**: Ensures all required extensions exist on destination before proceeding
- **Fail-Safe**: Continues with what can be synced if individual tables fail
- **Rate Limiting**: Configurable throttling to minimize source database impact

## ?? Monitoring

The web UI provides:
- **Dashboard**: Overview of all migration mappings
- **Detail Views**: Per-mapping drill-down with table/partition status
- **Dual Metrics**: Both row count delta and WAL lag (MB + minutes)
- **Live Updates**: Configurable refresh interval (default 5 seconds)
- **Cutover Instructions**: Step-by-step guide when ready

## ?? Deployment

- **Docker**: Production-ready container
- **Kubernetes**: On-premises deployment support
- **Flux-Friendly**: Environment variable configuration
- **Single Instance**: One deployment per migration project

## ?? License

MIT License - See [LICENSE](LICENSE) for details

## ?? Contributing

This is an open-source project. Contributions welcome!

## ?? Support

For issues, questions, or feature requests, please open an issue on GitHub.

---

**Status**: In active development  
**Target PostgreSQL Versions**: Source 15+, Destination 18+ only