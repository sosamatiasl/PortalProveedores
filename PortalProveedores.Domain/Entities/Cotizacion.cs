using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortalProveedores.Domain.Enums;

namespace PortalProveedores.Domain.Entities
{
    public class Cotizacion
    {
        public long Id { get; set; }
        public long OrdenCompraId { get; set; }
        public OrdenCompra OrdenCompra { get; set; } = null!;

        public long ProveedorId { get; set; }
        public Proveedor Proveedor { get; set; } = null!;

        public string NumeroCotizacion { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public decimal MontoTotal { get; set; }
        public int ValidezDias { get; set; }
        public EstadoCotizacion Estado { get; set; }
        public string? ArchivoPDF_URL { get; set; }

        // Relaciones de navegación
        public ICollection<CotizacionItem> Items { get; set; } = new List<CotizacionItem>();
    }
}
