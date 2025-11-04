using Microsoft.AspNetCore.Http;
using Moq;
using PortalProveedores.Application.Common.Interfaces;
//using PortalProveedores.Application.Features.OrdenesCompra.Commands; // Falta crear esta clase
using PortalProveedores.Application.Features.Compras.Commands;
using PortalProveedores.Application.Features.Cotizaciones.Commands;
using PortalProveedores.Application.Features.Facturas.Commands;
using PortalProveedores.Application.Features.Remitos.Commands;
using PortalProveedores.Application.Features.Remitos.Queries;
using PortalProveedores.Application.Features.Reports.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using NUnit.Framework;

namespace PortalProveedores.FunctionalTests
{
    /// <summary>
    /// Clase de prueba E2E del flujo completo.
    /// </summary>
    public class EndToEndTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        // Tokens de usuario de prueba (simulación tras un login exitoso)
        private const string ClienteAdminToken = "jwt_cliente_admin_1";
        private const string ProveedorVendedorToken = "jwt_proveedor_vendedor_1";
        private const string ClienteRecepcionToken = "jwt_cliente_recepcion_1";

        // IDs que se generarán en la prueba
        private long _ordenCompraId;
        private long _cotizacionId;
        private long _remitoId;
        private string _qrToken = string.Empty;

        public EndToEndTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();

