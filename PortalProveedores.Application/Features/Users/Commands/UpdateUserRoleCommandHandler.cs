using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Enums;
using System.Linq;

namespace PortalProveedores.Application.Features.Users.Commands
{
    public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, bool>
    {
        private readonly IIdentityService _identityService;
        private readonly ICurrentUserService _currentUser;

        // Lista de roles permitidos para ser asignados por un administrador (A o B)
        private static readonly List<string> PermittedRoles = new List<string>
    {
        "AdministrativoCliente", "RecepcionadorMercaderia", // Roles del Cliente
        "AdministrativoProveedor", "Vendedor", "DespachanteMercaderia" // Roles del Proveedor
    };

        public UpdateUserRoleCommandHandler(IIdentityService identityService, ICurrentUserService currentUser)
        {
            _identityService = identityService;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
        {
            // 1. Validar el Rol Solicitado
            if (!PermittedRoles.Contains(request.NewRole))
            {
                throw new Exception($"El rol '{request.NewRole}' no es un rol asignable.");
            }

            // 2. Seguridad del Administrador (solo A o B)
            if (!_currentUser.IsInRole("AdministrativoCliente") && !_currentUser.IsInRole("AdministrativoProveedor"))
            {
                throw new UnauthorizedAccessException("Acceso denegado. Solo roles administrativos pueden modificar roles.");
            }

            // 3. Obtener el usuario a modificar
            var targetUser = await _identityService.GetUserByIdAsync(request.UserId);
            if (targetUser == null)
            {
                throw new Exception("Usuario a modificar no encontrado.");
            }

            // 4. Seguridad de la Entidad (El Admin solo puede modificar usuarios de su propia entidad)
            var isClienteAdmin = _currentUser.IsInRole("AdministrativoCliente");
            var isProveedorAdmin = _currentUser.IsInRole("AdministrativoProveedor");

            if (isClienteAdmin && targetUser.ClienteId != _currentUser.ClienteId)
            {
                throw new UnauthorizedAccessException("No tiene permiso para modificar usuarios de otros Clientes.");
            }

            if (isProveedorAdmin && targetUser.ProveedorId != _currentUser.ProveedorId)
            {
                throw new UnauthorizedAccessException("No tiene permiso para modificar usuarios de otros Proveedores.");
            }

            // 5. Aplicar la restricción del ámbito del Rol
            if (isClienteAdmin && (request.NewRole.Contains("Proveedor") || request.NewRole.Contains("Vendedor") || request.NewRole.Contains("Despachante")))
            {
                throw new UnauthorizedAccessException("El Administrador Cliente solo puede asignar roles de Cliente (A, D).");
            }

            if (isProveedorAdmin && (request.NewRole.Contains("Cliente") || request.NewRole.Contains("Recepcionador")))
            {
                throw new UnauthorizedAccessException("El Administrador Proveedor solo puede asignar roles de Proveedor (B, C, E).");
            }

            // 6. Obtener roles actuales y remover
            var currentRoles = await _identityService.GetUserRolesAsync(request.UserId);
            var removeResult = await _identityService.RemoveUserFromRolesAsync(request.UserId, currentRoles.ToArray());

            if (!removeResult.Succeeded)
            {
                throw new Exception("Error al remover roles anteriores. " + string.Join("; ", removeResult.Errors));
            }

            // 7. Añadir el nuevo rol
            var addResult = await _identityService.AddUserToRoleAsync(request.UserId, request.NewRole);

            if (addResult == true)
            {
                // Si es exitoso, remover el rol anterior (si aplica)
                if (currentRoles.ToArray().Length > 0)
                {
                    removeResult = await _identityService.RemoveUserFromRolesAsync(request.UserId, currentRoles.ToArray());

                    if (!removeResult.Succeeded)
                    {
                        // Manejar error de remoción (aunque la adición fue exitosa)
                        throw new Exception("El rol fue añadido, pero el rol anterior no pudo ser removido.");
                    }
                }
                return true;
            }
            else
            {
                // Manejar el fallo de la adición
                throw new Exception("Fallo al añadir el usuario al rol especificado.");
            }
        }
    }
}
