using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Domain.Enums
{
    public enum TipoNotaAjuste
    {
        NotaCredito = 1, // Se resta valor a la factura (Proveedor facturó de más)
        NotaDebito = 2   // Se añade valor a la factura (Proveedor facturó de menos)
    }
}
