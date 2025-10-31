using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Entities;
using System.Security.Claims;

namespace PortalProveedores.Application.Features.Productos.Commands
{
    public class CrearProductoCommandHandler : IRequestHandler<CrearProductoCommand, long>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public CrearProductoCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<long> Handle(CrearProductoCommand request, CancellationToken cancellationToken)
        {
            // 1. Seguridad: Solo Rol A (Admin Cliente)
            var clienteId = _currentUser.ClienteId;
            if (!clienteId.HasValue || !_currentUser.IsInRole("AdministrativoCliente"))
            {
                throw new UnauthorizedAccessException("Solo el Administrador Cliente puede gestionar el catálogo.");
            }

            // 2. (Opcional) Validar si el SKU ya existe para este cliente
            var skuExistente = await _context.Productos
                .AnyAsync(p => p.ClienteId == clienteId.Value && p.Sku == request.Sku, cancellationToken);

            if (skuExistente)
            {
                throw new Exception($"El SKU '{request.Sku}' ya existe en su catálogo.");
            }

            // 3. Crear entidad
            var producto = new Producto
            {
                ClienteId = clienteId.Value,
                Sku = request.Sku,
                Descripcion = request.Descripcion,
                UnidadMedida = request.UnidadMedida,
                Activo = true
            };

            // 4. Guardar
            await _context.Productos.AddAsync(producto, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return producto.Id;
        }
    }
}
