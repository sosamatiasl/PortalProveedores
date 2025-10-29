using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Application.Common.Interfaces
{
    /// <summary>
    /// Servicio crucial para obtener la identidad del usuario que hace la petición.
    /// </summary>
    public interface ICurrentUserService
    {
        string? UserId { get; }
        long? ClienteId { get; }
        long? ProveedorId { get; }
        bool IsInRole(string roleName);
    }
}
