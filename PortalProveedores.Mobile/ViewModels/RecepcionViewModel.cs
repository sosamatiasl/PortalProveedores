using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Threading;

namespace PortalProveedores.Mobile.ViewModels
{
    public partial class RecepcionViewModel : BaseViewModel
    {
        // Esta propiedad se enlaza (Binding) al DrawingView en el XAML
        // public DrawingView MyDrawingView { get; set; } 
        // (Nota: Es mejor pasar el DrawingView como parámetro del comando)

        public RecepcionViewModel()
        {
            // Constructor
        }

        [RelayCommand]
        private async Task ConfirmarRecepcionAsync(DrawingView drawingView)
        {
            if (drawingView == null || IsBusy) return;

            IsBusy = true;
            try
            {
                // --- SOLUCIÓN ERROR 37 ---
                // Usamos la sobrecarga de 3 argumentos: (Width, Height, CancellationToken)
                // El formato (PNG) y el color deben estar preconfigurados en el XAML.
                using var stream = await drawingView.GetImageStream(
                    drawingView.Width, // 1. Ancho
                    drawingView.Height, // 2. Alto
                    default(CancellationToken)); // 3. CancellationToken

                if (stream != null && stream.Length > 0)
                {
                    stream.Position = 0;

                    // Convertir a Base64
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    byte[] bytes = memoryStream.ToArray();
                    string base64String = Convert.ToBase64String(bytes);

                    // Aquí enviarías el base64String al ApiClient
                    // await _apiClient.EnviarRecepcionAsync(base64String);

                    await Application.Current.MainPage.DisplayAlert("Éxito", "Firma enviada", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo guardar la firma: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
