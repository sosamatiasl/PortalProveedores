using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PortalProveedores.Mobile.ViewModels
{
    // Se usa ObservableObject para notificar cambios a la UI
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLoading))]
        bool isLoading;

        public bool IsNotLoading => !isLoading;

        [ObservableProperty]
        string title = string.Empty;
    }
}
