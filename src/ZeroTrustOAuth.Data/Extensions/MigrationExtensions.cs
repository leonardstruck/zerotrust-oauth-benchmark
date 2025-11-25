using System.Diagnostics;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ZeroTrustOAuth.Data.Extensions;

public static partial class MigrationExtensions
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Starting database migration for {DbContextName}")]
    private static partial void LogMigrationStarting(ILogger logger, string dbContextName);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Database migration completed successfully for {DbContextName}")]
    private static partial void LogMigrationCompleted(ILogger logger, string dbContextName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Database migration failed for {DbContextName}")]
    private static partial void LogMigrationFailed(ILogger logger, Exception ex, string dbContextName);

    public static IHostApplicationBuilder AddMigration<TDbContext>(this IHostApplicationBuilder builder)
        where TDbContext : DbContext
    {
        string activitySourceName = $"DbMigration.{typeof(TDbContext).Name}";

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing.AddSource(activitySourceName));

        builder.Services.AddSingleton<MigrationHealthCheck<TDbContext>>();
        builder.Services.AddHealthChecks()
            .AddCheck<MigrationHealthCheck<TDbContext>>("migration", tags: ["live"]);
        builder.Services.AddHostedService<MigrationService<TDbContext>>();
        return builder;
    }

    private sealed class MigrationHealthCheck<TDbContext> : IHealthCheck where TDbContext : DbContext
    {
        private volatile bool _migrationCompleted;
        private Exception? _migrationException;

        public bool MigrationCompleted { set => _migrationCompleted = value; }
        public Exception? MigrationException { set => _migrationException = value; }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            if (_migrationException != null)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Database migration failed for {typeof(TDbContext).Name}", _migrationException));
            }

            if (!_migrationCompleted)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Database migration in progress for {typeof(TDbContext).Name}"));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                $"Database migration completed for {typeof(TDbContext).Name}"));
        }
    }

    private sealed class MigrationService<TDbContext> : BackgroundService where TDbContext : DbContext
    {
        private readonly ActivitySource _activitySource;
        private readonly MigrationHealthCheck<TDbContext> _healthCheck;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IServiceProvider _serviceProvider;

        public MigrationService(IServiceProvider serviceProvider,
            IHostApplicationLifetime hostApplicationLifetime,
            MigrationHealthCheck<TDbContext> healthCheck)
        {
            _serviceProvider = serviceProvider;
            _hostApplicationLifetime = hostApplicationLifetime;
            _healthCheck = healthCheck;
            // Use a descriptive name based on the DbContext for the ActivitySource
            string activitySourceName = $"DbMigration.{typeof(TDbContext).Name}";
            _activitySource = new ActivitySource(activitySourceName);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using Activity? activity = _activitySource.StartActivity();
            activity?.SetTag("db.context", typeof(TDbContext).Name);

            try
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                ILogger<TDbContext> logger = scope.ServiceProvider.GetRequiredService<ILogger<TDbContext>>();
                TDbContext dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

                LogMigrationStarting(logger, typeof(TDbContext).Name);
                await dbContext.Database.MigrateAsync(stoppingToken);
                LogMigrationCompleted(logger, typeof(TDbContext).Name);

                _healthCheck.MigrationCompleted = true;
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                ILogger<TDbContext> logger = scope.ServiceProvider.GetRequiredService<ILogger<TDbContext>>();
                LogMigrationFailed(logger, ex, typeof(TDbContext).Name);

                _healthCheck.MigrationException = ex;
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }
    }
}