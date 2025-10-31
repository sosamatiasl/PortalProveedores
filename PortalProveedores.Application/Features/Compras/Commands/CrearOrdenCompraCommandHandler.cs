using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Entities;
using PortalProveedores.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace PortalProveedores.Application.Features.Compras.Commands
{
    public class CrearOrdenCompraCommandHandler : IRequestHandler<CrearOrdenCompraCommand, long>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly INotificationService _notificationService;

        public CrearOrdenCompraCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            INotificationService notificationService)
        {
            _context = context;
            _currentUser = currentUser;
            _notificationService = notificationService;
        }

        public async Task<long> Handle(CrearOrdenCompraCommand request, CancellationToken cancellationToken)
        {
            // 1. Seguridad: Rol A (Cliente)
            var clienteId = _currentUser.ClienteId;
            if (!clienteId.HasValue || !_currentUser.IsInRole("AdministrativoCliente"))
            {
                throw new UnauthorizedAccessException("El usuario no pertenece a un Cliente.");
            }

            var ordenCompraItems = new List<OrdenCompraItem>();

            // 2. Validar Productos y crear Items
            foreach (var itemDto in request.Items)
            {
                // Validar que el producto exista EN EL CATÁLOGO del cliente
                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p =>
                        p.Id == itemDto.ProductoId &&
                        p.ClienteId == clienteId.Value &&
                        p.Activo,
                        cancellationToken);

                if (producto == null)
                {
                    throw new Exception($"El Producto con ID {itemDto.ProductoId} no existe, está inactivo o no pertenece a su catálogo.");
                }

                // Crear el item (como snapshot)
                ordenCompraItems.Add(new OrdenCompraItem
                {
                    ProductoId = producto.Id,
                    Sku = producto.Sku, // Copia del catálogo
                    Descripcion = producto.Descripcion, // Copia del catálogo
                    Cantidad = itemDto.Cantidad,
                    UnidadMedida = producto.UnidadMedida // Copia del catálogo
                });
            }

            // 3. Mapeo de la OC
            var ordenCompra = new OrdenCompra
            {
                ClienteId = clienteId.Value,
                ProveedorId = request.ProveedorId,
                NumeroOrden = request.NumeroOrden,
                Detalles = request.Detalles,
                FechaEmision = DateTime.UtcNow,
                Estado = EstadoOrdenCompra.Pendiente,
                Items = ordenCompraItems // Asignar la lista validada
            };

            // 4. Guardar y Notificar
            await _context.OrdenesCompra.AddAsync(ordenCompra, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await _notificationService.NotificarNuevaOrdenCompraAsync(ordenCompra);

            return ordenCompra.Id;
        }
    }
}
