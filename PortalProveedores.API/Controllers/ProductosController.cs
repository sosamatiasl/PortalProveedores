using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalProveedores.Application.Features.Productos.Commands;

namespace PortalProveedores.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión del Catálogo (CRUD).
    /// </summary>
    [ApiController]
    [Route("api/catalogo/productos")]
    [Authorize(Roles = "AdministrativoCliente")] // ¡Todo este controlador es solo para Rol A!
    public class ProductosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Crea un nuevo producto maestro en el catálogo del Cliente.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CrearProducto([FromBody] CrearProductoCommand command)
        {
            try
            {
                var productoId = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetProductoById), new { id = productoId }, productoId);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Asigna un precio acordado a un Proveedor para un Producto específico.
        /// </summary>
        [HttpPost("asignar-precio")]
        [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AsignarPrecio([FromBody] AsignarPrecioProveedorCommand command)
        {
            try
            {
                var precioId = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetProductoById), new { id = command.ProductoId }, precioId); // Retorna el ID del precio
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id:long}")] // Endpoint helper para el CreatedAtAction
        public async Task<IActionResult> GetProductoById(long id)
        {
            // (Lógica de Query (GetProductoQuery) ... )
            return Ok(new { Message = $"Obteniendo Producto {id}" });
        }
    
        // ... Aquí irían los endpoints [HttpPut] para ActualizarProducto y [HttpDelete] ...
    }
}
