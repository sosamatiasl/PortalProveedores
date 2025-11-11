using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PortalProveedores.Web.Helpers
{
    public static class TokenHelper
    {
        // El tipo de claim estándar para roles en JWT es ClaimTypes.Role
        //private const string RoleClaimType = ClaimTypes.Role;
        private const string RoleClaimType = "role";

        /// <summary>
        /// Extrae la lista de roles (como strings) de un token JWT.
        /// </summary>
        public static List<string> GetRolesFromToken(string jwtToken)
        {
            if (string.IsNullOrEmpty(jwtToken))
            {
                return new List<string>();
            }

            var handler = new JwtSecurityTokenHandler();
            handler.InboundClaimTypeMap.Clear();
            var token = handler.ReadJwtToken(jwtToken);

            // Filtra los claims para obtener todos los valores de rol
            var roleClaims = token.Claims
                .Where(c => c.Type == RoleClaimType)
                .Select(c => c.Value)
                .ToList();

            return roleClaims;
        }
    }
}
