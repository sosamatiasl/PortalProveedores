using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Entities;
using System.Security.Claims;

namespace PortalProveedores.Application.Features.Productos.Commands
{
    public class CrearProductoCommand : IRequest<long> // Devuelve el ID del producto
    {
        public string Sku { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
    }
}
