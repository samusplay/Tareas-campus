namespace TareasService.Common.Exceptions;

// Regla de negocio violada: transición de estado inválida, materia que no existe o no es tuya,
// no eres integrante del grupo, la tarea ya está cancelada...
public class BadRequestException(string message) : Exception(message);