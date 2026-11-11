namespace TareasService.Common.Exceptions;

// Igual que en materias. En tareas el contrato no define un 409 hoy, pero queda disponible.
public class ConflictException(string message) : Exception(message);