using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Domain.Enums
{
    public enum EstadoOrdenCompra
    {
        // 0. La orden de compra ha sido creada por el cliente
        [Display(Name = "Orden Creada")]
        Creada,

        // 1. El proveedor ha enviado una cotización
        [Display(Name = "Cotización Enviada")]
        CotizacionEnviada,

        // 2. El cliente ha aceptado una cotización
        [Display(Name = "Cotización Aceptada")]
        CotizacionAceptada,

        // 3. El proveedor ha iniciado la preparación de la orden
        [Display(Name = "En Preparación")]
        EnPreparacion,

        // 4. La orden ha sido despachada (lista para transporte)
        [Display(Name = "Orden Enviada")]
        Enviada,

        // 5. El cliente ha confirmado la recepción
        [Display(Name = "Completada")]
        Completada,

        // 6. El proceso ha sido cancelado
        [Display(Name = "Cancelada")]
        Cancelada
    }
}
