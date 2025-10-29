using Microsoft.AspNetCore.Identity;

namespace PortalProveedores.Domain.Entities.Identity
{
    // Heredamos de IdentityUser usando un 'string' como tipo de la clave primaria (PK)
    public class ApplicationUser : IdentityUser<string>
    {
        // Las propiedades base (Id, UserName, Email, PasswordHash, etc.) ya existen

        public string NombreCompleto { get; set; } = string.Empty;
        public string? SelfieFotoURL { get; set; } // Path al blob storage

        // Relaciones con las entidades de negocio
        public long? ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }

        public long? ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
        public bool Activo { get; set; } = true;

        // Relación N-N con roles
        public virtual ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    }
}
