using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Mobile.Services
{
    public interface IAuthService
    {
        bool IsAuthenticated();
        Task<bool> LoginAsync(string username, string password);
    }

    public class AuthService : IAuthService
    {
        private readonly IApiClient _apiClient;

        public AuthService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public bool IsAuthenticated()
        {
            // Lógica para verificar si SecureStorage tiene un token
            return false; // Simulado
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            // Lógica de llamada al ApiClient
            // var response = await _apiClient.Login(new { username, password });
            // Si es exitoso, guardar token en SecureStorage
            await Task.Delay(500); // Simulación
            return true;
        }
    }
}
