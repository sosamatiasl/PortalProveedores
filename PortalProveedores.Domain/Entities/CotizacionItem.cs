using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Domain.Entities
{
    public class CotizacionItem
    {
        public long Id { get; set; }
        public long CotizacionId { get; set; }
        public Cotizacion Cotizacion { get; set; } = null!;

        public string Sku { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}
