using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Enums;

namespace PortalProveedores.Application.Features.Cotizaciones.Commands
{
    /// <summary>
    /// Comando para la aceptación masiva.
    /// </summary>
    public class AceptarCotizacionesCommand : IRequest<Unit> // Devuelve 'Unit' (vacío) si es exitoso
    {
        public long OrdenCompraId { get; set; }

        // El núcleo de la "aceptación masiva"
        public List<long> CotizacionIdsAAceptar { get; set; } = new();
    }
}
