using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace PortalProveedores.API.Hubs
{
    public class HubNotificationService : IHubNotificationService
    {
        // IHubContext se inyecta y puede ver PortalHub
        private readonly IHubContext<PortalHub> _hubContext;

        public HubNotificationService(IHubContext<PortalHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task NotifyAllAsync(string method, object data)
        {
            // Usa el contexto para enviar a todos los clientes
            return _hubContext.Clients.All.SendAsync(method, data);
        }
    }
}
