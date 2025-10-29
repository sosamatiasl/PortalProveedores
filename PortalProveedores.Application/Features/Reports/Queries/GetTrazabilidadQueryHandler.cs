using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Enums;
using System.Linq;

namespace PortalProveedores.Application.Features.Reports.Queries
{
    public class GetTrazabilidadQueryHandler : IRequestHandler<GetTrazabilidadQuery, TrazabilidadDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetTrazabilidadQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<TrazabilidadDto> Handle(GetTrazabilidadQuery request, CancellationToken cancellationToken)
        {
            // 1. Seguridad: Solo Rol A (Admin Cliente) o E (Admin Proveedor) acceden a reportes consolidados.
            if (!_currentUser.IsInRole("AdministrativoCliente") && !_currentUser.IsInRole("AdministrativoProveedor"))
            {
                throw new UnauthorizedAccessException("Acceso denegado a reportes consolidados.");
            }

            // 2. Consulta base: Se unifican los datos
            var resultado = await (
                from oc in _context.OrdenesCompra
                    // Obtener las cotizaciones aceptadas para esta OC
                join cotizacion in _context.Cotizaciones.Where(c => c.Estado == EstadoCotizacion.Aceptada)
                    on oc.Id equals cotizacion.OrdenCompraId into cotizacionesGroup
                from cot in cotizacionesGroup.DefaultIfEmpty() // LEFT JOIN para OCs sin cotizaciones aceptadas

                    // Obtener el remito asociado a esas cotizaciones
                join cotRemito in _context.CotizacionRemitos on cot.Id equals cotRemito.CotizacionId into cotRemitoGroup
                from cr in cotRemitoGroup.DefaultIfEmpty()
                join remito in _context.Remitos on cr.RemitoId equals remito.Id into remitoGroup
                from r in remitoGroup.DefaultIfEmpty()

                    // Obtener la factura asociada al remito
                join facRemito in _context.FacturaRemitos on r.Id equals facRemito.RemitoId into facRemitoGroup
                from fr in facRemitoGroup.DefaultIfEmpty()
                join factura in _context.Facturas on fr.FacturaId equals factura.Id into facturaGroup
                from f in facturaGroup.DefaultIfEmpty()

                    // Obtener el proveedor (solo si la cotización existe)
                join proveedor in _context.Proveedores on cot.ProveedorId equals proveedor.Id into proveedorGroup
                from p in proveedorGroup.DefaultIfEmpty()

                    // Filtroar por el documento solicitado
                where (request.TipoDocumento == TipoDocumento.OrdenCompra && oc.Id == request.DocumentoId) ||
                      (request.TipoDocumento == TipoDocumento.Remito && r.Id == request.DocumentoId) ||
                      (request.TipoDocumento == TipoDocumento.Factura && f.Id == request.DocumentoId)

                select new TrazabilidadDto
                {
                    OrdenCompraId = oc.Id,
                    NumeroOrden = oc.NumeroOrden,
                    ProveedorNombre = p.RazonSocial ?? "N/A",
                    EstadoOC = oc.Estado,

                    RemitoId = r.Id,
                    NumeroRemito = r.NumeroRemito,
                    EstadoRemito = r.Estado,
                    RemitoConDiferencias = r.Recepcion != null ? r.Recepcion.HuboDiferencias : false,

                    FacturaId = f.Id,
                    NumeroFactura = f.F3_NumeroFactura,
                    EstadoFactura = f.Estado,
                    FacturaValidaAFIP = f.EsValidaAFIP
                }
            ).FirstOrDefaultAsync(cancellationToken);

            if (resultado == null)
            {
                throw new Exception($"Documento de Tipo {request.TipoDocumento} con ID {request.DocumentoId} no encontrado.");
            }

            // 3. Devolver el resultado (se usa .Distinct() o GroupBy en un caso real para evitar duplicados por los joins)
            return resultado;
        }
    }
}
