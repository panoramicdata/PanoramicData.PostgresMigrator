using Npgsql;
using PanoramicData.PostgresMigrator.Interfaces;

namespace PanoramicData.PostgresMigrator.Services;

/// <summary>
/// Service for validating PostgreSQL extensions
/// </summary>
public class ExtensionValidationService(
	ISchemaDiscoveryService schemaDiscovery,
	IPostgresConnectionFactory connectionFactory,
	ILogger<ExtensionValidationService> logger) : IExtensionValidationService
{
	public async Task<List<string>> ValidateExtensionsAsync(
		string sourceName,
		string destinationName,
		CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Validating extensions for {Source} ? {Destination}",
			sourceName, destinationName);

		var errors = new List<string>();

		// Get all databases from source
		var sourceDatabases = await schemaDiscovery.DiscoverDatabasesAsync(sourceName, cancellationToken);

		// Get available extensions on destination
		var availableExtensions = await GetAvailableExtensionsAsync(destinationName, cancellationToken);
		var availableExtensionSet = availableExtensions.ToHashSet();

		// Check each source database for required extensions
		foreach (var sourceDb in sourceDatabases)
		{
			var sourceExtensions = await schemaDiscovery.DiscoverExtensionsAsync(sourceName, sourceDb.Name, cancellationToken);

			foreach (var extension in sourceExtensions)
			{
				if (!availableExtensionSet.Contains(extension.Name))
				{
					var error = $"Extension '{extension.Name}' (required by database '{sourceDb.Name}') is not available on destination instance '{destinationName}'";
					errors.Add(error);
					logger.LogError("{Error}", error);
				}
			}
		}

		if (errors.Count == 0)
		{
			logger.LogInformation("All required extensions are available on destination");
		}
		else
		{
			logger.LogError("Found {Count} missing extensions on destination", errors.Count);
		}

		return errors;
	}

	public async Task<List<string>> GetAvailableExtensionsAsync(
		string instanceName,
		CancellationToken cancellationToken = default)
	{
		logger.LogDebug("Getting available extensions on {Instance}", instanceName);

		await using var connection = await connectionFactory.CreateConnectionAsync(instanceName, "postgres", cancellationToken);

		var extensions = new List<string>();

		await using var cmd = new NpgsqlCommand(@"
			SELECT name 
			FROM pg_available_extensions 
			ORDER BY name", connection);

		await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
		while (await reader.ReadAsync(cancellationToken))
		{
			extensions.Add(reader.GetString(0));
		}

		logger.LogDebug("Found {Count} available extensions on {Instance}",
			extensions.Count, instanceName);

		return extensions;
	}
}
