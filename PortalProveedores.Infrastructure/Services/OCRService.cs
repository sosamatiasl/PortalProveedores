using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Application.Features.Facturas.Commands;
using Microsoft.Extensions.Logging;

namespace PortalProveedores.Infrastructure.Services
{
    /// <summary>
    /// Implementación de la simulación de extracción de datos.
    /// </summary>
    public class OCRService : IOCRService
    {
        private readonly ILogger<OCRService> _logger;

        public OCRService(ILogger<OCRService> logger)
        {
            _logger = logger;
        }

        public Task<ExtraccionFacturaDto> ExtraerDatosFacturaAsync(string pdfUrl)
        {
            _logger.LogInformation($"[Simulación OCR] Extrayendo datos del PDF: {pdfUrl}...");

            // --- SIMULACIÓN DE EXTRACCIÓN REALISTA ---
            // Se generan datos de ejemplo para simular una factura exitosa.
            var resultado = new ExtraccionFacturaDto
            {
                F1_TipoFactura = "A",
                F2_PuntoVenta = "0005",
                F3_NumeroFactura = new Random().Next(10000000, 99999999).ToString(),
                F4_FechaEmision = DateTime.UtcNow.Date,
                F5_MontoTotal = 1210.00m, // Monto simulado
                F6_MontoIVA = 210.00m,
                F7_ProveedorCUIT = "30-71000000-5",
                F8_ClienteCUIT = "30-71111111-8",
                F9_CAE = "12345678901234", // CAE simulado
                F10_VencimientoCAE = DateTime.UtcNow.AddDays(10).Date,
                F11_ObservacionesAFIP = null,
                Items = new List<FacturaItemDto>
            {
                new FacturaItemDto { Sku = "SKU001", Descripcion = "Producto A", Cantidad = 10, PrecioUnitario = 50.00m, Subtotal = 500.00m },
                new FacturaItemDto { Sku = "SKU002", Descripcion = "Producto B", Cantidad = 5, PrecioUnitario = 100.00m, Subtotal = 500.00m }
            }
            };

            _logger.LogInformation($"[Simulación OCR] Extracción finalizada. Factura N° {resultado.F3_NumeroFactura}. Monto: {resultado.F5_MontoTotal:C}");

            return Task.FromResult(resultado);
        }
    }
}
