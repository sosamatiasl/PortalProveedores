using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortalProveedores.Domain.Entities.Identity;

namespace PortalProveedores.Application.Common.Interfaces
{
    public interface IJwtGeneratorService
    {
        Task<string> CreateTokenAsync(ApplicationUser user);
    }
}
