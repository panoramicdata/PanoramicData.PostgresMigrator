using Npgsql;
using PanoramicData.PostgresMigrator.Interfaces;
using PanoramicData.PostgresMigrator.Models.Domain;

namespace PanoramicData.PostgresMigrator.Services;

/// <summary>
/// Service for discovering schema objects in PostgreSQL databases
/// </summary>
public class SchemaDiscoveryService(
	IPostgresConnectionFactory connectionFactory,
	ILogger<SchemaDiscoveryService> logger) : ISchemaDiscoveryService
{
	public async Task<List<DatabaseInfo>> DiscoverDatabasesAsync(
		string instanceName,
		CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Discovering databases on {Instance}", instanceName);

		await using var connection = await connectionFactory.CreateConnectionAsync(instanceName, "postgres", cancellationToken);

		var databases = new List<DatabaseInfo>();

		await using var cmd = new NpgsqlCommand(@"
			SELECT 
				d.datname,
				pg_catalog.pg_get_userbyid(d.datdba) as owner,
				pg_catalog.pg_encoding_to_char(d.encoding) as encoding
			FROM pg_catalog.pg_database d
			WHERE d.datistemplate = false
			AND d.datname NOT IN ('postgres', 'template0', 'template1')
			ORDER BY d.datname", connection);

		await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
		while (await reader.ReadAsync(cancellationToken))
		{
			databases.Add(new DatabaseInfo
			{
				Name = reader.GetString(0),
				Owner = reader.IsDBNull(1) ? null : reader.GetString(1),
				Encoding = reader.IsDBNull(2) ? null : reader.GetString(2)
			});
		}

		logger.LogInformation("Discovered {Count} databases on {Instance}",
			databases.Count, instanceName);

		return databases;
	}

	public async Task<List<TableInfo>> DiscoverTablesAsync(
		string instanceName,
		string database,
		CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Discovering tables in {Database} on {Instance}",
			database, instanceName);

		await using var connection = await connectionFactory.CreateConnectionAsync(instanceName, database, cancellationToken);

		var tables = new List<TableInfo>();

		await using var cmd = new NpgsqlCommand(@"
			SELECT 
				n.nspname as schema_name,
				c.relname as table_name,
				c.relkind = 'p' as is_partitioned,
				c.reltuples::bigint as estimated_rows
			FROM pg_catalog.pg_class c
			JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace
			WHERE c.relkind IN ('r', 'p')  -- regular tables and partitioned tables
			AND n.nspname NOT IN ('pg_catalog', 'information_schema', 'pg_toast')
			ORDER BY n.nspname, c.relname", connection);

		await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
		while (await reader.ReadAsync(cancellationToken))
		{
			tables.Add(new TableInfo
			{
				Schema = reader.GetString(0),
				Name = reader.GetString(1),
				IsPartitioned = reader.GetBoolean(2),
				EstimatedRowCount = reader.GetInt64(3)
			});
		}

		logger.LogInformation("Discovered {Count} tables in {Database}",
			tables.Count, database);

		return tables;
	}

	public async Task<List<PartitionInfo>> DiscoverPartitionsAsync(
		string instanceName,
		string database,
		string schema,
		string tableName,
		CancellationToken cancellationToken = default)
	{
		logger.LogDebug("Discovering partitions for {Schema}.{Table} in {Database} on {Instance}",
			schema, tableName, database, instanceName);

		await using var connection = await connectionFactory.CreateConnectionAsync(instanceName, database, cancellationToken);

		var partitions = new List<PartitionInfo>();

		await using var cmd = new NpgsqlCommand(@"
			SELECT 
				n.nspname as partition_schema,
				c.relname as partition_name,
				pn.nspname || '.' || pc.relname as parent_table,
				p.partstrat::text as partition_strategy,
				pg_get_expr(c.relpartbound, c.oid) as partition_expression,
				c.reltuples::bigint as estimated_rows
			FROM pg_catalog.pg_class c
			JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace
			JOIN pg_catalog.pg_inherits i ON i.inhrelid = c.oid
			JOIN pg_catalog.pg_class pc ON pc.oid = i.inhparent
			JOIN pg_catalog.pg_namespace pn ON pn.oid = pc.relnamespace
			LEFT JOIN pg_catalog.pg_partitioned_table p ON p.partrelid = pc.oid
			WHERE pn.nspname = @schema
			AND pc.relname = @tableName
			ORDER BY c.relname", connection);

		cmd.Parameters.AddWithValue("schema", schema);
		cmd.Parameters.AddWithValue("tableName", tableName);

		await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
		while (await reader.ReadAsync(cancellationToken))
		{
			partitions.Add(new PartitionInfo
			{
				Schema = reader.GetString(0),
				Name = reader.GetString(1),
				ParentTable = reader.IsDBNull(2) ? null : reader.GetString(2),
				PartitionStrategy = reader.IsDBNull(3) ? null : reader.GetString(3),
				PartitionExpression = reader.IsDBNull(4) ? null : reader.GetString(4),
				EstimatedRowCount = reader.GetInt64(5)
			});
		}

		logger.LogDebug("Discovered {Count} partitions for {Schema}.{Table}",
			partitions.Count, schema, tableName);

		return partitions;
	}

	public async Task<List<SequenceInfo>> DiscoverSequencesAsync(
		string instanceName,
		string database,
		CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Discovering sequences in {Database} on {Instance}",
			database, instanceName);

		await using var connection = await connectionFactory.CreateConnectionAsync(instanceName, database, cancellationToken);

		var sequences = new List<SequenceInfo>();

		await using var cmd = new NpgsqlCommand(@"
			SELECT 
				n.nspname as schema_name,
				c.relname as sequence_name,
				format_type(s.seqtypid, NULL) as data_type,
				s.seqincrement as increment_by,
				(SELECT last_value FROM pg_catalog.pg_sequences WHERE schemaname = n.nspname AND sequencename = c.relname) as current_value
			FROM pg_catalog.pg_class c
			JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace
			LEFT JOIN pg_catalog.pg_sequence s ON s.seqrelid = c.oid
			WHERE c.relkind = 'S'
			AND n.nspname NOT IN ('pg_catalog', 'information_schema')
			ORDER BY n.nspname, c.relname", connection);

		await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
		while (await reader.ReadAsync(cancellationToken))
		{
			sequences.Add(new SequenceInfo
			{
				Schema = reader.GetString(0),
				Name = reader.GetString(1),
				DataType = reader.IsDBNull(2) ? null : reader.GetString(2),
				IncrementBy = reader.IsDBNull(3) ? null : reader.GetInt64(3),
				CurrentValueSource = reader.IsDBNull(4) ? null : reader.GetInt64(4)
			});
		}

		logger.LogInformation("Discovered {Count} sequences in {Database}",
			sequences.Count, database);

		return sequences;
	}

	public async Task<List<RoleInfo>> DiscoverRolesAsync(
		string instanceName,
		CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Discovering roles on {Instance}", instanceName);

		await using var connection = await connectionFactory.CreateConnectionAsync(instanceName, "postgres", cancellationToken);

		var roles = new List<RoleInfo>();

		await using var cmd = new NpgsqlCommand(@"
			SELECT 
				rolname,
				rolsuper,
				rolcanlogin,
				rolpassword
			FROM pg_catalog.pg_roles
			WHERE rolname NOT LIKE 'pg_%'
			ORDER BY rolname", connection);

		await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
		while (await reader.ReadAsync(cancellationToken))
		{
			roles.Add(new RoleInfo
			{
				Name = reader.GetString(0),
				IsSuperuser = reader.GetBoolean(1),
				CanLogin = reader.GetBoolean(2),
				PasswordHash = reader.IsDBNull(3) ? null : reader.GetString(3)
			});
		}

		logger.LogInformation("Discovered {Count} roles on {Instance}",
			roles.Count, instanceName);

		return roles;
	}

	public async Task<List<ExtensionInfo>> DiscoverExtensionsAsync(
		string instanceName,
		string database,
		CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Discovering extensions in {Database} on {Instance}",
			database, instanceName);

		await using var connection = await connectionFactory.CreateConnectionAsync(instanceName, database, cancellationToken);

		var extensions = new List<ExtensionInfo>();

		await using var cmd = new NpgsqlCommand(@"
			SELECT 
				e.extname,
				e.extversion,
				n.nspname
			FROM pg_catalog.pg_extension e
			JOIN pg_catalog.pg_namespace n ON n.oid = e.extnamespace
			ORDER BY e.extname", connection);

		await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
		while (await reader.ReadAsync(cancellationToken))
		{
			extensions.Add(new ExtensionInfo
			{
				Name = reader.GetString(0),
				Version = reader.IsDBNull(1) ? null : reader.GetString(1),
				Schema = reader.IsDBNull(2) ? null : reader.GetString(2)
			});
		}

		logger.LogInformation("Discovered {Count} extensions in {Database}",
			extensions.Count, database);

		return extensions;
	}
}
