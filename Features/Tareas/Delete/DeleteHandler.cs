namespace TareasService.Features.Tareas.Delete;

// --- Entrada: el dueño (del token) y la tarea (de la URL). No devuelve datos ---
public record DeleteTareaCommand(int UsuarioId, int TareaId) : ICommand;

// --- Lógica: eliminación lógica → la tarea pasa a "cancelada", no se borra de la base ---
internal class DeleteTareaHandler(TareasDbContext db) : ICommandHandler<DeleteTareaCommand>
{
    public async Task<Unit> Handle(DeleteTareaCommand command, CancellationToken cancellationToken)
    {
        // Se busca por id Y por dueño: si es de otro usuario, 404 igual que si no existiera
        var tarea = await db.Tareas
            .FirstOrDefaultAsync(t => t.Id == command.TareaId && t.UsuarioId == command.UsuarioId, cancellationToken)
            ?? throw new NotFoundException("La tarea no existe");

        if (tarea.Estado == EstadoTarea.Cancelada)
            throw new BadRequestException("La tarea ya está cancelada");

        // Misma máquina de estados: una tarea completada no se puede cancelar
        if (!TransicionesEstado.EsValida(tarea.Estado, EstadoTarea.Cancelada))
            throw new BadRequestException("No se puede cancelar una tarea completada");

        tarea.Estado = EstadoTarea.Cancelada;
        tarea.FechaActualizacion = FechaColombia.Ahora();

        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}