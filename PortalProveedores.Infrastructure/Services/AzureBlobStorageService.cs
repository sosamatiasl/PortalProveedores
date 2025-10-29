using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using PortalProveedores.Application.Common.Interfaces;

namespace PortalProveedores.Infrastructure.Services
{
    public class AzureBlobStorageService : IFileStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureBlobStorageService(IConfiguration config)
        {
            _blobServiceClient = new BlobServiceClient(config.GetConnectionString("AzureBlobStorage"));
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, string containerName)
        {
            // Obtener o crear el contenedor (ej. "selfies")
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob); // Permite acceso público de lectura

            // Obtener el cliente del blob (archivo)
            var blobClient = containerClient.GetBlobClient(fileName);

            // Subir el stream
            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders
            {
                // Determinar el content type (ej. "image/jpeg")
                ContentType = GetContentType(fileName)
            });

            // Devolver la URL pública
            return blobClient.Uri.ToString();
        }

        public async Task DeleteAsync(string fileUrl, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var fileName = new Uri(fileUrl).Segments.Last();
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }

        private string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream",
            };
        }
    }
}
