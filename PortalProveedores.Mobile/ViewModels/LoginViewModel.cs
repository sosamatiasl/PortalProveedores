using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using PortalProveedores.Mobile.Services;
using PortalProveedores.Mobile.Views;

namespace PortalProveedores.Mobile.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                var success = await _authService.LoginAsync("user", "pass"); // Simulado

                if (success)
                {
                    // Navegar a la AppShell (página principal)
                    Application.Current.MainPage = new AppShell();
                }
                else
                {
                    // Mostrar alerta de error
                    await Application.Current.MainPage.DisplayAlert("Error", "Login fallido", "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
