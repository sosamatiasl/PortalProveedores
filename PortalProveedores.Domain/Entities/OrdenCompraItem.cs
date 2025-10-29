using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Domain.Entities
{
    public class OrdenCompraItem
    {
        public long Id { get; set; }
        public long OrdenCompraId { get; set; }
        public OrdenCompra OrdenCompra { get; set; } = null!;

        public string Sku { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
    }
}
