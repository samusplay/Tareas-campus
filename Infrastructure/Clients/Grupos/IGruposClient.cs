namespace TareasService.Infrastructure.Clients.Grupos;

public interface IGruposClient
{
    Task<int?> ObtenerMateriaIdAsync(int grupoId, CancellationToken cancellationToken);
}