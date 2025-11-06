using Microsoft.AspNetCore.Identity;

namespace PortalProveedores.Domain.Entities.Identity
{
    // Heredamos de IdentityRole usando 'int' como PK (se modificó a string porque ApplicationUser usa string)
    // OLD: public class ApplicationRole : IdentityRole<int>
    public class ApplicationRole : IdentityRole<long>
    {
        public string? Descripcion { get; set; }

        // Relación N-N con usuarios
        public virtual ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    }
}
