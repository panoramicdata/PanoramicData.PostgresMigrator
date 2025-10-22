namespace PanoramicData.PostgresMigrator.Models.Domain;

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
