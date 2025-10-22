namespace PanoramicData.PostgresMigrator.Models.Domain;

/// <summary>
/// Information about a role/user
/// </summary>
public class RoleInfo
{
	/// <summary>
	/// Role name
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Whether this is a superuser
	/// </summary>
	public bool IsSuperuser { get; set; }

	/// <summary>
	/// Whether this role can login
	/// </summary>
	public bool CanLogin { get; set; }

	/// <summary>
	/// Password hash (if accessible)
	/// </summary>
	public string? PasswordHash { get; set; }

	/// <summary>
	/// Whether password was successfully migrated
	/// </summary>
	public bool PasswordMigrated { get; set; }

	/// <summary>
	/// Whether role exists on destination
	/// </summary>
	public bool ExistsOnDestination { get; set; }

	/// <summary>
	/// Sync status
	/// </summary>
	public SyncStatus Status { get; set; } = SyncStatus.NotStarted;

	/// <summary>
	/// Last update timestamp
	/// </summary>
	public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
