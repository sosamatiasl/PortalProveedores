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

        Task<List<ApplicationUser>> GetUsersInRoleAsync(long roleNumber);
        Task<ApplicationUser?> GetUserByIdAsync(long userId);
        Task<IList<long>> GetUserRolesAsync(long userId);
        //Task<IdentityResult> RemoveUserFromRolesAsync(string userId, string[] roles);
        Task<bool> RemoveUserFromRolesAsync(long userId, long[] roleNumbers);
        Task<bool> AddUserToRoleAsync(long userId, long roleId);
        /// <summary>
        /// Obtiene el ID numérico (long) de un rol a partir de su nombre (string).
        /// </summary>
        Task<long?> GetRoleIdByNameAsync(string roleName);
        /// <summary>
        /// Obtiene los nombres de los roles (string) a partir de una lista de IDs (long).
        /// </summary>
        Task<IEnumerable<string>> GetRoleNamesByIdsAsync(IEnumerable<long> roleIds);


        // Seguridad y gestión de tokens
        Task<(string token, string refreshToken)> LoginAsync(string email, string password, string ipAddress);
        Task<string> GenerateAndStoreRefreshTokenAsync(long userId, string ipAddress);
        Task<ApplicationUser?> ValidateRefreshTokenAsync(string token, string ipAddress);
        Task<bool> RevokeRefreshTokenAsync(string token, string ipAddress);

    }
}