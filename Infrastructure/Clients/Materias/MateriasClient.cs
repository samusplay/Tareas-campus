using System.Net;
using Polly.CircuitBreaker;
using Polly.Timeout;
using TareasService.Common.Exceptions;

namespace TareasService.Infrastructure.Clients.Materias;

public class MateriasClient(HttpClient httpClient) : IMateriasClient
{
    public async Task<bool> ExisteAsync(int materiaId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.GetAsync($"api/v1/materias/{materiaId}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound) return false;

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex) when (ex is HttpRequestException or TimeoutRejectedException or BrokenCircuitException)
        {
            throw new ServiceUnavailableException("El servicio de materias no está disponible");
        }
    }
}