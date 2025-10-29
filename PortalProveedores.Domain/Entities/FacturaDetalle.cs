using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Domain.Entities
{
    /// <summary>
    /// Detalles de items de la factura.
    /// </summary>
    public class FacturaDetalle
    {
        public long Id { get; set; }
        public long FacturaId { get; set; }
        public Factura Factura { get; set; } = null!;

        public string Sku { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
