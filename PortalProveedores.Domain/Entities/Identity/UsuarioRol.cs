using Microsoft.AspNetCore.Identity;

namespace PortalProveedores.Domain.Entities.Identity
{
    // Hereda de IdentityUserRole usando 'string' para la FK de Usuario e 'int' para la FK de Rol
    public class UsuarioRol : IdentityUserRole<string>
    {
        public override string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser Usuario { get; set; } = null!;

        public override string RoleId { get; set; } = string.Empty;
        public virtual ApplicationRole Rol { get; set; } = null!;
    }
}
