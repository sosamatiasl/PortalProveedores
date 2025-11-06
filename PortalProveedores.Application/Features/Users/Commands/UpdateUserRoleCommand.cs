using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using PortalProveedores.Application.Common.Interfaces;
using PortalProveedores.Domain.Enums;
using System.Linq;

namespace PortalProveedores.Application.Features.Users.Commands
{
    public class UpdateUserRoleCommand : IRequest<bool>
    {
        public long UserId { get; set; }
        public string NewRoleId { get; set; } = string.Empty;
    }
}
