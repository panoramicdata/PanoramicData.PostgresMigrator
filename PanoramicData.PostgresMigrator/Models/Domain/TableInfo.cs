namespace PanoramicData.PostgresMigrator.Models.Domain;

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
