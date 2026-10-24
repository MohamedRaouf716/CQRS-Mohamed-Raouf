using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Products.Application.Common.Exceptions;

namespace Products.Api.Middleware;

/// <summary>
/// Converts known application exceptions into RFC 7807 Problem Details responses.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        IResult result;

        switch (exception)
        {
            case ValidationException validationException:
                logger.LogWarning(
                    "Validation failed for {Method} {Path}: {Errors}",
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    string.Join(" | ", validationException.Errors.Select(error => error.ErrorMessage)));

                result = Results.ValidationProblem(
                    ToErrors(validationException),
                    title: "One or more validation errors occurred.");
                break;

            case NotFoundException:
                logger.LogInformation("Resource not found: {Message}", exception.Message);

                result = Results.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Resource not found",
                    detail: exception.Message);
                break;

            default:
                logger.LogError(
                    exception,
                    "Unhandled exception while processing {Method} {Path}",
                    httpContext.Request.Method,
                    httpContext.Request.Path);

                result = Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "An unexpected error occurred",
                    detail: "An unexpected error occurred while processing the request.");
                break;
        }

        await result.ExecuteAsync(httpContext);
        return true;
    }

    private static Dictionary<string, string[]> ToErrors(ValidationException exception)
        => exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());
}
