namespace PanoramicData.PostgresMigrator.Models.Domain;

/// <summary>
/// Information about a PostgreSQL extension
/// </summary>
public class ExtensionInfo
{
	/// <summary>
	/// Extension name (e.g., "uuid-ossp", "pg_trgm")
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Extension version
	/// </summary>
	public string? Version { get; set; }

	/// <summary>
	/// Schema where extension is installed
	/// </summary>
	public string? Schema { get; set; }

	/// <summary>
	/// Whether extension exists on destination
	/// </summary>
	public bool ExistsOnDestination { get; set; }
}
