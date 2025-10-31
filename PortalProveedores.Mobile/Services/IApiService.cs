using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortalProveedores.Mobile.Models;

namespace PortalProveedores.Mobile.Services
{
    public interface IApiService
    {
        Task<AuthResult> LoginAsync(AuthRequest loginRequest);
        Task LogoutAsync(); // Método para eliminar el token de SecureStorage
        Task<string?> GetAuthTokenAsync(); // Obtiene el token de forma segura

        Task<RemitoParaRecepcionDto> ValidarQrRecepcionAsync(string qrToken);
        Task<long> ConfirmarRecepcionAsync(ConfirmarRecepcionCommand command);
    }
}
