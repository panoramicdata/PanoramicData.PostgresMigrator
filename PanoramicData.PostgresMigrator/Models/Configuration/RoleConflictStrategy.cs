namespace PanoramicData.PostgresMigrator.Models.Configuration;

/// <summary>
/// Strategy for handling role conflicts on destination
/// </summary>
public enum RoleConflictStrategy
{
	/// <summary>
	/// Use existing role if it exists (default)
	/// </summary>
	Merge,

	/// <summary>
	/// Skip role migration if it exists
	/// </summary>
	Skip,

	/// <summary>
	/// Fail migration if role conflict detected
	/// </summary>
	Fail
}
