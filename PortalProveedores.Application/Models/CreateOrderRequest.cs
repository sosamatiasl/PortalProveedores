using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Application.Models
{
    // DTO para crear una Orden de Compra
    public class CreateOrderRequest
    {
        [Required]
        public int ProviderId { get; set; } // ID del Proveedor al que se le compra
        public DateTime IssueDate { get; set; } = DateTime.Now;
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
        public decimal Total { get; set; } // El total se puede calcular en el cliente
    }

    public class OrderItemDto
    {
        [Required]
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
