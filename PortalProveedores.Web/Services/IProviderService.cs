using PortalProveedores.Application.Models;
using System.Net.Http.Headers;

namespace PortalProveedores.Web.Services
{
    // DTOs (pueden definirse aquí o en Application)
    public record ProviderDto(int Id, string CompanyName, string TaxId);

    public interface IProviderService
    {
        Task CreateProvider(CreateProviderRequest request);
        Task<List<ProviderDto>> GetMyProviders();
    }

    public class ProviderService : IProviderService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TokenStorageService _tokenStorage;
        private readonly string _apiBaseUrl = "https://localhost:7061"; // URL de su API

        public ProviderService(IHttpClientFactory httpClientFactory, TokenStorageService tokenStorage)
        {
            _httpClientFactory = httpClientFactory;
            _tokenStorage = tokenStorage;
        }

        private HttpClient GetAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient();
            if (!string.IsNullOrWhiteSpace(_tokenStorage.Token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenStorage.Token);
            }
            return client;
        }

        public async Task CreateProvider(CreateProviderRequest request)
        {
            var client = GetAuthenticatedClient();
            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/Provider", request);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<ProviderDto>> GetMyProviders()
        {
            var client = GetAuthenticatedClient();
            return await client.GetFromJsonAsync<List<ProviderDto>>($"{_apiBaseUrl}/api/Provider")
                ?? new List<ProviderDto>();
        }
    }
}
