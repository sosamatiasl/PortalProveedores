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
        private readonly ITokenStorageService _tokenStorage;
        private readonly string _apiBaseUrl = "https://localhost:7061"; // URL de su API

        public ProviderService(IHttpClientFactory httpClientFactory, ITokenStorageService tokenStorage)
        {
            _httpClientFactory = httpClientFactory;
            _tokenStorage = tokenStorage;
        }

        private async Task<HttpClient> GetAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient();

            var token = await _tokenStorage.GetToken();

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        public async Task CreateProvider(CreateProviderRequest request)
        {
            var client = await GetAuthenticatedClient();
            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/Provider", request);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<ProviderDto>> GetMyProviders()
        {
            var client = await GetAuthenticatedClient();
            return await client.GetFromJsonAsync<List<ProviderDto>>($"{_apiBaseUrl}/api/Provider")
                ?? new List<ProviderDto>();
        }
    }
}
