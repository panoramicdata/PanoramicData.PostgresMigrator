namespace PanoramicData.PostgresMigrator.Models.Domain;

/// <summary>
/// Synchronization status for a mapping, database, table, or partition
/// </summary>
public enum SyncStatus
{
	/// <summary>
	/// Migration has not started
	/// </summary>
	NotStarted,

	/// <summary>
	/// Pre-flight checks in progress
	/// </summary>
	PreFlightCheck,

	/// <summary>
	/// Schema migration in progress
	/// </summary>
	SchemaMigration,

	/// <summary>
	/// Role migration in progress
	/// </summary>
	RoleMigration,

	/// <summary>
	/// Replication setup in progress
	/// </summary>
	ReplicationSetup,

	/// <summary>
	/// Initial data sync in progress
	/// </summary>
	InitialSync,

	/// <summary>
	/// Continuous replication in progress (catching up)
	/// </summary>
	InCatchup,

	/// <summary>
	/// Fully synced (lag within acceptable thresholds)
	/// </summary>
	Synced,

	/// <summary>
	/// Ready for cutover (lag minimal and stable)
	/// </summary>
	CutoverReady,

	/// <summary>
	/// Error or replication issue detected
	/// </summary>
	Problematic,

	/// <summary>
	/// Migration marked as complete (cleanup in progress or done)
	/// </summary>
	Completed,

	/// <summary>
	/// Migration failed
	/// </summary>
	Failed
}
