using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Entities;
using PortalProveedores.Domain.Entities.Identity;
using System.Reflection;
using System.Security.Principal;

namespace PortalProveedores.Infrastructure.Persistence
{
    // Especificamos todas las clases personalizadas que Identity debe usar
    public class PortalProveedoresDbContext : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        string, // PK de Usuario (string)
        IdentityUserClaim<string>,
        UsuarioRol, // Clase de unión N-N
        IdentityUserLogin<string>,
        IdentityRoleClaim<string>, // PK de RolClaim (int) (cambiado a <string> para coincidir con ApplicationRole)
        IdentityUserToken<string>
    >, IApplicationDbContext
    {
        // Tablas del script SQL que no son de Identity (ejemplo)
        // public DbSet<Proveedor> Proveedores { get; set; } = null!;
        // public DbSet<Cliente> Clientes { get; set; } = null!;
        // public DbSet<OrdenCompra> OrdenesCompra { get; set; } = null!;
        // ...y todas las demás (Cotizaciones, Remitos, Facturas, etc.)

        public PortalProveedoresDbContext(DbContextOptions<PortalProveedoresDbContext> options)
            : base(options)
        {
        }


        // DBSETS DEL DOMINIO DE NEGOCIO
        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<Proveedor> Proveedores { get; set; } = null!;
        public DbSet<OrdenCompra> OrdenesCompra { get; set; } = null!;
        public DbSet<OrdenCompraItem> OrdenCompraItems { get; set; } = null!;
        public DbSet<Cotizacion> Cotizaciones { get; set; } = null!;
        public DbSet<CotizacionItem> CotizacionItems { get; set; } = null!;
        public DbSet<Remito> Remitos { get; set; } = null!;
        public DbSet<CotizacionRemitos> CotizacionRemitos { get; set; } = null!;
        public DbSet<RemitoQRCode> RemitoQRCodes { get; set; } = null!;
        public DbSet<Recepcion> Recepciones { get; set; } = null!;
        public DbSet<RecepcionDetalle> RecepcionDetalles { get; set; } = null!;
        public DbSet<Factura> Facturas { get; set; } = null!;
        public DbSet<FacturaDetalle> FacturaDetalles { get; set; } = null!;
        public DbSet<FacturaRemitos> FacturaRemitos { get; set; } = null!;


        // Seguridad - Refresh Tokens
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;


        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            // La implementación delega correctamente en la propiedad 'Database' de DbContext
            return Database.BeginTransactionAsync(cancellationToken);
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Mapeo explícito de las tablas de Identity a los nombres de tabla en la BD
            builder.Entity<ApplicationUser>(b =>
            {
                // Aplicar configuraciones de Identity
                b.ToTable("Usuarios");
                // Definir las relaciones con Cliente y Proveedor
                b.HasOne(u => u.Proveedor).WithMany().HasForeignKey(u => u.ProveedorId).IsRequired(false);
                b.HasOne(u => u.Cliente).WithMany().HasForeignKey(u => u.ClienteId).IsRequired(false);
            });

            builder.Entity<ApplicationRole>(b =>
            {
                b.ToTable("Roles");
            });

            builder.Entity<UsuarioRol>(b =>
            {
                b.ToTable("UsuarioRoles");
                // Definir la clave primaria compuesta
                b.HasKey(ur => new { ur.UserId, ur.RoleId });

                // Definir las relaciones
                b.HasOne(ur => ur.Rol)
                    .WithMany(r => r.UsuarioRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                b.HasOne(ur => ur.Usuario)
                    .WithMany(u => u.UsuarioRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            // Mapeo del resto de tablas de Identity
            builder.Entity<IdentityUserClaim<string>>(b => b.ToTable("UsuarioClaims"));
            builder.Entity<IdentityUserLogin<string>>(b => b.ToTable("UsuarioLogins"));
            builder.Entity<IdentityUserToken<string>>(b => b.ToTable("UsuarioTokens"));
            builder.Entity<IdentityRoleClaim<int>>(b => b.ToTable("RolClaims"));

            // Aquí iría el resto de la configuración de las entidades de negocio
            // (OrdenesCompra, Cotizaciones, etc.)

            // Orden de Compra
            builder.Entity<OrdenCompra>(b =>
            {
                b.ToTable("OrdenesCompra"); // Coincide con el script
                b.HasKey(oc => oc.Id);
                b.Property(oc => oc.NumeroOrden).HasMaxLength(100).IsRequired();

                b.HasOne(oc => oc.Cliente)
                    .WithMany() // Un cliente puede tener muchas OCs
                    .HasForeignKey(oc => oc.ClienteId)
                    .OnDelete(DeleteBehavior.NoAction); // Evitar borrado en cascada

                b.HasOne(oc => oc.Proveedor)
                    .WithMany() // Un proveedor puede tener muchas OCs
                    .HasForeignKey(oc => oc.ProveedorId)
                    .OnDelete(DeleteBehavior.NoAction);

                // 1:N con Items
                b.HasMany(oc => oc.Items)
                    .WithOne(item => item.OrdenCompra)
                    .HasForeignKey(item => item.OrdenCompraId)
                    .OnDelete(DeleteBehavior.Cascade); // Si se borra la OC, se borran sus items
            });

            builder.Entity<OrdenCompraItem>(b =>
            {
                b.ToTable("OrdenCompraItems"); // Nueva tabla
                b.HasKey(i => i.Id);
                b.Property(i => i.Descripcion).HasMaxLength(500).IsRequired();
                b.Property(i => i.Cantidad).HasColumnType("decimal(18,2)");
            });

            // Cotización
            builder.Entity<Cotizacion>(b =>
            {
                b.ToTable("Cotizaciones"); // Coincide con el script
                b.HasKey(c => c.Id);

                // Relación 1:N (OC -> Cotización)
                b.HasOne(c => c.OrdenCompra)
                    .WithMany(oc => oc.Cotizaciones) // Una OC tiene N Cotizaciones
                    .HasForeignKey(c => c.OrdenCompraId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.Property(c => c.MontoTotal).HasColumnType("decimal(18,2)");

                // 1:N con Items
                b.HasMany(c => c.Items)
                    .WithOne(item => item.Cotizacion)
                    .HasForeignKey(item => item.CotizacionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CotizacionItem>(b =>
            {
                b.ToTable("CotizacionItems"); // Nueva tabla
                b.HasKey(i => i.Id);
                b.Property(i => i.Descripcion).HasMaxLength(500).IsRequired();
                b.Property(i => i.Cantidad).HasColumnType("decimal(18,2)");
                b.Property(i => i.PrecioUnitario).HasColumnType("decimal(18,2)");
            });

            // Cliente
            builder.Entity<Cliente>(b =>
            {
                b.ToTable("Clientes"); // Coincide con el script
                b.HasKey(c => c.Id);
                b.Property(c => c.RazonSocial).HasMaxLength(200).IsRequired();
            });

            // Proveedor
            builder.Entity<Proveedor>(b =>
            {
                b.ToTable("Proveedores"); // Coincide con el script
                b.HasKey(p => p.Id);
                b.Property(p => p.RazonSocial).HasMaxLength(200).IsRequired();
            });

            // Remito
            builder.Entity<Remito>(b =>
            {
                b.ToTable("Remitos"); // Coincide con el script
                b.HasKey(r => r.Id);
                b.Property(r => r.NumeroRemito).HasMaxLength(100).IsRequired();

                b.HasOne(r => r.Proveedor).WithMany().HasForeignKey(r => r.ProveedorId).OnDelete(DeleteBehavior.NoAction);
                b.HasOne(r => r.Cliente).WithMany().HasForeignKey(r => r.ClienteId).OnDelete(DeleteBehavior.NoAction);
            });

            // Relación N-N: Cotizacion <-> Remito
            builder.Entity<CotizacionRemitos>(b =>
            {
                b.ToTable("CotizacionRemitos"); // Coincide con el script
                b.HasKey(cr => new { cr.CotizacionId, cr.RemitoId }); // Clave primaria compuesta

                b.HasOne(cr => cr.Cotizacion)
                    .WithMany() // Una cotización puede (parcialmente) estar en N remitos
                    .HasForeignKey(cr => cr.CotizacionId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(cr => cr.Remito)
                    .WithMany(r => r.CotizacionRemitos) // Un remito tiene N relaciones
                    .HasForeignKey(cr => cr.RemitoId)
                    .OnDelete(DeleteBehavior.Cascade); // Si se borra el remito, se borra la relación
            });

            // Remito QR Code
            builder.Entity<RemitoQRCode>(b =>
            {
                b.ToTable("RemitoQRCodes"); // Coincide con el script
                b.HasKey(qr => qr.Id);
                b.HasIndex(qr => qr.CodigoHash).IsUnique(); // El token DEBE ser único

                b.HasOne(qr => qr.Remito)
                    .WithMany(r => r.QRCodes) // Un remito puede tener N QRs (si expiran)
                    .HasForeignKey(qr => qr.RemitoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Recepcion
            builder.Entity<Recepcion>(b =>
            {
                b.ToTable("Recepciones"); // Coincide con el script
                b.HasKey(r => r.Id);

                //Relación 1:1 con Remito
                b.HasOne(r => r.Remito)
                    .WithOne(rem => rem.Recepcion) // Un remito solo tiene una recepción
                    .HasForeignKey<Recepcion>(r => r.RemitoId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(r => r.UsuarioRecepcion)
                    .WithMany()
                    .HasForeignKey(r => r.UsuarioRecepcionId)
                    .OnDelete(DeleteBehavior.NoAction);

                // 1:N con Detalles
                b.HasMany(r => r.Detalles)
                    .WithOne(d => d.Recepcion)
                    .HasForeignKey(d => d.RecepcionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // RecepcionDetalle
            builder.Entity<RecepcionDetalle>(b =>
            {
                b.ToTable("RecepcionDetalles"); // Coincide con el script
                b.HasKey(rd => rd.Id);
                b.Property(rd => rd.CantidadDeclarada).HasColumnType("decimal(18,2)");
                b.Property(rd => rd.CantidadRecibida).HasColumnType("decimal(18,2)");
            });

            // Factura
            builder.Entity<Factura>(b =>
            {
                b.ToTable("Facturas");
                b.HasKey(f => f.Id);

                b.Property(f => f.F5_MontoTotal).HasColumnType("decimal(18,2)");
                b.Property(f => f.F6_MontoIVA).HasColumnType("decimal(18,2)");

                // 1:N con Detalles
                b.HasMany(f => f.Detalles)
                    .WithOne(d => d.Factura)
                    .HasForeignKey(d => d.FacturaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // FacturaDetalle
            builder.Entity<FacturaDetalle>(b =>
            {
                b.ToTable("FacturaDetalles");
                b.HasKey(fd => fd.Id);
                b.Property(fd => fd.Cantidad).HasColumnType("decimal(18,2)");
                b.Property(fd => fd.PrecioUnitario).HasColumnType("decimal(18,2)");
                b.Property(fd => fd.Subtotal).HasColumnType("decimal(18,2)");
            });

            // Relación N-N: Factura <-> Remito
            builder.Entity<FacturaRemitos>(b =>
            {
                b.ToTable("FacturaRemitos");
                b.HasKey(fr => new { fr.FacturaId, fr.RemitoId }); // Clave primaria compuesta

                b.HasOne(fr => fr.Factura)
                    .WithMany(f => f.FacturaRemitos)
                    .HasForeignKey(fr => fr.FacturaId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(fr => fr.Remito)
                    .WithMany()
                    .HasForeignKey(fr => fr.RemitoId)
                    .OnDelete(DeleteBehavior.NoAction); // No se borra el Remito si se borra la Factura
            });

            // --- Configuración de RefreshToken ---
            builder.Entity<RefreshToken>(b =>
            {
                b.ToTable("RefreshTokens");
                b.HasKey(rt => rt.Id);

                b.HasOne(rt => rt.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade); // Si el usuario es borrado, sus tokens se borran
            });

            // Cargar los Roles (movido aquí desde el script SQL para que EF lo maneje)
            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole { Id = "1", Name = "AdministrativoCliente", NormalizedName = "ADMINISTRATIVOCLIENTE", Descripcion = "Rol A" },
                new ApplicationRole { Id = "2", Name = "AdministrativoProveedor", NormalizedName = "ADMINISTRATIVOPROVEEDOR", Descripcion = "Rol B" }
                //... (etc)
            );
        }

        // Sobre-escritura de SaveChangesAsync para que IApplicationDbContext funcione
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
