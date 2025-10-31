using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Domain.Enums
{
    public enum MotivoNotaAjuste
    {
        DiscrepanciaCantidad = 1, // Diferencia (Cantidad Facturada vs. Cantidad Recibida)
        DiscrepanciaPrecio = 2,    // Diferencia (Precio Facturado vs. Precio OC)
        Devolucion = 3,
        Otro = 99
    }
}
