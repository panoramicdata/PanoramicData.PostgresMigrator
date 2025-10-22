using Microsoft.Extensions.Options;
using Npgsql;
using PanoramicData.PostgresMigrator.Interfaces;
using PanoramicData.PostgresMigrator.Models.Configuration;
using Polly;
using Polly.Retry;

namespace PanoramicData.PostgresMigrator.Services;

/// <summary>
/// Factory for creating and managing PostgreSQL connections with retry policies
/// </summary>
public class PostgresConnectionFactory : IPostgresConnectionFactory
{
	private readonly PostgresMigratorConfig _config;
	private readonly ILogger<PostgresConnectionFactory> _logger;
	private readonly AsyncRetryPolicy _retryPolicy;

	public PostgresConnectionFactory(
		IOptions<PostgresMigratorConfig> config,
		ILogger<PostgresConnectionFactory> logger)
	{
		_config = config.Value;
		_logger = logger;

		// Configure exponential backoff retry policy using Polly
		_retryPolicy = Policy
			.Handle<NpgsqlException>()
			.Or<TimeoutException>()
			.WaitAndRetryAsync(
				retryCount: _config.Replication.MaxRetryAttempts,
				sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(
					_config.Replication.InitialRetryDelayMs * Math.Pow(2, attempt - 1)),
				onRetry: (exception, timeSpan, attempt, context) =>
				{
					_logger.LogWarning(exception,
						"Connection attempt {Attempt} failed. Retrying in {RetryDelay}ms",
						attempt, timeSpan.TotalMilliseconds);
				});
	}

	public async Task<NpgsqlConnection> CreateConnectionAsync(
		string instanceName,
		string? database = null,
		CancellationToken cancellationToken = default)
	{
		if (!_config.Instances.TryGetValue(instanceName, out var instanceConfig))
		{
			throw new InvalidOperationException($"Instance '{instanceName}' not found in configuration");
		}

		var connectionString = instanceConfig.GetConnectionString(database);

		_logger.LogDebug("Creating connection to {Instance} {Database}",
			instanceName, database ?? "(default)");

		return await _retryPolicy.ExecuteAsync(async () =>
		{
			var connection = new NpgsqlConnection(connectionString);
			await connection.OpenAsync(cancellationToken);

			_logger.LogInformation("Connected to {Instance} (PostgreSQL {Version}) {Database}",
				instanceName, connection.PostgreSqlVersion, database ?? "postgres");

			return connection;
		});
	}

	public async Task<bool> TestConnectionAsync(
		string instanceName,
		CancellationToken cancellationToken = default)
	{
		try
		{
			await using var connection = await CreateConnectionAsync(instanceName, null, cancellationToken);
			await using var cmd = new NpgsqlCommand("SELECT 1", connection);
			await cmd.ExecuteScalarAsync(cancellationToken);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Connection test failed for instance {Instance}", instanceName);
			return false;
		}
	}

	public async Task<List<string>> GetDatabasesAsync(
		string instanceName,
		CancellationToken cancellationToken = default)
	{
		await using var connection = await CreateConnectionAsync(instanceName, "postgres", cancellationToken);

		var databases = new List<string>();

		await using var cmd = new NpgsqlCommand(@"
			SELECT datname 
			FROM pg_database 
			WHERE datistemplate = false 
			AND datname NOT IN ('postgres', 'template0', 'template1')
			ORDER BY datname", connection);

		await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
		while (await reader.ReadAsync(cancellationToken))
		{
			databases.Add(reader.GetString(0));
		}

		_logger.LogInformation("Found {Count} databases on {Instance}: {Databases}",
			databases.Count, instanceName, string.Join(", ", databases));

		return databases;
	}
}
