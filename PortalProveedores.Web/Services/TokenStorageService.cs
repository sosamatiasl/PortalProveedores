using Microsoft.JSInterop;

namespace PortalProveedores.Web.Services
{
    /// <summary>
    /// Almacén Scoped (por circuito de usuario) para el Token JWT en Blazor Server.
    /// </summary>
    // Nota: El servicio debe ser 'public' para la inyección de dependencias.
    public class TokenStorageService : ITokenStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string AuthTokenKey = "authToken";
        private const string RefreshTokenKey = "refreshToken";

        public TokenStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task SetToken(string token)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", AuthTokenKey, token);
        }

        public async Task SetRefreshToken(string refreshToken)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", RefreshTokenKey, refreshToken);
        }

        public async Task<string?> GetToken()
        {
            // El 'InvokeAsync' devuelve el valor.
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", AuthTokenKey);
        }

        public async Task<string?> GetRefreshToken()
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", RefreshTokenKey);
        }

        public async Task RemoveTokens()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", AuthTokenKey);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", RefreshTokenKey);
        }
    }
}
