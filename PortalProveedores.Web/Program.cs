using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using PortalProveedores.Web.Auth;
using PortalProveedores.Web.Components;
using PortalProveedores.Web.Handlers;
using PortalProveedores.Web.Services;
using System.Text;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar servicios de Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 2. Configurar HttpClientFactory (para llamar a la API)
// Esto permite a los servicios inyectar IHttpClientFactory
builder.Services.AddHttpClient();

// 3. Configurar la autorización y el proveedor de estado
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();

// 4. Registrar el almacén de tokens (Scoped: vive lo que dura el circuito del usuario)
builder.Services.AddScoped<ITokenStorageService, TokenStorageService>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["Jwt:Key"]!;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    }; ;
});
builder.Services.AddScoped<IAuthorizationHandler, HasNoOperationalRoleHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("NoOperationalRolePolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new HasNoOperationalRoleRequirement());
    });
});
builder.Services.AddHttpContextAccessor();

// 5. Registrar los servicios de negocio
builder.Services.AddScoped<AuthorizationTokenHandler>();
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddHttpClient("ApiGateway", client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                  ?? throw new InvalidOperationException("ApiSettings:BaseUrl no está configurado.");
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<AuthorizationTokenHandler>();
builder.Services.AddScoped<ITokenStorageService, TokenStorageService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProviderService, ProviderService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Habilita el Antiforgery (importante en .NET 8 Blazor Server)
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
