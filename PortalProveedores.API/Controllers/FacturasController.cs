using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalProveedores.Application.Features.Facturas.Commands;

namespace PortalProveedores.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de Facturas.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Todas las operaciones requieren autenticación
    public class FacturasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FacturasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Carga el PDF de la Factura y desencadena el proceso de OCR/IA y Validación AFIP.
        /// Accesible por Rol B (Administrativo Proveedor).
        /// </summary>
        /// <remarks>
        /// Se usa [FromForm] porque incluye un archivo (PDF).
        /// </remarks>
        [HttpPost]
        [Authorize(Roles = "AdministrativoProveedor")] // Rol B
        [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CargarFactura([FromForm] CargarFacturaCommand command)
        {
            try
            {
                var facturaId = await _mediator.Send(command);
                // Devuelve 201 Created con la URL al nuevo recurso
                return CreatedAtAction(nameof(GetFacturaById), new { id = facturaId }, facturaId);
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
        /// Obtiene una Factura por su ID (endpoint de ejemplo).
        /// </summary>
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetFacturaById(long id)
        {
            // Lógica de Query (GetFacturaQuery) ...
            return Ok(new { Message = $"Obteniendo Factura {id}" });
        }
    }
}
