using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace PortalProveedores.Application.Features.Cotizaciones.Queries
{
    // --- DTOs de Respuesta ---
    // (DTOs para no exponer las entidades del Dominio)
    public class CotizacionItemDetalleDto
    {
        public long Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class CotizacionDetalleDto
    {
        public long Id { get; set; }
        public string NumeroCotizacion { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public decimal MontoTotal { get; set; }
        public EstadoCotizacion Estado { get; set; }
        public string? ArchivoPDF_URL { get; set; }
        public List<CotizacionItemDetalleDto> Items { get; set; } = new();
    }

    // --- Query ---
    public class GetCotizacionesPorOrdenCompraQuery : IRequest<List<CotizacionDetalleDto>>
    {
        public long OrdenCompraId { get; set; }
    }

    // --- Handler ---
    public class GetCotizacionesPorOrdenCompraQueryHandler : IRequestHandler<GetCotizacionesPorOrdenCompraQuery, List<CotizacionDetalleDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetCotizacionesPorOrdenCompraQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<CotizacionDetalleDto>> Handle(GetCotizacionesPorOrdenCompraQuery request, CancellationToken cancellationToken)
        {
            // Seguridad: El usuario debe ser el Cliente dueño de la OC, o el Proveedor asignado a la OC.
            var clienteId = _currentUser.ClienteId;
            var proveedorId = _currentUser.ProveedorId;

            if (!clienteId.HasValue && !proveedorId.HasValue)
            {
                throw new UnauthorizedAccessException("Usuario no válido.");
            }

            var query = _context.Cotizaciones
                .Include(c => c.Items) // Se incluyen los items de cada cotización
                .Where(c => c.OrdenCompraId == request.OrdenCompraId);

            // Filtro de seguridad: solo mostrar si el usuario es parte de la OC
            if (clienteId.HasValue)
            {
                query = query.Where(c => c.OrdenCompra.ClienteId == clienteId.Value);
            }
            else if (proveedorId.HasValue)
            {
                query = query.Where(c => c.OrdenCompra.ProveedorId == proveedorId.Value);
            }

            var cotizaciones = await query
                .OrderBy(c => c.FechaEmision)
                .AsNoTracking() // Es una consulta de solo lectura
                .ToListAsync(cancellationToken);

            // Mapeo manual a DTOs
            var resultadoDto = cotizaciones.Select(c => new CotizacionDetalleDto
            {
                Id = c.Id,
                NumeroCotizacion = c.NumeroCotizacion,
                FechaEmision = c.FechaEmision,
                MontoTotal = c.MontoTotal,
                Estado = c.Estado,
                ArchivoPDF_URL = c.ArchivoPDF_URL,
                Items = c.Items.Select(i => new CotizacionItemDetalleDto
                {
                    Id = i.Id,
                    Sku = i.Sku,
                    Descripcion = i.Descripcion,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario,
                    Subtotal = i.Subtotal
                }).ToList()
            }).ToList();

            return resultadoDto;
        }
    }
}
