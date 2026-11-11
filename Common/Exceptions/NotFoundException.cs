namespace TareasService.Common.Exceptions;

// La tarea no existe O no es del usuario (misma respuesta: no se revela que existe)
public class NotFoundException(string message) : Exception(message);