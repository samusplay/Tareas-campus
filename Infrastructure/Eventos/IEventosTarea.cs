namespace TareasService.Infrastructure.Eventos;

// Avisa a los demás servicios que algo pasó con una tarea.
// Hoy por HTTP; mañana por RabbitMQ sin tocar los handlers.
public interface IEventosTarea
{
    Task CreadaAsync(Tarea tarea, CancellationToken cancellationToken);

    Task ActualizadaAsync(Tarea tarea, CancellationToken cancellationToken);

    Task EstadoCambiadoAsync(Tarea tarea, CancellationToken cancellationToken);
}