using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortalProveedores.Application.Features.Reports.Queries;

namespace PortalProveedores.Application.Common.Interfaces
{
    /// <summary>
    /// Servicio dedicado a calcular la Conciliación a Tres Vías.
    /// </summary>
    public interface IConciliacionService
    {
        /// <summary>
        /// Calcula el reporte de conciliación para una factura específica.
        /// </summary>
        Task<ConciliacionReporteDto> CalcularConciliacionAsync(long facturaId, long clienteId);
    }

    // --- DTO de Reporte ---
    public class ConciliacionItemDto
    {
        public string Sku { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        public decimal CantidadOC { get; set; }
        public decimal CantidadRecibida { get; set; }
        public decimal CantidadFacturada { get; set; }

        public decimal PrecioUnitarioOC { get; set; }
        public decimal PrecioUnitarioFacturado { get; set; }

        // (Cantidad Facturada - Cantidad Recibida)
        public decimal DiferenciaCantidad { get => CantidadFacturada - CantidadRecibida; }
        // (Precio Facturado - Precio OC)
        public decimal DiferenciaPrecio { get => PrecioUnitarioFacturado - PrecioUnitarioOC; }

        // Monto del ajuste por cantidad (si facturó de más/menos)
        public decimal MontoAjusteCantidad { get => DiferenciaCantidad * PrecioUnitarioOC; }
        // Monto del ajuste por precio (si cobró de más/menos)
        public decimal MontoAjustePrecio { get => DiferenciaPrecio * CantidadFacturada; }
    }

    public class ConciliacionReporteDto
    {
        public long FacturaId { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public long ClienteId { get; set; }
        public long ProveedorId { get; set; }
        public List<ConciliacionItemDto> ItemsConciliados { get; set; } = new();

        public decimal TotalAjusteCantidad { get => ItemsConciliados.Sum(i => i.MontoAjusteCantidad); }
        public decimal TotalAjustePrecio { get => ItemsConciliados.Sum(i => i.MontoAjustePrecio); }

        // Total Neto: 
        // Si > 0, el Proveedor facturó de MÁS (requiere Nota de Crédito).
        // Si < 0, el Proveedor facturó de MENOS (requiere Nota de Débito).
        public decimal TotalAjusteNeto { get => TotalAjusteCantidad + TotalAjustePrecio; }

        /// <summary>
        /// Indica si hubo alguna diferencia entre la cantidad del Remito y la Orden de Compra.
        /// </summary>
        public bool HuboDiscrepanciasCantidad { get; set; }
    }
}
