using MediatR;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Entities;
using PortalProveedores.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Application.Features.Compras.Commands
{
    //// DTO para los items
    //public record OrdenCompraItemDto
    //(
    //    long ProductoId,
    //    decimal Cantidad
    //    // La unidad de Medida se toma del catálogo
    //);

    /// <summary>
    /// Crear una nueva Orden de Compra.
    /// </summary>
    public class CrearOrdenCompraCommand : IRequest<long> // Devuelve el Id de la nueva OC
    {
        public long ProveedorId { get; set; } // El Proveedor al que se le emite
        public string NumeroOrden { get; set; } = string.Empty;
        public string? Detalles { get; set; }
        public List<OrdenCompraItemDto> Items { get; set; } = new();

        // DTO para los items
        public record OrdenCompraItemDto
        (
            long ProductoId,
            decimal Cantidad
        // La unidad de Medida se toma del catálogo
        );
    }
}