            // Simular que el ICurrentUserService devuelve los IDs de entidad correctos
            // NOTA: Esto requiere que ICurrentUserService sea un Singleton/Scoped en la fábrica.
            // Aquí se simula la inyección de claims/headers para evitar depender del login real.
        }

        // --- Mock de Autenticación (Se simula la inyección de claims) ---
        private HttpClient GetClientWithAuth(string token)
        {
            // Crear un cliente nuevo o limpio
            var authenticatedClient = _factory.CreateClient();
            // Simular la cabecera de autenticación
            authenticatedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Nota: En una prueba real, se usaría un mock del servicio de Identity para devolver 
            // el usuario/rol basándose en este token simulado.
            return authenticatedClient;
        }

        // [Fase A] - Cliente crea Orden de Compra (OC)
        [Fact, Order(1)]
        public async Task FlujoCompleto_01_CrearOrdenCompra()
        {
            // Actúa como Administrativo Cliente (Rol A)
            var client = GetClientWithAuth(ClienteAdminToken);

            var command = new CrearOrdenCompraCommand
            {
                // Datos simulados (ClienteId 1 del SeedData)
                ProveedorId = 1,
                Items = new List<CrearOrdenCompraCommand.OrdenCompraItemDto>
            {
                new(ProductoId: 10, Cantidad: 50)
            }
            };

            // El endpoint requiere que el token simule ser Rol A y tener ClienteId 1
            var response = await client.PostAsJsonAsync("/api/ordenescompra", command);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<dynamic>();

            _ordenCompraId = (long)result.Value.ordenCompraId;
            Xunit.Assert.True(_ordenCompraId > 0);
        }

        // [Fase B & C] - Proveedor cotiza y Cliente acepta
        [Fact, Order(2)]
        public async Task FlujoCompleto_02_CotizarYAceptar()
        {
            // Actúa como Vendedor Proveedor (Rol B/C)
            var vendorClient = GetClientWithAuth(ProveedorVendedorToken);

            // 1. Crear Cotización
            var cotizacionCommand = new CrearCotizacionCommand
            {
                OrdenCompraId = _ordenCompraId,
                Items = new List<CrearCotizacionCommand.CotizacionItemDto>
            {
                new(ProductoId: 10, Cantidad: 50)
            }
            };
            var cotizacionResponse = await vendorClient.PostAsJsonAsync("/api/cotizaciones", cotizacionCommand);
            cotizacionResponse.EnsureSuccessStatusCode();

            var cotizacionResult = await cotizacionResponse.Content.ReadFromJsonAsync<dynamic>();
            long cotizacionId = (long)cotizacionResult.Value.cotizacionId;

            // 2. Aceptar Cotización (Actúa como Administrativo Cliente, Rol A)
            var clientAdmin = GetClientWithAuth(ClienteAdminToken);
            var acceptCommand = new AceptarCotizacionesCommand { CotizacionIdsAAceptar = new() { cotizacionId } };
            var acceptResponse = await clientAdmin.PostAsJsonAsync($"/api/cotizaciones/{cotizacionId}/aceptar", acceptCommand);
            acceptResponse.EnsureSuccessStatusCode();
        }

        // [Fase D] - Proveedor genera Remito y QR
        [Fact, Order(3)]
        public async Task FlujoCompleto_03_GenerarRemitoYQR()
        {
            // Actúa como Despachante (Rol E) o Vendedor (Rol B)
            var vendorClient = GetClientWithAuth(ProveedorVendedorToken);

            // 1. Generar Remito
            var remitoCommand = new CrearRemitoCommand
            {
                CotizacionIds = new() { _cotizacionId },
                // Esto asume que el Handler encuentra la cotización aceptada.
            };
            var remitoResponse = await vendorClient.PostAsJsonAsync("/api/remitos", remitoCommand);
            remitoResponse.EnsureSuccessStatusCode();

            var remitoResult = await remitoResponse.Content.ReadFromJsonAsync<dynamic>();
            _remitoId = (long)remitoResult.Value.remitoId;
            Xunit.Assert.True(_remitoId > 0);

            // 2. Generar QR (Actúa como Despachante)
            var qrResponse = await vendorClient.PostAsync($"/api/remitos/{_remitoId}/generar-qr", null);
            qrResponse.EnsureSuccessStatusCode();

            _qrToken = await qrResponse.Content.ReadAsStringAsync();
            Xunit.Assert.False(string.IsNullOrEmpty(_qrToken));
        }

        // [Fase E] - Cliente escanea QR y confirma Recepción
        [Fact, Order(4)]
        public async Task FlujoCompleto_04_ValidarQryConfirmarRecepcion()
        {
            // Actúa como Recepcionador (Rol D)
            var receiverClient = GetClientWithAuth(ClienteRecepcionToken);

            // 1. Validar QR (Query)
            var validationResponse = await receiverClient.GetAsync($"/api/remitos/validar-qr/{_qrToken.Trim('"')}");
            validationResponse.EnsureSuccessStatusCode();

            var validationResult = await validationResponse.Content.ReadFromJsonAsync<RemitoParaRecepcionDto>();
            Xunit.Assert.NotNull(validationResult);
            Xunit.Assert.Equal(_remitoId, validationResult.RemitoId);

            // 2. Confirmar Recepción (Command)
            var confirmCommand = new ConfirmarRecepcionCommand
            {
                RemitoId = _remitoId,
                QrToken = _qrToken.Trim('"'),
                // Simular recepción sin diferencias
                ItemsRecibidos = validationResult.ItemsEsperados.Select(i => new RecepcionItemDto
                {
                    Sku = i.Sku,
                    Descripcion = i.Descripcion,
                    CantidadDeclarada = i.CantidadDeclarada,
                    CantidadRecibida = i.CantidadDeclarada // Confirmación sin diferencias
                }).ToList(),
                // Las firmas deben ser Base64 válidas o los mocks fallarán
                FirmaRecepcionistaBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=",
                FirmaTransportistaBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII="
            };

            // Configurar Mocks de Storage
            _factory.FileStorageServiceMock.Setup(x => x.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("http://fakeurl.com/firma.png");

            var confirmResponse = await receiverClient.PostAsJsonAsync("/api/remitos/recepcionar", confirmCommand);
            confirmResponse.EnsureSuccessStatusCode();
        }

        // [Fase F] - Proveedor carga Factura
        [Fact, Order(5)]
        public async Task FlujoCompleto_05_CargarFacturaYConciliar()
        {
            // Actúa como Administrativo Proveedor (Rol B)
            var vendorClient = GetClientWithAuth(ProveedorVendedorToken);

            // 1. Configurar Mocks de OCR/AFIP para Factura
            _factory.OcrServiceMock.Setup(x => x.ExtraerDatosFacturaAsync(It.IsAny<string>()))
                .ReturnsAsync(new ExtraccionFacturaDto
                {
                    F1_TipoFactura = "A",
                    F5_MontoTotal = 1210.00m,
                    F6_MontoIVA = 210.00m,
                    F9_CAE = "12345678901234",
                    F4_FechaEmision = DateTime.UtcNow,
                    F7_ProveedorCUIT = "30-71000000-5",
                    F8_ClienteCUIT = "30-71111111-8",
                    Items = new List<FacturaItemDto> {
                    new() { Sku = "SKU001", Cantidad = 10, PrecioUnitario = 100.00m, Subtotal = 1000.00m }
                    }
                });

            _factory.AfipServiceMock.Setup(x => x.ValidarComprobanteAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new ValidacionAFIPDto { EsValido = true, Observaciones = "OK" });

            // 2. Crear el IFormFile simulado
            var content = new MultipartFormDataContent();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("Fake PDF Content"));
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            content.Add(fileContent, "ArchivoPDF", "factura_test.pdf");

            // 3. Añadir los RemitoIds (Necesario convertir la lista a un formato compatible con MultipartForm)
            // Por la limitación de FromForm con listas, se simplifica a un string en el content
            content.Add(new StringContent(_remitoId.ToString()), "RemitoIds[0]");

            var uploadResponse = await vendorClient.PostAsync("/api/facturas", content);
            uploadResponse.EnsureSuccessStatusCode();

            var facturaResult = await uploadResponse.Content.ReadFromJsonAsync<dynamic>();
            long facturaId = (long)facturaResult.Value.facturaId;
            Xunit.Assert.True(facturaId > 0);
        }

        // [Fase G] - Cliente consulta la Conciliación
        [Fact, Order(6)]
        public async Task FlujoCompleto_06_ConsultarConciliacion()
        {
            // Actúa como Administrativo Cliente (Rol A)
            var clientAdmin = GetClientWithAuth(ClienteAdminToken);

            // Recuperar el ID de la factura (se necesita guardar el ID en la prueba anterior o buscarlo)
            // Por la limitación de xUnit y el uso de Factory, se asume que Factura ID 1 existe.
            const long facturaId = 1;

            var conciliacionResponse = await clientAdmin.GetAsync($"/api/reports/conciliacion/{facturaId}");
            conciliacionResponse.EnsureSuccessStatusCode();

            var conciliacionResult = await conciliacionResponse.Content.ReadFromJsonAsync<ConciliacionReporteDto>();

            Xunit.Assert.NotNull(conciliacionResult);
            Xunit.Assert.False(conciliacionResult.HuboDiscrepanciasCantidad); // 10 recibidos vs 10 facturados
                                                                        // Assert.True(conciliacionResult.HuboDiscrepanciasPrecio); // Si OC tenía 5 y Factura 100
        }
    }
}
