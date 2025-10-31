using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Domain.Entities
{
    /// <summary>
    /// Catálogo Maestro de Productos. Definido por el Cliente (Rol A).
    /// Esta es la "fuente de verdad" para SKUs y Descripciones.
    /// </summary>
    public class Producto
    {
        public long Id { get; set; }
        public long ClienteId { get; set; } // El Cliente que define este producto
        public Cliente Cliente { get; set; } = null!;

        public string Sku { get; set; } = string.Empty; // SKU Interno del Cliente
        public string Descripcion { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty; // Ej: "Unidad", "Caja", "Kg"
        public bool Activo { get; set; } = true;

        // Relación 1-N: Un producto puede tener N precios asignados (uno por proveedor)
        public ICollection<ProductoPrecio> PreciosPorProveedor { get; set; } = new List<ProductoPrecio>();
    }
}
