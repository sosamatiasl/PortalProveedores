using Microsoft.AspNetCore.Components.Authorization;
using PortalProveedores.Web.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PortalProveedores.Web.Auth
{
    // Heredamos de AuthenticationStateProvider para que Blazor sepa que es el proveedor oficial
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ITokenStorageService _tokenStorageService;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

        // Define un estado de usuario ANÓNIMO para usar cuando el usuario no está logueado
        private readonly AuthenticationState _anonymous;

        public ApiAuthenticationStateProvider(
            ITokenStorageService tokenStorageService)
        {
            _tokenStorageService = tokenStorageService;
            _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            _jwtSecurityTokenHandler.InboundClaimTypeMap.Clear();

            // Crea un objeto ClaimsPrincipal vacío (no autenticado)
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        // Este es el método que Blazor llama al inicio para obtener el estado actual
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string? token = null;
            try
            {
                // Solo intentar llamar a JS Interop si la aplicación es interactiva
                // o si sabemos que no estamos en el estático inicial (a menudo, try/catch es suficiente)
                token = await _tokenStorageService.GetToken();
            }
            catch (InvalidOperationException)
            {
                // Esto captura específicamente el error de prerenderizado.
                // Devolvemos un estado anónimo para que el componente se renderice.
                return _anonymous;
            }
            catch (Exception)
            {
                // Manejar otras excepciones de token/storage si es necesario.
                return _anonymous;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                // Si no hay token, devuelve el estado ANÓNIMO
                return _anonymous;
            }

            // 2. Si el token existe, crea el ClaimsPrincipal a partir de él
            var principal = GetClaimsPrincipalFromJwt(token);

            if (!principal.Identity.IsAuthenticated)
            {
                await _tokenStorageService.RemoveTokens();
                return _anonymous;
            }

            // 3. Devuelve el estado con el usuario autenticado
            return new AuthenticationState(principal);
        }

        // --- MÉTODOS PÚBLICOS USADOS POR AuthService.cs ---

        /// <summary>
        /// Marca al usuario como autenticado y notifica a Blazor del cambio de estado.
        /// </summary>
        public void MarkUserAsAuthenticated(string token)
        {
            var authenticatedUser = GetClaimsPrincipalFromJwt(token);

            // Notifica a Blazor que el estado de autenticación ha cambiado.
            // Esto fuerza a los componentes AuthorizeView a re-renderizarse.
            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(authenticatedUser))
            );
        }

        /// <summary>
        /// Marca al usuario como desautenticado y notifica a Blazor del cambio de estado.
        /// </summary>
        public void MarkUserAsLoggedOut()
        {
            // Notifica a Blazor que el estado de autenticación ha cambiado a ANÓNIMO.
            NotifyAuthenticationStateChanged(
                Task.FromResult(_anonymous)
            );
        }

        // --- LÓGICA PRIVADA ---

        /// <summary>
        /// Lee el JWT, lo decodifica y devuelve un ClaimsPrincipal.
        /// </summary>
        private ClaimsPrincipal GetClaimsPrincipalFromJwt(string jwtToken)
        {
            try
            {
                // Decodifica el token sin validarlo (ya se validó en la API).
                var token = _jwtSecurityTokenHandler.ReadJwtToken(jwtToken);

                // Crea una identidad a partir de los claims del token
                var identity = new ClaimsIdentity(
                    token.Claims,
                    "jwtAuthType",
                    "nameid",
                    "role"
                );

                return new ClaimsPrincipal(identity);
            }
            catch
            {
                // Si la decodificación falla (token inválido o expirado), devuelve un anónimo.
                return _anonymous.User;
            }
        }
    }
}