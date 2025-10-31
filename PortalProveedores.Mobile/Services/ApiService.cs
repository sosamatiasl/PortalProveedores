using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortalProveedores.Mobile.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace PortalProveedores.Mobile.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private const string AuthTokenKey = "AuthToken";
        private const string UserIdKey = "UserId";

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private async Task SetAuthHeader()
        {
            string? token = await GetAuthTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<AuthResult> LoginAsync(AuthRequest loginRequest)
        {
            try
            {
                var json = JsonConvert.SerializeObject(loginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Endpoint de la API: api/auth/login
                var response = await _httpClient.PostAsync("auth/login", content);

                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var authResult = JsonConvert.DeserializeObject<AuthResult>(jsonResponse);
                    if (authResult != null && authResult.Success)
                    {
                        // Guardar el token y el ID de forma segura y encriptada
                        await SecureStorage.Default.SetAsync(AuthTokenKey, authResult.Token);
                        await SecureStorage.Default.SetAsync(UserIdKey, authResult.UserId);
                    }
                    return authResult;
                }
                else
                {
                    // Intentar deserializar el error de la API
                    return new AuthResult { Success = false, ErrorMessage = jsonResponse };
                }
            }
            catch (Exception ex)
            {
                return new AuthResult { Success = false, ErrorMessage = $"Error de conexión: {ex.Message}" };
            }
        }

        public Task LogoutAsync()
        {
            // Eliminar todos los datos sensibles guardados
            SecureStorage.Default.Remove(AuthTokenKey);
            SecureStorage.Default.Remove(UserIdKey);
            _httpClient.DefaultRequestHeaders.Authorization = null;
            return Task.CompletedTask;
        }

        public async Task<RemitoParaRecepcionDto> ValidarQrRecepcionAsync(string qrToken)
        {
            await SetAuthHeader();

            // Llama al endpoint de la API: api/remitos/recepcion/validar-qr/{qrToken}
            var response = await _httpClient.GetAsync($"remitos/recepcion/validar-qr/{qrToken}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al validar QR. Código: {response.StatusCode}. Mensaje: {error}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            // ¡ADVERTENCIA! La API devuelve un DTO (RemitoParaRecepcionDto)
            var result = JsonConvert.DeserializeObject<RemitoParaRecepcionDto>(jsonResponse);

            return result ?? throw new Exception("Respuesta del servidor inválida.");
        }

        /// <summary>
        /// Obtiene el token de autenticación cifrado del SecureStorage.
        /// </summary>
        public async Task<string?> GetAuthTokenAsync()
        {
            // SecureStorage guarda datos cifrados de forma nativa
            return await SecureStorage.Default.GetAsync(AuthTokenKey);
        }

        public async Task<long> ConfirmarRecepcionAsync(ConfirmarRecepcionCommand command)
        {
            await SetAuthHeader();

            var json = JsonConvert.SerializeObject(command);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Llama al endpoint de la API: api/remitos/recepcion/confirmar
            var response = await _httpClient.PostAsync("remitos/recepcion/confirmar", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al confirmar recepción. Código: {response.StatusCode}. Mensaje: {error}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            // La API devuelve el ID de la Recepción creada (long)

            if (long.TryParse(jsonResponse, out long recepcionId))
            {
                return recepcionId;
            }

            throw new Exception("La API no devolvió un ID de Recepción válido.");
        }

    }
}
