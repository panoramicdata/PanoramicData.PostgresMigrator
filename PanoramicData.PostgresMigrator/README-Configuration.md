# Configuration Guide

This document describes how to configure PanoramicData.PostgresMigrator using appsettings.json or environment variables.

## Configuration Methods

### 1. appsettings.json (Development)

During development, use `appsettings.Development.json`:

```json
{
  "PostgresMigrator": {
    "Instances": {
   "Source1": {
        "Server": "pg15-server.example.com",
        "Port": 5432,
    "Username": "replication_user",
"Password": "secure_password"
      },
      "Dest1": {
        "Server": "pg18-server.example.com",
        "Port": 5432,
        "Username": "admin_user",
    "Password": "secure_password"
      }
    },
    "Mappings": [
      {
    "SourceInstance": "Source1",
        "DestinationInstance": "Dest1",
     "RoleConflictStrategy": "Merge",
        "FallbackPassword": "changeme123"
      }
    ],
    "Replication": {
      "RateLimitMBps": 100,
    "UIRefreshIntervalSeconds": 5,
  "WalLagThresholdMB": 1024,
      "WalLagThresholdMinutes": 5,
      "CutoverReadyStabilitySeconds": 30,
      "MaxRetryAttempts": 5,
      "InitialRetryDelayMs": 1000
    }
  }
}
```

### 2. Environment Variables (Production/Kubernetes)

For Kubernetes/Flux deployments, use environment variables with the `PGMIGRATOR_` prefix:

#### Instance Configuration

```bash
# Source Instance 1
PGMIGRATOR_PostgresMigrator__Instances__Source1__Server=pg15-server.example.com
PGMIGRATOR_PostgresMigrator__Instances__Source1__Port=5432
PGMIGRATOR_PostgresMigrator__Instances__Source1__Username=replication_user
PGMIGRATOR_PostgresMigrator__Instances__Source1__Password=secure_password

# Destination Instance 1
PGMIGRATOR_PostgresMigrator__Instances__Dest1__Server=pg18-server.example.com
PGMIGRATOR_PostgresMigrator__Instances__Dest1__Port=5432
PGMIGRATOR_PostgresMigrator__Instances__Dest1__Username=admin_user
PGMIGRATOR_PostgresMigrator__Instances__Dest1__Password=secure_password
```

#### Mapping Configuration

```bash
# Mapping 1
PGMIGRATOR_PostgresMigrator__Mappings__0__SourceInstance=Source1
PGMIGRATOR_PostgresMigrator__Mappings__0__DestinationInstance=Dest1
PGMIGRATOR_PostgresMigrator__Mappings__0__RoleConflictStrategy=Merge
PGMIGRATOR_PostgresMigrator__Mappings__0__FallbackPassword=changeme123

# Mapping 2 (if multiple)
PGMIGRATOR_PostgresMigrator__Mappings__1__SourceInstance=Source2
PGMIGRATOR_PostgresMigrator__Mappings__1__DestinationInstance=Dest2
```

#### Replication Settings

```bash
PGMIGRATOR_PostgresMigrator__Replication__RateLimitMBps=100
PGMIGRATOR_PostgresMigrator__Replication__UIRefreshIntervalSeconds=5
PGMIGRATOR_PostgresMigrator__Replication__WalLagThresholdMB=1024
PGMIGRATOR_PostgresMigrator__Replication__WalLagThresholdMinutes=5
PGMIGRATOR_PostgresMigrator__Replication__CutoverReadyStabilitySeconds=30
PGMIGRATOR_PostgresMigrator__Replication__MaxRetryAttempts=5
PGMIGRATOR_PostgresMigrator__Replication__InitialRetryDelayMs=1000
```

## Configuration Reference

### Instances

Defines PostgreSQL instances (both source and destination).

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| Server | string | Yes | - | Hostname or IP address of PostgreSQL server |
| Port | int | No | 5432 | PostgreSQL port |
| Username | string | Yes | - | Username for authentication |
| Password | string | Yes | - | Password (cleartext acceptable) |

### Mappings

Defines source?destination instance pairs for migration.

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| SourceInstance | string | Yes | - | Source instance name (must exist in Instances) |
| DestinationInstance | string | Yes | - | Destination instance name (must exist in Instances) |
| RoleConflictStrategy | enum | No | Merge | How to handle role conflicts: Merge, Skip, or Fail |
| FallbackPassword | string | No | - | Password to use if role password migration fails |

