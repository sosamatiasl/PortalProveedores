using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Enums;

namespace PortalProveedores.Application.Features.Remitos.Queries
{
    /// <summary>
    /// Query: Valida el QR y devuelve los items esperados.
    /// </summary>
    // --- DTOs de Respuesta para la App Móvil ---
    public class RemitoItemEsperadoDto
    {
        public string Sku { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal CantidadDeclarada { get; set; }
    }

    public class RemitoParaRecepcionDto
    {
        public long RemitoId { get; set; }
        public string NumeroRemito { get; set; } = string.Empty;
        public string ProveedorNombre { get; set; } = string.Empty;
        public List<RemitoItemEsperadoDto> ItemsEsperados { get; set; } = new();
    }

    // --- Query ---
    public class GetRemitoParaRecepcionQuery : IRequest<RemitoParaRecepcionDto>
    {
        // El token (string) contenido en el QR
        public string QrToken { get; set; } = string.Empty;
    }
}
