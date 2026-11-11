namespace TareasService.Infrastructure.Clients.Notificaciones;

public interface INotificacionesClient
{
    Task NotificarAsync(CrearNotificacionRequest request, CancellationToken cancellationToken);
}