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
using System.Collections.Generic;

namespace PortalProveedores.Infrastructure.Services
{
    public class JwtGeneratorService : IJwtGeneratorService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IApplicationDbContext _context;

        public JwtGeneratorService(
            IConfiguration config, 
            UserManager<ApplicationUser> userManager,
            IApplicationDbContext context)
        {
            config = _configuration;
            _userManager = userManager;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            _issuer = config["Jwt:Issuer"]!;
            _audience = config["Jwt:Audience"]!;
            _context = context;
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

        /// <summary>
        /// Crea un JWT que incluye el User ID, UserName y los Roles.
        /// </summary>
        public string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Email!)
        };

            // Agregar roles como claims
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Obtener la clave secreta de la configuración
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:TokenDurationMinutes"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Genera un nuevo refresh token, lo guarda en la DB y devuelve su valor.
        /// </summary>
        public async Task<string> GenerateRefreshTokenAsync(ApplicationUser user)
        {
            // Generar un token único y aleatorio
            var token = Guid.NewGuid().ToString("N");

            var refreshToken = new RefreshToken
            {
                Token = token,
                UserId = user.Id,
                // Usamos la configuración para la expiración (ej: 30 días)
                Expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtSettings:RefreshTokenDurationDays"])),
                Created = DateTime.Now
            };

            // 1. Guardar el nuevo refresh token en la base de datos
            // Nota: Asumimos que DbContext tiene DbSet<RefreshToken>
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync(CancellationToken.None);

            // 2. Devolver el token generado
            return token;
        }
    }
}
