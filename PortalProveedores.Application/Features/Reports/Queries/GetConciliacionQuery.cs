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
    // --- DTOs para la Conciliación ---
    public class ConciliacionItemDto
    {
        public string Sku { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        // Cantidades
        public decimal CantidadOC { get; set; }
        public decimal CantidadRecibida { get; set; } // Viene de RecepcionDetalle
        public decimal CantidadFacturada { get; set; }

        // Precios (para Montos)
        public decimal PrecioUnitarioOC { get; set; }
        public decimal PrecioUnitarioFacturado { get; set; }

        // Diferencias
        public decimal DiferenciaCantidad { get => CantidadFacturada - CantidadRecibida; }
        public decimal DiferenciaPrecio { get => PrecioUnitarioFacturado - PrecioUnitarioOC; }
    }

    public class ConciliacionReporteDto
    {
        public long FacturaId { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public string NumeroOrden { get; set; } = string.Empty;
        public List<ConciliacionItemDto> ItemsConciliados { get; set; } = new();

        // Resumen de Discrepancias
        public bool HuboDiscrepanciasCantidad { get => ItemsConciliados.Any(i => i.DiferenciaCantidad != 0); }
        public bool HuboDiscrepanciasPrecio { get => ItemsConciliados.Any(i => i.DiferenciaPrecio != 0); }
    }

    // --- Query ---
    /// <summary>
    /// Query: Reporte de Conciliación a Tres Vías.
    /// </summary>
    public class GetConciliacionQuery : IRequest<ConciliacionReporteDto>
    {
        public long FacturaId { get; set; }
    }
}
