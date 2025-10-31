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
            // 1. Seguridad: Rol B (Proveedor)
            var proveedorId = _currentUser.ProveedorId;
            if (!proveedorId.HasValue)
            {
                throw new UnauthorizedAccessException("El usuario no pertenece a un Proveedor.");
            }

            // 2. Validar que la OC exista y le pertenezca
            var ordenCompra = await _context.OrdenesCompra
                .Include(oc => oc.Items) // Incluir items de la OC para validar
                .FirstOrDefaultAsync(oc => oc.Id == request.OrdenCompraId && oc.ProveedorId == proveedorId.Value, cancellationToken);

            if (ordenCompra == null)
                throw new Exception("Orden de Compra no válida o no pertenece a este proveedor.");

            // 3. Subir el PDF (sin cambios)
            string? pdfUrl = null;
            if (request.ArchivoPDF != null) { /* ... lógica de subida ... */ }

            var cotizacionItems = new List<CotizacionItem>();
            decimal montoTotalCalculado = 0;

            // 4. Validar Items y OBTENER PRECIOS DEL CATÁLOGO
            foreach (var itemDto in request.Items)
            {
                // 4a. Validar que el producto estaba en la OC original
                var itemOC = ordenCompra.Items.FirstOrDefault(i => i.ProductoId == itemDto.ProductoId);
                if (itemOC == null)
                {
                    throw new Exception($"El Producto ID {itemDto.ProductoId} no fue solicitado en la Orden de Compra.");
                }

                // 4b. OBTENER EL PRECIO del Catálogo
                var precioAcordado = await _context.ProductoPrecios
                    .FirstOrDefaultAsync(pp =>
                        pp.ProductoId == itemDto.ProductoId &&
                        pp.ProveedorId == proveedorId.Value && // Precio asignado a ESTE proveedor
                        pp.FechaVigenciaDesde <= DateTime.UtcNow &&
                        (pp.FechaVigenciaHasta == null || pp.FechaVigenciaHasta >= DateTime.UtcNow),
                        cancellationToken);

                if (precioAcordado == null)
                {
                    throw new Exception($"El Producto SKU {itemOC.Sku} no tiene un precio válido asignado a su proveedor. Contacte al Cliente (Rol A).");
                }

                // 4c. Crear el item (como snapshot)
                cotizacionItems.Add(new CotizacionItem
                {
                    ProductoId = itemOC.ProductoId,
                    Sku = itemOC.Sku,
                    Descripcion = itemOC.Descripcion,
                    Cantidad = itemDto.Cantidad,
                    PrecioUnitario = precioAcordado.PrecioAcordado // <-- PRECIO BLOQUEADO
                });

                // 4d. Sumar al total
                montoTotalCalculado += (itemDto.Cantidad * precioAcordado.PrecioAcordado);
            }

            // 5. Mapeo de la Cotización
            var cotizacion = new Cotizacion
            {
                OrdenCompraId = request.OrdenCompraId,
                ProveedorId = proveedorId.Value,
                NumeroCotizacion = request.NumeroCotizacion,
                FechaEmision = DateTime.UtcNow,
                ValidezDias = request.ValidezDias,
                Estado = EstadoCotizacion.Enviada,
                ArchivoPDF_URL = pdfUrl,
                Items = cotizacionItems,
                MontoTotal = montoTotalCalculado // <-- Total basado en precios del catálogo
            };

            // 6. Guardar y Notificar
            await _context.Cotizaciones.AddAsync(cotizacion, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await _notificationService.NotificarNuevaCotizacionAsync(cotizacion);

            return cotizacion.Id;
        }
    }
}
