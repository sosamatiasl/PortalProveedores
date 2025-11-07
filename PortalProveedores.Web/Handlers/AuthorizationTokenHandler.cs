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
            string? token = null;

            // 1. Obtener el token de la sesión/almacenamiento del usuario logueado
            // ¡Este método debe existir e implementarse para obtener el token guardado!
            //var token = await _tokenStorage.GetToken();
            try
            {
                // Esto intentará leer de la memoria (si existe) o de ProtectedLocalStorage (y fallará en el thread estático)
                token = await _tokenStorage.GetToken();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop calls cannot be issued at this time"))
            {
                // Si falla en el thread estático, asumimos que no hay token.
                // Esto permite que la solicitud de login (que aún no tiene token) pase.
                token = null;
            }

            if (!string.IsNullOrEmpty(token))
            {
                // 2. Adjuntar el token al encabezado de la solicitud
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // 3. Continuar con la solicitud HTTP a la API
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
