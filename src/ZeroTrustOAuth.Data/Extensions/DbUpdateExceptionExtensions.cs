using Microsoft.EntityFrameworkCore;

namespace ZeroTrustOAuth.Data.Extensions;

public static class DbUpdateExceptionExtensions
{
    /// <summary>
    ///     Checks if a DbUpdateException was caused by a unique constraint violation.
    /// </summary>
    /// <param name="exception">The DbUpdateException to check.</param>
    /// <param name="columnName">Optional: The name of the column to check for in the constraint violation message.</param>
    /// <returns>True if the exception is a unique constraint violation; otherwise, false.</returns>
    public static bool IsUniqueConstraintViolation(this DbUpdateException exception, string? columnName = null)
    {
        Exception? innerException = exception.InnerException;
        if (innerException == null)
        {
            return false;
        }

        // Check for common unique constraint violation patterns in exception messages
        string message = innerException.Message;

        // PostgreSQL: duplicate key value violates unique constraint
        // SQL Server: Cannot insert duplicate key / Violation of UNIQUE KEY constraint
        // MySQL: Duplicate entry
        // SQLite: UNIQUE constraint failed
        bool isUniqueViolation = message.Contains("unique", StringComparison.OrdinalIgnoreCase) &&
                                 (message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
                                  message.Contains("constraint", StringComparison.OrdinalIgnoreCase) ||
                                  message.Contains("violat", StringComparison.OrdinalIgnoreCase));

        if (!isUniqueViolation)
        {
            return false;
        }

        // If a specific column name is provided, check if it's mentioned in the error
        return columnName == null || message.Contains(columnName, StringComparison.OrdinalIgnoreCase);
    }
}