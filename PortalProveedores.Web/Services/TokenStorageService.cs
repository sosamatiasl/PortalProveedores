namespace PortalProveedores.Web.Services
{
    /// <summary>
    /// Almacén Scoped (por circuito de usuario) para el Token JWT en Blazor Server.
    /// </summary>
    public class TokenStorageService
    {
        public string? Token { get; set; }
    }
}
