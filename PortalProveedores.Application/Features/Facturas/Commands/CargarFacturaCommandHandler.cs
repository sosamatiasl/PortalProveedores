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

namespace PortalProveedores.Application.Features.Facturas.Commands
{
    public class CargarFacturaCommandHandler : IRequestHandler<CargarFacturaCommand, long>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorageService;
        private readonly IOCRService _ocrService; // Simulación IA/OCR
        private readonly IAFIPService _afipService; // Simulación AFIP

        public CargarFacturaCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IFileStorageService fileStorageService,
            IOCRService ocrService,
            IAFIPService afipService)
        {
            _context = context;
            _currentUser = currentUser;
            _fileStorageService = fileStorageService;
            _ocrService = ocrService;
            _afipService = afipService;
        }

        public async Task<long> Handle(CargarFacturaCommand request, CancellationToken cancellationToken)
        {
            // 1. Seguridad: Verificar que el usuario sea un Proveedor (Rol B)
            var proveedorId = _currentUser.ProveedorId;
            if (!proveedorId.HasValue || !_currentUser.IsInRole("AdministrativoProveedor"))
            {
                throw new UnauthorizedAccessException("El usuario no tiene permisos para cargar facturas.");
            }

            // 2. Validación de Remitos
            if (request.RemitoIds == null || !request.RemitoIds.Any())
            {
                throw new Exception("Debe asociar la factura a al menos un remito ya recepcionado.");
            }

            var remitos = await _context.Remitos
                .Where(r =>
                    request.RemitoIds.Contains(r.Id) &&
                    r.ProveedorId == proveedorId.Value &&
                    (r.Estado == EstadoRemito.Recibido || r.Estado == EstadoRemito.RecibidoConDiferencias))
                .ToListAsync(cancellationToken);

            if (remitos.Count != request.RemitoIds.Count)
            {
                throw new Exception("Uno o más remitos son inválidos, no pertenecen al proveedor o no han sido recepcionados.");
            }

            // 2b. Asegurarse que todos los remitos pertenezcan al MISMO Cliente
            var clienteId = remitos.First().ClienteId;
            if (remitos.Any(r => r.ClienteId != clienteId))
            {
                throw new Exception("Todos los remitos deben pertenecer al mismo Cliente para ser facturados juntos.");
            }

            // 3. Subir el archivo PDF
            var fileName = $"fac_{proveedorId.Value}_{Guid.NewGuid()}{Path.GetExtension(request.ArchivoPDF.FileName)}";
            await using var stream = request.ArchivoPDF.OpenReadStream();
            var pdfUrl = await _fileStorageService.UploadAsync(stream, fileName, "facturas");

            // 4. Extracción de Datos (Simulación IA/OCR)
            var extraccion = await _ocrService.ExtraerDatosFacturaAsync(pdfUrl);

            // 5. Validación AFIP (Simulación)
            var validacionAfip = await _afipService.ValidarComprobanteAsync(
                extraccion.F9_CAE,
                extraccion.F7_ProveedorCUIT,
                extraccion.F8_ClienteCUIT,
                extraccion.F1_TipoFactura,
                extraccion.F5_MontoTotal,
                extraccion.F4_FechaEmision ?? DateTime.UtcNow
            );

            // 6. Determinar Estado Inicial
            var estadoInicial = EstadoFactura.Aprobada;
            if (!validacionAfip.EsValido)
            {
                estadoInicial = EstadoFactura.RechazadaAFIP;
            }
            // Nota: La validación contra Remitos/OC (Conciliación) es un proceso manual/automático posterior.

            // 7. Crear la entidad Factura
            var factura = new Factura
            {
                ProveedorId = proveedorId.Value,
                ClienteId = clienteId,
                Estado = estadoInicial,
                ArchivoPDF_URL = pdfUrl,
                EsValidaAFIP = validacionAfip.EsValido,

                // Mapeo de F.1 a F.11
                F1_TipoFactura = extraccion.F1_TipoFactura,
                F2_PuntoVenta = extraccion.F2_PuntoVenta,
                F3_NumeroFactura = extraccion.F3_NumeroFactura,
                F4_FechaEmision = extraccion.F4_FechaEmision,
                F5_MontoTotal = extraccion.F5_MontoTotal,
                F6_MontoIVA = extraccion.F6_MontoIVA,
                F7_ProveedorCUIT = extraccion.F7_ProveedorCUIT,
                F8_ClienteCUIT = extraccion.F8_ClienteCUIT,
                F9_CAE = extraccion.F9_CAE,
                F10_VencimientoCAE = extraccion.F10_VencimientoCAE,
                F11_ObservacionesAFIP = validacionAfip.Observaciones, // Usamos la de AFIP

                // Relaciones N-N con Remitos
                FacturaRemitos = remitos.Select(r => new FacturaRemitos { RemitoId = r.Id }).ToList(),

                // Detalles de la factura (extraídos por OCR)
                Detalles = extraccion.Items.Select(i => new FacturaDetalle
                {
                    Sku = i.Sku,
                    Descripcion = i.Descripcion,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario,
                    Subtotal = i.Subtotal
                }).ToList()
            };

            // 8. Guardar en BD
            await _context.Facturas.AddAsync(factura, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // 9. (Acción futura) Notificar al Cliente sobre la nueva factura recibida.

            return factura.Id;
        }
    }
}
