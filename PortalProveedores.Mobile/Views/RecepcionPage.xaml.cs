using PortalProveedores.Mobile.ViewModels;

namespace PortalProveedores.Mobile.Views;

public partial class RecepcionPage : ContentPage
{
    public RecepcionPage(RecepcionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}