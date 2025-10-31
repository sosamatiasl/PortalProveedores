using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace PortalProveedores.Application.Features.Productos.Commands
{
    public class AsignarPrecioProveedorCommandHandler : IRequestHandler<AsignarPrecioProveedorCommand, long>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public AsignarPrecioProveedorCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<long> Handle(AsignarPrecioProveedorCommand request, CancellationToken cancellationToken)
        {
            // 1. Seguridad: Solo Rol A (Admin Cliente)
            var clienteId = _currentUser.ClienteId;
            if (!clienteId.HasValue || !_currentUser.IsInRole("AdministrativoCliente"))
            {
                throw new UnauthorizedAccessException("Solo el Administrador Cliente puede gestionar los precios.");
            }

            // 2. Validar que el Producto pertenezca al Cliente
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == request.ProductoId && p.ClienteId == clienteId.Value, cancellationToken);

            if (producto == null)
            {
                throw new Exception("Producto no encontrado o no pertenece a su catálogo.");
            }

            // 3. (Opcional) Invalidar precios anteriores para este Producto/Proveedor
            // ... (Lógica para setear FechaVigenciaHasta a los registros antiguos) ...

            // 4. Crear entidad de precio
            var productoPrecio = new ProductoPrecio
            {
                ProductoId = request.ProductoId,
                ProveedorId = request.ProveedorId,
                PrecioAcordado = request.PrecioAcordado,
                FechaVigenciaDesde = request.FechaVigenciaDesde,
                FechaVigenciaHasta = request.FechaVigenciaHasta,
                FechaUltimaModificacion = DateTime.UtcNow
            };

            // 5. Guardar
            await _context.ProductoPrecios.AddAsync(productoPrecio, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return productoPrecio.Id;
        }
    }
}
