using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace PortalProveedores.Application.Models
{
    // DTO para registrar un nuevo Proveedor (puede ser similar al cliente o tener más campos)
    public class RegisterProviderRequest
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        public string Username { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El CUIT/Identificación Fiscal es obligatorio")]
        public string TaxId { get; set; } // CUIT, RUT, etc.
    }
}
