using PortalProveedores.Application.Models;
using System.Net.Http.Headers;

namespace PortalProveedores.Web.Services
{
    public record OrderSummaryDto(int OrderId, string ProviderName, decimal Total, string Status);

    public interface IOrderService
    {
        Task<int> CreateOrder(CreateOrderRequest request);
        Task<List<OrderSummaryDto>> GetMyOrders();
        Task ConfirmPayment(int orderId);
    }

    public class OrderService : IOrderService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TokenStorageService _tokenStorage;
        private readonly string _apiBaseUrl = "https://localhost:7061"; // URL de su API

        public OrderService(IHttpClientFactory httpClientFactory, TokenStorageService tokenStorage)
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

        public async Task<int> CreateOrder(CreateOrderRequest request)
        {
            var client = GetAuthenticatedClient();
            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/Order", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            return (int)result?.OrderId;
        }

        public async Task<List<OrderSummaryDto>> GetMyOrders()
        {
            var client = GetAuthenticatedClient();
            return await client.GetFromJsonAsync<List<OrderSummaryDto>>($"{_apiBaseUrl}/api/Order/MyOrders")
                 ?? new List<OrderSummaryDto>();
        }

        public async Task ConfirmPayment(int orderId)
        {
            var client = GetAuthenticatedClient();
            var response = await client.PutAsync($"{_apiBaseUrl}/api/Order/ConfirmPayment/{orderId}", null);
            response.EnsureSuccessStatusCode();
        }
    }
}
