using PortalProveedores.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Application.Common.Interfaces
{
    /// <summary>
    /// Interfaz para manejar las notificaciones
    /// </summary>
    public interface INotificationService
    {
        Task NotificarNuevaOrdenCompraAsync(OrdenCompra ordenCompra);
        Task NotificarNuevaCotizacionAsync(Cotizacion cotizacion);

        /// <summary>
        /// Notifica al Proveedor que una o varias de sus cotizaciones han sido aceptadas.
        /// </summary>
        Task NotificarCotizacionesAceptadasAsync(long proveedorId, long ordenCompraId, List<long> cotizacionesAceptadasIds);

        /// <summary>
        /// Notifica al Cliente que se ha generado un remito y la mercadería está en camino.
        /// </summary>
        Task NotificarRemitoGeneradoAsync(Remito remito);

        /// <summary>
        /// Notifica al Cliente y al Proveedor que un remito fue recepcionado.
        /// </summary>
        Task NotificarRecepcionRemitoAsync(Recepcion recepcion);

        /// <summary>
        /// Notifica al Cliente y al Proveedor que una Factura fue cargada y procesada.
        /// </summary>
        Task NotificarFacturaCargadaAsync(Factura factura);

        /// <summary>
        /// Envía una notificación de sistema a un usuario específico.
        /// </summary>
        Task SendNotificationToUserAsync(string userId, string titulo, string mensaje);
    }
}
