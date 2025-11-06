using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalProveedores.Application.Features.Users.Commands;
using PortalProveedores.Application.Features.Users.Queries;

namespace PortalProveedores.API.Controllers
{
    /// <summary>
    /// Controlador para la Gestión de Usuarios.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "AdministrativoCliente, AdministrativoProveedor")] // Solo Roles A y B
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lista los usuarios asociados a la entidad del administrador (Cliente o Proveedor).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query)
        {
            try
            {
                var resultado = await _mediator.Send(query);
                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cambia el rol de un usuario dentro del ámbito del administrador.
        /// </summary>
        [HttpPut("{userId}/role")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateUserRole(long userId, [FromBody] UpdateUserRoleCommand command)
        {
            try
            {
                // Aseguramos que el ID en la ruta se use en el comando
                command.UserId = userId;
                await _mediator.Send(command);
                return Ok(new { message = $"Rol del usuario {userId} actualizado a {command.NewRoleId}." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
