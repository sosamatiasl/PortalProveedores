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

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// 1. Conexi�n a SQL Server
var connectionString = config.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PortalProveedoresDbContext>(options =>
    options.UseSqlServer(connectionString,
        // Habilitar "split queries" para mejorar performance en cargas complejas
        o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
);

// 2. Configurar ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Se debe cambiar a 'true' para producci�n
})
    .AddEntityFrameworkStores<PortalProveedoresDbContext>()
    .AddDefaultTokenProviders(); // Habilita generaci�n de tokens para reset de password, etc.

// 3. Configurar Autenticaci�n (JWT y Externa)
builder.Services.AddAuthentication(options =>
{
    // El esquema por defecto es JWT
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options => // Configuraci�n del Token JWT
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
    .AddGoogle(options => // Configuraci�n de Google
    {
        options.ClientId = config["Authentication:Google:ClientId"]!;
        options.ClientSecret = config["Authentication:Google:ClientSecret"]!;
        // Aqu� se configurar�a el callback path
    })
    .AddMicrosoftAccount(options => // Configuraci�n de Microsoft
    {
        options.ClientId = config["Authentication:Microsoft:ClientId"]!;
        options.ClientSecret = config["Authentication:Microsoft:ClientSecret"]!;
        // Aqu� se configurar�a el callback path
    });

// 4. Inyecci�n de Dependencias de los servicios propios
builder.Services.AddScoped<IJwtGeneratorService, JwtGeneratorService>();
builder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();
builder.Services.AddHttpContextAccessor(); // Necesario para CurrentUserService
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<INotificationService, NotificationService>(); // Usamos la simulaci�n

// Abstracci�n del DbContext
builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<PortalProveedoresDbContext>());

// 5. Registrar MediatR y FluentValidation
// Busca autom�ticamente todos los Handlers en el ensamblado de Application
builder.Services.AddValidatorsFromAssembly(typeof(PortalProveedores.Application.Common.Interfaces.IJwtGeneratorService).Assembly);
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(PortalProveedores.Application.Common.Interfaces.IJwtGeneratorService).Assembly);

    // Registrar el Pipeline de Validaci�n
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

// 6. Servicios de API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 7. CORS (Permitir que la App Web y M�vil se conecten)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("https://localhost:7123", // URL del proyecto Blazor Web
                                "http://localhost:5123") // URL del proyecto Blazor Web
                  .AllowAnyHeader()
                  .AllowAnyMethod();
            // Para la app m�vil, es posible que se necesiten configuraciones adicionales
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

// Usar CORS
app.UseCors("AllowSpecificOrigins");

// �Importante! Authentication (qui�n es) DEBE ir ANTES de Authorization (qu� se puede hacer)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// --- Mapeo de Endpoints SignalR ---
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    // Mapeo del Hub. La URL para conectarse ser�: /portalhub
    endpoints.MapHub<PortalHub>("/portalhub");
});

app.Run();