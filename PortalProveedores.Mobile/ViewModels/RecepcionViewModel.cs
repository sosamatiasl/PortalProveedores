using Android.Graphics;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PortalProveedores.Mobile.Models;
using PortalProveedores.Mobile.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Mobile.ViewModels
{
    // Se implementa IQueryAttributable para recibir el parámetro de navegación
    [QueryProperty(nameof(RemitoDetails), "RemitoDetails")]
    public partial class RecepcionViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        RemitoParaRecepcionDto remitoDetails = new(); // Objeto del Remito a recibir

        [ObservableProperty]
        string nombreReceptor = string.Empty; // Nombre del Rol D que firma

        [ObservableProperty]
        // Colección observable para mostrar y permitir la edición de CantidadARecibir
        ObservableCollection<RecepcionItemDto> itemsRecepcion = new();

        public RecepcionViewModel(IApiService apiService, DatabaseService databaseService)
        {
            _apiService = apiService;
            _databaseService = databaseService;
            Title = "Confirmar Recepción";
        }

        /// <summary>
        /// Se llama automáticamente al navegar a esta página.
        /// </summary>
        partial void OnRemitoDetailsChanged(RemitoParaRecepcionDto value)
        {
            // Al recibir los detalles, se convierten en una colección observable
            // para que la UI pueda reflejar cambios en CantidadARecibir.
            ItemsRecepcion.Clear();
            foreach (var item in value.Items)
            {
                ItemsRecepcion.Add(item);
            }
            Title = $"Recibir Remito: {value.NumeroRemito}";
        }

        /// <summary>
        /// Convierte el dibujo del DrawingView en Base64 para enviarlo a la API.
        /// </summary>
        [RelayCommand]
        async Task ConfirmarRecepcionAsync(IDrawingView drawingView)
        {
            if (IsLoading) return;
            if (string.IsNullOrWhiteSpace(NombreReceptor))
            {
                await Shell.Current.DisplayAlert("Falta información", "Ingrese su nombre para registrar la recepción.", "OK");
                return;
            }

            // 1. Validar que la firma exista (al menos un punto)
            if (drawingView.Lines.Count == 0)
            {
                await Shell.Current.DisplayAlert("Firma Requerida", "Debe capturar su firma digital.", "OK");
                return;
            }

            string firmaBase64 = string.Empty;
            IsLoading = true;
            try
            {
                // 2. Convertir la firma a Base64
                // Se usa un stream para obtener la imagen (requiere un formato, ej: PNG)
                using var stream = await drawingView.GetImageStream(
                    drawingView.Width,
                    drawingView.Height,
                    shouldDisposeStream: true,
                    format: ImageFormat.Png);

                stream.Position = 0;

                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);

                firmaBase64 = Convert.ToBase64String(memoryStream.ToArray());

                // 3. Crear el comando para la API
                var command = new ConfirmarRecepcionCommand
                {
                    RemitoId = RemitoDetails.RemitoId,
                    NombreReceptor = NombreReceptor,
                    FirmaBase64 = firmaBase64,
                    // Se usa la colección editable para los datos finales
                    ItemsRecibidos = ItemsRecepcion.ToList()
                };

                // 4. Intentar enviar a la API (Conexión Online)
                var recepcionId = await _apiService.ConfirmarRecepcionAsync(command);

                await Shell.Current.DisplayAlert("Éxito", $"Recepción {recepcionId} registrada correctamente.", "OK");
            }
            catch (Exception ex) when (ex is HttpRequestException || ex.Message.Contains("No se pudo conectar"))
            {
                // 5.1. SI FALLA LA CONEXIÓN: Guardar en la base de datos local(Offline)

                // Re-crear el objeto local para guardar
                var remitoLocal = new RemitoLocal
                {
                    RemitoId = RemitoDetails.RemitoId,
                    NumeroRemito = RemitoDetails.NumeroRemito,
                    NombreProveedor = RemitoDetails.NombreProveedor,
                    NombreReceptor = NombreReceptor,
                    FirmaBase64 = firmaBase64,
                    ItemsRecibidos = ItemsRecepcion.ToList(),
                    Sincronizado = false // Marcado como Pendiente
                };

                await _databaseService.SaveRemitoAsync(remitoLocal);

                await Shell.Current.DisplayAlert("Guardado Offline",
                    "No hay conexión a la API. Los datos del remito se han guardado localmente y se sincronizarán al recuperar la conexión.",
                    "OK");
            }
            catch (Exception ex)
            {
                // 5.2 Otros errores (Ej: Firma o datos inválidos)
                await Shell.Current.DisplayAlert("Error", $"No se pudo confirmar: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;

                // 6. Volver a la página de escaneo (limpiar el stack)
                await Shell.Current.GoToAsync("//MainPage/ScanQRPage");
            }
        }

        [RelayCommand]
        public void LimpiarFirma(IDrawingView drawingView)
        {
            drawingView.Clear();
        }
    }
}
