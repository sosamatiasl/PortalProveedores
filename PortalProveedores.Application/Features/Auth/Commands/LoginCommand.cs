using MediatR;
using PortalProveedores.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Application.Features.Auth.Commands
{
    public class LoginCommand : LoginRequest, IRequest<AuthResponse>
    {
        public string IpAddress { get; set; } = string.Empty;
    }
}
