namespace TareasService.Domain;

public class Tarea
{
    public int Id { get; set; }

    // Dueño: sale del token, nunca del body
    public int UsuarioId { get; set; }

    // Ids de otros servicios: sin FK (database per service)
    public int MateriaId { get; set; }
    public int? GrupoId { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public DateOnly FechaEntrega { get; set; }
    public TimeOnly? HoraEntrega { get; set; }

    public Prioridad Prioridad { get; set; } = Prioridad.Media;
    public EstadoTarea Estado { get; set; } = EstadoTarea.Pendiente;

    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }

    // Abierta = todavía se puede trabajar en ella (para "vencidas" en el resumen)
    public bool EstaAbierta => Estado is EstadoTarea.Pendiente or EstadoTarea.EnProgreso;
}