namespace TareasService.Domain;

public enum EstadoTarea
{
    Pendiente,
    EnProgreso,
    Completada,
    Cancelada
}

// Máquina de estados del contrato: completada y cancelada son finales
public static class TransicionesEstado
{
    private static readonly Dictionary<EstadoTarea, EstadoTarea[]> Permitidas = new()
    {
        [EstadoTarea.Pendiente] = [EstadoTarea.EnProgreso, EstadoTarea.Completada, EstadoTarea.Cancelada],
        [EstadoTarea.EnProgreso] = [EstadoTarea.Completada, EstadoTarea.Cancelada],
        [EstadoTarea.Completada] = [],
        [EstadoTarea.Cancelada] = []
    };

    public static bool EsValida(EstadoTarea actual, EstadoTarea nuevo) =>
        Permitidas[actual].Contains(nuevo);
}