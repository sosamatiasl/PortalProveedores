using MediatR;
using Microsoft.AspNetCore.Identity;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Application.Models;
using PortalProveedores.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Application.Features.Auth.Commands
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtGeneratorService _jwtGenerator;

        public LoginCommandHandler(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IJwtGeneratorService jwtGenerator)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // 1. Intentar el inicio de sesión con ASP.NET Identity
            var result = await _signInManager.PasswordSignInAsync(
                request.Username,
                request.Password,
                isPersistent: false, // No guardar cookie de sesión
                lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                // Manejar errores: credenciales inválidas, bloqueo, etc.
                throw new UnauthorizedAccessException("Credenciales inválidas. Por favor, verifique su usuario y contraseña.");
                // NOTA: Se recomienda no dar detalles específicos (ej. "usuario no existe") por seguridad.
            }

            // 2. Obtener el objeto ApplicationUser
            var user = await _userManager.FindByNameAsync(request.Username);

            if (user == null)
            {
                // Esto no debería pasar después de un result.Succeeded, pero es un control de seguridad.
                throw new Exception($"Usuario '{request.Username}' no encontrado después de autenticación exitosa.");
            }

            // 3. Generar JWT y Refresh Token
            IList<string> roles = await _userManager.GetRolesAsync(user);
            var jwtToken = await _jwtGenerator.CreateTokenAsync(user, roles);
            var refreshToken = await _jwtGenerator.GenerateRefreshTokenAsync(user);

            // 4. Devolver la respuesta de autenticación con los tokens
            return new AuthResponse
            {
                UserId = user.Id,
                Username = user.UserName!,
                Token = jwtToken,
                RefreshToken = refreshToken,
                // Aquí puede añadir otros datos como roles o NombreCompleto
                NombreCompleto = user.NombreCompleto ?? user.UserName
            };
        }
    }
}
