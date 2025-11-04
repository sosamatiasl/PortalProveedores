using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Application.Common.Interfaces
{
    /// <summary>
    /// Abstracción para enviar mensajes a través de SignalR Hubs.
    /// </summary>
    public interface IHubNotificationService
    {
        // Método genérico para enviar una notificación a todos los clientes
        Task NotifyAllAsync(string method, object data);

        // Método específico para enviar un mensaje a un usuario (si es necesario)
        // Task NotifyUserAsync(string userId, string method, object data);
    }
}
