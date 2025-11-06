using MediatR;
using PortalProveedores.Application.Models; // El IdentityResult personalizado

namespace PortalProveedores.Application.Features.Auth.Commands
{
    // Hereda los campos del DTO de la API y espera un IdentityResult.
    public class RegisterProviderCommand : RegisterProviderRequest, IRequest<IdentityResult>
    {
    }
}
