using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoResiduosESG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDadosIniciais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Resíduos iniciais
            migrationBuilder.InsertData(
                table: "RESIDUO",
                columns: new[] { "NM_RESIDUO", "DS_CATEGORIA", "DS_UNIDADE_MEDIDA" },
                values: new object[,]
                {
                    { "Papel e Papelão",    "Reciclavel",    "kg" },
                    { "Plástico Misto",     "Reciclavel",    "kg" },
                    { "Vidro",              "Reciclavel",    "kg" },
                    { "Metal/Alumínio",     "Reciclavel",    "kg" },
                    { "Resíduo Orgânico",   "Organico",      "kg" },
                    { "Pilhas e Baterias",  "Perigoso",      "un" },
                    { "Eletrônico (REEE)",  "Eletronico",    "un" },
                    { "Rejeito Geral",      "NaoReciclavel", "kg" }
                });

            // Pontos de coleta iniciais
            migrationBuilder.InsertData(
                table: "PONTO_COLETA",
                columns: new[] { "NM_PONTO", "DS_LOCALIZACAO", "VL_CAPACIDADE_MAXIMA", "VL_QUANTIDADE_ATUAL" },
                values: new object[,]
                {
                    { "Ponto Central — Refeitório", "Bloco A, Térreo",   500m, 0m },
                    { "Ponto Lab de Informática",   "Bloco B, 2º Andar", 200m, 0m },
                    { "Ponto Área Administrativa",  "Bloco C, Térreo",   300m, 0m }
                });

            // Vínculos ponto ↔ resíduo (PontoResiduo)
            // Ponto 1 aceita: Papel(1), Plástico(2), Vidro(3), Metal(4), Orgânico(5)
            // Ponto 2 aceita: Papel(1), Pilhas(6), Eletrônico(7)
            // Ponto 3 aceita: Papel(1), Plástico(2), Rejeito(8)
            migrationBuilder.InsertData(
                table: "PONTO_RESIDUO",
                columns: new[] { "CD_PONTO", "CD_RESIDUO" },
                values: new object[,]
                {
                    { 1, 1 }, { 1, 2 }, { 1, 3 }, { 1, 4 }, { 1, 5 },
                    { 2, 1 }, { 2, 6 }, { 2, 7 },
                    { 3, 1 }, { 3, 2 }, { 3, 8 }
                });

            // Limite de armazenamento: Ponto 1 / Papel — alerta em 75%
            migrationBuilder.InsertData(
                table: "LIMITE_ARMAZENAMENTO",
                columns: new[] { "CD_PONTO", "CD_RESIDUO", "VL_LIMITE_MINIMO", "VL_LIMITE_MAXIMO", "VL_PERCENTUAL_ALERTA" },
                values: new object[] { 1, 1, 20m, 450m, 75 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // LIMITE_ARMAZENAMENTO tem chave composta (CD_PONTO + CD_RESIDUO)
            migrationBuilder.DeleteData(
                table: "LIMITE_ARMAZENAMENTO",
                keyColumns: new[] { "CD_PONTO", "CD_RESIDUO" },
                keyValues: new object[] { 1, 1 });

            // PONTO_RESIDUO tem chave composta (CD_PONTO + CD_RESIDUO)
            // Ponto 1: Papel(1), Plástico(2), Vidro(3), Metal(4), Orgânico(5)
            migrationBuilder.DeleteData(table: "PONTO_RESIDUO", keyColumns: new[] { "CD_PONTO", "CD_RESIDUO" }, keyValues: new object[] { 1, 1 });
            migrationBuilder.DeleteData(table: "PONTO_RESIDUO", keyColumns: new[] { "CD_PONTO", "CD_RESIDUO" }, keyValues: new object[] { 1, 2 });
            migrationBuilder.DeleteData(table: "PONTO_RESIDUO", keyColumns: new[] { "CD_PONTO", "CD_RESIDUO" }, keyValues: new object[] { 1, 3 });
            migrationBuilder.DeleteData(table: "PONTO_RESIDUO", keyColumns: new[] { "CD_PONTO", "CD_RESIDUO" }, keyValues: new object[] { 1, 4 });
            migrationBuilder.DeleteData(table: "PONTO_RESIDUO", keyColumns: new[] { "CD_PONTO", "CD_RESIDUO" }, keyValues: new object[] { 1, 5 });
            // Ponto 2: Papel(1), Pilhas(6), Eletrônico(7)
            migrationBuilder.DeleteData(table: "PONTO_RESIDUO", keyColumns: new[] { "CD_PONTO", "CD_RESIDUO" }, keyValues: new object[] { 2, 1 });
            migrationBuilder.DeleteData(table: "PONTO_RESIDUO", keyColumns: new[] { "CD_PONTO", "CD_RESIDUO" }, keyValues: new object[] { 2, 6 });
            migrationBuilder.DeleteData(table: "PONTO_RESIDUO", keyColumns: new[] { "CD_PONTO", "CD_RESIDUO" }, keyValues: new object[] { 2, 7 });
            // Ponto 3: Papel(1), Plástico(2), Rejeito(8)
            migrationBuilder.DeleteData(table: "PONTO_RESIDUO", keyColumns: new[] { "CD_PONTO", "CD_RESIDUO" }, keyValues: new object[] { 3, 1 });
            migrationBuilder.DeleteData(table: "PONTO_RESIDUO", keyColumns: new[] { "CD_PONTO", "CD_RESIDUO" }, keyValues: new object[] { 3, 2 });
            migrationBuilder.DeleteData(table: "PONTO_RESIDUO", keyColumns: new[] { "CD_PONTO", "CD_RESIDUO" }, keyValues: new object[] { 3, 8 });

            migrationBuilder.DeleteData(table: "PONTO_COLETA", keyColumn: "CD_PONTO", keyValue: 1);
            migrationBuilder.DeleteData(table: "PONTO_COLETA", keyColumn: "CD_PONTO", keyValue: 2);
            migrationBuilder.DeleteData(table: "PONTO_COLETA", keyColumn: "CD_PONTO", keyValue: 3);

            for (int i = 1; i <= 8; i++)
                migrationBuilder.DeleteData(table: "RESIDUO", keyColumn: "CD_RESIDUO", keyValue: i);
        }
    }
}
