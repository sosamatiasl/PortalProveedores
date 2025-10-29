using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Entities;
using PortalProveedores.Domain.Enums;
using System.Security.Claims;

namespace PortalProveedores.Application.Features.Remitos.Commands
{
    /// <summary>
    /// Crear el remito, subir PDF y asociar cotizaciones.
    /// </summary>
    public class CrearRemitoCommand : IRequest<long> // Devuelve el ID del nuevo Remito
    {
        public string NumeroRemito { get; set; } = string.Empty;
        public IFormFile ArchivoPDF { get; set; } = null!; // PDF, foto o escaneo

        // IDs de las Cotizaciones (Estado = Aceptada) que cubre este remito
        public List<long> CotizacionIds { get; set; } = new();
    }
}
