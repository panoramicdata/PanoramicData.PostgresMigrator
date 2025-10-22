namespace PanoramicData.PostgresMigrator.Interfaces;

/// <summary>
/// Health check service for PostgreSQL connections
/// </summary>
public interface IConnectionHealthCheckService
{
	/// <summary>
	/// Check health of all configured instances
	/// </summary>
	Task<Dictionary<string, bool>> CheckAllInstancesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Check health of a specific instance
	/// </summary>
	Task<bool> CheckInstanceAsync(string instanceName, CancellationToken cancellationToken = default);
}
