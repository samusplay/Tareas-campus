using System.Text.Json;
using System.Text.Json.Serialization;

namespace TareasService.Infrastructure.Clients.Estadisticas;

public class EstadisticasClient(HttpClient httpClient, ILogger<EstadisticasClient> logger) : IEstadisticasClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
    };

    public async Task PublicarAsync(string evento, Tarea tarea, CancellationToken cancellationToken)
    {
        var request = new EventoRequest(
            Guid.NewGuid().ToString(),
            evento,
            new TareaPayload(
                tarea.Id,
                tarea.UsuarioId,
                tarea.MateriaId,
                tarea.GrupoId,
                tarea.Estado,
                tarea.FechaEntrega,
                tarea.HoraEntrega));

        try
        {
            var response = await httpClient.PostAsJsonAsync("api/v1/eventos", request, Json, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "No se pudo publicar {Evento} de la tarea {TareaId}", evento, tarea.Id);
        }
    }

    private record EventoRequest(string EventoId, string Evento, TareaPayload Payload);

    private record TareaPayload(
        int TareaId,
        int UsuarioId,
        int MateriaId,
        int? GrupoId,
        EstadoTarea Estado,
        DateOnly FechaEntrega,
        TimeOnly? HoraEntrega);
}