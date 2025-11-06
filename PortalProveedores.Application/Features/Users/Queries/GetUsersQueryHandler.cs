using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Enums;
using System.Collections.Generic;
using System.Linq;

namespace PortalProveedores.Application.Features.Users.Queries
{
    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
    {
        private readonly IIdentityService _identityService;
        private readonly ICurrentUserService _currentUser;
        private readonly IApplicationDbContext _context;

        public GetUsersQueryHandler(IIdentityService identityService, ICurrentUserService currentUser, IApplicationDbContext context)
        {
            _identityService = identityService;
            _currentUser = currentUser;
            _context = context;
        }

        public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            // 1. Seguridad: Solo Roles A y B pueden listar usuarios.
            if (!_currentUser.IsInRole("AdministrativoCliente") && !_currentUser.IsInRole("AdministrativoProveedor"))
            {
                throw new UnauthorizedAccessException("Acceso denegado. Solo roles administrativos pueden listar usuarios.");
            }

            // 2. Obtener la lista completa de usuarios con su rol actual
            var allUsers = await _identityService.GetUsersInRoleAsync(request.Rol);

            // 3. Filtrar por la entidad del usuario actual (Cliente o Proveedor)
            var isClienteAdmin = _currentUser.IsInRole("AdministrativoCliente");
            var isProveedorAdmin = _currentUser.IsInRole("AdministrativoProveedor");

            var filteredUsers = new List<UserDto>();

            foreach (var user in allUsers)
            {
                var userRoles = await _identityService.GetUserRolesAsync(user.Id);
                var rol = userRoles.FirstOrDefault(0);

                // Si es Admin Cliente, solo puede ver a usuarios asignados a su Cliente (D, C/E solo si es el mismo cliente)
                if (isClienteAdmin && user.ClienteId == _currentUser.ClienteId)
                {
                    // Solo listar usuarios de roles internos del Cliente (Administrativo (1), Recepcionador (4))
                    if (rol == 1 || rol == 4)
                    {
                        filteredUsers.Add(MapToUserDto(user.Id, user.Email, user.NombreCompleto, rol, user.ClienteId, "Cliente"));
                    }
                }
                // Si es Admin Proveedor, solo puede ver a usuarios asignados a su Proveedor (2, 3, 5)
                else if (isProveedorAdmin && user.ProveedorId == _currentUser.ProveedorId)
                {
                    if (rol == 2 || rol == 3 || rol == 5)
                    {
                        filteredUsers.Add(MapToUserDto(user.Id, user.Email, user.NombreCompleto, rol, user.ProveedorId, "Proveedor"));
                    }
                }

                // Filtrar por Email
                if (request.EmailFilter != null && !user.Email.Contains(request.EmailFilter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }

            return filteredUsers.OrderBy(u => u.Email).ToList();
        }

        private UserDto MapToUserDto(long id, string email, string nombre, long rol, long? entidadId, string tipoEntidad)
        {
            return new UserDto
            {
                Id = id,
                Email = email,
                NombreCompleto = nombre,
                RolActual = rol,
                EntidadId = entidadId,
                EntidadNombre = tipoEntidad
            };
        }
    }
}
