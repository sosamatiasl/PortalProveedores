using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalProveedores.Application.Features.Reports.Queries;
using PortalProveedores.Domain.Enums;

namespace PortalProveedores.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Todas las operaciones requieren autenticación
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Devuelve la trazabilidad completa de un documento (OC, Remito o Factura).
        /// Accesible por Roles A y B.
        /// </summary>
        [HttpGet("trazabilidad")]
        [Authorize(Roles = "AdministrativoCliente, AdministrativoProveedor")] // Roles A y B
        [ProducesResponseType(typeof(TrazabilidadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetTrazabilidad(long documentoId, TipoDocumento tipoDocumento)
        {
            try
            {
                var query = new GetTrazabilidadQuery { DocumentoId = documentoId, TipoDocumento = tipoDocumento };
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
        /// Devuelve el reporte de Conciliación a Tres Vías (OC vs Remito vs Factura) para una Factura.
        /// Accesible ÚNICAMENTE por Rol A (Administrativo Cliente).
        /// </summary>
        [HttpGet("conciliacion/{facturaId:long}")]
        [Authorize(Roles = "AdministrativoCliente")] // Rol A
        [ProducesResponseType(typeof(ConciliacionReporteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetConciliacion(long facturaId)
        {
            try
            {
                var query = new GetConciliacionQuery { FacturaId = facturaId };
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
    }
}
