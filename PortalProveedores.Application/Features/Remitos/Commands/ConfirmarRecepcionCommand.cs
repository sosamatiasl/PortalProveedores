    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Entities;
using PortalProveedores.Domain.Enums;
using System.Security.Claims;

namespace PortalProveedores.Application.Features.Remitos.Commands
{
    
    // --- DTO de entrada(lo que envía la App Móvil) ---
    public class RecepcionItemDto
    {
        public string Sku { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal CantidadDeclarada { get; set; }
        public decimal CantidadRecibida { get; set; }
    }

    /// <summary>
    /// Guarda la recepción, diferencias y firmas.
    /// </summary>
    public class ConfirmarRecepcionCommand : IRequest<long> // Devuelve el ID de la Recepción
    {
        public long RemitoId { get; set; }
        public string QrToken { get; set; } = string.Empty; // El token del QR escaneado
        public string? DetalleDiferencias { get; set; } // Comentarios

        // Firmas capturadas como strings Base64
        public string FirmaRecepcionistaBase64 { get; set; } = string.Empty;
        public string FirmaTransportistaBase64 { get; set; } = string.Empty;

        public List<RecepcionItemDto> ItemsRecibidos { get; set; } = new();
    }
}
