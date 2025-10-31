using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PortalProveedores.Mobile.Models;
using PortalProveedores.Mobile.Services;
using PortalProveedores.Mobile.Views;

namespace PortalProveedores.Mobile.ViewModels
{
    public partial class ScanQRViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;

        // Observable: Usado por la vista para saber si la cámara está activa
        [ObservableProperty]
        bool isScanning = true;

        public ScanQRViewModel(IApiService apiService)
        {
            _apiService = apiService;
            Title = "Escanear Remito";
        }

        /// <summary>
        /// Comando que se ejecuta automáticamente cuando el lector de QR identifica un código.
        /// </summary>
        [RelayCommand]
        async Task BarcodesDetected(string qrToken)
        {
            if (IsLoading || !IsScanning) return; // Evitar múltiples detecciones

            IsScanning = false; // Detener la cámara
            IsLoading = true;

            await Shell.Current.DisplayAlert("QR Detectado", $"Token: {qrToken}", "OK");

            try
            {
                // 1. Llamar a la API para validar el token y obtener los detalles
                var remitoDetails = await _apiService.ValidarQrRecepcionAsync(qrToken);

                // 2. Si es válido, navegar a la página de Recepción de Mercadería
                // Pasamos los detalles del Remito como parámetro a la siguiente vista.

                var navigationParameters = new Dictionary<string, object>
            {
                { "RemitoDetails", remitoDetails }
            };

                // Navegar usando una ruta registrada (ver AppShell)
                await Shell.Current.GoToAsync(nameof(RecepcionPage), navigationParameters);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error de Remito", ex.Message, "OK");
                IsScanning = true; // Si falla, se vuelve a escanear
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Método para reanudar el escaneo si se cancela o falla
        [RelayCommand]
        public void ResumeScanning()
        {
            IsScanning = true;
        }
    }
}
