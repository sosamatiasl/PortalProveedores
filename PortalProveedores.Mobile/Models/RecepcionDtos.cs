using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace PortalProveedores.Mobile.Models
{
    // --------------------------------------------------------
    // DTOs de Recepción (Similares a los de la API)
    // --------------------------------------------------------

    /// <summary>
    /// Representa un ítem que se espera recibir.
    /// </summary>
    public class RecepcionItemDto
    {
        [JsonProperty("ordenCompraItemId")]
        public long OrdenCompraItemId { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; } = string.Empty;

        [JsonProperty("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [JsonProperty("cantidadSolicitada")]
        public decimal CantidadSolicitada { get; set; }

        [JsonProperty("cantidadPreviamenteRecibida")]
        public decimal CantidadPreviamenteRecibida { get; set; }

        // Cantidad que el personal de depósito ingresa.
        // Inicialmente igual a la solicitada, pero puede ser ajustada.
        [JsonProperty("cantidadARecibir")]
        public decimal CantidadARecibir { get; set; }
    }

    /// <summary>
    /// Respuesta del endpoint /validar-qr/{token} de la API.
    /// </summary>
    public class RemitoParaRecepcionDto
    {
        [JsonProperty("remitoId")]
        public long RemitoId { get; set; }

        [JsonProperty("numeroRemito")]
        public string NumeroRemito { get; set; } = string.Empty;

        [JsonProperty("nombreProveedor")]
        public string NombreProveedor { get; set; } = string.Empty;

        [JsonProperty("items")]
        public List<RecepcionItemDto> Items { get; set; } = new();
    }


    // --------------------------------------------------------
    // DTO para Confirmar la Recepción (POST a la API)
    // --------------------------------------------------------

    /// <summary>
    /// Objeto que se envía a la API para finalizar la recepción.
    /// </summary>
    public class ConfirmarRecepcionCommand
    {
        [JsonProperty("remitoId")]
        public long RemitoId { get; set; }

        [JsonProperty("itemsRecibidos")]
        public List<RecepcionItemDto> ItemsRecibidos { get; set; } = new();

        // Imagen de la firma codificada en Base64 (PNG o JPG)
        [JsonProperty("firmaBase64")]
        public string FirmaBase64 { get; set; } = string.Empty;

        [JsonProperty("nombreReceptor")]
        public string NombreReceptor { get; set; } = string.Empty;

        // La IP Address es capturada por la API
    }
}
