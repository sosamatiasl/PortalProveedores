using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Application.Common.Interfaces
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Sube un archivo a un contenedor/bucket.
        /// </summary>
        /// <param name="fileStream">El stream del archivo.</param>
        /// <param name="fileName">El nombre único para el archivo en el storage.</param>
        /// <param name="containerName">El contenedor (ej. "selfies", "facturas").</param>
        /// <returns>La URL pública del archivo subido.</returns>
        Task<string> UploadAsync(Stream fileStream, string fileName, string containerName);

        /// <summary>
        /// Elimina un archivo del storage.
        /// </summary>
        Task DeleteAsync(string fileUrl, string containerName);
    }
}
