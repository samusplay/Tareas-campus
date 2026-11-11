namespace TareasService.Features.Tareas.CambiarEstado;

internal class CambiarEstadoValidator : AbstractValidator<CambiarEstadoCommand>
{
    public CambiarEstadoValidator()
    {
        RuleFor(x => x.Estado)
            .NotNull().WithMessage("El estado es obligatorio");
    }
}