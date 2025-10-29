using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Entities.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PortalProveedores.Infrastructure.Services
{
    public class JwtGeneratorService : IJwtGeneratorService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly UserManager<ApplicationUser> _userManager;

        public JwtGeneratorService(IConfiguration config, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            _issuer = config["Jwt:Issuer"]!;
            _audience = config["Jwt:Audience"]!;
        }

        public async Task<string> CreateTokenAsync(ApplicationUser user)
        {
            // 1. Claims básicos
            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.NameId, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.GivenName, user.NombreCompleto)
        };

            // 2. Claims de Roles
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // 3. Claims personalizados para la lógica de negocio
            if (user.ProveedorId.HasValue)
            {
                claims.Add(new Claim("proveedorId", user.ProveedorId.Value.ToString()));
            }
            if (user.ClienteId.HasValue)
            {
                claims.Add(new Claim("clienteId", user.ClienteId.Value.ToString()));
            }

            // 4. Credenciales de firma
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            // 5. Descriptor del token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7), // Token válido por 7 días
                SigningCredentials = creds,
                Issuer = _issuer,
                Audience = _audience
            };

            // 6. Crear y escribir el token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
