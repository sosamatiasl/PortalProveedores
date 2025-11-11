using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Entities.Identity;
using PortalProveedores.Infrastructure.Persistence;
using PortalProveedores.Infrastructure.Services;
using System.Text;
using FluentValidation;
using MediatR;
using PortalProveedores.Application.Common.Behaviours;
using PortalProveedores.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using PortalProveedores.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// 1. Conexión a SQL Server
var connectionString = config.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PortalProveedoresDbContext>(options =>
    options.UseSqlServer(connectionString,
        // Habilitar "split queries" para mejorar performance en cargas complejas
        o =>
        {
            // 1. Configuración de Resiliencia (Reintento de Conexión)
            o.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null // Usar los códigos de error predeterminados
            );
            // 2. Configuración de Query Splitting (Se mantiene la optimización de rendimiento)
            o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        })
);

// 2. Configurar ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Se debe cambiar a 'true' para producción
})
    .AddEntityFrameworkStores<PortalProveedoresDbContext>()
    .AddDefaultTokenProviders(); // Habilita generación de tokens para reset de password, etc.

// 3. Configurar Autenticación (JWT y Externa)
// Añade Registro de servicios de la capa de infraestructura
//builder.Services.AddInfrastructure();

builder.Services.AddAuthentication(options =>
{
    // El esquema por defecto es JWT
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options => // Configuración del Token JWT
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
        };
    })
    .AddGoogle(options => // Configuración de Google
    {
        options.ClientId = config["Authentication:Google:ClientId"]!;
        options.ClientSecret = config["Authentication:Google:ClientSecret"]!;
        // Aquí se configuraría el callback path
    })
    .AddMicrosoftAccount(options => // Configuración de Microsoft
    {
        options.ClientId = config["Authentication:Microsoft:ClientId"]!;
        options.ClientSecret = config["Authentication:Microsoft:ClientSecret"]!;
        // Aquí se configuraría el callback path
    });

// 4. Inyección de Dependencias de los servicios propios
builder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();
builder.Services.AddHttpContextAccessor(); // Necesario para CurrentUserService
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<INotificationService, NotificationService>(); // Usamos la simulación
builder.Services.AddScoped<IJwtGeneratorService, JwtGeneratorService>();
builder.Services.AddScoped<IHubNotificationService, HubNotificationService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IConciliacionService, ConciliacionService>();
builder.Services.AddScoped<IOCRService, OCRService>();
builder.Services.AddScoped<IAFIPService, AFIPService>();

// Abstracción del DbContext
builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<PortalProveedoresDbContext>());

// 5. Registrar MediatR y FluentValidation
// Busca automáticamente todos los Handlers en el ensamblado de Application
builder.Services.AddValidatorsFromAssembly(typeof(PortalProveedores.Application.Common.Interfaces.IJwtGeneratorService).Assembly);
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(PortalProveedores.Application.Common.Interfaces.IJwtGeneratorService).Assembly);

    // Registrar el Pipeline de Validación
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

// 6. Servicios de API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 7. CORS (Permitir que la App Web y Móvil se conecten)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("https://localhost:7123", // URL del proyecto Blazor Web
                                "http://localhost:5123") // URL del proyecto Blazor Web
                  .AllowAnyHeader()
                  .AllowAnyMethod();
            // Para la app móvil, es posible que se necesiten configuraciones adicionales
        });
});

// --- Servicios SignalR ---
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>(); // Mapeo de usuario


// --- Construir la App ---
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Usar routing
app.UseRouting();

// Usar CORS
app.UseCors("AllowSpecificOrigins");

// ¡Importante! Authentication (quién es) DEBE ir ANTES de Authorization (qué se puede hacer)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// --- Mapeo de Endpoints SignalR ---
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    // Mapeo del Hub. La URL para conectarse será: /portalhub
    endpoints.MapHub<PortalHub>("/portalhub");
});

app.Run();

public partial class Program { } // Necesario para pruebas E2E