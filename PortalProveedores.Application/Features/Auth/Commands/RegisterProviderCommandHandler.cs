using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Application.Models;
using PortalProveedores.Domain.Entities;
using PortalProveedores.Domain.Entities.Identity;
using System.Threading;
using System.Threading.Tasks;
using IdentityFrameworkResult = Microsoft.AspNetCore.Identity.IdentityResult;

namespace PortalProveedores.Application.Features.Auth.Commands
{
    public class RegisterProviderCommandHandler : IRequestHandler<RegisterProviderCommand, PortalProveedores.Application.Models.IdentityResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationDbContext _context;
        private const string ProviderRole = "Proveedor";

        public RegisterProviderCommandHandler(UserManager<ApplicationUser> userManager, IApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<PortalProveedores.Application.Models.IdentityResult> Handle(RegisterProviderCommand request, CancellationToken cancellationToken)
        {
            // 1. Crear el ApplicationUser
            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                EmailConfirmed = true,
                NombreCompleto = request.RazonSocial,
                UsuarioRoles = new List<UsuarioRol>()
            };

            // 2. Crear el usuario en Identity
            IdentityFrameworkResult frameworkResult = await _userManager.CreateAsync(user, request.Password);

            if (!frameworkResult.Succeeded)
            {
                // Mapear errores del framework a su modelo personalizado
                var customErrors = frameworkResult.Errors
                    .Select(e => new PortalProveedores.Application.Models.IdentityError { Code = e.Code, Description = e.Description })
                    .ToArray();
                return PortalProveedores.Application.Models.IdentityResult.Failed(customErrors);
            }

            // 3. Asignar el rol "Proveedor"
            await _userManager.AddToRoleAsync(user, ProviderRole);

            // 4. Crear la entidad Proveedor en la tabla de dominio
            try
            {
                var proveedor = new Proveedor
                {
                    Id = user.Id,
                    RazonSocial = request.RazonSocial,
                    CUIT = request.CUIT, // Asumimos que CUIT es un campo necesario
                    FechaCreacion = DateTime.UtcNow
                };

                await _context.Proveedores.AddAsync(proveedor, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                // 5. Vincular el ProveedorId al ApplicationUser
                user.ProveedorId = proveedor.Id;
                await _userManager.UpdateAsync(user);
            }
            catch (Exception)
            {
                // Si la DB de dominio falla, hacemos un rollback borrando el usuario de Identity
                await _userManager.DeleteAsync(user);
                return PortalProveedores.Application.Models.IdentityResult.Failed(new PortalProveedores.Application.Models.IdentityError
                {
                    Description = "El usuario se creó pero falló la creación de la entidad de proveedor asociada."
                });
            }

            return PortalProveedores.Application.Models.IdentityResult.Success;
        }
    }
}
