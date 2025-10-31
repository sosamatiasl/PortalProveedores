using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using PortalProveedores.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace PortalProveedores.Application.Common.Interfaces
{
    public interface IIdentityService
    {
        // ... (Métodos de Login/Registro existentes) ...

        Task<List<ApplicationUser>> GetUsersInRoleAsync(string? roleName);
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<IList<string>> GetUserRolesAsync(string userId);
        //Task<IdentityResult> RemoveUserFromRolesAsync(string userId, string[] roles);
        Task<bool> RemoveUserFromRolesAsync(string userId, string[] roleNames);
        Task<bool> AddUserToRoleAsync(string userId, string roleName);

        // Seguridad y gestión de tokens
        Task<(string token, string refreshToken)> LoginAsync(string email, string password, string ipAddress);
        Task<string> GenerateAndStoreRefreshTokenAsync(string userId, string ipAddress);
        Task<ApplicationUser?> ValidateRefreshTokenAsync(string token, string ipAddress);
        Task<bool> RevokeRefreshTokenAsync(string token, string ipAddress);
    }
}