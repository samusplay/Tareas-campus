using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using TareasService.Common.Exceptions;

namespace TareasService.Common;

// Convierte cualquier excepción en el formato de error del contrato de tareas (sección H):
// { "exito": false, "error": { "codigo": 404, "tipo": "RECURSO_NO_ENCONTRADO", "mensaje": "..." } }
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        // JSON con un valor inválido: System.Text.Json dice dónde ("$.prioridad" → "prioridad")
        var campoJson = exception is BadHttpRequestException { InnerException: JsonException { Path: { Length: > 2 } path } }
            ? path[2..]
            : null;

        var (codigo, tipo, mensaje) = exception switch
        {
            ValidationException => (400, "SOLICITUD_INCORRECTA", "Datos de entrada inválidos"),
            BadRequestException ex => (400, "SOLICITUD_INCORRECTA", ex.Message),
            BadHttpRequestException when campoJson is not null => (400, "SOLICITUD_INCORRECTA", "Datos de entrada inválidos"),
            BadHttpRequestException => (400, "SOLICITUD_INCORRECTA", "El cuerpo de la petición no es un JSON válido"),
            UnauthorizedAccessException => (401, "NO_AUTORIZADO", "No autorizado"),
            NotFoundException ex => (404, "RECURSO_NO_ENCONTRADO", ex.Message),
            ConflictException ex => (409, "CONFLICTO", ex.Message),
            ServiceUnavailableException ex => (503, "SERVICIO_NO_DISPONIBLE", ex.Message),
            _ => (500, "ERROR_INTERNO", "Ocurrió un error interno en el servicio de Tareas")
        };

        // Errores por campo para que el front los pinte en su FieldError
        IEnumerable<CampoError>? errores = exception switch
        {
            ValidationException vex => vex.Errors.Select(e =>
                new CampoError(JsonNamingPolicy.SnakeCaseLower.ConvertName(e.PropertyName), e.ErrorMessage)),
            _ when campoJson is not null => [new CampoError(campoJson, "El valor no es válido")],
            _ => null
        };

        // El detalle técnico solo va al log, nunca al cliente
        if (codigo >= 500)
            logger.LogError(exception, "Error {Codigo} en {Path}", codigo, context.Request.Path);
        else
            logger.LogWarning("Error {Codigo} en {Path}: {Mensaje}", codigo, context.Request.Path, exception.Message);

        context.Response.StatusCode = codigo;

        await context.Response.WriteAsJsonAsync(new
        {
            exito = false,
            error = new { codigo, tipo, mensaje, errores }
        }, cancellationToken);

        return true; // ya respondimos: ASP.NET no hace nada más
    }

    private record CampoError(string Campo, string Mensaje);
}