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
using System.Security.Cryptography;

namespace PortalProveedores.Application.Features.Remitos.Commands
{
    public class GenerarQRRemitoCommandHandler : IRequestHandler<GenerarQRRemitoCommand, string>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GenerarQRRemitoCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<string> Handle(GenerarQRRemitoCommand request, CancellationToken cancellationToken)
        {
            // 1. Seguridad: Verificar Rol E (Despachante)
            if (!_currentUser.IsInRole("DespachanteMercaderia")) // Rol E
            {
                throw new UnauthorizedAccessException("Solo un 'Despachante de Mercadería' (Rol E) puede generar un QR.");
            }

            // 2. Seguridad: Verificar que el usuario sea Proveedor
            var proveedorId = _currentUser.ProveedorId;
            if (!proveedorId.HasValue)
            {
                throw new UnauthorizedAccessException("El usuario no pertenece a un Proveedor.");
            }

            // 3. Obtener el Remito
            var remito = await _context.Remitos
                .FirstOrDefaultAsync(r =>
                    r.Id == request.RemitoId &&
                    r.ProveedorId == proveedorId.Value, // Seguridad: Le pertenece
                    cancellationToken);

            if (remito == null)
            {
                throw new Exception("Remito no encontrado o no pertenece a este proveedor.");
            }

            // 4. Invalidar QRs activos anteriores (si los hubiera)
            var qrsActivos = await _context.RemitoQRCodes
                .Where(qr => qr.RemitoId == remito.Id && !qr.Usado && qr.FechaExpiracion > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var qr in qrsActivos)
            {
                qr.FechaExpiracion = DateTime.UtcNow; // Expirar ahora
            }

            // 5. Generar el nuevo "puntero" seguro (el token)
            // Se usa un GUID para asegurar unicidad
            var token = $"rem-{Guid.NewGuid()}";

            var nuevoQR = new RemitoQRCode
            {
                RemitoId = remito.Id,
                CodigoHash = token, // Este es el token que irá en el QR
                FechaExpiracion = DateTime.UtcNow.AddHours(24), // Válido por 24h
                Usado = false
            };

            // 6. Actualizar el estado del Remito
            remito.Estado = EstadoRemito.EnTransporte;

            // 7. Guardar en BD
            await _context.RemitoQRCodes.AddAsync(nuevoQR, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // 8. Devolver el token (el puntero)
            // La app móvil/web usará esto para generar la imagen QR
            return token;
        }
    }
}
