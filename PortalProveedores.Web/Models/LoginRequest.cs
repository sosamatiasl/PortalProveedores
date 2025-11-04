using System.ComponentModel.DataAnnotations;

namespace PortalProveedores.Web.Models
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "El campo Usuario es obligatorio.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "El campo Contraseña es obligatorio.")]
        public string Password { get; set; }
    }
}
