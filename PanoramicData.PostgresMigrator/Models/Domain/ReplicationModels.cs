namespace PanoramicData.PostgresMigrator.Models.Domain;

/// <summary>
/// Information about a replication slot
/// </summary>
public class ReplicationSlotInfo
{
	/// <summary>
	/// Slot name
	/// </summary>
	public required string SlotName { get; set; }

	/// <summary>
	/// Plugin used (e.g., "pgoutput")
	/// </summary>
	public string? Plugin { get; set; }

	/// <summary>
	/// Current WAL position (LSN) on source
	/// </summary>
	public string? CurrentWalPosition { get; set; }

	/// <summary>
	/// Confirmed flush LSN (destination acknowledged position)
	/// </summary>
	public string? ConfirmedFlushLsn { get; set; }

	/// <summary>
	/// WAL lag in bytes
	/// </summary>
	public long WalLagBytes { get; set; }

	/// <summary>
	/// WAL lag in megabytes
	/// </summary>
	public double WalLagMB => WalLagBytes / 1024.0 / 1024.0;

	/// <summary>
	/// Estimated time lag in minutes
	/// </summary>
	public double? TimeLagMinutes { get; set; }

	/// <summary>
	/// Whether lag is within acceptable thresholds
	/// </summary>
	public bool IsLagAcceptable { get; set; }

	/// <summary>
	/// Last update timestamp
	/// </summary>
	public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
