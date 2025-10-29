using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Entities;
using PortalProveedores.Domain.Enums;
using System.Security.Claims;

namespace PortalProveedores.Application.Features.Remitos.Commands
{
    public class ConfirmarRecepcionCommandHandler : IRequestHandler<ConfirmarRecepcionCommand, long>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorageService;
        private readonly INotificationService _notificationService;

        public ConfirmarRecepcionCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IFileStorageService fileStorageService,
            INotificationService notificationService)
        {
            _context = context;
            _currentUser = currentUser;
            _fileStorageService = fileStorageService;
            _notificationService = notificationService;
        }

        public async Task<long> Handle(ConfirmarRecepcionCommand request, CancellationToken cancellationToken)
        {
            // 1. Seguridad: Verificar Rol D y obtener su UserID
            var usuarioId = _currentUser.UserId;
            if (usuarioId == null || !_currentUser.IsInRole("RecepcionadorMercaderia"))
            {
                throw new UnauthorizedAccessException("Acción permitida solo para el Rol (D) Recepcionador.");
            }

            // 2. Usar una transacción
            await using var transaccion = await _context.BeginTransactionAsync(cancellationToken);

            try
            {
                // 3. Validar el QR y el Remito (de nuevo, pero esta vez para escribir)
                var qrCode = await _context.RemitoQRCodes
                    .Include(q => q.Remito)
                    .FirstOrDefaultAsync(qr =>
                        qr.CodigoHash == request.QrToken &&
                        qr.RemitoId == request.RemitoId &&
                        !qr.Usado,
                        cancellationToken);

                if (qrCode == null) throw new Exception("QR inválido o ya utilizado.");
                var remito = qrCode.Remito;

                // 4. Subir las firmas (convertir Base64 a Stream)
                var firmaRecepcionistaUrl = await UploadBase64SignatureAsync(
                    request.FirmaRecepcionistaBase64,
                    $"firma_recep_{remito.Id}_{Guid.NewGuid()}.png");

                var firmaTransportistaUrl = await UploadBase64SignatureAsync(
                    request.FirmaTransportistaBase64,
                    $"firma_trans_{remito.Id}_{Guid.NewGuid()}.png");

                // 5. Determinar si hubo diferencias
                bool huboDiferencias = request.ItemsRecibidos
                    .Any(i => i.CantidadDeclarada != i.CantidadRecibida);

                // 6. Crear la entidad Recepcion
                var recepcion = new Recepcion
                {
                    RemitoId = remito.Id,
                    UsuarioRecepcionId = usuarioId,
                    FechaRecepcion = DateTime.UtcNow,
                    HuboDiferencias = huboDiferencias,
                    DetalleDiferencias = request.DetalleDiferencias,
                    FirmaRecepcionista_URL = firmaRecepcionistaUrl,
                    FirmaTransportista_URL = firmaTransportistaUrl,
                    Detalles = request.ItemsRecibidos.Select(i => new RecepcionDetalle
                    {
                        IdProducto = i.Sku,
                        DescripcionProducto = i.Descripcion,
                        CantidadDeclarada = i.CantidadDeclarada,
                        CantidadRecibida = i.CantidadRecibida
                    }).ToList()
                };

                await _context.Recepciones.AddAsync(recepcion, cancellationToken);

                // 7. Actualizar el estado del Remito y marcar el QR como usado
                remito.Estado = huboDiferencias ? EstadoRemito.RecibidoConDiferencias : EstadoRemito.Recibido;
                qrCode.Usado = true;

                // 8. Guardar todo
                await _context.SaveChangesAsync(cancellationToken);
                await transaccion.CommitAsync(cancellationToken);

                // 9. Notificar
                await _notificationService.NotificarRecepcionRemitoAsync(recepcion);

                return recepcion.Id;
            }
            catch (Exception)
            {
                await transaccion.RollbackAsync(cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// Método helper para convertir Base64 a Stream y subirlo.
        /// </summary>
        private async Task<string> UploadBase64SignatureAsync(string base64String, string fileName)
        {
            if (string.IsNullOrEmpty(base64String))
            {
                throw new Exception($"Firma '{fileName}' está vacía.");
            }

            // Limpiar el prefijo (ej. "data:image/png;base66,") si la app móvil lo envía
            if (base64String.Contains(','))
            {
                base64String = base64String.Split(',')[1];
            }

            var bytes = Convert.FromBase64String(base64String);
            await using var stream = new MemoryStream(bytes);

            // "firmas" es el nombre del contenedor en Azure Blob Storage
            return await _fileStorageService.UploadAsync(stream, fileName, "firmas");
        }
    }
}
