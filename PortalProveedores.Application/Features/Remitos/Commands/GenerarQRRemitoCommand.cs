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
    /// <summary>
    /// Genera el token seguro para el QR. Restringido a Rol E.
    /// </summary>
    // Devuelve el STRING (token) que se usará para generar la imagen QR
    public class GenerarQRRemitoCommand : IRequest<string>
    {
        public long RemitoId { get; set; }
    }
}
