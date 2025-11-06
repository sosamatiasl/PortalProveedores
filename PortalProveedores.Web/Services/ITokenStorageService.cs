namespace PortalProveedores.Web.Services
{
    public interface ITokenStorageService
    {
        Task SetToken(string token);
        Task SetRefreshToken(string refreshToken);
        Task<string?> GetToken();
        Task<string?> GetRefreshToken();
        Task RemoveTokens();
    }
}
