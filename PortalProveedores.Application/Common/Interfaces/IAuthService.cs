using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortalProveedores.Application.Models;
using System.Threading.Tasks;
using PortalProveedores.Domain.Entities.Identity;

namespace PortalProveedores.Application.Common.Interfaces
{
    public interface IAuthService
    {
        // Método para registrar un nuevo usuario (para pruebas iniciales)
        Task<ApplicationUser> Register(string username, string password);

        // Método principal para iniciar sesión
        Task<LoginResponse> Login(LoginRequest request);
    }
}
