using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Enums;

namespace PortalProveedores.Application.Features.Remitos.Queries
{
    public class GetRemitoParaRecepcionQueryHandler : IRequestHandler<GetRemitoParaRecepcionQuery, RemitoParaRecepcionDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetRemitoParaRecepcionQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<RemitoParaRecepcionDto> Handle(GetRemitoParaRecepcionQuery request, CancellationToken cancellationToken)
        {
            // 1. Seguridad: Solo un "Recepcionador" (Rol D) puede escanear para recibir
            if (!_currentUser.IsInRole("RecepcionadorMercaderia"))
            {
                throw new UnauthorizedAccessException("Acción permitida solo para el Rol (D) Recepcionador.");
            }

            // 2. Validar el QR
            var qrCode = await _context.RemitoQRCodes
                .FirstOrDefaultAsync(qr =>
                    qr.CodigoHash == request.QrToken &&
                    !qr.Usado &&
                    qr.FechaExpiracion > DateTime.UtcNow,
                    cancellationToken);

            if (qrCode == null)
            {
                throw new Exception("QR inválido, expirado o ya utilizado.");
            }

            // 3. Obtener el Remito y el Proveedor
            var remito = await _context.Remitos
                .Include(r => r.Proveedor)
                .FirstOrDefaultAsync(r => r.Id == qrCode.RemitoId, cancellationToken);

            if (remito == null) throw new Exception("Remito asociado no encontrado.");

            // 4. Obtener los items esperados.
            // Los items están en las Cotizaciones vinculadas al Remito.
            var itemsEsperados = await _context.CotizacionRemitos
                .Where(cr => cr.RemitoId == remito.Id)
                .Select(cr => cr.Cotizacion) // Ir a la cotización
                .SelectMany(c => c.Items)    // Obtener sus items
                .Select(item => new RemitoItemEsperadoDto
                {
                    Sku = item.Sku,
                    Descripcion = item.Descripcion,
                    CantidadDeclarada = item.Cantidad
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // (Opcional: Agrupar por SKU si un mismo SKU vino en varias cotizaciones)
            var itemsAgrupados = itemsEsperados
                .GroupBy(i => i.Sku)
                .Select(g => new RemitoItemEsperadoDto
                {
                    Sku = g.Key,
                    Descripcion = g.First().Descripcion,
                    CantidadDeclarada = g.Sum(i => i.CantidadDeclarada)
                }).ToList();

            // 5. Devolver DTO para la App Móvil
            return new RemitoParaRecepcionDto
            {
                RemitoId = remito.Id,
                NumeroRemito = remito.NumeroRemito,
                ProveedorNombre = remito.Proveedor.RazonSocial,
                ItemsEsperados = itemsAgrupados
            };
        }
    }
}
