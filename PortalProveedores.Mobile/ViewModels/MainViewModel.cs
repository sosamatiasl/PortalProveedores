using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PortalProveedores.Mobile.Services;

namespace PortalProveedores.Mobile.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        private readonly ISynchronizationService _syncService;

        [ObservableProperty]
        string syncStatus = "Última sincronización: N/A";

        public MainViewModel(ISynchronizationService syncService)
        {
            _syncService = syncService;
            Title = "Dashboard";
        }

        /// <summary>
        /// Comando para la Sincronización Manual (botón en la UI).
        /// </summary>
        [RelayCommand]
        async Task RunManualSynchronizationAsync()
        {
            if (IsLoading) return;

            IsLoading = true;
            SyncStatus = "Sincronizando, por favor espere...";

            try
            {
                var count = await _syncService.SynchronizePendingRemitosAsync();

                if (count > 0)
                {
                    SyncStatus = $"Última sincronización exitosa: {DateTime.Now:HH:mm:ss}. Remitos enviados: {count}";
                    await Shell.Current.DisplayAlert("Sincronización", $"{count} remitos pendientes sincronizados.", "OK");
                }
                else
                {
                    SyncStatus = $"Última sincronización: {DateTime.Now:HH:mm:ss}. No hay remitos pendientes.";
                    await Shell.Current.DisplayAlert("Sincronización", "No hay remitos pendientes de enviar.", "OK");
                }
            }
            catch (Exception ex)
            {
                SyncStatus = $"Sincronización fallida: {ex.Message}";
                await Shell.Current.DisplayAlert("Error", $"Sincronización fallida: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
