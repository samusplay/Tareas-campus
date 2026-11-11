using System.Security.Claims;

namespace TareasService.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    // El usuario_id viaja en el claim "sub" del JWT (como texto: "5")
    public static int GetUsuarioId(this ClaimsPrincipal user)
    {
        // ASP.NET renombra "sub" a NameIdentifier al leer el token; revisamos los dos por si acaso
        var valor = user.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user.FindFirstValue("sub");

        return int.TryParse(valor, out var usuarioId) && usuarioId > 0
            ? usuarioId
            : throw new UnauthorizedAccessException("El token no trae un usuario válido");
    }
}