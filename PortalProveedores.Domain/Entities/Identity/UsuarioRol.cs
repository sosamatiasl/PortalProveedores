using Microsoft.AspNetCore.Identity;

namespace PortalProveedores.Domain.Entities.Identity
{
    // Hereda de IdentityUserRole usando 'string' para la FK de Usuario e 'int' para la FK de Rol
    public class UsuarioRol : IdentityUserRole<long>
    {
        public override long UserId { get; set; }
        public virtual ApplicationUser Usuario { get; set; } = null!;

        public override long RoleId { get; set; }
        public virtual ApplicationRole Rol { get; set; } = null!;
    }
}
