using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortalProveedores.Application.Features.Facturas.Commands;

namespace PortalProveedores.Application.Common.Interfaces
{
    /// <summary>
    /// Simula la extracción de datos de un PDF mediante un servicio de IA/OCR.
    /// </summary>
    public interface IOCRService
    {
        Task<ExtraccionFacturaDto> ExtraerDatosFacturaAsync(string pdfUrl);
    }

    /// <summary>
    /// DTO de resultado de la extracción (Contiene F.1 a F.11 + Detalle de Items).
    /// </summary>
    public class ExtraccionFacturaDto
    {
        // Campos F.1 a F.11
        public string F1_TipoFactura { get; set; } = string.Empty;
        public string F2_PuntoVenta { get; set; } = string.Empty;
        public string F3_NumeroFactura { get; set; } = string.Empty;
        public DateTime? F4_FechaEmision { get; set; }
        public decimal F5_MontoTotal { get; set; }
        public decimal F6_MontoIVA { get; set; }
        public string F7_ProveedorCUIT { get; set; } = string.Empty;
        public string F8_ClienteCUIT { get; set; } = string.Empty;
        public string F9_CAE { get; set; } = string.Empty;
        public DateTime? F10_VencimientoCAE { get; set; }
        public string? F11_ObservacionesAFIP { get; set; }

        // Items
        public List<FacturaItemDto> Items { get; set; } = new();
    }

    public class FacturaItemDto
    {
        public string Sku { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
