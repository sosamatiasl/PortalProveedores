namespace PortalProveedores.Web.Models
{
    public class LoginResponse
    {
        // El nombre de esta propiedad debe coincidir con la respuesta JSON de tu API
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
