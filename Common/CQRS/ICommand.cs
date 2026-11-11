using MediatR;

namespace TareasService.Common.CQRS;

// Comando que no devuelve datos (MediatR usa Unit como "vacío")
public interface ICommand : ICommand<Unit>;

// Comando que cambia datos y devuelve un resultado
public interface ICommand<out TResponse> : IRequest<TResponse>;