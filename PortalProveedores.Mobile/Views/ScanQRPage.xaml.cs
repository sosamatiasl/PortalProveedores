using PortalProveedores.Mobile.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace PortalProveedores.Mobile.Views;

public partial class ScanQRPage : ContentPage
{
    private readonly ScanQRViewModel _viewModel;

    public ScanQRPage(ScanQRViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    // Al salir de la página, hay que asegurarse de que el escaneo esté activo 
    // en caso de que volvamos a ella (sólo si no estamos cargando)
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.ResumeScanningCommand.Execute(null);
    }
}