using BackendTest.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace BackendTest.Api.Middleware;

public class GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Ha ocurrido un error: {Message}", exception.Message);

        context.Response.ContentType = "application/problem+json";

        var statusCode = HttpStatusCode.InternalServerError;
        var title = "Error interno del servidor";
        var detail = "Ocurrió un error inesperado.";
        object? errors = null;

        switch (exception)
        {
            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                title = "Recurso no encontrado";
                detail = notFoundEx.Message;
                break;

            case FluentValidation.ValidationException fluentValidationEx:
                statusCode = HttpStatusCode.BadRequest;
                title = "Error de validación";
                detail = "Uno o más campos de validación fallaron.";
                errors = fluentValidationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                break;

            case ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                title = "Error de validación";
                detail = validationEx.Message;
                errors = validationEx.Errors;
                break;

            case BusinessRuleException businessEx:
                statusCode = HttpStatusCode.BadRequest;
                title = "Error de regla de negocio";
                detail = businessEx.Message;
                break;

            case DbUpdateException:
                statusCode = HttpStatusCode.InternalServerError;
                title = "Error de base de datos";
                detail = "Ocurrió un error al actualizar la base de datos.";
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            type = GetProblemType(statusCode),
            title,
            status = (int)statusCode,
            detail,
            errors
        };

        var json = JsonSerializer.Serialize(response, _jsonOptions);

        await context.Response.WriteAsync(json);
    }

    private static string GetProblemType(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            HttpStatusCode.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
            HttpStatusCode.Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            HttpStatusCode.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            HttpStatusCode.MethodNotAllowed => "https://tools.ietf.org/html/rfc7231#section-6.5.5",
            HttpStatusCode.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            HttpStatusCode.UnprocessableEntity => "https://tools.ietf.org/html/rfc4918#section-11.2",

            HttpStatusCode.InternalServerError => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            HttpStatusCode.NotImplemented => "https://tools.ietf.org/html/rfc7231#section-6.6.2",
            HttpStatusCode.BadGateway => "https://tools.ietf.org/html/rfc7231#section-6.6.3",
            HttpStatusCode.ServiceUnavailable => "https://tools.ietf.org/html/rfc7231#section-6.6.4",

            _ => "about:blank"
        };
    }
}
