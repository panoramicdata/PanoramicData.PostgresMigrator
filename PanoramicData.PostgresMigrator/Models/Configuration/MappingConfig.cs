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

/// <summary>
/// Mapping configuration between source and destination instances
/// </summary>
public class MappingConfig
{
	/// <summary>
	/// Name of the source instance (must match key in Instances dictionary)
	/// </summary>
	public required string SourceInstance { get; set; }

	/// <summary>
	/// Name of the destination instance (must match key in Instances dictionary)
	/// </summary>
	public required string DestinationInstance { get; set; }

	/// <summary>
	/// Strategy for handling role conflicts (default: Merge)
	/// </summary>
	public RoleConflictStrategy RoleConflictStrategy { get; set; } = RoleConflictStrategy.Merge;

	/// <summary>
	/// Optional fallback password if role password migration fails
	/// </summary>
	public string? FallbackPassword { get; set; }

	/// <summary>
	/// Get unique identifier for this mapping
	/// </summary>
	public string GetMappingId() => $"{SourceInstance}->{DestinationInstance}";
}
