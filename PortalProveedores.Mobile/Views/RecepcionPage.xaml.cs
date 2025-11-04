using PortalProveedores.Mobile.ViewModels;
using ZXing.Net.Maui;

namespace PortalProveedores.Mobile.Views;

public partial class RecepcionPage : ContentPage
{
    public RecepcionPage(RecepcionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        // Lógica para manejar el QR detectado
        var qrCode = e.Results?.FirstOrDefault()?.Value;
        if (!string.IsNullOrEmpty(qrCode))
        {
            // (Opcional) Detener el scanner
            barcodeReader.IsDetecting = false;

            // Enviar el QR al ViewModel
            // (viewModel.ProcesarQRCommand.Execute(qrCode));
        }
    }
}