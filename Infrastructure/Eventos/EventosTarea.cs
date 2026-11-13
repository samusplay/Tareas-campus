namespace TareasService.Infrastructure.Eventos;

// Implementación por HTTP: cada evento va directo a los servicios que lo necesitan
public class EventosTarea(
    INotificacionesClient notificaciones,
    IEstadisticasClient estadisticas) : IEventosTarea
{
    public Task CreadaAsync(Tarea tarea, CancellationToken cancellationToken) =>
        Task.WhenAll(
            notificaciones.NotificarAsync(
                new CrearNotificacionRequest(
                    tarea.UsuarioId,
                    "tarea",
                    "Nueva tarea creada",
                    $"{tarea.Titulo} vence el {tarea.FechaEntrega:dd/MM/yyyy}",
                    tarea.Id),
                cancellationToken),
            estadisticas.PublicarAsync("tarea.creada", tarea, cancellationToken));

    public Task ActualizadaAsync(Tarea tarea, CancellationToken cancellationToken) =>
        estadisticas.PublicarAsync("tarea.actualizada", tarea, cancellationToken);

    public Task EstadoCambiadoAsync(Tarea tarea, CancellationToken cancellationToken) =>
        estadisticas.PublicarAsync("tarea.estado_cambiado", tarea, cancellationToken);
}