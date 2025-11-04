using PortalProveedores.Mobile.Services;
using PortalProveedores.Mobile.Views;

namespace PortalProveedores.Mobile
{
    public partial class App : Application
    {
        // El constructor recibe servicios registrados en MauiProgram.cs
        public App(IAuthService authService)
        {
            InitializeComponent();

            // Lógica de arranque (Patrón estándar de MAUI)
            if (authService.IsAuthenticated())
            {
                // Si está autenticado, la página principal es el Shell
                MainPage = new AppShell();
            }
            else
            {
                // Si no, la página principal es LoginPage
                // Resolvemos LoginPage desde el contenedor de DI
                MainPage = IPlatformApplication.Current.Services.GetService<LoginPage>();
            }
        }
    }
}
