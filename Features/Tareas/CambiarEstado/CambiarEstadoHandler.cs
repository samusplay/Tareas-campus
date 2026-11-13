using System.Text.Json;
using TareasService.Infrastructure.Eventos;

namespace TareasService.Features.Tareas.CambiarEstado;

// --- Entrada: el dueño (del token), la tarea (de la URL) y el nuevo estado (del body) ---
public record CambiarEstadoCommand(
    int UsuarioId,
    int TareaId,
    EstadoTarea? Estado) : ICommand<CambiarEstadoResult>;

// --- Salida ---
public record CambiarEstadoResult(
    int TareaId,
    EstadoTarea Estado,
    DateTime FechaActualizacion);

// --- Lógica ---
internal class CambiarEstadoHandler(TareasDbContext db, IEventosTarea eventos)
    : ICommandHandler<CambiarEstadoCommand, CambiarEstadoResult>
{
    public async Task<CambiarEstadoResult> Handle(
        CambiarEstadoCommand command,
        CancellationToken cancellationToken)
    {
        // Se busca por id Y por dueño: si es de otro usuario, 404 igual que si no existiera
        var tarea = await db.Tareas
            .FirstOrDefaultAsync(t => t.Id == command.TareaId && t.UsuarioId == command.UsuarioId, cancellationToken)
            ?? throw new NotFoundException("La tarea no existe");

        var nuevo = command.Estado!.Value;   // el validator garantiza que viene

        if (tarea.Estado == nuevo)
            throw new BadRequestException($"La tarea ya está en estado {Nombre(nuevo)}");

        // Máquina de estados: completada y cancelada son finales
        if (!TransicionesEstado.EsValida(tarea.Estado, nuevo))
            throw new BadRequestException($"No se permite pasar de {Nombre(tarea.Estado)} a {Nombre(nuevo)}");

        tarea.Estado = nuevo;
        tarea.FechaActualizacion = FechaColombia.Ahora();

        await db.SaveChangesAsync(cancellationToken);

        // Estadísticas recalcula completadas, pendientes y vencidas con el estado nuevo
        await eventos.EstadoCambiadoAsync(tarea, cancellationToken);

        return new CambiarEstadoResult(tarea.Id, tarea.Estado, tarea.FechaActualizacion);
    }

    // EnProgreso → "en_progreso" (el mismo texto que ve el front)
    private static string Nombre(EstadoTarea estado) =>
        JsonNamingPolicy.SnakeCaseLower.ConvertName(estado.ToString());
}