using System.Net;
using System.Text.Json;
using DoubleCheck.Exceptions;

namespace DoubleCheck.Middleware;

/// <summary>Maps typed AppExceptions to HTTP status codes + JSON.</summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var status = ex switch
            {
                NotFoundException   => HttpStatusCode.NotFound,
                UnauthorizedException => HttpStatusCode.Unauthorized,
                ForbiddenException  => HttpStatusCode.Forbidden,
                ValidationException => HttpStatusCode.BadRequest,
                ConflictException   => HttpStatusCode.Conflict,
                DomainException     => HttpStatusCode.BadRequest,
                BadGatewayException => HttpStatusCode.BadGateway,
                _                   => HttpStatusCode.InternalServerError
            };

            if (status == HttpStatusCode.InternalServerError)
                _logger.LogError(ex, "Unhandled exception");

            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                status = (int)status,
                error = status == HttpStatusCode.InternalServerError ? "An unexpected error occurred." : ex.Message
            }));
        }
    }
}
