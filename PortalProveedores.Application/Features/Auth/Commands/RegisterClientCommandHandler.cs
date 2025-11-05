using MediatR;
using Microsoft.AspNetCore.Identity;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Entities;
using PortalProveedores.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Application.Features.Auth.Commands
{
    public class RegisterClientCommandHandler : IRequestHandler<RegisterClientCommand, PortalProveedores.Application.Models.IdentityResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationDbContext _context;

        // Rol a asignar al nuevo cliente
        private const string ClientRole = "Cliente";

        public RegisterClientCommandHandler(
            UserManager<ApplicationUser> userManager,
            IApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IdentityResult> Handle(RegisterClientCommand request, CancellationToken cancellationToken)
        {
            // 1. Crear el objeto ApplicationUser
            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                EmailConfirmed = true, // Se puede cambiar si requiere confirmación por email
                NombreCompleto = request.CompanyName,
                // Asignar el rol por defecto de Cliente para la autorización
                UsuarioRoles = new List<UsuarioRol>() { }
            };

            // 2. Intentar crear el usuario en la tabla Identity
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                // Devolver el error de Identity si la creación falló (ej. password muy débil, email ya existe)
                return result;
            }

            // 3. Asignar el rol "Cliente"
            await _userManager.AddToRoleAsync(user, ClientRole);

            // 4. Crear la entidad Cliente en la tabla de dominio
            try
            {
                var cliente = new Cliente
                {
                    // Asumimos que Cliente tiene una referencia al ApplicationUser
                    Id = Int64.Parse(user.Id),
                    RazonSocial = request.CompanyName,
                    FechaCreacion = DateTime.UtcNow
                };

                await _context.Clientes.AddAsync(cliente, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                // 5. Vincular el ClienteId al ApplicationUser (si es necesario para claims futuros)
                user.ClienteId = cliente.Id;
                await _userManager.UpdateAsync(user);
            }
            catch (Exception)
            {
                // Si la creación del cliente en la tabla de dominio falla,
                // por consistencia, deberíamos intentar eliminar el ApplicationUser creado.
                await _userManager.DeleteAsync(user);

                // Devolver un error genérico (o loggearlo para evitar exponer detalles sensibles)
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "El usuario se creó pero falló la creación de la entidad de cliente asociada."
                });
            }

            return IdentityResult.Success;
        }
    }
}
