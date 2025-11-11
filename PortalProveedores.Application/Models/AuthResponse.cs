using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Application.Models
{
    public class AuthResponse
    {
        /// <summary>
        /// El JSON Web Token (JWT) de corta duración para las peticiones API.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// El Refresh Token de larga duración para obtener un nuevo JWT sin re-loggearse.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Identificador único del usuario (ApplicationUser.Id).
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Nombre de usuario para mostrar en la interfaz.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del usuario.
        /// </summary>
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Rol principal del usuario (ej: 'Cliente' o 'Proveedor').
        /// </summary>
        public string RolDefecto { get; set; } = string.Empty;

        /// <summary>
        /// Roles del usuario
        /// </summary>
        public IList<string>? Roles { get; set; }
    }
}
