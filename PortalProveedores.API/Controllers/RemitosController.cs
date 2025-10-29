using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalProveedores.Application.Features.Remitos.Commands;
using PortalProveedores.Application.Features.Remitos.Queries;

namespace PortalProveedores.API.Controllers
{
    /// <summary>
    /// Controlador para gestionar los Remitos.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Todas las operaciones requieren autenticación
    public class RemitosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RemitosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Crea un nuevo Remito, adjunta el PDF y lo asocia a cotizaciones aceptadas.
        /// Accesible por Roles B (Admin Proveedor) y E (Despachante).
        /// </summary>
        /// <remarks>
        /// Se usa [FromForm] porque la petición incluye un archivo (PDF).
        /// </remarks>
        [HttpPost]
        [Authorize(Roles = "AdministrativoProveedor, DespachanteMercaderia")] // Roles B y E
        [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateRemito([FromForm] CrearRemitoCommand command)
        {
            try
            {
                var remitoId = await _mediator.Send(command);
                // Devuelve 201 Created con la URL al nuevo recurso (práctica REST)
                return CreatedAtAction(nameof(GetRemitoById), new { id = remitoId }, remitoId);
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
        /// Genera el token seguro (puntero) para el QR del transportista.
        /// Accesible ÚNICAMENTE por Rol E (Despachante).
        /// </summary>
        [HttpPost("{id:long}/generar-qr")]
        [Authorize(Roles = "DespachanteMercaderia")] // ¡Solo Rol E!
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GenerarQR(long id)
        {
            try
            {
                var command = new GenerarQRRemitoCommand { RemitoId = id };
                var qrToken = await _mediator.Send(command);

                // Se devuelve el string (token). La app cliente (móvil/web) se encargará de convertir este string en una imagen QR.
                return Ok(qrToken);
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
        /// Valida un token de QR y devuelve los items esperados para la recepción.
        /// Accesible por Rol D (Recepcionador).
        /// </summary>
        [HttpGet("validar-qr/{qrToken}")]
        [Authorize(Roles = "RecepcionadorMercaderia")] // Rol D
        [ProducesResponseType(typeof(RemitoParaRecepcionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ValidarQrRecepcion(string qrToken)
        {
            try
            {
                var query = new GetRemitoParaRecepcionQuery { QrToken = qrToken };
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
        /// Confirma la recepción de un remito, registrando diferencias y firmas.
        /// Accesible por Rol D (Recepcionador).
        /// </summary>
        [HttpPost("recepcionar")]
        [Authorize(Roles = "RecepcionadorMercaderia")] // Rol D
        [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ConfirmarRecepcion([FromBody] ConfirmarRecepcionCommand command)
        {
            try
            {
                var recepcionId = await _mediator.Send(command);
                return Ok(new { recepcionId = recepcionId });
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
        /// Obtiene un Remito por su ID (endpoint de ejemplo).
        /// </summary>
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetRemitoById(long id)
        {
            // Lógica de Query (GetRemitoQuery) ...
            return Ok(new { Message = $"Obteniendo Remito {id}" });
        }
    }
}
