using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortalProveedores.Domain.Entities.Identity;
using System.Threading.Tasks;

namespace PortalProveedores.Application.Common.Interfaces
{
    /// <summary>
    /// Interfaz para la generación y gestión de tokens de seguridad (JWT y Refresh Tokens).
    /// </summary>
    public interface IJwtGeneratorService
    {
        Task<string> CreateTokenAsync(ApplicationUser user);

        /// <summary>
        /// // Genera el token JWT.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        string GenerateJwtToken(ApplicationUser user, IList<string> roles);

        /// <summary>
        /// Genera el Refresh Token.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<string> GenerateRefreshTokenAsync(ApplicationUser user);
    }
}
