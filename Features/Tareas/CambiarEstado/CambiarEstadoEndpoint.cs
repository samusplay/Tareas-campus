using System.Security.Claims;

namespace TareasService.Features.Tareas.CambiarEstado;

// --- Lo que manda el front: { "estado": "completada" } ---
public record CambiarEstadoRequest(EstadoTarea? Estado);

// --- Lo que recibe el front ---
public record CambiarEstadoResponse(
    int TareaId,
    EstadoTarea Estado,
    DateTime FechaActualizacion);

public class CambiarEstadoEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/tareas/{id:int}/estado", async (
            int id,                                  // id de la tarea, viene de la URL
            CambiarEstadoRequest request,            // cuerpo JSON con el nuevo estado
            ClaimsPrincipal user,                    // el token ya validado: de aquí sale el dueño
            ISender sender,                          // mensajero para mandarlo a su handler
            CancellationToken cancellationToken) =>  // freno de mano si el cliente cancela
        {
            var command = new CambiarEstadoCommand(user.GetUsuarioId(), id, request.Estado);
            var result = await sender.Send(command, cancellationToken);
            var response = result.Adapt<CambiarEstadoResponse>();
            return Results.Ok(response);
        })
        .RequireAuthorization()
        .WithName("CambiarEstadoTarea")
        .WithTags("Tareas")
        .Produces<CambiarEstadoResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}