using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortalProveedores.Domain.Enums;

namespace PortalProveedores.Domain.Entities
{
    public class Remito
    {
        public long Id { get; set; }
        public long ProveedorId { get; set; }
        public Proveedor Proveedor { get; set; } = null!;

        public long ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public string NumeroRemito { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public EstadoRemito Estado { get; set; }
        public string? ArchivoPDF_URL { get; set; } // Copia escaneada

        // Relación N-N con Cotizaciones (a través de CotizacionRemitos)
        public ICollection<CotizacionRemitos> CotizacionRemitos { get; set; } = new List<CotizacionRemitos>();

        // Relación 1-1 con la Recepción
        //public Recepcion? Recepcion { get; set; }

        // Relación 1-N con QRCodes (puede tener varios si expiran, pero solo 1 activo)
        public ICollection<RemitoQRCode> QRCodes { get; set; } = new List<RemitoQRCode>();
    }
}
