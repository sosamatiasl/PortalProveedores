using Microsoft.AspNetCore.Components.Authorization;
using PortalProveedores.Web.Auth;
using PortalProveedores.Web.Components;
using PortalProveedores.Web.Services;

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

// 4. Registrar nuestro almacén de tokens (Scoped: vive lo que dura el circuito del usuario)
builder.Services.AddScoped<TokenStorageService>();

// 5. Registrar los servicios de negocio
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

// Habilita el Antiforgery (importante en .NET 8 Blazor Server)
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
