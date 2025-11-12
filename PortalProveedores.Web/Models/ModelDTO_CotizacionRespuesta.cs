namespace PortalProveedores.Web.Models
{
    // DTO anidado para las cotizaciones
    public class ModelDTO_CotizacionRespuesta
    {
        public int CotizacionId { get; set; }
        public decimal MontoTotal { get; set; }
        public string Estado { get; set; } = string.Empty; // "Aceptada", "Rechazada", "Pendiente"
        public DateTime FechaEnvio { get; set; }
    }
}
