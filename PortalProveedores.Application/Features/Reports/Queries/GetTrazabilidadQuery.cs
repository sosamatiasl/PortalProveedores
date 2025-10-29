using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Enums;
using System.Linq;

namespace PortalProveedores.Application.Features.Reports.Queries
{
    // --- DTO de Trazabilidad (Un ciclo completo) ---
    public class TrazabilidadDto
    {
        public long OrdenCompraId { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public string ProveedorNombre { get; set; } = string.Empty;

        public EstadoOrdenCompra EstadoOC { get; set; }

        public long? RemitoId { get; set; }
        public string? NumeroRemito { get; set; }
        public EstadoRemito? EstadoRemito { get; set; }
        public bool RemitoConDiferencias { get; set; }

        public long? FacturaId { get; set; }
        public string? NumeroFactura { get; set; }
        public EstadoFactura? EstadoFactura { get; set; }
        public bool FacturaValidaAFIP { get; set; }
    }

    // --- Query ---
    /// <summary>
    /// Query: Reporte de Trazabilidad y Estado de Documentos.
    /// </summary>
    public class GetTrazabilidadQuery : IRequest<TrazabilidadDto>
    {
        // Buscamos por el ID de la OC o del Remito o de la Factura
        public long DocumentoId { get; set; }
        public TipoDocumento TipoDocumento { get; set; } // Enum (OC, Remito, Factura)
    }
}
