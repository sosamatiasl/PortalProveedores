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

namespace PortalProveedores.Application.Features.Remitos.Commands
{
    public class CrearRemitoCommandHandler : IRequestHandler<CrearRemitoCommand, long>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorageService;
        private readonly INotificationService _notificationService;

        public CrearRemitoCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IFileStorageService fileStorageService,
            INotificationService notificationService)
        {
            _context = context;
            _currentUser = currentUser;
            _fileStorageService = fileStorageService;
            _notificationService = notificationService;
        }

        public async Task<long> Handle(CrearRemitoCommand request, CancellationToken cancellationToken)
        {
            // 1. Seguridad: Verificar que el usuario sea un Proveedor (Rol B o E)
            var proveedorId = _currentUser.ProveedorId;
            if (!proveedorId.HasValue)
            {
                throw new UnauthorizedAccessException("El usuario no pertenece a un Proveedor.");
            }

            // (Se puede añadir validación de Rol B o E si fuese necesario, pero ser Proveedor es suficiente aquí)

            if (request.CotizacionIds == null || !request.CotizacionIds.Any())
            {
                throw new Exception("Debe asociar al menos una cotización aceptada.");
            }

            // 2. Validar Cotizaciones
            var cotizaciones = await _context.Cotizaciones
                .Include(c => c.OrdenCompra) // Se necesita la OC para sacar el ClienteId
                .Where(c =>
                    request.CotizacionIds.Contains(c.Id) &&
                    c.ProveedorId == proveedorId.Value) // Seguridad: Le pertenecen
                .ToListAsync(cancellationToken);

            // 2a. Asegurarse que todas las cotizaciones sean válidas
            if (cotizaciones.Count != request.CotizacionIds.Count)
            {
                throw new Exception("Una o más cotizaciones no existen o no pertenecen a este proveedor.");
            }

            // 2b. Asegurarse que TODAS estén 'Aceptadas'
            if (cotizaciones.Any(c => c.Estado != EstadoCotizacion.Aceptada))
            {
                throw new Exception("Solo se pueden generar remitos para cotizaciones en estado 'Aceptada'.");
            }

            // 2c. Asegurarse que TODAS pertenezcan a la MISMA Orden de Compra (y mismo Cliente)
            var primerClienteId = cotizaciones.First().OrdenCompra.ClienteId;
            if (cotizaciones.Any(c => c.OrdenCompra.ClienteId != primerClienteId))
            {
                throw new Exception("Todas las cotizaciones deben pertenecer a la misma Orden de Compra / Cliente.");
            }

            // 3. Subir el archivo PDF/Foto
            string pdfUrl;
            if (request.ArchivoPDF != null && request.ArchivoPDF.Length > 0)
            {
                var fileName = $"rem_{proveedorId.Value}_{Guid.NewGuid()}{Path.GetExtension(request.ArchivoPDF.FileName)}";
                await using var stream = request.ArchivoPDF.OpenReadStream();
                pdfUrl = await _fileStorageService.UploadAsync(stream, fileName, "remitos");
            }
            else
            {
                throw new Exception("Es obligatorio adjuntar el archivo PDF o foto del remito.");
            }

            // 4. Crear la entidad Remito
            var remito = new Remito
            {
                ProveedorId = proveedorId.Value,
                ClienteId = primerClienteId,
                NumeroRemito = request.NumeroRemito,
                FechaEmision = DateTime.UtcNow,
                Estado = EstadoRemito.PendienteEnvio, // Aún no tiene QR
                ArchivoPDF_URL = pdfUrl
            };

            // 5. Crear las relaciones N-N
            foreach (var cotizacion in cotizaciones)
            {
                remito.CotizacionRemitos.Add(new CotizacionRemitos
                {
                    CotizacionId = cotizacion.Id
                });
            }

            // 6. Guardar en BD
            await _context.Remitos.AddAsync(remito, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // 7. Notificar al Cliente (Simulación)
            await _notificationService.NotificarRemitoGeneradoAsync(remito);

            return remito.Id;
        }
    }
}
