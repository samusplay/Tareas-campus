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
        var (codigo, tipo, mensaje) = exception switch
        {
            ValidationException => (400, "SOLICITUD_INCORRECTA", "Datos de entrada inválidos"),
            BadRequestException ex => (400, "SOLICITUD_INCORRECTA", ex.Message),
            BadHttpRequestException => (400, "SOLICITUD_INCORRECTA", "El cuerpo de la petición no es un JSON válido"),
            UnauthorizedAccessException => (401, "NO_AUTORIZADO", "No autorizado"),
            NotFoundException ex => (404, "RECURSO_NO_ENCONTRADO", ex.Message),
            ConflictException ex => (409, "CONFLICTO", ex.Message),
            ServiceUnavailableException ex => (503, "SERVICIO_NO_DISPONIBLE", ex.Message),
            _ => (500, "ERROR_INTERNO", "Ocurrió un error interno en el servicio de Tareas")
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
            error = new
            {
                codigo,
                tipo,
                mensaje,
                // Solo en validaciones: errores por campo para que el front los pinte en su FieldError
                errores = exception is ValidationException vex
                    ? vex.Errors.Select(e => new
                    {
                        campo = JsonNamingPolicy.SnakeCaseLower.ConvertName(e.PropertyName),
                        mensaje = e.ErrorMessage
                    })
                    : null
            }
        }, cancellationToken);

        return true; // ya respondimos: ASP.NET no hace nada más
    }
}