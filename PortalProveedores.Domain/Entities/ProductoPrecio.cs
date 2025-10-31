using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Domain.Entities
{
    /// <summary>
    /// Define el precio acordado para un Producto específico, 
    /// para un Proveedor específico.
    /// </summary>
    public class ProductoPrecio
    {
        public long Id { get; set; }

        public long ProductoId { get; set; }
        public Producto Producto { get; set; } = null!;

        public long ProveedorId { get; set; }
        public Proveedor Proveedor { get; set; } = null!;

        public decimal PrecioAcordado { get; set; }
        public DateTime FechaVigenciaDesde { get; set; }
        public DateTime? FechaVigenciaHasta { get; set; } // Null = precio indefinido

        public DateTime FechaUltimaModificacion { get; set; }
    }
}
