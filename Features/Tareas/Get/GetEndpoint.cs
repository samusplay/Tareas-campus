using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace TareasService.Features.Tareas.Get;

public class GetTareasEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/tareas", async (
            [FromQuery] DateOnly? desde,             // opcional: ?desde=2026-10-01
            [FromQuery] DateOnly? hasta,             // opcional: &hasta=2026-10-31
            ClaimsPrincipal user,                    // el token ya validado: de aquí sale el dueño
            ISender sender,                          // mensajero para mandarlo a su handler
            CancellationToken cancellationToken) =>  // freno de mano si el cliente cancela
        {
            var query = new GetTareasQuery(user.GetUsuarioId(), desde, hasta);
            var result = await sender.Send(query, cancellationToken);
            return Results.Ok(result.Tareas);
        })
        .RequireAuthorization()
        .WithName("GetTareas")
        .WithTags("Tareas")
        .Produces<IEnumerable<TareaItem>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}