namespace TareasService.Features.Tareas.Update;

public class UpdateTareaValidator : AbstractValidator<UpdateTareaCommand>
{
    public UpdateTareaValidator()
    {
        RuleFor(x => x.TareaId)
            .GreaterThan(0).WithMessage("El id de la tarea no es válido");

        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("El título es obligatorio")
            .MaximumLength(150).WithMessage("El título no puede superar 150 caracteres");

        RuleFor(x => x.Descripcion)
            .MaximumLength(1000).WithMessage("La descripción no puede superar 1000 caracteres");

        RuleFor(x => x.MateriaId)
            .GreaterThan(0).WithMessage("La materia no es válida");

        RuleFor(x => x.GrupoId)
            .GreaterThan(0).When(x => x.GrupoId is not null).WithMessage("El grupo no es válido");

        RuleFor(x => x.Prioridad)
            .IsInEnum().WithMessage("La prioridad no es válida");
    }
}