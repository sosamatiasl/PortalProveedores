using System.Net.Http.Headers;
using PortalProveedores.Web.Services;

namespace PortalProveedores.Web.Handlers
{
    public class AuthorizationTokenHandler : DelegatingHandler
    {
        private readonly ITokenStorageService _tokenStorage;

        // Este handler debe recibir el servicio que permite leer el token
        public AuthorizationTokenHandler(ITokenStorageService tokenStorage)
        {
            _tokenStorage = tokenStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1. Omitir el token SOLO para la solicitud de Login/Register (las rutas NO protegidas)
            var requestPath = request.RequestUri?.AbsolutePath;
            // El login es la única ruta que debe pasar sin token
            bool isAuthEndpoint = requestPath != null &&
                          (requestPath.EndsWith("/api/Auth/Login", StringComparison.OrdinalIgnoreCase) ||
                           requestPath.Contains("/api/Auth/Register", StringComparison.OrdinalIgnoreCase));

            if (!isAuthEndpoint)
            {
                // 2. Para rutas protegidas, obtenemos el token.
                // GetToken() está blindado (Paso 2) para no lanzar la excepción de interop.
                string? token = await _tokenStorage.GetToken();

                if (!string.IsNullOrEmpty(token))
                {
                    // 2. Adjuntar el token al encabezado de la solicitud
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            // 3. Continuar con la solicitud HTTP a la API
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
