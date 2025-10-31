using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Entities;
using PortalProveedores.Domain.Enums;

namespace PortalProveedores.Application.Features.Ajustes.Commands
{
    public class GenerarNotaAjusteCommand : IRequest<long> // Devuelve el ID de la Nota
    {
        public long FacturaId { get; set; }
        public string MotivoDetallado { get; set; } = string.Empty; // Razón manual
    }
}
