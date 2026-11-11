namespace TareasService.Infrastructure.Clients.Notificaciones;

public record CrearNotificacionRequest(
    int UsuarioId,
    string Tipo,
    string Titulo,
    string Mensaje,
    int? ReferenciaId);