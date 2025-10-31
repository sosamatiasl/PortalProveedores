using MediatR;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Entities;
using PortalProveedores.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Application.Features.Ajustes.Commands
{
    public class GenerarNotaAjusteCommandHandler : IRequestHandler<GenerarNotaAjusteCommand, long>
    {
        private readonly IConciliacionService _conciliacionService;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        // (Opcional: inyectar INotificationService para avisar al Proveedor)

        public GenerarNotaAjusteCommandHandler(
            IConciliacionService conciliacionService,
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _conciliacionService = conciliacionService;
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<long> Handle(GenerarNotaAjusteCommand request, CancellationToken cancellationToken)
        {
            // 1. Seguridad: Rol A (Admin Cliente) y obtener IDs
            var clienteId = _currentUser.ClienteId;
            var usuarioId = _currentUser.UserId;

            if (!clienteId.HasValue || usuarioId == null || !_currentUser.IsInRole("AdministrativoCliente"))
            {
                throw new UnauthorizedAccessException("Solo el Administrador Cliente puede generar notas de ajuste.");
            }

            // 2. Calcular la conciliación (usando el servicio)
            var reporte = await _conciliacionService.CalcularConciliacionAsync(request.FacturaId, clienteId.Value);

            // 3. Validar si realmente hay un ajuste que hacer
            if (reporte.TotalAjusteNeto == 0)
            {
                throw new Exception("La conciliación no arrojó discrepancias. No se requiere ajuste.");
            }

            // 4. Determinar Tipo de Nota y Motivo
            TipoNotaAjuste tipoNota;
            decimal montoAjusteAbsoluto;

            if (reporte.TotalAjusteNeto > 0)
            {
                // Proveedor facturó de MÁS (Cantidad o Precio)
                tipoNota = TipoNotaAjuste.NotaCredito;
                montoAjusteAbsoluto = reporte.TotalAjusteNeto;
            }
            else
            {
                // Proveedor facturó de MENOS
                tipoNota = TipoNotaAjuste.NotaDebito;
                montoAjusteAbsoluto = Math.Abs(reporte.TotalAjusteNeto);
            }

            // (Simplificación del motivo)
            var motivo = (reporte.TotalAjusteCantidad != 0) ? MotivoNotaAjuste.DiscrepanciaCantidad : MotivoNotaAjuste.DiscrepanciaPrecio;

            // 5. Construir el detalle del ajuste
            var detalleBuilder = new StringBuilder();
            detalleBuilder.AppendLine($"Ajuste generado por Conciliación. Motivo manual: {request.MotivoDetallado}");
            detalleBuilder.AppendLine($"Total Ajuste Neto: {reporte.TotalAjusteNeto:C2}");
            detalleBuilder.AppendLine($"Detalle Cantidad: {reporte.TotalAjusteCantidad:C2}. Detalle Precio: {reporte.TotalAjustePrecio:C2}.");

            // 6. Crear la entidad NotaDebitoCredito
            var notaAjuste = new NotaDebitoCredito
            {
                FacturaId = reporte.FacturaId,
                ClienteId = reporte.ClienteId,
                ProveedorId = reporte.ProveedorId,
                Tipo = tipoNota,
                Motivo = motivo,
                MontoAjuste = montoAjusteAbsoluto,
                Detalle = detalleBuilder.ToString(),
                FechaCreacion = DateTime.UtcNow,
                UsuarioCreadorId = usuarioId
            };

            // 7. Guardar en BD
            await _context.NotasDebitoCredito.AddAsync(notaAjuste, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // 8. (Opcional) Notificar al Proveedor (Rol B)
            // await _notificationService.NotificarNotaAjusteGenerada(notaAjuste);

            return notaAjuste.Id;
        }
    }
}
