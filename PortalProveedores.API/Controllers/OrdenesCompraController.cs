using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalProveedores.Application.Features.Compras.Commands;
using PortalProveedores.Domain.Entities.Identity; // Para los roles

namespace PortalProveedores.API.Controllers
{
    /// <summary>
    /// Se exponen los Casos de Uso (Commands) como Endpoints y actualizamos la configuración.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Todas las operaciones requieren autenticación
    public class OrdenesCompraController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdenesCompraController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Crea una nueva Orden de Compra.
        /// Solo accesible por usuarios con rol "AdministrativoCliente".
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "AdministrativoCliente")] // Rol A
        [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateOrdenCompra([FromBody] CrearOrdenCompraCommand command)
        {
            try
            {
                var ordenCompraId = await _mediator.Send(command);
                // Devuelve 201 Created con la URL al nuevo recurso (práctica REST)
                return CreatedAtAction(nameof(GetOrdenCompraById), new { id = ordenCompraId }, ordenCompraId);
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
        /// Obtiene una Orden de Compra por su ID (endpoint de ejemplo).
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrdenCompraById(long id)
        {
            // Aquí iría la lógica de un Query de MediatR (ej. GetOrdenCompraQuery)
            // ...
            return Ok(new { Message = $"Obteniendo OC {id}" });
        }
    }
}
