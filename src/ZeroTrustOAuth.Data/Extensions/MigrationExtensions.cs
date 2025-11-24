using System.Diagnostics;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ZeroTrustOAuth.Data.Extensions;

public static partial class MigrationExtensions
{
    private static readonly ActivitySource ActivitySource = new("Migrations");

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
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Database migration failed for {typeof(TDbContext).Name}", _migrationException));

            if (!_migrationCompleted)
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Database migration in progress for {typeof(TDbContext).Name}"));

            return Task.FromResult(HealthCheckResult.Healthy(
                $"Database migration completed for {typeof(TDbContext).Name}"));
        }
    }

    private sealed class MigrationService<TDbContext>(
        IServiceProvider serviceProvider,
        IHostApplicationLifetime hostApplicationLifetime,
        MigrationHealthCheck<TDbContext> healthCheck)
        : BackgroundService where TDbContext : DbContext
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var activity = ActivitySource.StartActivity(ActivityKind.Client);
            activity?.SetTag("db.context", typeof(TDbContext).Name);

            try
            {
                using var scope = serviceProvider.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<TDbContext>>();
                var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

                LogMigrationStarting(logger, typeof(TDbContext).Name);
                await dbContext.Database.MigrateAsync(stoppingToken);
                LogMigrationCompleted(logger, typeof(TDbContext).Name);

                healthCheck.MigrationCompleted = true;
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                using var scope = serviceProvider.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<TDbContext>>();
                LogMigrationFailed(logger, ex, typeof(TDbContext).Name);

                healthCheck.MigrationException = ex;
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                hostApplicationLifetime.StopApplication();
                throw;
            }
        }
    }
}
