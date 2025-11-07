using PortalProveedores.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Domain.Entities
{
    public class Proveedor
    {
        public long Id { get; set; }
        public string? RazonSocial { get; set; }
        public string? CUIT { get; set; }
        public DateTime FechaCreacion { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
