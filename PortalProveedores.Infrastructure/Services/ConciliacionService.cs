using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Application.Features.Reports.Queries; // Para los DTOs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortalProveedores.Infrastructure.Services
{
    public class ConciliacionService : IConciliacionService
    {
        private readonly IApplicationDbContext _context;

        public ConciliacionService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ConciliacionReporteDto> CalcularConciliacionAsync(long facturaId, long clienteId)
        {
            // 1. Cargar Factura y Remitos asociados
            var factura = await _context.Facturas
                .Include(f => f.FacturaRemitos)
                .Include(f => f.Detalles) // Incluir detalles de la factura
                .FirstOrDefaultAsync(f => f.Id == facturaId && f.ClienteId == clienteId);

            if (factura == null)
                throw new Exception("Factura no encontrada o no pertenece al cliente.");

            var remitoIds = factura.FacturaRemitos.Select(fr => fr.RemitoId).ToList();

            // 2. Obtener Recepciones
            var recepciones = await _context.Recepciones
                .Include(r => r.Detalles)
                .Where(r => remitoIds.Contains(r.RemitoId))
                .ToListAsync();

            // 3. Obtener Cotizaciones Aceptadas
            var cotizaciones = await _context.CotizacionRemitos
                .Where(cr => remitoIds.Contains(cr.RemitoId))
                .Select(cr => cr.Cotizacion)
                .Include(c => c.Items) // Incluir items de la cotización
                .Distinct()
                .ToListAsync();

            // 4. Agrupar y Conciliar por SKU
            var itemsConciliados = new List<ConciliacionItemDto>();
            var todosSkus = factura.Detalles.Select(fd => fd.Sku)
                .Union(recepciones.SelectMany(r => r.Detalles).Select(rd => rd.IdProducto))
                .Union(cotizaciones.SelectMany(c => c.Items).Select(ci => ci.Sku))
                .Distinct();

            foreach (var sku in todosSkus)
            {
                var itemFactura = factura.Detalles.FirstOrDefault(fd => fd.Sku == sku);
                var itemsRecepcion = recepciones.SelectMany(r => r.Detalles).Where(rd => rd.IdProducto == sku);
                var itemsCotizacion = cotizaciones.SelectMany(c => c.Items).Where(ci => ci.Sku == sku);

                itemsConciliados.Add(new ConciliacionItemDto
                {
                    Sku = sku,
                    Descripcion = itemFactura?.Descripcion ?? itemsRecepcion.FirstOrDefault()?.DescripcionProducto ?? itemsCotizacion.FirstOrDefault()?.Descripcion ?? "N/A",

                    CantidadFacturada = itemFactura?.Cantidad ?? 0,
                    PrecioUnitarioFacturado = itemFactura?.PrecioUnitario ?? 0,

                    CantidadRecibida = itemsRecepcion.Sum(r => r.CantidadRecibida),

                    CantidadOC = itemsCotizacion.Sum(c => c.Cantidad),
                    PrecioUnitarioOC = itemsCotizacion.FirstOrDefault()?.PrecioUnitario ?? 0 // (Simplificado)
                });
            }

            // 6. Devolver el Reporte
            return new ConciliacionReporteDto
            {
                FacturaId = factura.Id,
                NumeroFactura = factura.F3_NumeroFactura ?? "N/A",
                ClienteId = factura.ClienteId,
                ProveedorId = factura.ProveedorId,
                ItemsConciliados = itemsConciliados
            };
        }
    }
}
