using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Application.Features.Cotizaciones.Commands
{
    // DTO para los items
    public record CotizacionItemDto(
        string Sku,
        string Descripcion,
        decimal Cantidad,
        decimal PrecioUnitario
    );

    /// <summary>
    /// Crear una nueva Cotización.
    /// </summary>
    public class CrearCotizacionCommand : IRequest<long> // Devuelve el Id de la nueva Cotización
    {
        public long OrdenCompraId { get; set; }
        public string NumeroCotizacion { get; set; } = string.Empty;
        public int ValidezDias { get; set; }
        public IFormFile? ArchivoPDF { get; set; } // Opcional por si solo carga items
        public List<CotizacionItemDto> Items { get; set; } = new();
    }
}
