using PanoramicData.PostgresMigrator.Models.Domain;

namespace PanoramicData.PostgresMigrator.Services;

/// <summary>
/// Service for detecting conflicts between source and destination
/// </summary>
public class ConflictDetectionService : IConflictDetectionService
{
	private readonly ISchemaDiscoveryService _schemaDiscovery;
	private readonly IPostgresConnectionFactory _connectionFactory;
	private readonly ILogger<ConflictDetectionService> _logger;

	public ConflictDetectionService(
		ISchemaDiscoveryService schemaDiscovery,
		IPostgresConnectionFactory connectionFactory,
		ILogger<ConflictDetectionService> logger)
	{
		_schemaDiscovery = schemaDiscovery;
		_connectionFactory = connectionFactory;
		_logger = logger;
	}

	public async Task<List<ConflictInfo>> DetectConflictsAsync(
		string sourceName,
		string destinationName,
		CancellationToken cancellationToken = default)
	{
		_logger.LogInformation("Detecting conflicts between {Source} and {Destination}",
			sourceName, destinationName);

		var allConflicts = new List<ConflictInfo>();

		// Detect all types of conflicts
		var databaseConflicts = await DetectDatabaseConflictsAsync(sourceName, destinationName, cancellationToken);
		var roleConflicts = await DetectRoleConflictsAsync(sourceName, destinationName, cancellationToken);
		var extensionConflicts = await DetectMissingExtensionsAsync(sourceName, destinationName, cancellationToken);

		allConflicts.AddRange(databaseConflicts);
		allConflicts.AddRange(roleConflicts);
		allConflicts.AddRange(extensionConflicts);

		var blockingCount = allConflicts.Count(c => c.IsBlocking);
		_logger.LogInformation("Detected {Total} conflicts ({Blocking} blocking) for {Source} ? {Destination}",
			allConflicts.Count, blockingCount, sourceName, destinationName);

		return allConflicts;
	}

	public async Task<List<ConflictInfo>> DetectDatabaseConflictsAsync(
		string sourceName,
		string destinationName,
		CancellationToken cancellationToken = default)
	{
		var conflicts = new List<ConflictInfo>();

		var sourceDatabases = await _schemaDiscovery.DiscoverDatabasesAsync(sourceName, cancellationToken);
		var destDatabases = await _schemaDiscovery.DiscoverDatabasesAsync(destinationName, cancellationToken);

		var destDatabaseNames = destDatabases.Select(d => d.Name).ToHashSet();

		foreach (var sourceDb in sourceDatabases)
		{
			if (destDatabaseNames.Contains(sourceDb.Name))
			{
				conflicts.Add(new ConflictInfo
				{
					ConflictType = ConflictType.DatabaseNameConflict,
					ObjectName = sourceDb.Name,
					Description = $"Database '{sourceDb.Name}' already exists on destination instance",
					SuggestedResolution = "Manual intervention required: Drop database on destination or rename source database",
					IsBlocking = true
				});
			}
		}

		return conflicts;
	}

	public async Task<List<ConflictInfo>> DetectRoleConflictsAsync(
		string sourceName,
		string destinationName,
		CancellationToken cancellationToken = default)
	{
		var conflicts = new List<ConflictInfo>();

		var sourceRoles = await _schemaDiscovery.DiscoverRolesAsync(sourceName, cancellationToken);
		var destRoles = await _schemaDiscovery.DiscoverRolesAsync(destinationName, cancellationToken);

		var destRoleNames = destRoles.Select(r => r.Name).ToHashSet();

		foreach (var sourceRole in sourceRoles)
		{
			if (destRoleNames.Contains(sourceRole.Name))
			{
				var destRole = destRoles.First(r => r.Name == sourceRole.Name);

				// Check if role properties differ
				var propertiesDiffer = sourceRole.IsSuperuser != destRole.IsSuperuser ||
									   sourceRole.CanLogin != destRole.CanLogin;

				conflicts.Add(new ConflictInfo
				{
					ConflictType = ConflictType.RoleNameConflict,
					ObjectName = sourceRole.Name,
					Description = $"Role '{sourceRole.Name}' already exists on destination. " +
								  (propertiesDiffer ? "Properties differ between source and destination." : "Properties match."),
					SuggestedResolution = "Role conflict strategy in configuration will determine behavior (Merge/Skip/Fail)",
					IsBlocking = false // Not blocking - handled by configured strategy
				});
			}
		}

		return conflicts;
	}

	public async Task<List<ConflictInfo>> DetectMissingExtensionsAsync(
		string sourceName,
		string destinationName,
		CancellationToken cancellationToken = default)
	{
		var conflicts = new List<ConflictInfo>();

		var sourceDatabases = await _schemaDiscovery.DiscoverDatabasesAsync(sourceName, cancellationToken);

		foreach (var sourceDb in sourceDatabases)
		{
			var sourceExtensions = await _schemaDiscovery.DiscoverExtensionsAsync(sourceName, sourceDb.Name, cancellationToken);

			// Check if destination database exists
			var destDatabases = await _schemaDiscovery.DiscoverDatabasesAsync(destinationName, cancellationToken);
			var destDbExists = destDatabases.Any(d => d.Name == sourceDb.Name);

			List<ExtensionInfo> destExtensions;
			if (destDbExists)
			{
				destExtensions = await _schemaDiscovery.DiscoverExtensionsAsync(destinationName, sourceDb.Name, cancellationToken);
			}
			else
			{
				// Destination database doesn't exist yet, so check template database
				destExtensions = await _schemaDiscovery.DiscoverExtensionsAsync(destinationName, "postgres", cancellationToken);
			}

			var destExtensionNames = destExtensions.Select(e => e.Name).ToHashSet();

			foreach (var sourceExt in sourceExtensions)
			{
				if (!destExtensionNames.Contains(sourceExt.Name))
				{
					conflicts.Add(new ConflictInfo
					{
						ConflictType = ConflictType.MissingExtension,
						ObjectName = sourceExt.Name,
						Description = $"Extension '{sourceExt.Name}' (version {sourceExt.Version}) required by source database '{sourceDb.Name}' is not available on destination",
						SuggestedResolution = $"Install extension '{sourceExt.Name}' on destination instance before proceeding",
						IsBlocking = true // Missing extensions are blocking
					});
				}
			}
		}

		return conflicts;
	}

	public async Task<List<ConflictInfo>> DetectActiveConnectionsAsync(
		string destinationName,
		string database,
		CancellationToken cancellationToken = default)
	{
		var conflicts = new List<ConflictInfo>();

		try
		{
			await using var connection = await _connectionFactory.CreateConnectionAsync(destinationName, "postgres", cancellationToken);

			await using var cmd = connection.CreateCommand();
			cmd.CommandText = @"
				SELECT count(*)
				FROM pg_stat_activity
				WHERE datname = @database
				AND pid <> pg_backend_pid()";
			cmd.Parameters.AddWithValue("database", database);

			var activeConnections = (long)(await cmd.ExecuteScalarAsync(cancellationToken) ?? 0L);

			if (activeConnections > 0)
			{
				conflicts.Add(new ConflictInfo
				{
					ConflictType = ConflictType.ActiveConnectionsOnDestination,
					ObjectName = database,
					Description = $"Database '{database}' on destination has {activeConnections} active connections",
					SuggestedResolution = "Close all connections to the destination database before migration",
					IsBlocking = true
				});
			}
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Could not check active connections for {Database} on {Destination}",
				database, destinationName);
		}

		return conflicts;
	}
}
