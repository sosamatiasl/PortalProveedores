using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Application.Common.Interfaces
{
    /// <summary>
    /// Interfaz de Simulación de Web Service AFIP.
    /// </summary>
    /// <remarks>
    /// Simula la validación de un comprobante ante el Web Service de la AFIP (Argentina).
    /// </remarks>
    public interface IAFIPService
    {
        /// <summary>
        /// Valida el CAE, CUITs y montos de la factura.
        /// </summary>
        Task<ValidacionAFIPDto> ValidarComprobanteAsync(
            string cae,
            string cuitProveedor,
            string cuitCliente,
            string tipoFactura,
            decimal montoTotal,
            DateTime fechaEmision);
    }

    public class ValidacionAFIPDto
    {
        public bool EsValido { get; set; }
        public string Observaciones { get; set; } = string.Empty;
    }
}
