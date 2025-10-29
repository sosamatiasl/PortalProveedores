using Microsoft.AspNetCore.SignalR;
using PortalProveedores.Application.Common.Interfaces;
using System.Threading.Tasks;

namespace PortalProveedores.API.Hubs
{
    /// <summary>
    /// Creación del Hub de SignalR.
    /// </summary>
    /// <remarks>
    /// Hub central de SignalR para manejar la comunicación en tiempo real.
    /// Se usa IUserIdProvider para enviar mensajes a usuarios autenticados.
    /// </remarks>
    public class PortalHub : Hub
    {
        private readonly ICurrentUserService _currentUser;

        public PortalHub(ICurrentUserService currentUser)
        {
            _currentUser = currentUser;
        }

        // Método para que el cliente se conecte y se una a grupos si es necesario
        public override Task OnConnectedAsync()
        {
            // En un sistema real, el usuario se une a un grupo basado en su ClienteId/ProveedorId
            // Ejemplo: Clients.GroupAdd(Context.ConnectionId, $"Cliente-{_currentUser.ClienteId}");
            return base.OnConnectedAsync();
        }

        // --- Métodos que la aplicación cliente puede invocar ---

        public async Task SendMessage(string user, string message)
        {
            // Método de ejemplo. En nuestro caso, el backend iniciará la mayoría de las llamadas.
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
