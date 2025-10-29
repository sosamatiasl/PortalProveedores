using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Domain.Enums
{
    public enum EstadoCotizacion
    {
        Enviada = 0,     // El proveedor la envió
        Aceptada = 1,    // El cliente la aceptó
        Rechazada = 2,   // El cliente la rechazó
        Vencida = 3
    }
}
