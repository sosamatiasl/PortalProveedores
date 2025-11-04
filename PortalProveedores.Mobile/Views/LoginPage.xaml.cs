using PortalProveedores.Mobile.ViewModels;

namespace PortalProveedores.Mobile.Views;

public partial class LoginPage : ContentPage
{
    // Constructor modificado para DI (aunque el de App.xaml.cs usa AuthService)
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}