using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortalProveedores.Mobile.Models;
using Microsoft.Extensions.Logging;

namespace PortalProveedores.Mobile.Services
{
    /// <summary>
    /// Lógica que interactúa con la DB local y la API.
    /// </summary>
    public class SynchronizationService : ISynchronizationService
    {
        private readonly IApiService _apiService;
        private readonly DatabaseService _databaseService;
        private readonly ILogger<SynchronizationService> _logger; // Para Shiny logs

        public SynchronizationService(
            IApiService apiService,
            DatabaseService databaseService,
            ILogger<SynchronizationService> logger)
        {
            _apiService = apiService;
            _databaseService = databaseService;
            _logger = logger;
        }

        /// <summary>
        /// Proceso central: sincroniza todos los remitos pendientes.
        /// </summary>
        /// <returns>El número de remitos sincronizados exitosamente.</returns>
        public async Task<int> SynchronizePendingRemitosAsync()
        {
            _logger.LogInformation("Iniciando tarea de sincronización de remitos pendientes...");

            // 1. Verificar Token de Autenticación
            var token = await _apiService.GetAuthTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Sincronización abortada: Token de usuario no encontrado (no autenticado).");
                return 0; // No se puede sincronizar sin autenticación
            }

            // 2. Obtener Remitos Pendientes
            var remitosPendientes = await _databaseService.GetRemitosPendientesAsync();
            if (remitosPendientes.Count == 0)
            {
                _logger.LogInformation("No hay remitos pendientes de sincronización.");
                return 0;
            }

            int count = 0;
            foreach (var remitoLocal in remitosPendientes)
            {
                try
                {
                    // 3. Crear el Command para la API
                    var command = new ConfirmarRecepcionCommand
                    {
                        RemitoId = remitoLocal.RemitoId,
                        NombreReceptor = remitoLocal.NombreReceptor,
                        FirmaBase64 = remitoLocal.FirmaBase64,
                        // Usa la propiedad deserializada
                        ItemsRecibidos = remitoLocal.ItemsRecibidos
                    };

                    // 4. Enviar a la API (puede fallar por desconexión)
                    await _apiService.ConfirmarRecepcionAsync(command);

                    // 5. Éxito: Eliminar o Marcar como sincronizado
                    // La opción más limpia es eliminar el registro local.
                    await _databaseService.DeleteRemitoAsync(remitoLocal);

                    _logger.LogInformation($"Remito {remitoLocal.NumeroRemito} sincronizado y eliminado localmente.");
                    count++;
                }
                catch (Exception ex)
                {
                    // Si falla una llamada, se registra y se pasa al siguiente remito.
                    // Podría fallar por desconexión, token expirado, o error de datos.
                    _logger.LogError(ex, $"Error al sincronizar Remito {remitoLocal.NumeroRemito}. Se mantendrá en cola.");

                    // Si es un error de conexión, se termina el proceso para no agotar la batería.
                    if (ex is HttpRequestException) return count;
                }
            }

            _logger.LogInformation($"Sincronización finalizada. Remitos sincronizados: {count}.");
            return count;
        }
    }
}
