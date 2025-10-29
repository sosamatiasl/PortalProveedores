using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Enums;

namespace PortalProveedores.Application.Features.Cotizaciones.Commands
{
    public class AceptarCotizacionesCommandHandler : IRequestHandler<AceptarCotizacionesCommand, Unit>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly INotificationService _notificationService;

        public AceptarCotizacionesCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            INotificationService notificationService)
        {
            _context = context;
            _currentUser = currentUser;
            _notificationService = notificationService;
        }

        public async Task<Unit> Handle(AceptarCotizacionesCommand request, CancellationToken cancellationToken)
        {
            // 1. Seguridad: Verificar que el usuario sea un Cliente y obtener su ID
            var clienteId = _currentUser.ClienteId;
            if (!clienteId.HasValue)
            {
                throw new UnauthorizedAccessException("El usuario no tiene permisos para aceptar cotizaciones.");
            }

            if (request.CotizacionIdsAAceptar == null || !request.CotizacionIdsAAceptar.Any())
            {
                throw new Exception("Debe seleccionar al menos una cotización para aceptar.");
            }

            // 2. Usar una transacción: Si falla al actualizar una, revierte todas.
            await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

            try
            {
                // 3. Obtener la Orden de Compra (y bloquearla para actualización)
                var ordenCompra = await _context.OrdenesCompra
                    .FirstOrDefaultAsync(oc =>
                        oc.Id == request.OrdenCompraId &&
                        oc.ClienteId == clienteId.Value, // Seguridad: solo el dueño
                        cancellationToken);

                if (ordenCompra == null)
                {
                    throw new UnauthorizedAccessException("Orden de Compra no encontrada o no pertenece al cliente.");
                }

                // 4. Obtener las cotizaciones a aceptar
                // (Deben estar en estado 'Enviada' y pertenecer a la OC)
                var cotizacionesAceptar = await _context.Cotizaciones
                    .Where(c =>
                        c.OrdenCompraId == request.OrdenCompraId &&
                        request.CotizacionIdsAAceptar.Contains(c.Id) &&
                        c.Estado == EstadoCotizacion.Enviada)
                    .ToListAsync(cancellationToken);

                if (cotizacionesAceptar.Count != request.CotizacionIdsAAceptar.Count)
                {
                    // Esto significa que algunas IDs enviadas no eran válidas
                    // (ya estaban aceptadas, no existían o eran de otra OC)
                    throw new Exception("Una o más cotizaciones seleccionadas no son válidas para la aceptación.");
                }

                long proveedorId = 0; // Para notificar

                // 5. Actualizar estado (Aceptación Masiva)
                foreach (var cotizacion in cotizacionesAceptar)
                {
                    cotizacion.Estado = EstadoCotizacion.Aceptada;
                    proveedorId = cotizacion.ProveedorId; // Se asume que todas son del mismo proveedor (por la OC)
                }

                // 6. Actualizar el estado de la Orden de Compra
                ordenCompra.Estado = EstadoOrdenCompra.Cotizada;

                // 7. Guardar todos los cambios
                await _context.SaveChangesAsync(cancellationToken);

                // 8. Si todo salió bien, confirmar la transacción
                await transaction.CommitAsync(cancellationToken);

                // 9. Notificar al Proveedor
                if (proveedorId != 0)
                {
                    await _notificationService.NotificarCotizacionesAceptadasAsync(
                        proveedorId,
                        ordenCompra.Id,
                        cotizacionesAceptar.Select(c => c.Id).ToList()
                    );
                }

                return Unit.Value;
            }
            catch (Exception)
            {
                // 10. Si algo falló, revertir todo
                await transaction.RollbackAsync(cancellationToken);
                throw; // Relanzar la excepción para que el Controller la atrape
            }
        }
    }
}
