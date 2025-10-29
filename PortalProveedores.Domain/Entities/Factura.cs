using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortalProveedores.Domain.Enums;

namespace PortalProveedores.Domain.Entities
{
    public class Factura
    {
        public long Id { get; set; }
        public long ProveedorId { get; set; }
        public long ClienteId { get; set; }
        public EstadoFactura Estado { get; set; }
        public string? ArchivoPDF_URL { get; set; }
        public DateTime FechaCarga { get; set; }
        public int EstadoProcesamiento { get; set; }
        public string? CuitEmisor { get; set; }
        public string? RazonSocialEmisor { get; set; }
        public DateTime FechaEmision { get; set; }
        public int TipoFactura { get; set; }
        public char LetraFactura { get; set; }
        public string? CuitDestinatario { get; set; }
        public string? CAE { get; set; }
        public decimal TotalSinImpuestos { get; set; }
        public decimal TotalImpuestos { get; set; }
        public decimal TotalConImpuestos { get; set; }
        public int EstadoValidacionAFIP { get; set; }
        public string? MensajeValidacionAFIP { get; set; }
        public bool ConsistenciaCuit { get; set; }

        // --- Campos de Conciliación y OCR/IA (F.1 a F.11) ---
        public string F1_TipoFactura { get; set; } = string.Empty; // Ej: A, B, C
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
        // --------------------------------------------------------

        public bool EsValidaAFIP { get; set; } = false;
        public bool EsConciliadaOK { get; set; } = false;

        // Relaciones 1-N
        public ICollection<FacturaDetalle> Detalles { get; set; } = new List<FacturaDetalle>();

        // Relación N-N: Factura <-> Remitos (a través de FacturaRemitos)
        public ICollection<FacturaRemitos> FacturaRemitos { get; set; } = new List<FacturaRemitos>();
    }
}
