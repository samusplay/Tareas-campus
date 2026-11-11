using System.Security.Claims;

namespace TareasService.Features.Tareas.Delete;

// DELETE no recibe cuerpo: el id viaja en la ruta y no hay respuesta con datos (204).
public class DeleteTareaEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/tareas/{id:int}", async (
            int id,                                  // id de la tarea, viene de la URL
            ClaimsPrincipal user,                    // el token ya validado: de aquí sale el dueño
            ISender sender,                          // mensajero para mandarlo a su handler
            CancellationToken cancellationToken) =>  // freno de mano si el cliente cancela
        {
            var command = new DeleteTareaCommand(user.GetUsuarioId(), id);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("DeleteTarea")
        .WithTags("Tareas")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}