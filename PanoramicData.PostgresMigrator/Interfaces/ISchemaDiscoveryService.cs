using PanoramicData.PostgresMigrator.Models.Domain;

namespace PanoramicData.PostgresMigrator.Interfaces;

/// <summary>
/// Service for discovering schema objects in PostgreSQL databases
/// </summary>
public interface ISchemaDiscoveryService
{
	/// <summary>
	/// Discover all databases on an instance
	/// </summary>
	Task<List<DatabaseInfo>> DiscoverDatabasesAsync(string instanceName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Discover tables in a specific database
	/// </summary>
	Task<List<TableInfo>> DiscoverTablesAsync(string instanceName, string database, CancellationToken cancellationToken = default);

	/// <summary>
	/// Discover partitions for a partitioned table
	/// </summary>
	Task<List<PartitionInfo>> DiscoverPartitionsAsync(string instanceName, string database, string schema, string tableName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Discover sequences in a database
	/// </summary>
	Task<List<SequenceInfo>> DiscoverSequencesAsync(string instanceName, string database, CancellationToken cancellationToken = default);

	/// <summary>
	/// Discover roles/users on an instance
	/// </summary>
	Task<List<RoleInfo>> DiscoverRolesAsync(string instanceName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Discover extensions in a database
	/// </summary>
	Task<List<ExtensionInfo>> DiscoverExtensionsAsync(string instanceName, string database, CancellationToken cancellationToken = default);
}
