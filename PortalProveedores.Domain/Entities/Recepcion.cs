using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortalProveedores.Domain.Entities.Identity;

namespace PortalProveedores.Domain.Entities
{
    public class Recepcion
    {
        public long Id { get; set; }
        public long RemitoId { get; set; }
        public Remito Remito { get; set; } = null!;

        public long? UsuarioRecepcionId { get; set; }
        public ApplicationUser UsuarioRecepcion { get; set; } = null!; // Usuario con Rol "D"

        public DateTime FechaRecepcion { get; set; }
        public bool HuboDiferencias { get; set; }
        public string? DetalleDiferencias { get; set; } // Notas/Comentarios

        public string FirmaRecepcionista_URL { get; set; } = string.Empty;
        public string FirmaTransportista_URL { get; set; } = string.Empty;

        public ICollection<RecepcionDetalle> Detalles { get; set; } = new List<RecepcionDetalle>();
    }
}
