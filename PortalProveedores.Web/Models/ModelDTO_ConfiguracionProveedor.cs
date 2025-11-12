using System.ComponentModel.DataAnnotations;

namespace PortalProveedores.Web.Models
{
    /// <summary>
    /// DTO para la transferencia y validación de datos de configuración del proveedor.
    /// </summary>
    public class ModelDTO_ConfiguracionProveedor
    {
        [Required(ErrorMessage = "El Nombre Fiscal es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string NombreFiscal { get; set; } = string.Empty;

        [Required(ErrorMessage = "El CUIT/ID Fiscal es obligatorio.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "El CUIT/ID Fiscal debe tener 11 dígitos.")]
        public string IdentificacionFiscal { get; set; } = string.Empty;

        [Required(ErrorMessage = "La Dirección es obligatoria.")]
        [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres.")]
        public string DireccionPrincipal { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Email de Contacto es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string EmailContacto { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Número de teléfono inválido.")]
        public string Telefono { get; set; } = string.Empty;
    }
}
