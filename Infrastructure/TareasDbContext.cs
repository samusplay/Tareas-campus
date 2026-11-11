using Microsoft.EntityFrameworkCore;
using TareasService.Domain;

namespace TareasService.Infrastructure;

public class TareasDbContext(DbContextOptions<TareasDbContext> options) : DbContext(options)
{
    public DbSet<Tarea> Tareas => Set<Tarea>();
    public DbSet<EventoPendiente> EventosPendientes => Set<EventoPendiente>();

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        // Obligatorio con Npgsql: hora de Colombia sin zona (si no, exige UTC y falla al guardar)
        builder.Properties<DateTime>().HaveColumnType("timestamp without time zone");

        // Enums como texto en la base ("Pendiente") y no como números
        builder.Properties<EstadoTarea>().HaveConversion<string>();
        builder.Properties<Prioridad>().HaveConversion<string>();
    }
}