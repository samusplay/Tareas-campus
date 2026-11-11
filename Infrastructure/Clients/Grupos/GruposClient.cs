using System.Net;
using System.Text.Json.Serialization;
using Polly.CircuitBreaker;
using Polly.Timeout;
using TareasService.Common.Exceptions;

namespace TareasService.Infrastructure.Clients.Grupos;

public class GruposClient(HttpClient httpClient) : IGruposClient
{
    public async Task<int?> ObtenerMateriaIdAsync(int grupoId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.GetAsync($"api/v1/grupos/{grupoId}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            response.EnsureSuccessStatusCode();

            var grupo = await response.Content.ReadFromJsonAsync<GrupoResponse>(cancellationToken);
            return grupo!.MateriaId;
        }
        catch (Exception ex) when (ex is HttpRequestException or TimeoutRejectedException or BrokenCircuitException)
        {
            throw new ServiceUnavailableException("El servicio de grupos no está disponible");
        }
    }

    private record GrupoResponse([property: JsonPropertyName("materia_id")] int MateriaId);
}