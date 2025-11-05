using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Application.Models
{
    public class IdentityResult
    {
        // Propiedad clave: indica si la operación fue exitosa
        public bool Succeeded { get; protected set; }

        // Lista de errores en caso de fallo
        public IEnumerable<IdentityError> Errors { get; protected set; }

        // Constructor privado para forzar el uso de los métodos estáticos
        protected IdentityResult(bool succeeded, IEnumerable<IdentityError> errors)
        {
            Succeeded = succeeded;
            Errors = errors ?? Enumerable.Empty<IdentityError>();
        }

        // --- MÉTODOS ESTÁTICOS DE FÁBRICA ---

        // Método estático para devolver un resultado exitoso
        public static IdentityResult Success
        {
            get => new IdentityResult(true, null);
        }

        // Método estático para devolver un resultado fallido
        public static IdentityResult Failed(params IdentityError[] errors)
        {
            return new IdentityResult(false, errors);
        }
    }
}
