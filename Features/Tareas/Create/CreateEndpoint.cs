using System.Security.Claims;

namespace TareasService.Features.Tareas.Create;

// --- Lo que manda el front (sin usuario_id: sale del token) ---
public record CreateTareaRequest(
    string Titulo,
    string? Descripcion,
    int MateriaId,
    int? GrupoId,
    DateOnly FechaEntrega,
    TimeOnly? HoraEntrega,
    Prioridad? Prioridad);

// --- Lo que recibe el front ---
public record CreateTareaResponse(
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

public class CreateTareaEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/tareas", async (
            CreateTareaRequest request,              // cuerpo JSON
            ClaimsPrincipal user,                    // el token ya validado: de aquí sale el dueño
            ISender sender,                          // mensajero para mandarlo a su handler
            CancellationToken cancellationToken) =>  // freno de mano si el cliente cancela
        {
            var command = request.Adapt<CreateTareaCommand>() with { UsuarioId = user.GetUsuarioId() };
            var result = await sender.Send(command, cancellationToken);
            var response = result.Adapt<CreateTareaResponse>();

            return Results.Created($"/api/v1/tareas/{response.TareaId}", response);
        })
        .RequireAuthorization()
        .WithName("CreateTarea")
        .WithTags("Tareas")
        .Produces<CreateTareaResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}