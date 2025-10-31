using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace PortalProveedores.Application.Features.Productos.Commands
{
    public class AsignarPrecioProveedorCommand : IRequest<long> // Devuelve el ID del precio
    {
        public long ProductoId { get; set; }
        public long ProveedorId { get; set; }
        public decimal PrecioAcordado { get; set; }
        public DateTime FechaVigenciaDesde { get; set; }
        public DateTime? FechaVigenciaHasta { get; set; }
    }
}
