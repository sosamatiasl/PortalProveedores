using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Domain.Enums
{
    public enum EstadoOrdenCompra
    {
        Pendiente = 0, // Recién creada, pendiente de cotizaciones
        Cotizada = 1,  // Al menos una cotización fue aceptada
        Cerrada = 2,   // Proceso finalizado
        Cancelada = 3
    }
}
