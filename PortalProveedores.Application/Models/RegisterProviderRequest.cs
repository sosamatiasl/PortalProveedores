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
        [StringLength(50, MinimumLength = 4, ErrorMessage = "El usuario debe tener entre 4 y 50 caracteres.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El CUIT es obligatorio")]
        [DisplayFormat(DataFormatString = "0;##-########-#")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "El CUIT debe estar en formato XX-XXXXXXXX-X")]
        public string CUIT { get; set; }

        [Required(ErrorMessage = "La razón social es obligatoria")]
        [StringLength(300, MinimumLength = 1, ErrorMessage = "La razón social debe tener como máximo 300 caracteres.")]
        public string RazonSocial { get; set; }
    }
}
