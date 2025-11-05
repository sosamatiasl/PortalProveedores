using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalProveedores.Application.Models;
using System.Threading.Tasks;

namespace PortalProveedores.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere autenticación
    public class ProviderController : ControllerBase
    {
        public ProviderController()
        {
            // Inyectar el servicio de Proveedores (IProviderService)
        }

        // POST api/Provider
        [HttpPost]
        public async Task<IActionResult> CreateProvider([FromBody] CreateProviderRequest request)
        {
            // Lógica para guardar el proveedor en la DB (usando IProviderService)
            // ...
            // var provider = await _providerService.Create(request, GetUserId());

            // GetUserId() obtendría el Id del Cliente desde el Token JWT
            // (ej: User.FindFirst(ClaimTypes.NameIdentifier)?.Value)

            return CreatedAtAction(nameof(GetProviderById), new { id = 1 /* id del nuevo proveedor */ }, request);
        }

        [HttpGet("{id}")]
        public IActionResult GetProviderById(int id)
        {
            // Endpoint de marcador de posición para CreatedAtAction
            return Ok();
        }
    }
}
