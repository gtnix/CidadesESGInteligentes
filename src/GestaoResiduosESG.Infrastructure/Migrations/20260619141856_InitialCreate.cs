using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoResiduosESG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PONTO_COLETA",
                columns: table => new
                {
                    CD_PONTO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NM_PONTO = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DS_LOCALIZACAO = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VL_CAPACIDADE_MAXIMA = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    VL_QUANTIDADE_ATUAL = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PONTO_COLETA", x => x.CD_PONTO);
                    table.CheckConstraint("CK_capacidade_maxima", "VL_CAPACIDADE_MAXIMA > 0");
                    table.CheckConstraint("CK_quantidade_atual", "VL_QUANTIDADE_ATUAL >= 0");
                });

            migrationBuilder.CreateTable(
                name: "RESIDUO",
                columns: table => new
                {
                    CD_RESIDUO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NM_RESIDUO = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DS_CATEGORIA = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DS_UNIDADE_MEDIDA = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RESIDUO", x => x.CD_RESIDUO);
                });

            migrationBuilder.CreateTable(
                name: "ALERTA",
                columns: table => new
                {
                    CD_ALERTA = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CD_PONTO = table.Column<int>(type: "int", nullable: false),
                    CD_RESIDUO = table.Column<int>(type: "int", nullable: false),
                    DT_ALERTA = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    DS_TIPO_ALERTA = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DS_MENSAGEM = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ST_ALERTA = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pendente")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ALERTA", x => x.CD_ALERTA);
                    table.ForeignKey(
                        name: "FK_ALERTA_PONTO_COLETA_CD_PONTO",
                        column: x => x.CD_PONTO,
                        principalTable: "PONTO_COLETA",
                        principalColumn: "CD_PONTO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALERTA_RESIDUO_CD_RESIDUO",
                        column: x => x.CD_RESIDUO,
                        principalTable: "RESIDUO",
                        principalColumn: "CD_RESIDUO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DESCARTE",
                columns: table => new
                {
                    CD_DESCARTE = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CD_RESIDUO = table.Column<int>(type: "int", nullable: false),
                    CD_PONTO = table.Column<int>(type: "int", nullable: false),
                    DT_DESCARTE = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VL_QUANTIDADE = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    NM_RESPONSAVEL = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DESCARTE", x => x.CD_DESCARTE);
                    table.CheckConstraint("CK_descarte_quantidade", "VL_QUANTIDADE > 0");
                    table.ForeignKey(
                        name: "FK_DESCARTE_PONTO_COLETA_CD_PONTO",
                        column: x => x.CD_PONTO,
                        principalTable: "PONTO_COLETA",
                        principalColumn: "CD_PONTO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DESCARTE_RESIDUO_CD_RESIDUO",
                        column: x => x.CD_RESIDUO,
                        principalTable: "RESIDUO",
                        principalColumn: "CD_RESIDUO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LIMITE_ARMAZENAMENTO",
                columns: table => new
                {
                    CD_LIMITE = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CD_PONTO = table.Column<int>(type: "int", nullable: false),
                    CD_RESIDUO = table.Column<int>(type: "int", nullable: false),
                    VL_LIMITE_MINIMO = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    VL_LIMITE_MAXIMO = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    VL_PERCENTUAL_ALERTA = table.Column<int>(type: "int", nullable: false, defaultValue: 80)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LIMITE_ARMAZENAMENTO", x => x.CD_LIMITE);
                    table.CheckConstraint("CK_limite_maximo", "VL_LIMITE_MAXIMO > VL_LIMITE_MINIMO");
                    table.CheckConstraint("CK_percentual_alerta", "VL_PERCENTUAL_ALERTA BETWEEN 1 AND 100");
                    table.ForeignKey(
                        name: "FK_LIMITE_ARMAZENAMENTO_PONTO_COLETA_CD_PONTO",
                        column: x => x.CD_PONTO,
                        principalTable: "PONTO_COLETA",
                        principalColumn: "CD_PONTO",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LIMITE_ARMAZENAMENTO_RESIDUO_CD_RESIDUO",
                        column: x => x.CD_RESIDUO,
                        principalTable: "RESIDUO",
                        principalColumn: "CD_RESIDUO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PONTO_RESIDUO",
                columns: table => new
                {
                    CD_PONTO = table.Column<int>(type: "int", nullable: false),
                    CD_RESIDUO = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PONTO_RESIDUO", x => new { x.CD_PONTO, x.CD_RESIDUO });
                    table.ForeignKey(
                        name: "FK_PONTO_RESIDUO_PONTO_COLETA_CD_PONTO",
                        column: x => x.CD_PONTO,
                        principalTable: "PONTO_COLETA",
                        principalColumn: "CD_PONTO",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PONTO_RESIDUO_RESIDUO_CD_RESIDUO",
                        column: x => x.CD_RESIDUO,
                        principalTable: "RESIDUO",
                        principalColumn: "CD_RESIDUO",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ALERTA_CD_PONTO",
                table: "ALERTA",
                column: "CD_PONTO");

            migrationBuilder.CreateIndex(
                name: "IX_ALERTA_CD_RESIDUO",
                table: "ALERTA",
                column: "CD_RESIDUO");

            migrationBuilder.CreateIndex(
                name: "IX_DESCARTE_CD_PONTO",
                table: "DESCARTE",
                column: "CD_PONTO");

            migrationBuilder.CreateIndex(
                name: "IX_DESCARTE_CD_RESIDUO",
                table: "DESCARTE",
                column: "CD_RESIDUO");

            migrationBuilder.CreateIndex(
                name: "IX_LIMITE_ARMAZENAMENTO_CD_PONTO_CD_RESIDUO",
                table: "LIMITE_ARMAZENAMENTO",
                columns: new[] { "CD_PONTO", "CD_RESIDUO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LIMITE_ARMAZENAMENTO_CD_RESIDUO",
                table: "LIMITE_ARMAZENAMENTO",
                column: "CD_RESIDUO");

            migrationBuilder.CreateIndex(
                name: "IX_PONTO_RESIDUO_CD_RESIDUO",
                table: "PONTO_RESIDUO",
                column: "CD_RESIDUO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ALERTA");

            migrationBuilder.DropTable(
                name: "DESCARTE");

            migrationBuilder.DropTable(
                name: "LIMITE_ARMAZENAMENTO");

            migrationBuilder.DropTable(
                name: "PONTO_RESIDUO");

            migrationBuilder.DropTable(
                name: "PONTO_COLETA");

            migrationBuilder.DropTable(
                name: "RESIDUO");
        }
    }
}
