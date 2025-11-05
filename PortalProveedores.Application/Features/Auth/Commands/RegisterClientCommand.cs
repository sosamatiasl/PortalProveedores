using PortalProveedores.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace PortalProveedores.Application.Features.Auth.Commands
{
    public class RegisterClientCommand : RegisterClientRequest, IRequest<IdentityResult>
    {
    }
}
