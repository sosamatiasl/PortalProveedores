using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace PortalProveedores.Infrastructure.Services
{
    // Esta es una implementación simple.
    // En un caso real, se inyectaría 'IHubContext<NotificationHub>' de SignalR para enviar notificaciones en tiempo real a los clientes (Web y Móvil).
    /// <summary>
    /// mplementación (simulada) del servicio de notificación.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IHubNotificationService _hubNotifier;

        // En un caso real, se inyecta IApplicationDbContext para buscar los IDs
        // de Cliente/Proveedor si no vienen en la entidad.
        //private readonly IHubContext<PortalHub> _hubContext; // Inyección de SignalR
        private readonly IApplicationDbContext _context; // Para buscar IDs de entidad

        public NotificationService(
            ILogger<NotificationService> logger,
            //IHubContext<PortalHub> hubContext,
            IHubNotificationService hubNotifier,
            IApplicationDbContext context)
        {
            _logger = logger;
            //_hubContext = hubContext;
            _hubNotifier = hubNotifier;
            _context = context;
        }

        public Task NotificarNuevaOrdenCompraAsync(OrdenCompra ordenCompra)
        {
            // SIMULACIÓN: Aquí se enviaría una notificación (SignalR, Email, Push) al Proveedor (OrdenCompra.ProveedorId).

            _logger.LogInformation($"--- NOTIFICACIÓN ---");
            _logger.LogInformation($"Nueva Orden de Compra ({ordenCompra.NumeroOrden}) creada.");
            _logger.LogInformation($"Destino: Proveedor ID {ordenCompra.ProveedorId}");
            _logger.LogInformation($"Emisor: Cliente ID {ordenCompra.ClienteId}");

            return Task.CompletedTask;
        }

        public Task NotificarNuevaCotizacionAsync(Cotizacion cotizacion)
        {
            // SIMULACIÓN: Aquí se notificaría al Cliente (Cotizacion.OrdenCompra.ClienteId).

            _logger.LogInformation($"--- NOTIFICACIÓN ---");
            _logger.LogInformation($"Nueva Cotización ({cotizacion.NumeroCotizacion}) recibida.");
            _logger.LogInformation($"Para la OC ID: {cotizacion.OrdenCompraId}");
            _logger.LogInformation($"Emisor: Proveedor ID {cotizacion.ProveedorId}");

            return Task.CompletedTask;
        }

        public Task NotificarCotizacionesAceptadasAsync(long proveedorId, long ordenCompraId, List<long> cotizacionesAceptadasIds)
        {
            _logger.LogInformation($"--- NOTIFICACIÓN (Cotización Aceptada) ---");
            _logger.LogInformation($"Proveedor ID {proveedorId}: El cliente ha ACEPTADO sus cotizaciones (IDs: {string.Join(", ", cotizacionesAceptadasIds)}) para la OC ID {ordenCompraId}.");
            _logger.LogInformation("Acción requerida: Preparar envío y generar Remito.");
            return Task.CompletedTask;
        }

        public async Task NotificarRemitoGeneradoAsync(Remito remito)
        {
            //_logger.LogInformation($"--- NOTIFICACIÓN (Remito Generado) ---");
            //_logger.LogInformation($"Cliente ID {remito.ClienteId}: El Proveedor ID {remito.ProveedorId} ha generado el remito {remito.NumeroRemito}.");
            //_logger.LogInformation($"Mercadería en camino. PDF: {remito.ArchivoPDF_URL}");
            //return Task.CompletedTask;

            // 1. Notificar a todos los usuarios que sean "Recepcionadores" del Cliente.
            // En un caso real, se busca los IDs de usuario por ClienteId y Rol.

            var title = "Nuevo Remito Generado";
            var message = $"El proveedor ha generado el remito N° {remito.NumeroRemito}. Listo para su recepción.";

            // Simulación: Enviar al cliente de forma general.
            //await _hubContext.Clients.All.SendAsync("ReceiveNotification", title, message);
            await _hubNotifier.NotifyAllAsync("ReceiveNotification", new { title, message });
            _logger.LogInformation($"[SignalR REAL] Remito Generado: Notificación enviada a todos.");
        }

        public async Task NotificarRecepcionRemitoAsync(Recepcion recepcion)
        {
            //_logger.LogInformation($"--- NOTIFICACIÓN (Remito Recepcionado) ---");
            //_logger.LogInformation($"Remito ID {recepcion.RemitoId} fue recepcionado.");

            //if (recepcion.HuboDiferencias)
            //{
            //    _logger.LogWarning("ATENCIÓN: Se registraron diferencias en la recepción.");
            //}
            //else
            //{
            //    _logger.LogInformation("Recepción exitosa y sin diferencias.");
            //}

            //// Aquí se busca el ClienteId y ProveedorId asociados al Remito para notificar a ambos.

            //return Task.CompletedTask;

            // 1. Notificar a todos los usuarios del Proveedor (Roles B y C).
            // Se necesita cargar el ProveedorId del Remito.
            var remito = await _context.Remitos.FindAsync(recepcion.RemitoId);

            if (remito != null)
            {
                var title = recepcion.HuboDiferencias ? "⚠️ Recepción con Diferencias" : "✅ Recepción Exitosa";
                var message = $"El remito N° {remito.NumeroRemito} fue recibido por el cliente. Hubo diferencias: {recepcion.HuboDiferencias}.";

                // Simulación: Enviar al cliente de forma general.
                //await _hubContext.Clients.All.SendAsync("ReceiveNotification", title, message);
                await _hubNotifier.NotifyAllAsync("ReceiveNotification", new { title, message });
                _logger.LogInformation($"[SignalR REAL] Recepción Remito: Notificación enviada al Proveedor {remito.ProveedorId}.");
            }
        }

        public async Task NotificarFacturaCargadaAsync(Factura factura)
        {
            //_logger.LogInformation($"--- [SignalR SIMULADO] Factura Cargada ---");
            //_logger.LogInformation($"Enviando a Cliente ID {factura.ClienteId}: Factura N° {factura.F3_NumeroFactura} cargada. Estado AFIP: {factura.Estado}.");
            //return Task.CompletedTask;

            var title = "🧾 Nueva Factura Recibida";
            var message = $"Factura N° {factura.F3_NumeroFactura} cargada. Estado fiscal: {factura.Estado}. Requiere conciliación.";

            // Simulación: Enviar al cliente de forma general.
            //await _hubContext.Clients.All.SendAsync("ReceiveNotification", title, message);
            await _hubNotifier.NotifyAllAsync("ReceiveNotification", new { title, message });
            _logger.LogInformation($"[SignalR REAL] Factura Cargada: Notificación enviada al Cliente {factura.ClienteId}.");
        }

        public Task SendNotificationToUserAsync(string userId, string titulo, string mensaje)
        {
            //_logger.LogInformation($"--- [SignalR SIMULADO] Notificación Usuario ---");
            //_logger.LogInformation($"Enviando a Usuario ID {userId}: Título='{titulo}', Mensaje='{mensaje}'");

            //// Aquí se usaría el método Real Hub.Clients.User(userId).SendAsync("ReceiveNotification", titulo, mensaje);

            //return Task.CompletedTask;

            // Usa el IUserIdProvider configurado en la API para enviar solo a ese usuario.
            //return _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", titulo, mensaje);
            return _hubNotifier.NotifyAllAsync("ReceiveNotification", new { titulo, mensaje });
        }
    }
}
