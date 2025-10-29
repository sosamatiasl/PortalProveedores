using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Domain.Entities
{
    /// <summary>
    /// Entidad de unión N-N.
    /// </summary>
    /// <remarks>
    /// Tabla de unión: Una Factura puede cubrir N Remitos.
    /// </remarks>
    public class FacturaRemitos
    {
        public long FacturaId { get; set; }
        public Factura Factura { get; set; } = null!;

        public long RemitoId { get; set; }
        public Remito Remito { get; set; } = null!;
    }
}
