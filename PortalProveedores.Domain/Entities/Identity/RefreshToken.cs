using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Domain.Entities.Identity
{
    public class RefreshToken
    {
        public long Id { get; set; }

        // El token en sí (generalmente un GUID o un string seguro)
        public string Token { get; set; } = string.Empty;

        // Fecha de creación del token
        public DateTime Created { get; set; }

        // Fecha de expiración (generalmente más larga que el Access Token)
        public DateTime Expires { get; set; }

        // Indica si el token ha sido revocado (usado para cerrar sesión)
        public DateTime? Revoked { get; set; }

        // Dirección IP desde donde se solicitó el token
        public string? RemoteIpAddress { get; set; }

        // Clave foránea al usuario de Identity
        public long UserId { get; set; }

        // Propiedad de navegación
        public ApplicationUser User { get; set; } = null!;

        // Propiedad calculada
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsActive => Revoked == null && !IsExpired;
    }
}
