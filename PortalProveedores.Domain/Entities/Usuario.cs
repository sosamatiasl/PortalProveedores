using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Domain.Entities
{
    public class User // O Usuario
    {
        public int Id { get; set; }
        public string Username { get; set; } // O Email/Nombre de usuario

        // --- Campos de Seguridad ---
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        // ---------------------------

        // ... otras propiedades ...
    }
}
