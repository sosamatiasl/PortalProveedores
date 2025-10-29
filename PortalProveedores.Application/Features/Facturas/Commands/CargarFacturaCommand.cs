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

namespace PortalProveedores.Application.Features.Facturas.Commands
{
    /// <summary>
    /// Carga, Extracción, Validación y Guardado.
    /// </summary>
    public class CargarFacturaCommand : IRequest<long> // Devuelve el ID de la nueva Factura
    {
        public IFormFile ArchivoPDF { get; set; } = null!; // Factura en PDF

        // IDs de los Remitos YA RECIBIDOS que cubre esta Factura
        public List<long> RemitoIds { get; set; } = new();
    }
}
