using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OCR.Application.Common.Exceptions;

namespace OCR.Host.Middlewares
{
    public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {

            logger.LogError(exception, exception.Message);

            var problemDetails = new ProblemDetails
            {
                Status = exception switch
                {
                    ArgumentException => StatusCodes.Status400BadRequest,
                    UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                    NotFoundException => StatusCodes.Status404NotFound,
                    ValidationException => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status500InternalServerError
                },

                Title = exception switch
                {
                    ValidationException => "Validation failed",
                    UnauthorizedAccessException => "Unauthorized",
                    NotFoundException => "Resource not found",
                    ArgumentException => "Bad Request",
                    _ => "Internal server error"
                },
                Type = null,
                Detail = exception switch
                {
                    ValidationException => exception.Message,
                    UnauthorizedAccessException => exception.Message,
                    NotFoundException => exception.Message,
                    ArgumentException => exception.Message,
                    _ => "An unexpected error occurred."
                }

            };

            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

            if (exception is ValidationException validationException)
            {
                problemDetails.Extensions["errors"] = validationException.Errors;
            }

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                Exception = exception,
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            });
            }
        }

}
