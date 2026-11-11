namespace TareasService.Common.Exceptions;

// Materias o grupos no respondieron (tras los reintentos o con el circuito abierto).
// Contrato J: un servicio caído NUNCA se interpreta como "el dato no existe".
public class ServiceUnavailableException(string servicio)
    : Exception($"El servicio de {servicio} no está disponible en este momento");