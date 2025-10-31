using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using PortalProveedores.Mobile.Services;
using PortalProveedores.Mobile.ViewModels;
using PortalProveedores.Mobile.Views;
using ZXing.Net.Maui.Controls;
using Shiny;
using Microsoft.Extensions.DependencyInjection;

namespace PortalProveedores.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()

                // --- INICIALIZACIÓN DE PAQUETES ---
                .UseBarcodeReader() // (ZXing.Net.Maui.Controls)
                .UseMauiCommunityToolkit() // (CommunityToolkit.Maui)
                .UseShiny(jobs => // (Shiny)
                {
                    // Registrar el Job para que se ejecute en el background
                    jobs.Add(new JobInfo(typeof(SynchronizationJob), "RemitoSync")
                    {
                        // Activar la periodicidad
                        Repeat = true,
                        // Mínima periodicidad recomendada
                        RequiredInternetAccess = InternetAccess.Any,
                        // Usamos la duración mínima de 15 minutos
                        BatteryNotLow = true,
                        RepeatInterval = TimeSpan.FromMinutes(15)
                    });
                })

                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
#if DEBUG
            builder.Logging.AddDebug();
#endif

            // --- INYECCIÓN DE DEPENDENCIAS ---

            // 1. Configuración de HttpClient para la API
            builder.Services.AddSingleton<IApiService, ApiService>();
            builder.Services.AddHttpClient<IApiService, ApiService>(client =>
            {
                // ¡MUY IMPORTANTE!
                // Si la API (PortalProveedores.API) corre en https://localhost:7001
                // el emulador de Android NO puede acceder a "localhost".
                // Se debe usar la IP especial 10.0.2.2 para que el emulador
                // apunte al localhost de la máquina host.

                // Hay que asegurarse de que la API esté corriendo en HTTP para pruebas de emulador,
                // o configurar la confianza SSL en el emulador (lo cual es más complejo).

                // Se utiliza 10.0.2.2 apuntando al puerto de la API (ej: 5123 si es HTTP)
                client.BaseAddress = new Uri("http://10.0.2.2:5123/api/");
            });


            // 2.1 Registro de Vistas (Pages)
            builder.Services.AddSingleton<DatabaseService>(); // Servicio de base de datos local
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<LoadingPage>(); // Transient porque se usa poco
            builder.Services.AddTransient<ScanQRPage>(); // Transient para un uso flexible
            builder.Services.AddTransient<RecepcionPage>(); // Transient para recibir parámetros de navegación

            // 2.2 Registro de Servicios
            builder.Services.AddSingleton<ISynchronizationService, SynchronizationService>();

            // 3. Registro de ViewModels
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<MainViewModel>(); // (Se asume que existe)
            builder.Services.AddTransient<ScanQRViewModel>();
            builder.Services.AddTransient<RecepcionViewModel>();
            builder.Services.AddSingleton<MainViewModel>();


            return builder.Build();
        }
    }
}
