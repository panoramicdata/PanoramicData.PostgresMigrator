namespace PanoramicData.PostgresMigrator.Models.Configuration;

/// <summary>
/// Configuration for a PostgreSQL instance (source or destination)
/// </summary>
public class PostgresInstanceConfig
{
	/// <summary>
	/// Server hostname or IP address
	/// </summary>
	public required string Server { get; set; }

	/// <summary>
	/// Port number (default: 5432)
	/// </summary>
	public int Port { get; set; } = 5432;

	/// <summary>
	/// Username for authentication
	/// </summary>
	public required string Username { get; set; }

	/// <summary>
	/// Password for authentication (cleartext)
	/// </summary>
	public required string Password { get; set; }

	/// <summary>
	/// Get connection string for this instance
	/// </summary>
	public string GetConnectionString(string? database = null)
	{
		var connString = $"Host={Server};Port={Port};Username={Username};Password={Password}";
		if (!string.IsNullOrEmpty(database))
		{
			connString += $";Database={database}";
		}

		return connString;
	}
}
