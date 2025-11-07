using PortalProveedores.Domain.Enums;

namespace PortalProveedores.Domain.Constants
{
    public static class DashboardUserRoles
    {
        public const string RolCliente = nameof(TipoRolUsuario.AdministrativoCliente) + "," + nameof(TipoRolUsuario.RecepcionadorCliente);
        public const string RolProveedor = nameof(TipoRolUsuario.AdministrativoProveedor) + "," + nameof(TipoRolUsuario.DespachanteProveedor);
        public const string RolTransportista = nameof(TipoRolUsuario.Transportista);

        public const string AllOperationalRoles = RolCliente + "," + RolProveedor + "," + RolTransportista;
    }
}
