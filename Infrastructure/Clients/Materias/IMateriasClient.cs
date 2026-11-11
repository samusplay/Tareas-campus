namespace TareasService.Infrastructure.Clients.Materias;

public interface IMateriasClient
{
    Task<bool> ExisteAsync(int materiaId, CancellationToken cancellationToken);
}