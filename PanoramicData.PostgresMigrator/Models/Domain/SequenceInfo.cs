namespace PanoramicData.PostgresMigrator.Models.Domain;

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
