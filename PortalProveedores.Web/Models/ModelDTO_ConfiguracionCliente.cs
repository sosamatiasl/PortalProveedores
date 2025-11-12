using System.ComponentModel.DataAnnotations;

namespace PortalProveedores.Web.Models
{
    /// <summary>
    /// DTO para la transferencia y validación de datos de configuración del cliente.
    /// </summary>
    public class ModelDTO_ConfiguracionCliente
    {
        [Required(ErrorMessage = "El Nombre de la Empresa es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string NombreEmpresa { get; set; } = string.Empty;

        [Required(ErrorMessage = "El CUIT/ID Fiscal es obligatorio.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "El CUIT/ID Fiscal debe tener 11 dígitos.")]
        public string IdentificacionFiscal { get; set; } = string.Empty;

        [Required(ErrorMessage = "La Dirección es obligatoria.")]
        [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres.")]
        public string DireccionOficinaPrincipal { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Email Administrativo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string EmailAdministrativo { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Número de teléfono inválido.")]
        public string Telefono { get; set; } = string.Empty;
    }
}
