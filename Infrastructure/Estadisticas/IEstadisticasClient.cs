namespace TareasService.Infrastructure.Clients.Estadisticas;

public interface IEstadisticasClient
{
    Task PublicarAsync(string evento, Tarea tarea, CancellationToken cancellationToken);
}