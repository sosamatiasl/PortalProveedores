using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalProveedores.Application.Features.Cotizaciones.Commands;
using PortalProveedores.Application.Features.Cotizaciones.Queries;

namespace PortalProveedores.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Todas las operaciones requieren autenticación
    public class CotizacionesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CotizacionesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Crea una nueva Cotización asociada a una Orden de Compra.
        /// Solo accesible por usuarios con rol "AdministrativoProveedor".
        /// </summary>
        /// <remarks>
        /// Se usa [FromForm] porque la petición incluye un archivo (PDF).
        /// </remarks>
        [HttpPost]
        [Authorize(Roles = "AdministrativoProveedor")] // Rol B
        [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateCotizacion([FromForm] CrearCotizacionCommand command)
        {
            try
            {
                var cotizacionId = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetCotizacionById), new { id = cotizacionId }, cotizacionId);
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
        /// Obtiene una Cotización por su ID (endpoint de ejemplo).
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCotizacionById(long id)
        {
            // Lógica de Query (GetCotizacionQuery)
            // ...
            return Ok(new { Message = $"Obteniendo Cotización {id}" });
        }

        /// <summary>
        /// Obtiene todas las cotizaciones (y sus items) para una Orden de Compra específica.
        /// Accesible por el Cliente (dueño) y el Proveedor (asignado).
        /// </summary>
        [HttpGet("por-orden-compra/{ordenCompraId}")]
        [Authorize(Roles = "AdministrativoCliente, AdministrativoProveedor")] // Roles A y B
        [ProducesResponseType(typeof(List<CotizacionDetalleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCotizacionesPorOrdenCompra(long ordenCompraId)
        {
            try
            {
                var query = new GetCotizacionesPorOrdenCompraQuery { OrdenCompraId = ordenCompraId };
                var resultado = await _mediator.Send(query);
                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>
        /// Acepta masivamente una o más cotizaciones para una Orden de Compra.
        /// Solo accesible por el Cliente (Rol A).
        /// </summary>
        [HttpPost("aceptar-masivo")]
        [Authorize(Roles = "AdministrativoCliente")] // Rol A
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AceptarCotizacionesMasivo([FromBody] AceptarCotizacionesCommand command)
        {
            try
            {
                await _mediator.Send(command);
                return Ok(new { message = "Cotizaciones aceptadas exitosamente." });
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
