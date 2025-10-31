using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortalProveedores.Domain.Enums;

namespace PortalProveedores.Domain.Entities
{
    /// <summary>
    /// Documento de ajuste (NC/ND) generado por el Cliente (Rol A)
    /// como resultado de una discrepancia en la conciliación.
    /// </summary>
    public class NotaDebitoCredito
    {
        public long Id { get; set; }
        public long FacturaId { get; set; } // La factura que se está ajustando
        public Factura Factura { get; set; } = null!;

        public long ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public long ProveedorId { get; set; }
        public Proveedor Proveedor { get; set; } = null!;

        public TipoNotaAjuste Tipo { get; set; }
        public MotivoNotaAjuste Motivo { get; set; }

        public decimal MontoAjuste { get; set; } // El valor (neto) del ajuste
        public string Detalle { get; set; } = string.Empty; // Razón del ajuste

        public DateTime FechaCreacion { get; set; }
        public string UsuarioCreadorId { get; set; } = string.Empty; // ID del Rol A
    }
}
