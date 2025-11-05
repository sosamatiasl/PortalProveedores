using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalProveedores.Application.Models;
using System.Threading.Tasks;

namespace PortalProveedores.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Todo el circuito requiere autenticación
    public class OrderController : ControllerBase
    {
        public OrderController()
        {
            // Inyectar el servicio de Órdenes (IOrderService)
        }

        // POST api/Order (Cliente crea la Orden)
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            // Lógica para guardar la orden en la DB (IOrderService)
            // ...
            // var newOrder = await _orderService.CreateOrder(request, GetUserId());
            return Ok(new { OrderId = 123 /* Id de la nueva orden */ });
        }

        // GET api/Order/MyOrders (Cliente o Proveedor ven sus órdenes)
        [HttpGet("MyOrders")]
        public async Task<IActionResult> GetMyOrders()
        {
            // Lógica para obtener órdenes basadas en el rol/id del usuario
            // ...
            // if (User.IsInRole("Provider")) { ... }
            // else { ... }
            return Ok(/* Lista de OrderDto */);
        }

        // PUT api/Order/ConfirmPayment/{id} (Proveedor confirma pago)
        [HttpPut("ConfirmPayment/{id}")]
        [Authorize(Roles = "Provider")] // Solo el rol "Proveedor" puede hacer esto
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            // Lógica para actualizar el estado de la orden a "Pagada"
            // await _orderService.ConfirmPayment(id, GetProviderId());
            return Ok(new { message = "Pago confirmado." });
        }
    }
}
