using PortalProveedores.Mobile.Views;

namespace PortalProveedores.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registrar rutas para navegación (Patrón de Microsoft)
            // learn.microsoft.com/dotnet/maui/fundamentals/shell/navigation

            // Aunque LoginPage se usa al inicio, la registramos por si 
            // necesitamos navegar a ella (ej. "Cerrar Sesión").
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));

            // La RecepcionPage ya está definida en el XAML (Route="RecepcionPage")
            // pero podemos registrar otras páginas aquí si las hubiera.
        }
    }
}
