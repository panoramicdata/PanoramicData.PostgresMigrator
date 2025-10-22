using PanoramicData.PostgresMigrator.Models.Domain;

namespace PanoramicData.PostgresMigrator.Services;

/// <summary>
/// Service for validating PostgreSQL extensions
/// </summary>
public interface IExtensionValidationService
{
	/// <summary>
	/// Validate that all required extensions are available on destination
	/// </summary>
	/// <returns>List of validation errors (empty if all extensions available)</returns>
	Task<List<string>> ValidateExtensionsAsync(
		string sourceName,
		string destinationName,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Get available extensions on an instance
	/// </summary>
	Task<List<string>> GetAvailableExtensionsAsync(
		string instanceName,
		CancellationToken cancellationToken = default);
}
