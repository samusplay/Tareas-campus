namespace TareasService.Features.Tareas.Create;

// Reglas del contrato (H.6). Se ejecuta solo antes del handler (ValidationBehavior)
internal class CreateTareaValidator : AbstractValidator<CreateTareaCommand>
{
    public CreateTareaValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("El título es obligatorio")
            .MaximumLength(150).WithMessage("El título no puede superar 150 caracteres");

        RuleFor(x => x.Descripcion)
            .MaximumLength(1000).WithMessage("La descripción no puede superar 1000 caracteres");

        RuleFor(x => x.MateriaId)
            .GreaterThan(0).WithMessage("La materia es obligatoria");

        RuleFor(x => x.GrupoId)
            .GreaterThan(0).When(x => x.GrupoId is not null)
            .WithMessage("El grupo no es válido");

        RuleFor(x => x.FechaEntrega)
            .GreaterThanOrEqualTo(_ => FechaColombia.Hoy())
            .WithMessage("La fecha de entrega no puede ser anterior a hoy");
    }
}