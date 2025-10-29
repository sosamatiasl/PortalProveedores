using System.ComponentModel.DataAnnotations;

namespace PortalProveedores.API.Models
{
    /// <summary>
    /// DTO para la solicitud de renovación.
    /// </summary>
    public class RefreshRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
