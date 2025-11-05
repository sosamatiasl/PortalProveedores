using Microsoft.AspNetCore.Components.Authorization;
using PortalProveedores.Web.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt; // Paquete System.IdentityModel.Tokens.Jwt
using System.Linq;
using System.Collections.Generic;

namespace PortalProveedores.Web.Auth
{
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly TokenStorageService _tokenStorage;

        public ApiAuthenticationStateProvider(TokenStorageService tokenStorage)
        {
            _tokenStorage = tokenStorage;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = _tokenStorage.Token;

            if (string.IsNullOrWhiteSpace(token))
            {
                // Usuario anónimo
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            }

            // Usuario autenticado (parseamos el token)
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return Task.FromResult(new AuthenticationState(user));
        }

        // Método para notificar a Blazor que el estado cambió (Login/Logout)
        public void NotifyAuthenticationStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            return token.Claims;
        }
    }
}