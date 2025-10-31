using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PortalProveedores.Mobile.Models
{
    /// <summary>
    /// Modelo local para los Remitos que se han empezado a recibir
    /// o están pendientes de sincronizar con la API.
    /// </summary>
    public class RemitoLocal
    {
        // Clave primaria de la tabla SQLite local
        [PrimaryKey, AutoIncrement]
        public int IdLocal { get; set; }

        // ID del Remito en el sistema principal (después de la sincronización)
        public long RemitoId { get; set; }

        public string NumeroRemito { get; set; } = string.Empty;
        public string NombreProveedor { get; set; } = string.Empty;
        public string NombreReceptor { get; set; } = string.Empty;
        public string FirmaBase64 { get; set; } = string.Empty;

        // Se usa un campo de texto para guardar la lista de ítems
        // serializada en JSON. Esto simplifica el esquema de la DB local.
        public string ItemsRecibidosJson { get; set; } = string.Empty;

        public bool Sincronizado { get; set; } = false; // Falso = pendiente de enviar

        // --- Métodos Helper para serialización ---

        [Ignore] // Indica a SQLite que ignore esta propiedad
        public List<RecepcionItemDto> ItemsRecibidos
        {
            get => string.IsNullOrEmpty(ItemsRecibidosJson)
                   ? new List<RecepcionItemDto>()
                   : JsonConvert.DeserializeObject<List<RecepcionItemDto>>(ItemsRecibidosJson) ?? new List<RecepcionItemDto>();

            set => ItemsRecibidosJson = JsonConvert.SerializeObject(value);
        }
    }
}
