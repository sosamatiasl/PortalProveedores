using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Maui;
using ZXing.Net.Maui.Controls;
using PortalProveedores.Mobile.Services;
using PortalProveedores.Mobile.ViewModels;
using PortalProveedores.Mobile.Views;
namespace PortalProveedores.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()

                // 1. Inicializar CommunityToolkit (Error 37)
                // (Documentado en learn.microsoft.com)
                .UseMauiCommunityToolkit()

                // 2. Inicializar ZXing (QR Scanner) (Error 37)
                // (Patrón de registro de Handler documentado por Microsoft)
                .UseBarcodeReader()

                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // 3. Registro de Servicios (Inyección de Dependencias)
            // (Documentado en learn.microsoft.com/dotnet/maui/fundamentals/dependency-injection)

            // Servicios de la App (Singleton para HttpClient y Auth)
            builder.Services.AddSingleton<IApiClient, ApiClient>();
            builder.Services.AddSingleton<IAuthService, AuthService>();

            // ViewModels (Transitorios, uno por página)
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RecepcionViewModel>();

            // Vistas (Pages) (Transitorios)
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RecepcionPage>();

            // OMITIDO: El registro de Shiny (SynchronizationJob, UseShiny, ShinyStartup)
            // se omite por no ser del dominio de Microsoft.

            return builder.Build();
        }
    }
}