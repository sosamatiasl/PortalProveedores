using Microsoft.AspNetCore.Authorization;
using PortalProveedores.Domain.Constants;
using System.Security.Claims;

namespace PortalProveedores.Web.Auth
{
    public class HasNoOperationalRoleRequirement : IAuthorizationRequirement { }

    public class HasNoOperationalRoleHandler : AuthorizationHandler<HasNoOperationalRoleRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasNoOperationalRoleRequirement requirement)
        {
            // 1. Obtener la lista de roles operativos (limpiando la coma)
            var rolesToCheck = DashboardUserRoles.AllOperationalRoles.Split(',');

            // 2. Verificar si el usuario tiene ALGUNO de los roles operativos
            if (context.User.Claims.Any(c => c.Type == ClaimTypes.Role && rolesToCheck.Contains(c.Value)))
            {
                // El usuario tiene un rol operativo -> Fallar la autorización
                context.Fail();
            }
            else
            {
                // El usuario NO tiene ninguno de los roles operativos -> Éxito
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
