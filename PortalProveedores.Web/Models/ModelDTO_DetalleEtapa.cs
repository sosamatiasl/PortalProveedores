using PortalProveedores.Domain.Enums;

namespace PortalProveedores.Web.Models
{
    // DTO para la vista detallada de una etapa en la línea de proceso
    public class ModelDTO_DetalleEtapa
    {
        // Datos de la etapa actual
        public EstadoOrdenCompra EtapaActual { get; set; }
        public DateTime? FechaMovimiento { get; set; }
        public string Descripcion { get; set; } = string.Empty;

        // Datos de la Orden de Compra Origen
        public int OrdenCompraId { get; set; }
        public string NumeroReferencia { get; set; } = string.Empty;
        public List<string> ItemsSolicitados { get; set; } = new List<string>();

        // Datos específicos de la etapa "Cotización Enviada/Aceptada"
        public List<ModelDTO_CotizacionRespuesta> Cotizaciones { get; set; } = new List<ModelDTO_CotizacionRespuesta>();
    }
}
