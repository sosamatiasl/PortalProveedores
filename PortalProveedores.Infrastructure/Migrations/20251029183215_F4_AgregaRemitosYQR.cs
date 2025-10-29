using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalProveedores.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class F4_AgregaRemitosYQR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NumeroRemito",
                table: "Remitos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            //migrationBuilder.CreateTable(
            //    name: "CotizacionRemitos",
            //    columns: table => new
            //    {
            //        CotizacionId = table.Column<long>(type: "bigint", nullable: false),
            //        RemitoId = table.Column<long>(type: "bigint", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CotizacionRemitos", x => new { x.CotizacionId, x.RemitoId });
            //        table.ForeignKey(
            //            name: "FK_CotizacionRemitos_Cotizaciones_CotizacionId",
            //            column: x => x.CotizacionId,
            //            principalTable: "Cotizaciones",
            //            principalColumn: "Id");
            //        table.ForeignKey(
            //            name: "FK_CotizacionRemitos_Remitos_RemitoId",
            //            column: x => x.RemitoId,
            //            principalTable: "Remitos",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "RemitoQRCodes",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        RemitoId = table.Column<long>(type: "bigint", nullable: false),
            //        CodigoHash = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Usado = table.Column<bool>(type: "bit", nullable: false),
            //        FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_RemitoQRCodes", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_RemitoQRCodes_Remitos_RemitoId",
            //            column: x => x.RemitoId,
            //            principalTable: "Remitos",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            migrationBuilder.CreateIndex(
                name: "IX_Remitos_ClienteId",
                table: "Remitos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Remitos_ProveedorId",
                table: "Remitos",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionRemitos_RemitoId",
                table: "CotizacionRemitos",
                column: "RemitoId");

            migrationBuilder.CreateIndex(
                name: "IX_RemitoQRCodes_CodigoHash",
                table: "RemitoQRCodes",
                column: "CodigoHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RemitoQRCodes_RemitoId",
                table: "RemitoQRCodes",
                column: "RemitoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Remitos_Clientes_ClienteId",
                table: "Remitos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Remitos_Proveedores_ProveedorId",
                table: "Remitos",
                column: "ProveedorId",
                principalTable: "Proveedores",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Remitos_Clientes_ClienteId",
                table: "Remitos");

            migrationBuilder.DropForeignKey(
                name: "FK_Remitos_Proveedores_ProveedorId",
                table: "Remitos");

            migrationBuilder.DropTable(
                name: "CotizacionRemitos");

            migrationBuilder.DropTable(
                name: "RemitoQRCodes");

            migrationBuilder.DropIndex(
                name: "IX_Remitos_ClienteId",
                table: "Remitos");

            migrationBuilder.DropIndex(
                name: "IX_Remitos_ProveedorId",
                table: "Remitos");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroRemito",
                table: "Remitos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
