using Microsoft.Extensions.Options;
using PanoramicData.PostgresMigrator.Models.Configuration;

namespace PanoramicData.PostgresMigrator.Services;

/// <summary>
/// Health check service for PostgreSQL connections
/// </summary>
public class ConnectionHealthCheckService : IConnectionHealthCheckService
{
	private readonly PostgresMigratorConfig _config;
	private readonly IPostgresConnectionFactory _connectionFactory;
	private readonly ILogger<ConnectionHealthCheckService> _logger;

	public ConnectionHealthCheckService(
		IOptions<PostgresMigratorConfig> config,
		IPostgresConnectionFactory connectionFactory,
		ILogger<ConnectionHealthCheckService> logger)
	{
		_config = config.Value;
		_connectionFactory = connectionFactory;
		_logger = logger;
	}

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
		_logger.LogInformation("Health check complete: {Healthy}/{Total} instances healthy",
			healthyCount, results.Count);

		return results;
	}

	public async Task<bool> CheckInstanceAsync(string instanceName, CancellationToken cancellationToken = default)
	{
		try
		{
			var isHealthy = await _connectionFactory.TestConnectionAsync(instanceName, cancellationToken);

			if (isHealthy)
			{
				_logger.LogDebug("Health check passed for {Instance}", instanceName);
			}
			else
			{
				_logger.LogWarning("Health check failed for {Instance}", instanceName);
			}

			return isHealthy;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Health check error for {Instance}", instanceName);
			return false;
		}
	}
}
