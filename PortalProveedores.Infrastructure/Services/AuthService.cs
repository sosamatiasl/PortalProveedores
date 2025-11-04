using PortalProveedores.Application.Common.Interfaces; // Interfaz de Application
using PortalProveedores.Application.Models; // DTOs de Application
using PortalProveedores.Domain.Entities.Identity; // Entidad User de Domain
using PortalProveedores.Infrastructure.Persistence; // DbContext está en Persistence
using Microsoft.EntityFrameworkCore; // Para FirstOrDefaultAsync
using System.Security.Cryptography; // Para hashing
using System.Text; // Para Encoding
using System.IdentityModel.Tokens.Jwt; // Para JWT
using System.Security.Claims; // Para Claims
using Microsoft.IdentityModel.Tokens; // Para SigningCredentials
using Microsoft.Extensions.Configuration; // Para leer la configuración JWT
using System.Threading.Tasks;
using System; // Para UnauthorizedAccessException
namespace PortalProveedores.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly PortalProveedoresDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(PortalProveedoresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // --- HASHING Y VERIFICACIÓN DE CONTRASEÑAS (sin cambios) ---

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }
            return true;
        }

        // --- GENERACIÓN DE TOKEN JWT ---

        // El argumento ahora es ApplicationUser
        private string GenerateJwtToken(ApplicationUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt Key no configurada.")));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // --- IMPLEMENTACIÓN REAL DE IAuthService ---

        // El tipo de retorno ahora es ApplicationUser
        public async Task<ApplicationUser> Register(string username, string password)
        {
            // Nota: Aquí se debería validar si el usuario ya existe.
            CreatePasswordHash(password, out var passwordHash, out var passwordSalt);

            var user = new ApplicationUser // <-- TIPO CORREGIDO
            {
                UserName = username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            // Asumo que tu DbSet se llama Users en el DbContext
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            // 1. Buscar usuario en la base de datos
            // El resultado se asigna a ApplicationUser
            var user = await _context.Users.OfType<ApplicationUser>().FirstOrDefaultAsync(u => u.UserName == request.Username);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Credenciales inválidas.");
            }

            // 2. Verificar la contraseña
            // user.PasswordHash y user.PasswordSalt ahora son accesibles
            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                throw new UnauthorizedAccessException("Credenciales inválidas.");
            }

            // 3. Generar y devolver el token
            var token = GenerateJwtToken(user);

            return new LoginResponse
            {
                Token = token,
                Expiration = DateTime.Now.AddMinutes(30)
            };
        }
    }
}
