using System.Diagnostics;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using DbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace ZeroTrustOAuth.Data.Extensions;

public static partial class MigrationExtensions
{
    private static readonly ActivitySource ActivitySource = new("Migrations");

    /// <summary>
    ///     Adds a hosted service that will run database migrations on application startup.
    /// </summary>
    public static IHostApplicationBuilder AddDatabaseMigration<T>(this IHostApplicationBuilder builder)
        where T : DbContext
    {
        builder.Services.AddHostedService<DatabaseMigrationWorker<T>>();
        return builder;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting database migration for {DbContextName}")]
    private static partial void LogMigrationStarting(ILogger logger, string dbContextName);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Database migration completed successfully for {DbContextName}")]
    private static partial void LogMigrationCompleted(ILogger logger, string dbContextName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Database migration failed for {DbContextName}")]
    private static partial void LogMigrationFailed(ILogger logger, Exception ex, string dbContextName);

    private sealed class DatabaseMigrationWorker<T>(
        IServiceProvider serviceProvider,
        IHostApplicationLifetime lifetime,
        ILogger<DatabaseMigrationWorker<T>> logger)
        : BackgroundService where T : DbContext
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using Activity? activity = ActivitySource.StartActivity(ActivityKind.Internal);
            activity?.SetTag("db.context", typeof(T).Name);

            try
            {
                // Wait for the application to be started before running migrations
                await WaitForApplicationStarted(lifetime, stoppingToken);

                await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
                await using T dbContext = scope.ServiceProvider.GetRequiredService<T>();

                IExecutionStrategy executionStrategy = dbContext.Database.CreateExecutionStrategy();

                LogMigrationStarting(logger, typeof(T).Name);
                await executionStrategy.ExecuteAsync(async () =>
                    await dbContext.Database.MigrateAsync(stoppingToken));
                LogMigrationCompleted(logger, typeof(T).Name);

                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                LogMigrationFailed(logger, ex, typeof(T).Name);

                // Stop the application if migration fails
                lifetime.StopApplication();
                throw;
            }
        }

        private static async Task WaitForApplicationStarted(IHostApplicationLifetime lifetime,
            CancellationToken stoppingToken)
        {
            TaskCompletionSource tcs = new();
            using CancellationTokenRegistration registration =
                lifetime.ApplicationStarted.Register(() => tcs.SetResult());
            await using CancellationTokenRegistration _ =
                stoppingToken.Register(() => tcs.TrySetCanceled(stoppingToken));
            await tcs.Task;
        }
    }
}