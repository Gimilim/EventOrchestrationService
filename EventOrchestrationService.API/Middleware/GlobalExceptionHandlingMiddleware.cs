using EventOrchestrationService.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using ValidationException = FluentValidation.ValidationException;

namespace EventOrchestrationService.API.Middleware;

public class GlobalExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleException(httpContext, ex);
        }
    }

    private async Task HandleException(HttpContext httpContext, Exception ex)
    {
        logger.LogError(
            ex,
            "Unhandled exception. Method={Method}, Path={Path}, RequestId={RequestId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.Request.Headers["x-request-id"]);

        if (httpContext.Response.HasStarted)
        {
            return;
        }

        var statusCode = MapStatusCode(ex);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var error = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(ex),
            Detail = ex.Message
        };

        await httpContext.Response.WriteAsJsonAsync(error);
    }

    private static int MapStatusCode(Exception ex)
        => ex switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            BadRequestException => StatusCodes.Status400BadRequest,
            ValidationException => StatusCodes.Status400BadRequest,
            NoAvailableSeatsException => StatusCodes.Status409Conflict,
            ConcurrencyException => StatusCodes.Status409Conflict,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };
    
    private static string GetTitle(Exception ex)
        => ex switch
        {
            NotFoundException => "Resource Not Found",
            BadRequestException => "Bad Request",
            ValidationException => "Validation Error",
            NoAvailableSeatsException => "No available seats for this even",
            ConcurrencyException => "Concurrency Conflict",
            UnauthorizedException => "Unauthorized",
            _ => "Server Error"
        };
}