using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Enums;
using System.Collections.Generic;
using System.Linq;

namespace PortalProveedores.Application.Features.Reports.Queries
{
    // OLD - Handler directo en Query (ahora se usa IConciliacionService)
    /*
    public class GetConciliacionQueryHandler : IRequestHandler<GetConciliacionQuery, ConciliacionReporteDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetConciliacionQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<ConciliacionReporteDto> Handle(GetConciliacionQuery request, CancellationToken cancellationToken)
        {
            // 1. Seguridad: Solo Rol A (Admin Cliente) puede ver la conciliación de pagos.
            if (!_currentUser.IsInRole("AdministrativoCliente"))
            {
                throw new UnauthorizedAccessException("Acceso denegado a reportes de conciliación.");
            }

            // 2. Cargar Factura y sus Remitos asociados
            var factura = await _context.Facturas
                .Include(f => f.FacturaRemitos)
                .ThenInclude(fr => fr.Remito)
                .FirstOrDefaultAsync(f => f.Id == request.FacturaId, cancellationToken);

            if (factura == null) throw new Exception("Factura no encontrada.");

            // Se obtienen los IDs de los remitos asociados a esta factura
            var remitoIds = factura.FacturaRemitos.Select(fr => fr.RemitoId).ToList();

            // 3. Obtener todas las Recepciones (Fase E) y sus detalles para estos Remitos
            var recepciones = await _context.Recepciones
                .Include(r => r.Detalles)
                .Where(r => remitoIds.Contains(r.RemitoId))
                .ToListAsync(cancellationToken);

            // 4. Obtener todos los detalles de la Orden de Compra/Cotización Aceptada (Fase B/C)
            // Se buscan las cotizaciones que generaron estos remitos y sus OCs.
            var ordenesCompra = await _context.CotizacionRemitos
                .Where(cr => remitoIds.Contains(cr.RemitoId))
                .Select(cr => cr.Cotizacion.OrdenCompra)
                .Distinct()
                .ToListAsync(cancellationToken);

            // 5. Agrupar y Conciliar por SKU
            var itemsConciliados = new List<ConciliacionItemDto>();

            // La fuente de verdad para los items es la factura (lo que quieren cobrar)
            foreach (var itemFactura in factura.Detalles)
            {
                var itemConciliado = new ConciliacionItemDto
                {
                    Sku = itemFactura.Sku,
                    Descripcion = itemFactura.Descripcion,
                    CantidadFacturada = itemFactura.Cantidad,
                    PrecioUnitarioFacturado = itemFactura.PrecioUnitario
                };

                // a) Sumar Cantidad Recibida (Remito/Recepción)
                itemConciliado.CantidadRecibida = recepciones
                    .SelectMany(r => r.Detalles)
                    .Where(d => d.IdProducto == itemFactura.Sku)
                    .Sum(d => d.CantidadRecibida);

                // b) Obtener Cantidad y Precio OC (desde las Cotizaciones/OC)
                // (Esta es una simplificación; en un sistema real, el precio OC es más complejo)
                itemConciliado.CantidadOC = _context.Cotizaciones
                    .Include(c => c.Items)
                    .Where(c => ordenesCompra.Select(oc => oc.Id).Contains(c.OrdenCompraId))
                    .SelectMany(c => c.Items)
                    .Where(i => i.Sku == itemFactura.Sku)
                    .Sum(i => i.Cantidad);

                // Precio Unitario de la OC (tomamos el primero encontrado, simplificado)
                var itemOc = _context.Cotizaciones
                    .Include(c => c.Items)
                    .Where(c => ordenesCompra.Select(oc => oc.Id).Contains(c.OrdenCompraId))
                    .SelectMany(c => c.Items)
                    .FirstOrDefault(i => i.Sku == itemFactura.Sku);

                itemConciliado.PrecioUnitarioOC = itemOc?.PrecioUnitario ?? 0.00m;


                itemsConciliados.Add(itemConciliado);
            }

            // 6. Devolver el Reporte
            return new ConciliacionReporteDto
            {
                FacturaId = factura.Id,
                NumeroFactura = factura.F3_NumeroFactura,
                NumeroOrden = ordenesCompra.FirstOrDefault()?.NumeroOrden ?? "Múltiples OCs",
                ItemsConciliados = itemsConciliados
            };
        }
    }
    */

    // NEW - Ahora se usa el servicio IConciliacionService
    public class GetConciliacionQueryHandler : IRequestHandler<GetConciliacionQuery, ConciliacionReporteDto>
    {
        private readonly IConciliacionService _conciliacionService;
        private readonly ICurrentUserService _currentUser;

        public GetConciliacionQueryHandler(IConciliacionService conciliacionService, ICurrentUserService currentUser)
        {
            _conciliacionService = conciliacionService;
            _currentUser = currentUser;
        }

        public async Task<ConciliacionReporteDto> Handle(GetConciliacionQuery request, CancellationToken cancellationToken)
        {
            // 1. Seguridad: Solo Rol A (Admin Cliente) puede ver la conciliación
            var clienteId = _currentUser.ClienteId;
            if (!clienteId.HasValue || !_currentUser.IsInRole("AdministrativoCliente"))
            {
                throw new UnauthorizedAccessException("Acceso denegado a reportes de conciliación.");
            }

            // 2. Delegar el cálculo al servicio
            var reporte = await _conciliacionService.CalcularConciliacionAsync(request.FacturaId, clienteId.Value);

            return reporte;
        }
    }
}
