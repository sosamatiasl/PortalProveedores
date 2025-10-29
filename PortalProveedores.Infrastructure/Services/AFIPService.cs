using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortalProveedores.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace PortalProveedores.Infrastructure.Services
{
    /// <summary>
    /// Implementación de la simulación de validación AFIP.
    /// </summary>
    public class AFIPService : IAFIPService
    {
        private readonly ILogger<AFIPService> _logger;

        public AFIPService(ILogger<AFIPService> logger)
        {
            _logger = logger;
        }

        public Task<ValidacionAFIPDto> ValidarComprobanteAsync(
            string cae,
            string cuitProveedor,
            string cuitCliente,
            string tipoFactura,
            decimal montoTotal,
            DateTime fechaEmision)
        {
            _logger.LogInformation($"[Simulación AFIP] Validando CAE: {cae} para CUIT {cuitProveedor}...");

            // --- SIMULACIÓN DE WEB SERVICE AFIP ---
            var resultado = new ValidacionAFIPDto();

            if (cae.EndsWith("3")) // Simular CAE Inválido para fines de prueba
            {
                resultado.EsValido = false;
                resultado.Observaciones = "CAE con estructura incorrecta o no registrado en AFIP (Error simulado).";
            }
            else
            {
                resultado.EsValido = true;
                resultado.Observaciones = "Comprobante verificado y aprobado fiscalmente (Simulación OK).";
            }

            _logger.LogInformation($"[Simulación AFIP] Resultado: {(resultado.EsValido ? "VÁLIDO" : "INVÁLIDO")}.");

            return Task.FromResult(resultado);
        }
    }
}
