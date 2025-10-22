namespace PanoramicData.PostgresMigrator.Models.Domain;

/// <summary>
/// Type of conflict detected
/// </summary>
public enum ConflictType
{
	/// <summary>
	/// Database name already exists on destination
	/// </summary>
	DatabaseNameConflict,

	/// <summary>
	/// Role/username already exists on destination
	/// </summary>
	RoleNameConflict,

	/// <summary>
	/// Schema object name conflict
	/// </summary>
	SchemaObjectConflict,

	/// <summary>
	/// Required extension missing on destination
	/// </summary>
	MissingExtension,

	/// <summary>
	/// Active connections on destination database
	/// </summary>
	ActiveConnectionsOnDestination,

	/// <summary>
	/// Other conflict type
	/// </summary>
	Other
}
