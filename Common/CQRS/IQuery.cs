using MediatR;

namespace TareasService.Common.CQRS;

// Consulta: solo lee, nunca cambia datos
public interface IQuery<out TResponse> : IRequest<TResponse>
    where TResponse : notnull;