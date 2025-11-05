using Microsoft.AspNetCore.Components.Authorization;
using PortalProveedores.Application.Models;
using PortalProveedores.Web.Auth;

namespace PortalProveedores.Web.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> Login(LoginRequest loginRequest);
        Task Logout();
        Task RegisterClient(RegisterClientRequest request);
        Task RegisterProvider(RegisterProviderRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly TokenStorageService _tokenStorage;
        private readonly string _apiBaseUrl = "https://localhost:7061"; // URL de su API

        public AuthService(IHttpClientFactory httpClientFactory, AuthenticationStateProvider authenticationStateProvider, TokenStorageService tokenStorage)
        {
            _httpClientFactory = httpClientFactory;
            _authenticationStateProvider = authenticationStateProvider;
            _tokenStorage = tokenStorage;
        }

        public async Task<LoginResponse> Login(LoginRequest loginRequest)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/Authentication/Login", loginRequest);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error al iniciar sesión.");
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

            // 1. Guardar el token en el servicio Scoped
            _tokenStorage.Token = loginResponse.Token;

            // 2. Notificar a Blazor que el estado cambió
            ((ApiAuthenticationStateProvider)_authenticationStateProvider).NotifyAuthenticationStateChanged();

            return loginResponse;
        }

        public async Task Logout()
        {
            // 1. Limpiar el token
            _tokenStorage.Token = null;

            // 2. Notificar a Blazor
            ((ApiAuthenticationStateProvider)_authenticationStateProvider).NotifyAuthenticationStateChanged();
        }

        public async Task RegisterClient(RegisterClientRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/Auth/RegisterClient", request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error al registrar cliente.");
            }
        }

        public async Task RegisterProvider(RegisterProviderRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/Authentication/RegisterProvider", request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error al registrar proveedor.");
            }
        }
    }
}
