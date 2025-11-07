using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;

namespace PortalProveedores.Web.Services
{
    /// <summary>
    /// Almacén Scoped (por circuito de usuario) para el Token JWT en Blazor Server.
    /// </summary>
    // Nota: El servicio debe ser 'public' para la inyección de dependencias.
    public class TokenStorageService : ITokenStorageService
    {
        //private readonly IJSRuntime _jsRuntime;
        private readonly ProtectedLocalStorage _protectedLocalStorage;
        private const string AuthTokenKey = "authToken";
        private const string RefreshTokenKey = "refreshToken";
        private string? _cachedAuthToken;

        public TokenStorageService(/*IJSRuntime jsRuntime*/ProtectedLocalStorage protectedLocalStorage)
        {
            //_jsRuntime = jsRuntime;
            _protectedLocalStorage = protectedLocalStorage;
        }

        public async Task SetToken(string token)
        {
            //await _jsRuntime.InvokeVoidAsync("localStorage.setItem", AuthTokenKey, token);
            await _protectedLocalStorage.SetAsync(AuthTokenKey, token);
            _cachedAuthToken = token;
        }

        public async Task SetRefreshToken(string refreshToken)
        {
            //await _jsRuntime.InvokeVoidAsync("localStorage.setItem", RefreshTokenKey, refreshToken);
            await _protectedLocalStorage.SetAsync(RefreshTokenKey, refreshToken);
        }

        public async Task<string?> GetToken()
        {
            // 1. Priorizar el token en memoria (más rápido y seguro de usar por el Handler)
            if (_cachedAuthToken != null)
            {
                return _cachedAuthToken;
            }

            // 2. Si no está en memoria (ej: primera carga), leer de ProtectedLocalStorage
            var result = await _protectedLocalStorage.GetAsync<string>(AuthTokenKey);

            // 3. Almacenar en memoria si se lee de ProtectedLocalStorage
            if (result.Success && result.Value != null)
            {
                _cachedAuthToken = result.Value;
            }

            return _cachedAuthToken;
        }

        public async Task<string?> GetRefreshToken()
        {
            //return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", RefreshTokenKey);
            var result = await _protectedLocalStorage.GetAsync<string>(RefreshTokenKey);
            return result.Success ? result.Value : null;
        }

        public async Task RemoveTokens()
        {
            //await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", AuthTokenKey);
            //await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", RefreshTokenKey);
            await _protectedLocalStorage.DeleteAsync(AuthTokenKey);
            await _protectedLocalStorage.DeleteAsync(RefreshTokenKey);
        }
    }
}
