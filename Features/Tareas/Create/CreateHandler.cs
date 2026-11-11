using TareasService.Infrastructure.Clients.Grupos;
using TareasService.Infrastructure.Clients.Materias;
using TareasService.Infrastructure.Clients.Notificaciones;

namespace TareasService.Features.Tareas.Create;

// --- Entrada: lo que manda el front + el dueño, que sale del token ---
public record CreateTareaCommand(
    int UsuarioId,
    string Titulo,
    string? Descripcion,
    int MateriaId,
    int? GrupoId,
    DateOnly FechaEntrega,
    TimeOnly? HoraEntrega,
    Prioridad? Prioridad) : ICommand<CreateTareaResult>;

// --- Salida: lo que el front necesita para pintar la tarea ---
public record CreateTareaResult(
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
internal class CreateTareaHandler(
    TareasDbContext db,
    IMateriasClient materiasClient,
    IGruposClient gruposClient,
    INotificacionesClient notificacionesClient)
    : ICommandHandler<CreateTareaCommand, CreateTareaResult>
{
    public async Task<CreateTareaResult> Handle(
        CreateTareaCommand command,
        CancellationToken cancellationToken)
    {
        // Tarea de grupo: el grupo decide la materia (las materias son personales)
        if (command.GrupoId is int grupoId)
        {
            var materiaDelGrupo = await gruposClient.ObtenerMateriaIdAsync(grupoId, cancellationToken)
                ?? throw new BadRequestException($"El grupo {grupoId} no existe");

            if (materiaDelGrupo != command.MateriaId)
                throw new BadRequestException("La materia no corresponde al grupo");
        }
        // Tarea personal: la materia tiene que ser del usuario
        else if (!await materiasClient.ExisteAsync(command.MateriaId, cancellationToken))
        {
            throw new BadRequestException($"La materia {command.MateriaId} no existe");
        }

        var ahora = FechaColombia.Ahora();

        // Toda tarea nace pendiente; si no mandan prioridad, queda en media
        var tarea = new Tarea
        {
            UsuarioId = command.UsuarioId,
            Titulo = command.Titulo.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(command.Descripcion) ? null : command.Descripcion.Trim(),
            MateriaId = command.MateriaId,
            GrupoId = command.GrupoId,
            FechaEntrega = command.FechaEntrega,
            HoraEntrega = command.HoraEntrega,
            Prioridad = command.Prioridad ?? Prioridad.Media,
            Estado = EstadoTarea.Pendiente,
            FechaCreacion = ahora,
            FechaActualizacion = ahora
        };

        db.Tareas.Add(tarea);
        await db.SaveChangesAsync(cancellationToken);

        // Aviso a la campanita: si notificaciones falla, la tarea ya quedó guardada
        await notificacionesClient.NotificarAsync(
            new CrearNotificacionRequest(
                tarea.UsuarioId,
                "tarea",
                "Nueva tarea creada",
                $"{tarea.Titulo} vence el {tarea.FechaEntrega:dd/MM/yyyy}",
                tarea.Id),
            cancellationToken);

        return new CreateTareaResult(
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