using PanoramicData.PostgresMigrator.Models.Domain;

namespace PanoramicData.PostgresMigrator.Services;

/// <summary>
/// Service for detecting conflicts between source and destination
/// </summary>
public interface IConflictDetectionService
{
	/// <summary>
	/// Detect all conflicts for a mapping
	/// </summary>
	Task<List<ConflictInfo>> DetectConflictsAsync(
		string sourceName,
		string destinationName,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Detect database name conflicts
	/// </summary>
	Task<List<ConflictInfo>> DetectDatabaseConflictsAsync(
		string sourceName,
		string destinationName,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Detect role name conflicts
	/// </summary>
	Task<List<ConflictInfo>> DetectRoleConflictsAsync(
		string sourceName,
		string destinationName,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Detect missing extensions on destination
	/// </summary>
	Task<List<ConflictInfo>> DetectMissingExtensionsAsync(
		string sourceName,
		string destinationName,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Check if destination database has active connections
	/// </summary>
	Task<List<ConflictInfo>> DetectActiveConnectionsAsync(
		string destinationName,
		string database,
		CancellationToken cancellationToken = default);
}
