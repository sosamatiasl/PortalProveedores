using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PortalProveedores.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Application.Common.Interfaces
{
    /// <summary>
    /// Se abstrae el DbContext para que la capa de Aplicación no dependa directamente de EF Core.
    /// </summary>
    public interface IApplicationDbContext
    {
        DbSet<OrdenCompra> OrdenesCompra { get; }
        DbSet<OrdenCompraItem> OrdenCompraItems { get; }
        DbSet<Cotizacion> Cotizaciones { get; }
        DbSet<CotizacionItem> CotizacionItems { get; }
        DbSet<Cliente> Clientes { get; }
        DbSet<Proveedor> Proveedores { get; }
        DbSet<Remito> Remitos { get; }
        DbSet<RemitoQRCode> RemitoQRCodes { get; }
        DbSet<CotizacionRemitos> CotizacionRemitos { get; }
        DbSet<Recepcion> Recepciones { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}
