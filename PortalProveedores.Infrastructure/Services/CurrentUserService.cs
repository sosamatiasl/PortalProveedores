using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PortalProveedores.Application.Common.Interfaces;
using System.Security.Claims;

namespace PortalProveedores.Infrastructure.Services
{
    /// <summary>
    /// Implementación para leer los claims del JWT.
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public long? UserId
        {
            get
            {
                var claimValue = User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return long.TryParse(claimValue, out var id) ? id : null;
            }
        }

        // Se leen los claims personalizados que fueron definidos en JwtGeneratorService
        public long? ClienteId
        {
            get
            {
                var claimValue = User?.FindFirstValue("clienteId");
                return long.TryParse(claimValue, out var id) ? id : null;
            }
        }

        public long? ProveedorId
        {
            get
            {
                var claimValue = User?.FindFirstValue("proveedorId");
                return long.TryParse(claimValue, out var id) ? id : null;
            }
        }

        public bool IsInRole(string roleName)
        {
            return User?.IsInRole(roleName) ?? false;
        }
    }
}
