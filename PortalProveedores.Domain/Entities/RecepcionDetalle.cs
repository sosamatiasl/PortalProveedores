using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Domain.Entities
{
    public class RecepcionDetalle
    {
        public long Id { get; set; }
        public long RecepcionId { get; set; }
        public Recepcion Recepcion { get; set; } = null!;

        public string IdProducto { get; set; } = string.Empty; // SKU
        public string DescripcionProducto { get; set; } = string.Empty;
        public decimal CantidadDeclarada { get; set; } // Lo que decía el remito/cotización
        public decimal CantidadRecibida { get; set; }  // Lo que se contó
    }
}
