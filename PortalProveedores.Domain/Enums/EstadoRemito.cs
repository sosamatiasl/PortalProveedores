using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Domain.Enums
{
    public enum EstadoRemito
    {
        PendienteEnvio = 0,    // Recién creado, esperando el QR
        EnTransporte = 1,      // QR generado, mercadería en camino
        Recibido = 2,          // Recepción confirmada por el Cliente
        RecibidoConDiferencias = 3,
        Cancelado = 4
    }
}
