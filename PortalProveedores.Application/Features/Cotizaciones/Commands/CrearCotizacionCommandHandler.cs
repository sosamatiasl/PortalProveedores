using MediatR;
using Microsoft.EntityFrameworkCore;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Entities;
using PortalProveedores.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Application.Features.Cotizaciones.Commands
{
    public class CrearCotizacionCommandHandler : IRequestHandler<CrearCotizacionCommand, long>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly INotificationService _notificationService;
        private readonly IFileStorageService _fileStorageService; // Para subir el PDF

        public CrearCotizacionCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            INotificationService notificationService,
            IFileStorageService fileStorageService)
        {
            _context = context;
            _currentUser = currentUser;
            _notificationService = notificationService;
            _fileStorageService = fileStorageService;
        }

        public async Task<long> Handle(CrearCotizacionCommand request, CancellationToken cancellationToken)
        {
            // 1. Seguridad: Verificar que el usuario sea un Proveedor
            var proveedorId = _currentUser.ProveedorId;
            if (!proveedorId.HasValue)
            {
                throw new UnauthorizedAccessException("El usuario no pertenece a un Proveedor.");
            }

            // 2. Validar que la Orden de Compra exista y le pertenezca
            var ordenCompra = await _context.OrdenesCompra
                .FirstOrDefaultAsync(oc => oc.Id == request.OrdenCompraId && oc.ProveedorId == proveedorId.Value, cancellationToken);

            if (ordenCompra == null)
            {
                throw new Exception("Orden de Compra no válida o no pertenece a este proveedor."); // O usar una excepción custom
            }

            // 3. Subir el PDF (si existe)
            string? pdfUrl = null;
            if (request.ArchivoPDF != null && request.ArchivoPDF.Length > 0)
            {
                var fileName = $"cot_{proveedorId.Value}_{Guid.NewGuid()}{Path.GetExtension(request.ArchivoPDF.FileName)}";
                await using var stream = request.ArchivoPDF.OpenReadStream();
                pdfUrl = await _fileStorageService.UploadAsync(stream, fileName, "cotizaciones");
            }

            // 4. Mapeo del DTO a la Entidad
            var cotizacion = new Cotizacion
            {
                OrdenCompraId = request.OrdenCompraId,
                ProveedorId = proveedorId.Value, // ID obtenido del Token
                NumeroCotizacion = request.NumeroCotizacion,
                FechaEmision = DateTime.UtcNow,
                ValidezDias = request.ValidezDias,
                Estado = EstadoCotizacion.Enviada,
                ArchivoPDF_URL = pdfUrl,
                Items = request.Items.Select(i => new CotizacionItem
                {
                    Sku = i.Sku,
                    Descripcion = i.Descripcion,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario
                }).ToList()
            };

            // Se calcula el total basado en los items
            cotizacion.MontoTotal = cotizacion.Items.Sum(i => i.Subtotal);

            // 5. Guardar en BD
            await _context.Cotizaciones.AddAsync(cotizacion, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // 6. Notificar al Cliente (Paso B)
            await _notificationService.NotificarNuevaCotizacionAsync(cotizacion);

            return cotizacion.Id;
        }
    }
}
