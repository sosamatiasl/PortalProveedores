using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Entities.Identity;
using PortalProveedores.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly PortalProveedoresDbContext _context; // Añadir inyección del DbContext
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtGeneratorService _tokenService;

        public IdentityService(
            PortalProveedoresDbContext context, 
            UserManager<ApplicationUser> userManager,
            IJwtGeneratorService tokenService)
        {
            _context = context;
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<List<ApplicationUser>> GetUsersInRoleAsync(long roleId)
        {
            // Filtra los usuarios que tengan una entrada en la tabla de relaciones de roles
            var users = await _context.Users
                .Where(u => u.UsuarioRoles.Any(ur => ur.RoleId == roleId))
                .ToListAsync();

            if (users.Count == 0)
            {
                return new List<ApplicationUser>();
            }

            // Se deuvelve la lista de ApplicationUser
            return users.ToList();
        }

        /// <summary>
        /// Obtiene un ApplicationUser por su ID.
        /// </summary>
        public async Task<ApplicationUser?> GetUserByIdAsync(long userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            return user; // Devuelve el objeto ApplicationUser o null si no se encuentra.
        }

        /// <summary>
        /// Obtiene los nombres de los roles asignados a un usuario por su ID.
        /// </summary>
        public async Task<IList<long>> GetUserRolesAsync(long userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                // Devuelve una lista vacía si el usuario no existe
                return new List<long>();
            }

            var result = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            return result;
        }

        /// <summary>
        /// Elimina a un usuario de una lista de roles especificados.
        /// </summary>
        public async Task<bool> RemoveUserFromRolesAsync(long userId, long[] roleNumbers)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return false;
            }

            // UserManager.RemoveFromRolesAsync toma el usuario y una IList<string>
            var rolesList = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Rol.Name)
                .ToListAsync();
            if (rolesList == null) rolesList = new List<string>();
            var result = await _userManager.RemoveFromRolesAsync(user, rolesList);

            // Devuelve true si la remoción fue exitosa para todos los roles
            return result.Succeeded;
        }

        public async Task<bool> AddUserToRoleAsync(long userId, long roleId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return false;
            }

            var roleName = await _context.Roles
                .AsNoTracking()
                .Where(r => r.Id == roleId)
                .Select(r => r.Name)
                .FirstOrDefaultAsync();

            var result = await _userManager.AddToRoleAsync(user, roleName);

            // Retorna true si la adición fue exitosa
            return result.Succeeded;
        }

        public async Task<long?> GetRoleIdByNameAsync(string roleName)
        {
            var roleId = await _context.Roles
                                       .AsNoTracking()
                                       .Where(r => r.Name == roleName)
                                       .Select(r => (long?)r.Id)
                                       .FirstOrDefaultAsync();
            return roleId;
        }

        public async Task<IEnumerable<string>> GetRoleNamesByIdsAsync(IEnumerable<long> roleIds)
        {
            var roleNames = await _context.Roles
                                          .AsNoTracking()
                                          .Where(r => roleIds.Contains(r.Id))
                                          .Select(r => r.Name)
                                          .ToListAsync();
            return roleNames;
        }



        // Tokens y Seguridad

        /// <summary>
        /// Genera y almacena un nuevo Refresh Token para el usuario.
        /// </summary>
        public async Task<string> GenerateAndStoreRefreshTokenAsync(long userId, string ipAddress)
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)), // Token seguro
                Expires = DateTime.UtcNow.AddDays(7), // Token válido por 7 días
                Created = DateTime.UtcNow,
                RemoteIpAddress = ipAddress,
                UserId = userId
            };

            // Revocar tokens anteriores para este usuario (solo uno activo a la vez)
            var previousTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .ToListAsync();

            foreach (var token in previousTokens)
            {
                token.Revoked = DateTime.UtcNow;
            }

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken.Token;
        }

        /// <summary>
        /// Valida si el Refresh Token es activo y pertenece al usuario.
        /// </summary>
        public async Task<ApplicationUser?> ValidateRefreshTokenAsync(string token, string ipAddress)
        {
            var refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken == null || !refreshToken.IsActive)
            {
                return null; // Token no existe o ya fue usado/revocado/expirado
            }

            // Revocar el token usado (estrategia de un solo uso)
            refreshToken.Revoked = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return refreshToken.User;
        }

        /// <summary>
        /// Revoca un Refresh Token.
        /// </summary>
        public async Task<bool> RevokeRefreshTokenAsync(string token, string ipAddress)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token && rt.Revoked == null);

            if (refreshToken == null) return false;

            refreshToken.Revoked = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(string token, string refreshToken)> LoginAsync(string email, string password, string ipAddress)
        {
            // 1. Obtener el usuario y validar
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            {
                // ... devolver error ...
            }

            // 2. Obtener roles
            var roles = await _userManager.GetRolesAsync(user);

            // 3. Generar Access Token
            var accessToken = _tokenService.GenerateJwtToken(user, roles); // Crear este método

            // 4. Generar y almacenar Refresh Token
            var refreshToken = await GenerateAndStoreRefreshTokenAsync(user.Id, ipAddress);

            return (accessToken, refreshToken);
        }
    }
}
