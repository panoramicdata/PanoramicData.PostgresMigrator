namespace PanoramicData.PostgresMigrator.Models.Configuration;

/// <summary>
/// Replication and monitoring configuration
/// </summary>
public class ReplicationConfig
{
	/// <summary>
	/// Rate limit in megabytes per second (default: 100 MB/s)
	/// </summary>
	public int RateLimitMBps { get; set; } = 100;

	/// <summary>
	/// UI refresh interval in seconds (default: 5)
	/// </summary>
	public int UIRefreshIntervalSeconds { get; set; } = 5;

	/// <summary>
	/// WAL lag threshold in megabytes for alerting (default: 1024 MB = 1 GB)
	/// </summary>
	public int WalLagThresholdMB { get; set; } = 1024;

	/// <summary>
	/// WAL lag threshold in minutes for alerting (default: 5 minutes)
	/// </summary>
	public int WalLagThresholdMinutes { get; set; } = 5;

	/// <summary>
	/// Cutover ready stability duration in seconds (lag must be stable for this long)
	/// </summary>
	public int CutoverReadyStabilitySeconds { get; set; } = 30;

	/// <summary>
	/// Maximum retry attempts for transient failures (default: 5)
	/// </summary>
	public int MaxRetryAttempts { get; set; } = 5;

	/// <summary>
	/// Initial retry delay in milliseconds for exponential backoff (default: 1000ms = 1s)
	/// </summary>
	public int InitialRetryDelayMs { get; set; } = 1000;
}