### Replication Settings

Global settings for replication and monitoring.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| RateLimitMBps | int | 100 | Rate limit in megabytes per second |
| UIRefreshIntervalSeconds | int | 5 | How often UI refreshes (seconds) |
| WalLagThresholdMB | int | 1024 | WAL lag threshold in MB for alerting |
| WalLagThresholdMinutes | int | 5 | WAL lag threshold in minutes for alerting |
| CutoverReadyStabilitySeconds | int | 30 | How long lag must be stable before "Cutover Ready" |
| MaxRetryAttempts | int | 5 | Max retry attempts for transient failures |
| InitialRetryDelayMs | int | 1000 | Initial delay for exponential backoff (milliseconds) |

## Kubernetes Example

Example Kubernetes Secret and Deployment:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: postgres-migrator-config
type: Opaque
stringData:
  PGMIGRATOR_PostgresMigrator__Instances__Source1__Server: "pg15-prod.example.com"
  PGMIGRATOR_PostgresMigrator__Instances__Source1__Port: "5432"
  PGMIGRATOR_PostgresMigrator__Instances__Source1__Username: "replication_user"
  PGMIGRATOR_PostgresMigrator__Instances__Source1__Password: "super_secret_password"
  PGMIGRATOR_PostgresMigrator__Instances__Dest1__Server: "pg18-new.example.com"
  PGMIGRATOR_PostgresMigrator__Instances__Dest1__Port: "5432"
  PGMIGRATOR_PostgresMigrator__Instances__Dest1__Username: "admin_user"
  PGMIGRATOR_PostgresMigrator__Instances__Dest1__Password: "another_secret_password"
  PGMIGRATOR_PostgresMigrator__Mappings__0__SourceInstance: "Source1"
  PGMIGRATOR_PostgresMigrator__Mappings__0__DestinationInstance: "Dest1"
  PGMIGRATOR_PostgresMigrator__Mappings__0__RoleConflictStrategy: "Merge"
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: postgres-migrator
spec:
  replicas: 1
  selector:
    matchLabels:
      app: postgres-migrator
  template:
    metadata:
      labels:
        app: postgres-migrator
    spec:
   containers:
    - name: migrator
 image: panoramicdata/postgres-migrator:latest
        ports:
        - containerPort: 8080
        envFrom:
   - secretRef:
            name: postgres-migrator-config
```

## Validation

Configuration is validated on startup. The application will fail to start if:

- No instances are configured
- No mappings are configured
- A mapping references an unknown instance
- A mapping has the same source and destination
- Instance credentials are missing or invalid
- Port numbers are out of range

Check logs for validation errors.

## Security Notes

- Passwords are stored in cleartext in configuration (acceptable per requirements)
- For production, use Kubernetes Secrets or Azure Key Vault
- Network-level security is assumed (no authentication on web UI)
- Ensure PostgreSQL instances are accessible from the migrator pod/container

## Example N:M Configuration

Example with multiple source and destination instances:

```json
{
  "PostgresMigrator": {
    "Instances": {
      "Source1": { "Server": "pg15-prod1.local", "Port": 5432, "Username": "user1", "Password": "pass1" },
      "Source2": { "Server": "pg15-prod2.local", "Port": 5432, "Username": "user2", "Password": "pass2" },
      "Source3": { "Server": "pg15-prod3.local", "Port": 5432, "Username": "user3", "Password": "pass3" },
      "Dest1": { "Server": "pg18-new1.local", "Port": 5432, "Username": "admin1", "Password": "admin_pass1" },
      "Dest2": { "Server": "pg18-new2.local", "Port": 5432, "Username": "admin2", "Password": "admin_pass2" }
    },
    "Mappings": [
      { "SourceInstance": "Source1", "DestinationInstance": "Dest1" },
      { "SourceInstance": "Source2", "DestinationInstance": "Dest1" },
      { "SourceInstance": "Source3", "DestinationInstance": "Dest2" }
    ]
  }
}
```

This configures:
- 3 source instances ? 2 destination instances
- Source1 and Source2 both migrate to Dest1
- Source3 migrates to Dest2

---

For more information, see the [Master Plan](../Master%20Plan.md) and [Implementation Plan](../Implementation%20Plan.md).
