using MediatR;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Entities;
using PortalProveedores.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // 1. Seguridad: Verificar que el usuario sea un Cliente y obtener su ID
            var clienteId = _currentUser.ClienteId;
            if (!clienteId.HasValue)
            {
                throw new UnauthorizedAccessException("El usuario no pertenece a un Cliente.");
            }

            // 2. Mapeo del DTO a la Entidad
            var ordenCompra = new OrdenCompra
            {
                ClienteId = clienteId.Value, // ID obtenido del Token
                ProveedorId = request.ProveedorId,
                NumeroOrden = request.NumeroOrden,
                Detalles = request.Detalles,
                FechaEmision = DateTime.UtcNow,
                Estado = EstadoOrdenCompra.Pendiente,
                Items = request.Items.Select(i => new OrdenCompraItem
                {
                    Sku = i.Sku,
                    Descripcion = i.Descripcion,
                    Cantidad = i.Cantidad,
                    UnidadMedida = i.UnidadMedida
                }).ToList()
            };

            // 3. Guardar en BD
            await _context.OrdenesCompra.AddAsync(ordenCompra, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // 4. Notificar al Proveedor
            await _notificationService.NotificarNuevaOrdenCompraAsync(ordenCompra);

            return ordenCompra.Id;
        }
    }
}
