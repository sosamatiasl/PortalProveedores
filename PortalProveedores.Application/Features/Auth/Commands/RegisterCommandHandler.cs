﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Entities.Identity;
using System.Transactions;

namespace PortalProveedores.Application.Features.Auth.Commands
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileStorageService _fileStorageService;
        private readonly IJwtGeneratorService _jwtGeneratorService;

        public RegisterCommandHandler(
            UserManager<ApplicationUser> userManager,
            IFileStorageService fileStorageService,
            IJwtGeneratorService jwtGeneratorService)
        {
            _userManager = userManager;
            _fileStorageService = fileStorageService;
            _jwtGeneratorService = jwtGeneratorService;
        }

        public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // 1. Validar que el usuario no exista
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new AuthResult(false, "", "", Enumerable.Empty<string>(), "El email ya está registrado.");
            }

            // 2. Validar que se haya enviado un ID de Cliente o Proveedor, pero no ambos
            if (!request.ProveedorId.HasValue && !request.ClienteId.HasValue)
            {
                return new AuthResult(false, "", "", Enumerable.Empty<string>(), "Se debe asignar un Cliente o Proveedor al usuario.");
            }
            if (request.ProveedorId.HasValue && request.ClienteId.HasValue)
            {
                return new AuthResult(false, "", "", Enumerable.Empty<string>(), "El usuario no puede ser Cliente y Proveedor a la vez.");
            }

            string selfieUrl = string.Empty;

            // 3. Subir la foto selfie
            if (request.SelfieFoto != null && request.SelfieFoto.Length > 0)
            {
                try
                {
                    // Generar un nombre de archivo único
                    var fileName = $"user_{Guid.NewGuid()}{Path.GetExtension(request.SelfieFoto.FileName)}";

                    await using var stream = request.SelfieFoto.OpenReadStream();

                    // Usamos "selfies" como nombre del contenedor en Blob Storage
                    selfieUrl = await _fileStorageService.UploadAsync(stream, fileName, "selfies");
                }
                catch (Exception ex)
                {
                    // Manejar error de subida (ej. loggear)
                    return new AuthResult(false, "", "", Enumerable.Empty<string>(), $"Error al subir la selfie: {ex.Message}");
                }
            }
            else
            {
                return new AuthResult(false, "", "", Enumerable.Empty<string>(), "La foto selfie es obligatoria.");
            }

            // 4. Crear el objeto ApplicationUser
            var newUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(), // Generamos el ID como string
                UserName = request.Email,
                Email = request.Email,
                NombreCompleto = request.NombreCompleto,
                SelfieFotoURL = selfieUrl,
                ProveedorId = request.ProveedorId,
                ClienteId = request.ClienteId,
                FechaRegistro = DateTime.UtcNow,
                EmailConfirmed = true, // Idealmente, deberíamos enviar un email de confirmación
                Activo = true
            };

            // 5. Crear el usuario en la BD
            // Usamos una transacción para asegurar que si falla el rol, se revierte el usuario
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var identityResult = await _userManager.CreateAsync(newUser, request.Password);

            if (!identityResult.Succeeded)
            {
                var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                return new AuthResult(false, "", "", Enumerable.Empty<string>(), $"Error al crear usuario: {errors}");
            }

            // 6. Asignar roles
            if (request.Roles.Any())
            {
                var roleResult = await _userManager.AddToRolesAsync(newUser, request.Roles);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    return new AuthResult(false, "", "", Enumerable.Empty<string>(), $"Error al asignar roles: {errors}");
                }
            }

            scope.Complete();

            // 7. Generar Token JWT
            var token = await _jwtGeneratorService.CreateTokenAsync(newUser);

            return new AuthResult(true, token, newUser.Id, request.Roles, null);
        }
    }
}
