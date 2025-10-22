namespace PanoramicData.PostgresMigrator.Models.Domain;

/// <summary>
/// Represents a migration mapping between source and destination instances
/// </summary>
public class MigrationMapping
{
	/// <summary>
	/// Unique identifier for this mapping
	/// </summary>
	public required string MappingId { get; set; }

	/// <summary>
	/// Source instance name
	/// </summary>
	public required string SourceInstance { get; set; }

	/// <summary>
	/// Destination instance name
	/// </summary>
	public required string DestinationInstance { get; set; }

	/// <summary>
	/// Current migration phase
	/// </summary>
	public MigrationPhase CurrentPhase { get; set; } = MigrationPhase.NotStarted;

	/// <summary>
	/// Overall sync status
	/// </summary>
	public SyncStatus Status { get; set; } = SyncStatus.NotStarted;

	/// <summary>
	/// List of databases discovered on source
	/// </summary>
	public List<DatabaseInfo> Databases { get; set; } = [];

	/// <summary>
	/// Detected conflicts (if any)
	/// </summary>
	public List<ConflictInfo> Conflicts { get; set; } = [];

	/// <summary>
	/// Replication slot information
	/// </summary>
	public ReplicationSlotInfo? ReplicationSlot { get; set; }

	/// <summary>
	/// Last status update timestamp
	/// </summary>
	public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

	/// <summary>
	/// Error message (if in problematic/failed state)
	/// </summary>
	public string? ErrorMessage { get; set; }

	/// <summary>
	/// Whether cut-over ready indicator is shown
	/// </summary>
	public bool IsCutOverReady => Status == SyncStatus.CutoverReady;
}
