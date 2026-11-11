using System.Security.Claims;

namespace TareasService.Features.Tareas.Update;

// --- Lo que manda el front (sin usuario_id: sale del token; sin id: sale de la URL) ---
public record UpdateTareaRequest(
    string Titulo,
    string? Descripcion,
    int MateriaId,
    int? GrupoId,
    DateOnly FechaEntrega,
    TimeOnly? HoraEntrega,
    Prioridad Prioridad);

// --- Lo que recibe el front ---
public record UpdateTareaResponse(
    int TareaId,
    string Titulo,
    string? Descripcion,
    int MateriaId,
    DateOnly FechaEntrega,
    TimeOnly? HoraEntrega,
    Prioridad Prioridad,
    EstadoTarea Estado,
    DateTime FechaCreacion,
    DateTime FechaActualizacion);

public class UpdateTareaEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/tareas/{id:int}", async (
            int id,                                  // de la URL: qué tarea editar
            UpdateTareaRequest request,              // cuerpo JSON: la tarea completa
            ClaimsPrincipal user,                    // el token ya validado: de aquí sale el dueño
            ISender sender,                          // mensajero para mandarlo a su handler
            CancellationToken cancellationToken) =>  // freno de mano si el cliente cancela
        {
            var command = request.Adapt<UpdateTareaCommand>() with
            {
                TareaId = id,
                UsuarioId = user.GetUsuarioId()
            };
            var result = await sender.Send(command, cancellationToken);
            var response = result.Adapt<UpdateTareaResponse>();

            return Results.Ok(response);
        })
        .RequireAuthorization()
        .WithName("UpdateTarea")
        .WithTags("Tareas")
        .Produces<UpdateTareaResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}