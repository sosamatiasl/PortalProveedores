using PortalProveedores.Mobile.ViewModels;

namespace PortalProveedores.Mobile.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}