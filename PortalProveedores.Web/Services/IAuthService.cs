using Microsoft.AspNetCore.Components.Authorization;
using PortalProveedores.Application.Models;
using PortalProveedores.Web.Auth;
using System.Net.Http;

namespace PortalProveedores.Web.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> Login(LoginRequest loginRequest);
        Task Logout();
        Task RegisterClient(RegisterClientRequest request);
        Task RegisterProvider(RegisterProviderRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ITokenStorageService _tokenStorage;

        public AuthService(
            IHttpClientFactory httpClientFactory,
            AuthenticationStateProvider authenticationStateProvider,
            ITokenStorageService tokenStorage)
        {
            _httpClient = httpClientFactory.CreateClient("ApiGateway");
            _authenticationStateProvider = authenticationStateProvider;
            _tokenStorage = tokenStorage;
        }

        public async Task<AuthResponse> Login(LoginRequest loginRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/Login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                // 2. Deserializar la respuesta exitosa (AuthResponse)
                var authResult = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResult == null)
                {
                    throw new Exception("La respuesta del servidor fue nula.");
                }

                // 3. Persistir los tokens en el navegador (Local Storage)
                await _tokenStorage.SetToken(authResult.Token);
                await _tokenStorage.SetRefreshToken(authResult.RefreshToken);

                // Opcional: Persistir el nombre de usuario
                //await _tokenStorage.SetToken(authResult.Username);

                // 4. Notificar a Blazor sobre el cambio de estado (Autenticado)
                // Esto desencadena que la aplicación Blazor sepa que el usuario ha iniciado sesión.
                ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(authResult.Token);

                await Task.Delay(100);

                return authResult;
            }
            else
            {
                // 5. Manejar errores de la API (ej: 401 Unauthorized)
                string errorContent = await response.Content.ReadAsStringAsync();

                // Lanzar una excepción que el componente pueda atrapar (ej. Login.razor)
                throw new HttpRequestException($"Fallo el inicio de sesión: {response.ReasonPhrase}. Detalles: {errorContent}");
            }
        }

        public async Task Logout()
        {
            // 1. Eliminar los tokens de Local Storage
            await _tokenStorage.RemoveTokens();

            // 2. Notificar a Blazor sobre el cambio de estado (Desautenticado)
            ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
        }

        public async Task RegisterClient(RegisterClientRequest request)
        {
            // Nota: El BaseAddress del HttpClient ya debe apuntar a la API (ej: https://localhost:7061/)
            var response = await _httpClient.PostAsJsonAsync("api/Auth/RegisterClient", request);

            if (!response.IsSuccessStatusCode)
            {
                // Leer el mensaje de error de la API (ej. validaciones fallidas)
                var errorContent = await response.Content.ReadAsStringAsync();

                // Lanzar una excepción para que el componente RegisterClient.razor la capture
                throw new HttpRequestException($"Fallo el registro del cliente. Código: {response.StatusCode}. Detalles: {errorContent}");
            }

            // Si tiene éxito (código 201), el método termina y el componente puede redirigir.
        }

        public async Task RegisterProvider(RegisterProviderRequest request)
        {
            // 1. Envía la solicitud POST al endpoint de la API
            var response = await _httpClient.PostAsJsonAsync($"api/Auth/register-provider", request);

            // 2. Maneja la respuesta
            if (!response.IsSuccessStatusCode)
            {
                // Si el código de estado no es de éxito, leer el mensaje de error de la API
                string errorContent = await response.Content.ReadAsStringAsync();

                // Mejorar el manejo de errores lanzando una excepción con el contenido de la API
                // Esto permite al componente RegisterProvider.razor mostrar un mensaje útil.
                throw new HttpRequestException($"El registro falló: {errorContent}");
            }
        }
    }
}
