using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microservicio.Vuelos.Api.Models.Common;
using Microservicio.Vuelos.Business.Exceptions;

namespace Microservicio.Vuelos.Api.Middleware;

/// <summary>
/// CAMBIO MICROSERVICIO: namespace Model.Common → Models.Common
/// Sin cambios de lógica — maneja las mismas excepciones tipadas.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Error de validación. TraceId: {TraceId}", context.TraceIdentifier);

            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status400BadRequest,
                ApiErrorResponse.Create(ex.Message, ex.Errors, context.TraceIdentifier));
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Recurso no encontrado. TraceId: {TraceId}", context.TraceIdentifier);

            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status404NotFound,
                ApiErrorResponse.Create(ex.Message, null, context.TraceIdentifier));
        }
        catch (UnauthorizedBusinessException ex)
        {
            _logger.LogWarning(ex, "Acceso no autorizado. TraceId: {TraceId}", context.TraceIdentifier);

            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status401Unauthorized,
                ApiErrorResponse.Create(ex.Message, null, context.TraceIdentifier));
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "Error de negocio. TraceId: {TraceId}", context.TraceIdentifier);

            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status409Conflict,
                ApiErrorResponse.Create(ex.Message, null, context.TraceIdentifier));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Error de persistencia de datos. TraceId: {TraceId}", context.TraceIdentifier);

            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status409Conflict,
                ApiErrorResponse.Create(
                    "La operación no pudo completarse por una restricción de datos.",
                    null,
                    context.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno no controlado. TraceId: {TraceId}", context.TraceIdentifier);

            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status500InternalServerError,
                ApiErrorResponse.Create(
                    "Ha ocurrido un error interno en el servidor.",
                    null,
                    context.TraceIdentifier));
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        int statusCode,
        ApiErrorResponse response)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(json);
    }
}