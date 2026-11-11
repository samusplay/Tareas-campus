using System.Text.Json;

namespace TareasService.Infrastructure.Clients.Notificaciones;

public class NotificacionesClient(HttpClient httpClient, ILogger<NotificacionesClient> logger)
    : INotificacionesClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task NotificarAsync(CrearNotificacionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/v1/notificaciones", request, Json, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "No se pudo notificar al usuario {UsuarioId}", request.UsuarioId);
        }
    }
}