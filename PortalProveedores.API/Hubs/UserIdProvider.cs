using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace PortalProveedores.API.Hubs
{
    /// <summary>
    /// Permite que SignalR sepa qué ConnectionId corresponde a qué UserId
    /// basado en el Claim principal (el ID de usuario de ASP.NET Identity).
    /// </summary>
    public class UserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            // Se usa el ClaimTypes.NameIdentifier, que es donde ASP.NET Identity
            // almacena el ApplicationUser.Id
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
