using Npgsql;

namespace PanoramicData.PostgresMigrator.Interfaces;

/// <summary>
/// Factory for creating PostgreSQL connections
/// </summary>
public interface IPostgresConnectionFactory
{
	/// <summary>
	/// Create a connection to a PostgreSQL instance
	/// </summary>
	/// <param name="instanceName">Name of the instance from configuration</param>
	/// <param name="database">Optional specific database to connect to</param>
	/// <returns>Open NpgsqlConnection</returns>
	Task<NpgsqlConnection> CreateConnectionAsync(string instanceName, string? database = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Test connection to an instance
	/// </summary>
	/// <param name="instanceName">Name of the instance from configuration</param>
	/// <returns>True if connection successful</returns>
	Task<bool> TestConnectionAsync(string instanceName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Get list of all databases on an instance
	/// </summary>
	/// <param name="instanceName">Name of the instance from configuration</param>
	/// <returns>List of database names</returns>
	Task<List<string>> GetDatabasesAsync(string instanceName, CancellationToken cancellationToken = default);
}
