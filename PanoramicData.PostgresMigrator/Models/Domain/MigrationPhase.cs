namespace PanoramicData.PostgresMigrator.Models.Domain;

/// <summary>
/// Migration phase
/// </summary>
public enum MigrationPhase
{
	NotStarted,
	PreFlight,
	SchemaDiscovery,
	ConflictDetection,
	SchemaMigration,
	SequenceMigration,
	RoleMigration,
	PermissionMigration,
	ReplicationSetup,
	InitialDataSync,
	ContinuousSync,
	CutoverReady,
	Completed
}
