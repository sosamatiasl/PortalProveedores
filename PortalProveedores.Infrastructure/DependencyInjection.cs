using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Infrastructure.Persistence;
using PortalProveedores.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Infrastructure
{
    /// <summary>
    /// Se registran los nuevos servicios simulados en el contenedor IoC.
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // ... (Configuración de DbContext y Servicios de Identidad) ...
            services.AddDbContext<PortalProveedoresDbContext>(options =>
               options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Registros de Servicios
            services.AddTransient<IOCRService, OCRService>(); // Servicio de Simulación OCR/IA
            services.AddTransient<IAFIPService, AFIPService>(); // Servicio de Simulación AFIP
            services.AddScoped<IAuthService, AuthService>();

            // ... (Otros servicios: ICurrentUserService, IDateTime, INotificationService, etc.) ...

            return services;
        }
    }
}
