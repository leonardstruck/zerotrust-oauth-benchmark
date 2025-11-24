using ErrorOr;

using Microsoft.AspNetCore.Http;

namespace ZeroTrustOAuth.ServiceDefaults;

public static class ErrorOrExtensions
{
    /// <summary>
    /// Converts ErrorOr errors to an RFC 7807 Problem Details response.
    /// </summary>
    public static IResult ToProblem(this List<Error> errors)
    {
        if (errors.Count == 0)
        {
            return Results.Problem();
        }

        if (errors.All(e => e.Type == ErrorType.Validation))
        {
            var validationErrors = errors
                .GroupBy(e => e.Code)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.Description).ToArray());

            return Results.ValidationProblem(validationErrors);
        }

        var firstError = errors[0];
        var statusCode = GetStatusCode(firstError.Type);

        return Results.Problem(
            title: GetTitle(firstError.Type),
            detail: firstError.Description,
            statusCode: statusCode,
            extensions: errors.Count > 1
                ? new Dictionary<string, object?> { ["errors"] = errors.Select(e => e.Description) }
                : null);
    }

    /// <summary>
    /// Converts ErrorOr errors to an RFC 7807 Problem Details response.
    /// </summary>
    public static IResult ToProblem<T>(this ErrorOr<T> errorOr)
    {
        return errorOr.Errors.ToProblem();
    }

    private static int GetStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status500InternalServerError
    };

    private static string GetTitle(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => "One or more validation errors occurred.",
        ErrorType.NotFound => "The requested resource was not found.",
        ErrorType.Conflict => "A conflict occurred.",
        ErrorType.Unauthorized => "Unauthorized.",
        ErrorType.Forbidden => "Forbidden.",
        _ => "An error occurred."
    };
}
