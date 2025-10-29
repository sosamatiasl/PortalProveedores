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

        public IdentityService(PortalProveedoresDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Genera y almacena un nuevo Refresh Token para el usuario.
        /// </summary>
        public async Task<string> GenerateAndStoreRefreshTokenAsync(string userId, string ipAddress)
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

            // 2. Generar Access Token
            var accessToken = GenerateJwtToken(user); // Crear este método

            // 3. Generar y almacenar Refresh Token
            var refreshToken = await GenerateAndStoreRefreshTokenAsync(user.Id, ipAddress);

            return (accessToken, refreshToken);
        }
    }
}
