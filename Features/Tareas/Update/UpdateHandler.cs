

namespace TareasService.Features.Tareas.Update;

// --- Entrada: la tarea completa (PUT reemplaza), el id de la URL y el dueño del token ---
public record UpdateTareaCommand(
    int TareaId,
    int UsuarioId,
    string Titulo,
    string? Descripcion,
    int MateriaId,
    int? GrupoId,
    DateOnly FechaEntrega,
    TimeOnly? HoraEntrega,
    Prioridad Prioridad) : ICommand<UpdateTareaResult>;

// --- Salida: la tarea ya actualizada ---
public record UpdateTareaResult(
    int TareaId,
    string Titulo,
    string? Descripcion,
    int MateriaId,
    DateOnly FechaEntrega,
    TimeOnly? HoraEntrega,
    Prioridad Prioridad,
    EstadoTarea Estado,
    DateTime FechaCreacion,
    DateTime FechaActualizacion);

// --- Lógica ---
internal class UpdateTareaHandler(
    TareasDbContext db,
    IMateriasClient materiasClient,
    IGruposClient gruposClient)
    : ICommandHandler<UpdateTareaCommand, UpdateTareaResult>
{
    public async Task<UpdateTareaResult> Handle(
        UpdateTareaCommand command,
        CancellationToken cancellationToken)
    {
        // Solo el dueño la ve: si no existe o es de otro, la misma respuesta
        var tarea = await db.Tareas.FirstOrDefaultAsync(
                t => t.Id == command.TareaId && t.UsuarioId == command.UsuarioId,
                cancellationToken)
            ?? throw new NotFoundException($"La tarea {command.TareaId} no existe");

        // Una tarea cerrada ya no se edita
        if (tarea.Estado is EstadoTarea.Completada or EstadoTarea.Cancelada)
            throw new ConflictException("No se puede editar una tarea completada o cancelada");

        // Solo se pregunta a otros servicios si cambió la materia o el grupo
        if (tarea.MateriaId != command.MateriaId || tarea.GrupoId != command.GrupoId)
        {
            if (command.GrupoId is int grupoId)
            {
                var materiaDelGrupo = await gruposClient.ObtenerMateriaIdAsync(grupoId, cancellationToken)
                    ?? throw new BadRequestException($"El grupo {grupoId} no existe");

                if (materiaDelGrupo != command.MateriaId)
                    throw new BadRequestException("La materia no corresponde al grupo");
            }
            else if (!await materiasClient.ExisteAsync(command.MateriaId, cancellationToken))
            {
                throw new BadRequestException($"La materia {command.MateriaId} no existe");
            }
        }

        tarea.Titulo = command.Titulo.Trim();
        tarea.Descripcion = string.IsNullOrWhiteSpace(command.Descripcion) ? null : command.Descripcion.Trim();
        tarea.MateriaId = command.MateriaId;
        tarea.GrupoId = command.GrupoId;
        tarea.FechaEntrega = command.FechaEntrega;
        tarea.HoraEntrega = command.HoraEntrega;
        tarea.Prioridad = command.Prioridad;
        tarea.FechaActualizacion = FechaColombia.Ahora();

        await db.SaveChangesAsync(cancellationToken);

        return new UpdateTareaResult(
            tarea.Id,
            tarea.Titulo,
            tarea.Descripcion,
            tarea.MateriaId,
            tarea.FechaEntrega,
            tarea.HoraEntrega,
            tarea.Prioridad,
            tarea.Estado,
            tarea.FechaCreacion,
            tarea.FechaActualizacion);
    }
}