namespace TareasService.Domain;

public class EventoPendiente
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Tipo { get; set; } = string.Empty;      // tarea_creada, tarea_actualizada, tarea_cancelada
    public int TareaId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public DateTime FechaEvento { get; set; }
    public int Intentos { get; set; } = 1;
    public DateTime UltimoIntento { get; set; }
}