using Serilog;
using PanoramicData.PostgresMigrator.Models.Configuration;
using PanoramicData.PostgresMigrator.Components;
using PanoramicData.PostgresMigrator.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for stdout logging
Log.Logger = new LoggerConfiguration()
	.ReadFrom.Configuration(builder.Configuration)
	.WriteTo.Console()
	.CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

// Add configuration from environment variables
builder.Configuration.AddEnvironmentVariables(prefix: "PGMIGRATOR_");

// Register and validate configuration
builder.Services.Configure<PostgresMigratorConfig>(
	builder.Configuration.GetSection(PostgresMigratorConfig.SectionName));

// Validate configuration on startup
var config = builder.Configuration
	.GetSection(PostgresMigratorConfig.SectionName)
	.Get<PostgresMigratorConfig>();

if (config != null && (config.Instances.Count > 0 || config.Mappings.Count > 0))
{
	try
	{
		config.Validate();
		Log.Information("Configuration validated successfully");
		Log.Information("Configured {InstanceCount} instances and {MappingCount} mappings",
			config.Instances.Count, config.Mappings.Count);
	}
	catch (Exception ex)
	{
		Log.Fatal(ex, "Configuration validation failed");
		throw;
	}
}
else
{
	Log.Warning("No configuration found. Please configure instances and mappings in appsettings.json or environment variables.");
}

// Phase 2: Register PostgreSQL services
builder.Services.AddSingleton<IPostgresConnectionFactory, PostgresConnectionFactory>();
builder.Services.AddSingleton<IConnectionHealthCheckService, ConnectionHealthCheckService>();
builder.Services.AddSingleton<ISchemaDiscoveryService, SchemaDiscoveryService>();
builder.Services.AddSingleton<IConflictDetectionService, ConflictDetectionService>();
builder.Services.AddSingleton<IExtensionValidationService, ExtensionValidationService>();

// TODO: Phase 3+ - Add services
// - IReplicationSlotService
// - IReplicationService
// - IPublicationManager
// - ISubscriptionManager
// - ISchemaMigrationService
// - ISequenceSyncService
// - IRoleMigrationService
// - IPermissionMigrationService

// TODO: Phase 4+ - Add services
// - IMigrationOrchestrator (IHostedService)
// - IStateDiscoveryService
// - IMonitoringService
// - ICutoverReadinessService
// - ICutoverInstructionService

// TODO: Phase 5+ - Add services
// - IUiStateService

var app = builder.Build();

// Perform initial health checks on startup
if (config != null && config.Instances.Count > 0)
{
	using var scope = app.Services.CreateScope();
	var healthCheck = scope.ServiceProvider.GetRequiredService<IConnectionHealthCheckService>();

	try
	{
		Log.Information("Performing initial health checks...");
		var results = await healthCheck.CheckAllInstancesAsync();

		foreach (var (instance, isHealthy) in results)
		{
			if (isHealthy)
			{
				Log.Information("✓ Instance {Instance} is healthy", instance);
			}
			else
			{
				Log.Warning("✗ Instance {Instance} is not healthy", instance);
			}
		}
	}
	catch (Exception ex)
	{
		Log.Error(ex, "Health check failed during startup");
	}
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// No HSTS - we assume network-level security
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

try
{
	Log.Information("Starting PanoramicData.PostgresMigrator");
	app.Run();
}
catch (Exception ex)
{
	Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
	Log.CloseAndFlush();
}