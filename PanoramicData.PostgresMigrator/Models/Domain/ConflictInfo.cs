namespace PanoramicData.PostgresMigrator.Models.Domain;

/// <summary>
/// Information about a detected conflict
/// </summary>
public class ConflictInfo
{
	/// <summary>
	/// Type of conflict
	/// </summary>
	public ConflictType ConflictType { get; set; }

	/// <summary>
	/// Name of the conflicting object
	/// </summary>
	public required string ObjectName { get; set; }

	/// <summary>
	/// Detailed description of the conflict
	/// </summary>
	public required string Description { get; set; }

	/// <summary>
	/// Suggested resolution action
	/// </summary>
	public string? SuggestedResolution { get; set; }

	/// <summary>
	/// Whether this conflict is blocking migration
	/// </summary>
	public bool IsBlocking { get; set; }

	/// <summary>
	/// Detection timestamp
	/// </summary>
	public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
}
