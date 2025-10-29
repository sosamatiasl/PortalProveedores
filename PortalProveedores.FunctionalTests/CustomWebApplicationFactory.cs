using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PortalProveedores.API;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Infrastructure.Persistence;
using PortalProveedores.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace PortalProveedores.FunctionalTests
{
    /// <summary>
    /// WebApplicationFactory personalizado para configurar el entorno de pruebas.
    /// Permite reemplazar servicios reales por mocks (ej: la base de datos o servicios externos).
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        // Mocks de servicios externos que no se desean ejecutar en la prueba E2E
        public Mock<IFileStorageService> FileStorageServiceMock { get; } = new();
        public Mock<IAFIPService> AfipServiceMock { get; } = new();
        public Mock<IOCRService> OcrServiceMock { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // 1. Reemplazar DbContext para usar una base de datos en memoria
                services.RemoveAll(typeof(DbContextOptions<PortalProveedoresDbContext>));
                services.AddDbContext<PortalProveedoresDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString());
                });

                // 2. Reemplazar los servicios externos con Mocks
                services.RemoveAll<IFileStorageService>();
                services.AddSingleton(FileStorageServiceMock.Object);

                services.RemoveAll<IAFIPService>();
                services.AddSingleton(AfipServiceMock.Object);

                services.RemoveAll<IOCRService>();
                services.AddSingleton(OcrServiceMock.Object);

                // 3. Crear roles y usuarios iniciales para la prueba (E2E)
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<PortalProveedoresDbContext>();
                var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();

                db.Database.EnsureCreated();

                // Inicializar Roles (si no existen)
                if (!roleManager.RoleExistsAsync("AdministrativoCliente").Result)
                {
                    roleManager.CreateAsync(new IdentityRole("AdministrativoCliente")).Wait();
                    roleManager.CreateAsync(new IdentityRole("Vendedor")).Wait();
                    roleManager.CreateAsync(new IdentityRole("RecepcionadorMercaderia")).Wait();
                    roleManager.CreateAsync(new IdentityRole("DespachanteMercaderia")).Wait();
                }

                // Inicializar datos semilla (Clientes, Proveedores, etc.)
                SeedData.Initialize(db, roleManager).Wait();
            });
        }
    }

    // Clase simplificada para inicializar datos base
    public static class SeedData
    {
        public static async Task Initialize(PortalProveedoresDbContext context, RoleManager<IdentityRole> roleManager)
        {
            // 1. Crear Entidades (simuladas)
            var cliente = new Domain.Entities.Cliente { Id = 1, RazonSocial = "Cliente Test E2E" };
            var proveedor = new Domain.Entities.Proveedor { Id = 1, RazonSocial = "Proveedor Test E2E" };

            await context.Clientes.AddAsync(cliente);
            await context.Proveedores.AddAsync(proveedor);
            await context.SaveChangesAsync();

            // 2. Crear usuarios de prueba (se necesita un UserManager real para esto)
            // Por simplicidad, este paso se omite o se usa un UserManager mock. 
            // En una prueba E2E, se usarían endpoints de registro y login reales.
        }
    }
}
