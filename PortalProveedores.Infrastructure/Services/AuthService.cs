using PortalProveedores.Application.Common.Interfaces; // Interfaz de Application
using PortalProveedores.Application.Models; // DTOs de Application
using PortalProveedores.Domain.Entities.Identity; // Entidad User de Domain
using PortalProveedores.Infrastructure.Persistence; // DbContext está en Persistence
using Microsoft.Extensions.Configuration; // Para leer la configuración JWT
using Microsoft.AspNetCore.Identity; // Para UnauthorizedAccessException
namespace PortalProveedores.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly PortalProveedoresDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtGeneratorService _jwtGeneratorService;
        private readonly IConfiguration _configuration;

        public AuthService(
            PortalProveedoresDbContext context,
            UserManager<ApplicationUser> userManager,
            IJwtGeneratorService jwtGeneratorService,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _jwtGeneratorService = jwtGeneratorService;
            _configuration = configuration;
        }

        public async Task<ApplicationUser> Register(string username, string password)
        {
            // 1. Validar si el usuario ya existe (usando UserManager)
            var existingUser = await _userManager.FindByNameAsync(username);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"El usuario '{username}' ya existe.");
            }

            // 2. Crear la entidad ApplicationUser (SIN PasswordHash ni PasswordSalt)
            var newUser = new ApplicationUser
            {
                UserName = username
                // Configurar otras propiedades necesarias aquí (ej: Email, NombreCompleto, etc.)
                // Email = "algun@email.com", 
            };

            // 3. Crear el usuario en la BD y HASHear la contraseña usando UserManager
            var identityResult = await _userManager.CreateAsync(newUser, password);

            if (!identityResult.Succeeded)
            {
                var errors = string.Join("; ", identityResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Fallo el registro del usuario: {errors}");
            }

            // 4. Retornar el usuario creado (ya tiene su Id asignado)
            return newUser;
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            // 1. Buscar usuario en la base de datos
            var user = await _userManager.FindByNameAsync(request.Username);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Credenciales inválidas.");
            }

            // 2. Verificar la contraseña
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Credenciales inválidas.");
            }

            // 3. Obtener los roles para el JWT
            IList<string> roles = await _userManager.GetRolesAsync(user);

            // 4. Generar y devolver el token
            var token = await _jwtGeneratorService.CreateTokenAsync(user, roles);

            return new LoginResponse
            {
                Token = token,
                Expiration = DateTime.Now.AddMinutes(30)
            };
        }
    }
}
