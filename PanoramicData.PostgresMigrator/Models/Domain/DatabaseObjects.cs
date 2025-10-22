namespace PanoramicData.PostgresMigrator.Models.Domain;

/// <summary>
/// Information about a database
/// </summary>
public class DatabaseInfo
{
	/// <summary>
	/// Database name
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Owner role
	/// </summary>
	public string? Owner { get; set; }

	/// <summary>
	/// Encoding (e.g., UTF8)
	/// </summary>
	public string? Encoding { get; set; }

	/// <summary>
	/// Tables in this database
	/// </summary>
	public List<TableInfo> Tables { get; set; } = [];

	/// <summary>
	/// Sequences in this database
	/// </summary>
	public List<SequenceInfo> Sequences { get; set; } = [];

	/// <summary>
	/// Extensions in this database
	/// </summary>
	public List<ExtensionInfo> Extensions { get; set; } = [];

	/// <summary>
	/// Sync status for this database
	/// </summary>
	public SyncStatus Status { get; set; } = SyncStatus.NotStarted;

	/// <summary>
	/// Last update timestamp
	/// </summary>
	public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Information about a table
/// </summary>
public class TableInfo
{
	/// <summary>
	/// Schema name (e.g., "public")
	/// </summary>
	public required string Schema { get; set; }

	/// <summary>
	/// Table name
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Fully qualified table name (schema.table)
	/// </summary>
	public string FullName => $"{Schema}.{Name}";

	/// <summary>
	/// Whether this table is partitioned
	/// </summary>
	public bool IsPartitioned { get; set; }

	/// <summary>
	/// Partitions (if partitioned)
	/// </summary>
	public List<PartitionInfo> Partitions { get; set; } = [];

	/// <summary>
	/// Estimated row count (from pg_class.reltuples)
	/// </summary>
	public long EstimatedRowCount { get; set; }

	/// <summary>
	/// Actual row count (expensive query, only if needed)
	/// </summary>
	public long? ActualRowCount { get; set; }

	/// <summary>
	/// Sync status
	/// </summary>
	public SyncStatus Status { get; set; } = SyncStatus.NotStarted;

	/// <summary>
	/// Last update timestamp
	/// </summary>
	public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Information about a partition
/// </summary>
public class PartitionInfo
{
	/// <summary>
	/// Schema name
	/// </summary>
	public required string Schema { get; set; }

	/// <summary>
	/// Partition name
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Fully qualified partition name
	/// </summary>
	public string FullName => $"{Schema}.{Name}";

	/// <summary>
	/// Parent table name
	/// </summary>
	public string? ParentTable { get; set; }

	/// <summary>
	/// Partition strategy (RANGE, LIST, HASH)
	/// </summary>
	public string? PartitionStrategy { get; set; }

	/// <summary>
	/// Partition expression/bounds
	/// </summary>
	public string? PartitionExpression { get; set; }

	/// <summary>
	/// Estimated row count
	/// </summary>
	public long EstimatedRowCount { get; set; }

	/// <summary>
	/// Sync status
	/// </summary>
	public SyncStatus Status { get; set; } = SyncStatus.NotStarted;

	/// <summary>
	/// Last update timestamp
	/// </summary>
	public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Information about a sequence
/// </summary>
public class SequenceInfo
{
	/// <summary>
	/// Schema name
	/// </summary>
	public required string Schema { get; set; }

	/// <summary>
	/// Sequence name
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Fully qualified sequence name
	/// </summary>
	public string FullName => $"{Schema}.{Name}";

	/// <summary>
	/// Current value on source
	/// </summary>
	public long? CurrentValueSource { get; set; }

	/// <summary>
	/// Current value on destination
	/// </summary>
	public long? CurrentValueDestination { get; set; }

	/// <summary>
	/// Data type (e.g., bigint, integer)
	/// </summary>
	public string? DataType { get; set; }

	/// <summary>
	/// Increment value
	/// </summary>
	public long? IncrementBy { get; set; }

	/// <summary>
	/// Sync status
	/// </summary>
	public SyncStatus Status { get; set; } = SyncStatus.NotStarted;

	/// <summary>
	/// Last update timestamp
	/// </summary>
	public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Information about a PostgreSQL extension
/// </summary>
public class ExtensionInfo
{
	/// <summary>
	/// Extension name (e.g., "uuid-ossp", "pg_trgm")
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Extension version
	/// </summary>
	public string? Version { get; set; }

	/// <summary>
	/// Schema where extension is installed
	/// </summary>
	public string? Schema { get; set; }

	/// <summary>
	/// Whether extension exists on destination
	/// </summary>
	public bool ExistsOnDestination { get; set; }
}

/// <summary>
/// Information about a role/user
/// </summary>
public class RoleInfo
{
	/// <summary>
	/// Role name
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Whether this is a superuser
	/// </summary>
	public bool IsSuperuser { get; set; }

	/// <summary>
	/// Whether this role can login
	/// </summary>
	public bool CanLogin { get; set; }

	/// <summary>
	/// Password hash (if accessible)
	/// </summary>
	public string? PasswordHash { get; set; }

	/// <summary>
	/// Whether password was successfully migrated
	/// </summary>
	public bool PasswordMigrated { get; set; }

	/// <summary>
	/// Whether role exists on destination
	/// </summary>
	public bool ExistsOnDestination { get; set; }

	/// <summary>
	/// Sync status
	/// </summary>
	public SyncStatus Status { get; set; } = SyncStatus.NotStarted;

	/// <summary>
	/// Last update timestamp
	/// </summary>
	public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
