namespace TareasService.Features.Tareas.Get;

// --- Entrada: el dueño (del token) + el rango opcional que pide el calendario ---
public record GetTareasQuery(
    int UsuarioId,
    DateOnly? Desde,
    DateOnly? Hasta) : IQuery<GetTareasResult>;

// --- Salida: lo que el front necesita para pintar cada tarea ---
public record TareaItem(
    int TareaId,
    string Titulo,
    string? Descripcion,
    int MateriaId,
    DateOnly FechaEntrega,
    TimeOnly? HoraEntrega,
    Prioridad Prioridad,
    EstadoTarea Estado);

public record GetTareasResult(IEnumerable<TareaItem> Tareas);

// --- Lógica ---
internal class GetTareasHandler(TareasDbContext db)
    : IQueryHandler<GetTareasQuery, GetTareasResult>
{
    public async Task<GetTareasResult> Handle(
        GetTareasQuery query,
        CancellationToken cancellationToken)
    {
        // Solo MIS tareas; las canceladas no se muestran
        var consulta = db.Tareas
            .AsNoTracking()
            .Where(t => t.UsuarioId == query.UsuarioId && t.Estado != EstadoTarea.Cancelada);

        // El calendario pide solo el mes visible
        if (query.Desde is DateOnly desde)
            consulta = consulta.Where(t => t.FechaEntrega >= desde);

        if (query.Hasta is DateOnly hasta)
            consulta = consulta.Where(t => t.FechaEntrega <= hasta);

        var tareas = await consulta
            .OrderBy(t => t.FechaEntrega)
            .ThenBy(t => t.HoraEntrega)
            .Select(t => new TareaItem(
                t.Id,
                t.Titulo,
                t.Descripcion,
                t.MateriaId,
                t.FechaEntrega,
                t.HoraEntrega,
                t.Prioridad,
                t.Estado))
            .ToListAsync(cancellationToken);

        return new GetTareasResult(tareas);
    }
}