using CleanArchitecture.Application.DTOs.Responses;
using CleanArchitecture.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace CleanArchitecture.API.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            NotFoundException notFound => (HttpStatusCode.NotFound, notFound.Message, (IEnumerable<string>?)null),
            Domain.Exceptions.ValidationException validation => (HttpStatusCode.BadRequest, validation.Message,
                validation.Errors.SelectMany(e => e.Value.Select(v => $"{e.Key}: {v}"))),
            UnauthorizedException unauthorized => (HttpStatusCode.Unauthorized, unauthorized.Message, (IEnumerable<string>?)null),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", (IEnumerable<string>?)null)
        };

        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.Fail(message, errors);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
