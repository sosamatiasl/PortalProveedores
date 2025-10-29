using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Domain.Entities
{
    /// <summary>
    /// Entidad para el token seguro del QR, basada en el script SQL.
    /// </summary>
    public class RemitoQRCode
    {
        public long Id { get; set; }
        public long RemitoId { get; set; }
        public Remito Remito { get; set; } = null!;

        // Este es el "puntero" seguro: un token único (ej. GUID)
        public string CodigoHash { get; set; } = string.Empty;
        public DateTime FechaExpiracion { get; set; }
        public bool Usado { get; set; } = false;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
