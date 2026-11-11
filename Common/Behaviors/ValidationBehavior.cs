using FluentValidation;
using MediatR;

namespace TareasService.Common.Behaviors;

// Pipeline de MediatR: corre los validators del request ANTES de llegar al handler.
// Si alguno falla → ValidationException → el GlobalExceptionHandler responde 400 con los errores por campo.
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Sin validator para este request (ej. GetById) → directo al handler
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        // Todos los validators del request a la vez, y juntamos todos sus errores
        var resultados = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errores = resultados
            .SelectMany(r => r.Errors)
            .Where(e => e is not null)
            .ToList();

        if (errores.Count != 0)
            throw new ValidationException(errores);

        return await next();
    }
}