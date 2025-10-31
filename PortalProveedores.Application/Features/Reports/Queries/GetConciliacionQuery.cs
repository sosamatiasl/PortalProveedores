using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Enums;
using System.Collections.Generic;
using System.Linq;

namespace PortalProveedores.Application.Features.Reports.Queries
{
    // --- Query ---
    /// <summary>
    /// Query: Reporte de Conciliación a Tres Vías.
    /// </summary>
    public class GetConciliacionQuery : IRequest<ConciliacionReporteDto>
    {
        public long FacturaId { get; set; }
    }
}
