using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortalProveedores.Domain.Enums;

namespace PortalProveedores.Domain.Entities
{
    public class OrdenCompra
    {
        public long Id { get; set; }
        public long ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public long ProveedorId { get; set; }
        public Proveedor Proveedor { get; set; } = null!;

        public string NumeroOrden { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public EstadoOrdenCompra Estado { get; set; }
        public string? Detalles { get; set; }

        // Relaciones de navegación
        public ICollection<OrdenCompraItem> Items { get; set; } = new List<OrdenCompraItem>();
        public ICollection<Cotizacion> Cotizaciones { get; set; } = new List<Cotizacion>();
    }
}
