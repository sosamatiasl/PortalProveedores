using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Domain.Enums
{
    public enum TipoRolUsuario
    {
        None = 0,
        AdministrativoCliente = 1,
        AdministrativoProveedor = 2,
        Transportista = 3,
        RecepcionadorCliente = 4,
        DespachanteProveedor = 5
    }
}
