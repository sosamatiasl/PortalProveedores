using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace PortalProveedores.Application.Features.Auth.Commands
{
    // IRequest<AuthResult> significa que este comando, al ser ejecutado, devolverá un objeto de tipo AuthResult.
    public class RegisterCommand : IRequest<AuthResult>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;

        // El archivo de la selfie
        public IFormFile SelfieFoto { get; set; } = null!;

        // ID de la empresa a la que pertenece
        public long? ProveedorId { get; set; }
        public long? ClienteId { get; set; }

        // Lista de nombres de roles, ej: ["AdministrativoProveedor", "Transportista"]
        public List<long> Roles { get; set; } = new List<long>();
    }

    // Objeto de respuesta para Login y Registro
    public record AuthResult(
        bool Success,
        string Token,
        long UserId,
        IEnumerable<long> Roles,
        string? ErrorMessage
    );
}
