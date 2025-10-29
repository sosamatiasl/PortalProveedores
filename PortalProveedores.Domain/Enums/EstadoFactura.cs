using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Domain.Enums
{
    public enum EstadoFactura
    {
        PendienteOCR = 0,           // Recién cargada, esperando extracción de datos
        PendienteAFIP = 1,          // Datos extraídos, esperando validación fiscal
        Aprobada = 2,               // Aprobada fiscal y administrativamente
        ConDiferencias = 3,         // Montos no coinciden con Remito/OC
        RechazadaAFIP = 4,          // CAE inválido
        RechazadaAdministrativa = 5 // Rechazo manual por errores
    }
}
