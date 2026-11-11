namespace TareasService.Features.Tareas.Get;

internal class GetTareasValidator : AbstractValidator<GetTareasQuery>
{
	public GetTareasValidator()
	{
		RuleFor(x => x.Hasta)
			.GreaterThanOrEqualTo(x => x.Desde)
			.When(x => x.Desde is not null && x.Hasta is not null)
			.WithMessage("La fecha 'hasta' no puede ser anterior a 'desde'");
	}
}