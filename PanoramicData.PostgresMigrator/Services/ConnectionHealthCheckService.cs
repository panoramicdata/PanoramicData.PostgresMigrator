using Microsoft.Extensions.Options;
using PanoramicData.PostgresMigrator.Interfaces;
using PanoramicData.PostgresMigrator.Models.Configuration;

namespace PanoramicData.PostgresMigrator.Services;

/// <summary>
/// Health check service for PostgreSQL connections
/// </summary>
public class ConnectionHealthCheckService(
	IOptions<PostgresMigratorConfig> config,
	IPostgresConnectionFactory connectionFactory,
	ILogger<ConnectionHealthCheckService> logger) : IConnectionHealthCheckService
{
	private readonly PostgresMigratorConfig _config = config.Value;

	public async Task<Dictionary<string, bool>> CheckAllInstancesAsync(CancellationToken cancellationToken = default)
	{
		var results = new Dictionary<string, bool>();

		var tasks = _config.Instances.Keys.Select(async instanceName =>
		{
			var isHealthy = await CheckInstanceAsync(instanceName, cancellationToken);
			return (instanceName, isHealthy);
		});

		var healthChecks = await Task.WhenAll(tasks);

		foreach (var (instanceName, isHealthy) in healthChecks)
		{
			results[instanceName] = isHealthy;
		}

		var healthyCount = results.Count(r => r.Value);
		logger.LogInformation("Health check complete: {Healthy}/{Total} instances healthy",
			healthyCount, results.Count);

		return results;
	}

	public async Task<bool> CheckInstanceAsync(string instanceName, CancellationToken cancellationToken = default)
	{
		try
		{
			var isHealthy = await connectionFactory.TestConnectionAsync(instanceName, cancellationToken);

			if (isHealthy)
			{
				logger.LogDebug("Health check passed for {Instance}", instanceName);
			}
			else
			{
				logger.LogWarning("Health check failed for {Instance}", instanceName);
			}

			return isHealthy;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Health check error for {Instance}", instanceName);
			return false;
		}
	}
}
