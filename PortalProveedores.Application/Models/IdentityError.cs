using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Application.Models
{
    public class IdentityError
    {
        // Un código que puede ser usado para identificar el tipo de error (opcional)
        public string Code { get; set; }

        // Mensaje descriptivo del error para el usuario
        public string Description { get; set; }
    }
}
