using System.Diagnostics;
using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using DbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace ZeroTrustOAuth.Data.Extensions;

public static partial class MigrationExtensions
{
    private static readonly ActivitySource ActivitySource = new("Migrations");
    private static readonly Assembly EntryAssembly = Assembly.GetEntryAssembly()!;

    public static async Task EnsureCreated<T>(this IApplicationBuilder app) where T : DbContext
    {
        using Activity? activity = ActivitySource.StartActivity(ActivityKind.Internal);
        activity?.SetTag("db.context", typeof(T).Name);

        try
        {
            await using AsyncServiceScope scope = app.ApplicationServices.CreateAsyncScope();
            await using T dbContext = scope.ServiceProvider.GetRequiredService<T>();
            ILogger<T> logger = scope.ServiceProvider.GetRequiredService<ILogger<T>>();

            IExecutionStrategy executionStrategy = dbContext.Database.CreateExecutionStrategy();

            LogMigrationStarting(logger, typeof(T).Name);
            await executionStrategy.ExecuteAsync(async () => await dbContext.Database.EnsureCreatedAsync());
            LogMigrationCompleted(logger, typeof(T).Name);
        }
        catch (Exception ex)
        {
            await using AsyncServiceScope scope = app.ApplicationServices.CreateAsyncScope();
            ILogger<T> logger = scope.ServiceProvider.GetRequiredService<ILogger<T>>();

            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            LogMigrationFailed(logger, ex, typeof(T).Name);
            throw;
        }

        activity?.SetStatus(ActivityStatusCode.Ok);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting database migration for {dbContextName}")]
    private static partial void LogMigrationStarting(ILogger logger, string dbContextName);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Database migration completed successfully for {DbContextName}")]
    private static partial void LogMigrationCompleted(ILogger logger, string dbContextName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Database migration failed for {DbContextName}")]
    private static partial void LogMigrationFailed(ILogger logger, Exception ex, string dbContextName);
}