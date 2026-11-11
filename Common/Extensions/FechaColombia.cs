namespace TareasService.Common.Extensions;

public static class FechaColombia
{
    private static readonly TimeZoneInfo Bogota = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");

    // Hora actual de Colombia, sin milisegundos
    public static DateTime Ahora()
    {
        var ahora = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Bogota);
        return new DateTime(ahora.Year, ahora.Month, ahora.Day, ahora.Hour, ahora.Minute, ahora.Second);
    }

    // "Hoy" en Colombia (para validar que fecha_entrega no sea anterior)
    public static DateOnly Hoy() => DateOnly.FromDateTime(Ahora());
}