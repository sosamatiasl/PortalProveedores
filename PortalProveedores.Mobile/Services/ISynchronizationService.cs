using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Mobile.Services
{
    /// <summary>
    /// Interfaz del servicio central.
    /// </summary>
    public interface ISynchronizationService
    {
        // Método que ejecuta la lógica central de sincronización
        Task<int> SynchronizePendingRemitosAsync();
    }
}
