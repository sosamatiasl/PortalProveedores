using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace PortalProveedores.Mobile.Services
{
    // Interfaz
    public interface IApiClient
    {
        // Define aquí los métodos de tu API
        // Task<bool> EnviarRecepcionAsync(object recepcion);
    }

    // Implementación
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;

        // URL Base de tu API (debe leerse de appsettings o configuración)
        private const string BaseApiUrl = "https://10.0.2.2:5153/api/";

        public ApiClient()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseApiUrl) };
            // Aquí se configuraría el Token JWT si ya está autenticado
        }

        // ... Implementación de los métodos ...
    }
}
