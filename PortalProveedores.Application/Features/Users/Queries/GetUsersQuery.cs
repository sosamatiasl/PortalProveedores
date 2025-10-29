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
    // --- DTO de Respuesta ---
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string RolActual { get; set; } = string.Empty;
        public long? EntidadId { get; set; } // ClienteId o ProveedorId
        public string EntidadNombre { get; set; } = string.Empty;
    }

    // --- Query ---
    /// <summary>
    /// Query: Obtener la lista de usuarios para la administración.
    /// </summary>
    public class GetUsersQuery : IRequest<List<UserDto>>
    {
        // Criterios de filtrado
        public string? Rol { get; set; }
        public string? EmailFilter { get; set; }
    }
}
