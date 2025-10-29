using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Domain.Entities
{
    // Tabla de unión: Un Remito puede cubrir N Cotizaciones Aceptadas.
    // (Y una Cotización aceptada podría (aunque es raro) estar en N Remitos si se envía parcializado)
    /// <summary>
    /// Entidad de unión N-N, basada en el script SQL.
    /// </summary>
    public class CotizacionRemitos
    {
        public long CotizacionId { get; set; }
        public Cotizacion Cotizacion { get; set; } = null!;

        public long RemitoId { get; set; }
        public Remito Remito { get; set; } = null!;
    }
}
