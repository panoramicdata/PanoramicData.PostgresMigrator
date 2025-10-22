namespace PanoramicData.PostgresMigrator.Models.Configuration;

/// <summary>
/// Root configuration for PostgreSQL Migrator
/// </summary>
public class PostgresMigratorConfig
{
	/// <summary>
	/// Configuration section name in appsettings.json
	/// </summary>
	public const string SectionName = "PostgresMigrator";

	/// <summary>
	/// Dictionary of instance configurations (source and destination)
	/// Key is the instance name, value is the connection config
	/// </summary>
	public Dictionary<string, PostgresInstanceConfig> Instances { get; set; } = [];

	/// <summary>
	/// List of migration mappings (source -> destination pairs)
	/// </summary>
	public List<MappingConfig> Mappings { get; set; } = [];

	/// <summary>
	/// Replication and monitoring settings
	/// </summary>
	public ReplicationConfig Replication { get; set; } = new();

	/// <summary>
	/// Validate configuration on startup
	/// </summary>
	public void Validate()
	{
		if (Instances.Count == 0)
		{
			throw new InvalidOperationException("No instances configured. Add at least one source and one destination instance.");
		}

		if (Mappings.Count == 0)
		{
			throw new InvalidOperationException("No mappings configured. Add at least one source->destination mapping.");
		}

		foreach (var mapping in Mappings)
		{
			if (!Instances.ContainsKey(mapping.SourceInstance))
			{
				throw new InvalidOperationException($"Mapping references unknown source instance: {mapping.SourceInstance}");
			}

			if (!Instances.ContainsKey(mapping.DestinationInstance))
			{
				throw new InvalidOperationException($"Mapping references unknown destination instance: {mapping.DestinationInstance}");
			}

			if (mapping.SourceInstance == mapping.DestinationInstance)
			{
				throw new InvalidOperationException($"Mapping cannot have same source and destination: {mapping.SourceInstance}");
			}
		}

		// Validate instance configurations
		foreach (var kvp in Instances)
		{
			if (string.IsNullOrWhiteSpace(kvp.Value.Server))
			{
				throw new InvalidOperationException($"Instance '{kvp.Key}' has no server specified.");
			}

			if (string.IsNullOrWhiteSpace(kvp.Value.Username))
			{
				throw new InvalidOperationException($"Instance '{kvp.Key}' has no username specified.");
			}

			if (string.IsNullOrWhiteSpace(kvp.Value.Password))
			{
				throw new InvalidOperationException($"Instance '{kvp.Key}' has no password specified.");
			}

			if (kvp.Value.Port is <= 0 or > 65535)
			{
				throw new InvalidOperationException($"Instance '{kvp.Key}' has invalid port: {kvp.Value.Port}");
			}
		}
	}
}
