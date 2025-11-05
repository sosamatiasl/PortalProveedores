using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PortalProveedores.API.Models;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Application.Features.Auth.Commands;
using PortalProveedores.Application.Models;
using PortalProveedores.Domain.Entities.Identity;
using PortalProveedores.Infrastructure.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace PortalProveedores.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtGeneratorService _jwtGenerator;
        private readonly IIdentityService _identityService;

        public AuthController(
            IMediator mediator,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IJwtGeneratorService jwtGenerator,
            IIdentityService identityService)
        {
            _mediator = mediator;
            _signInManager = signInManager;
            _userManager = userManager;
            _jwtGenerator = jwtGenerator;
            _identityService = identityService;
        }

        /// <summary>
        /// Registra un nuevo usuario con su selfie.
        /// </summary>
        /// <remarks>
        /// Esto es [FromForm] porque incluye un archivo (multipart/form-data).
        /// </remarks>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromForm] RegisterCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }
            return Ok(result);
        }

        // Endpoint de Registro (solo para DEVELOPMENT)
        [HttpPost("Register")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Register([FromBody] Application.Models.LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // ---> TODO: MAL IMPLEMENTADO <---
                // await _authService.Register(request.Username, request.Password);
                // _authService no tiene que aparecer en el API Controller

                return StatusCode(201, new { message = "Usuario registrado exitosamente." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al registrar usuario.", details = ex.Message });
            }
        }

        // POST api/Authentication/RegisterClient
        [HttpPost("RegisterClient")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RegisterClient([FromBody] RegisterClientRequest request)
        {
            // Nota: En IAuthService, necesitará un método "RegisterClient"
            // que cree el ApplicationUser y también la entidad Cliente asociada.
            var command = new RegisterClientCommand
            {
                Username = request.Username,
                CompanyName = request.CompanyName,
                Email = request.Email,
                Password = request.Password
            };

            try
            {
                var result = await _mediator.Send(command);
                if (!result.Succeeded)
                {
                    return BadRequest(new {
                        message = "Falló el registro del nuevo cliente.",
                        errors = result.Errors.Select(e => e.Description).ToList()
                    });
                }

                return StatusCode(201, new { message = "Cliente registrado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error interno en el servidor.", details = ex.Message });
            }
        }

        // POST api/Authentication/RegisterProvider
        [HttpPost("RegisterProvider")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RegisterProvider([FromBody] RegisterProviderRequest request)
        {
            // Similar al anterior, pero para Proveedores
            try
            {
                // await _authService.RegisterProvider(request);
                return StatusCode(201, new { message = "Proveedor registrado exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DTO solo para el login
        public record LoginDto(string Email, string Password);

        /// <summary>
        /// Inicia sesión con email y password.
        /// </summary>
        [HttpPost("Login")]
        //[ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] Application.Models.LoginRequest request)
        {
            /*
            //var user = await _userManager.FindByEmailAsync(request.Email);
            //if (user == null || !user.Activo)
            //{
            //    return Unauthorized(new { message = "Credenciales inválidas o usuario inactivo." });
            //}

            //var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            //if (!result.Succeeded)
            //{
            //    return Unauthorized(new { message = "Credenciales inválidas." });
            //}

            //// Generar y devolver el token
            //var roles = await _userManager.GetRolesAsync(user);
            //var token = await _jwtGenerator.CreateTokenAsync(user);

            //return Ok(new AuthResult(true, token, user.Id, roles, null));

            //// Obtener la IP del cliente
            //var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            //var (token, refreshToken) = await _identityService.LoginAsync(request.Email, request.Password, ipAddress);

            //// Devuelve el Access Token y el Refresh Token
            //return Ok(new { token, refreshToken });
            */

            try
            {
                var response = await _authService.Login(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Credenciales inválidas." });
            }
            catch (InvalidOperationException ex) // Para errores como "usuario ya existe" en Register si se reusa
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Loggear el error real para depuración
                return StatusCode(500, new { message = "Ocurrió un error interno en el servidor.", details = ex.Message });
            }
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // 1. Validar y revocar el token antiguo
            var user = await _identityService.ValidateRefreshTokenAsync(request.RefreshToken, ipAddress);

            if (user == null)
            {
                return Unauthorized(new { message = "Refresh Token inválido o expirado." });
            }

            // 2. Generar nuevo Access Token y un nuevo Refresh Token
            // Se usa el mismo método de Login
            // (simplificado aquí, en la vida real se usa un método dedicado a solo generar JWT)
            var newAccessToken = "NuevoAccessTokenGenerado"; // Placeholder
            var newRefreshToken = await _identityService.GenerateAndStoreRefreshTokenAsync(user.Id, ipAddress);

            // NOTA: En la implementación real de IIdentityService, se debe exponer un método 
            // para generar solo el JWT sin pasar por toda la lógica de login.

            return Ok(new { token = newAccessToken, refreshToken = newRefreshToken });
        }

        // --- ENDPOINT PARA REVOCAR TOKEN (CERRAR SESIÓN) ---
        [HttpPost("revoke")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Revoke([FromBody] RefreshRequest request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var success = await _identityService.RevokeRefreshTokenAsync(request.RefreshToken, ipAddress);

            if (!success)
            {
                return BadRequest(new { message = "El Refresh Token no existe o ya ha sido revocado." });
            }

            return Ok(new { message = "Sesión cerrada correctamente." });
        }


        // Aquí van los endpoints para login con Google y Microsoft
        // [HttpGet("login-google")]
        // [HttpGet("login-microsoft")]
        // ...y sus respectivos callbacks

    }
}
