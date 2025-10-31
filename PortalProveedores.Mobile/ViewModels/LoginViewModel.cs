using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PortalProveedores.Mobile.Models;
using PortalProveedores.Mobile.Services;

namespace PortalProveedores.Mobile.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;

        [ObservableProperty]
        string email = string.Empty;

        [ObservableProperty]
        string password = string.Empty;

        public LoginViewModel(IApiService apiService)
        {
            _apiService = apiService;
            Title = "Inicio de Sesión";
        }

        [RelayCommand]
        async Task LoginAsync()
        {
            if (IsLoading) return;
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlert("Error", "Debe ingresar Email y Contraseña.", "OK");
                return;
            }

            IsLoading = true;
            try
            {
                var request = new AuthRequest { Email = Email, Password = Password };
                var result = await _apiService.LoginAsync(request);

                if (result != null && result.Success)
                {
                    // ¡Éxito! Se navega a la página principal de la app
                    // La ruta "//MainPage" fue definida en AppShell.xaml
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Login Fallido", result?.ErrorMessage ?? "Error desconocido", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"No se pudo conectar: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
