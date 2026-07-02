using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AziendeDati.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Aziende",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RAG_SOC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    P_IVA = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    DataRegistrazione = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aziende", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categorie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descrizione = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorie", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ruoli",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ruoli", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ordini",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AziendaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ordini", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ordini_Aziende_AziendaId",
                        column: x => x.AziendaId,
                        principalTable: "Aziende",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dati",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AziendaId = table.Column<int>(type: "int", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dati", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dati_Aziende_AziendaId",
                        column: x => x.AziendaId,
                        principalTable: "Aziende",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dati_Categorie_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Utenti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Attivo = table.Column<bool>(type: "bit", nullable: false),
                    AziendaId = table.Column<int>(type: "int", nullable: false),
                    RuoloId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utenti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Utenti_Aziende_AziendaId",
                        column: x => x.AziendaId,
                        principalTable: "Aziende",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Utenti_Ruoli_RuoloId",
                        column: x => x.RuoloId,
                        principalTable: "Ruoli",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RigheOrdine",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdineId = table.Column<int>(type: "int", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    Descrizione = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantita = table.Column<int>(type: "int", nullable: false),
                    PrezzoUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RigheOrdine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RigheOrdine_Categorie_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RigheOrdine_Ordini_OrdineId",
                        column: x => x.OrdineId,
                        principalTable: "Ordini",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Aziende",
                columns: new[] { "Id", "DataRegistrazione", "P_IVA", "RAG_SOC" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "01234567890", "ACME S.p.A." },
                    { 2, new DateTime(2024, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "09876543210", "Globex S.r.l." }
                });

            migrationBuilder.InsertData(
                table: "Categorie",
                columns: new[] { "Id", "Descrizione", "Nome" },
                values: new object[,]
                {
                    { 1, "Temperature rilevate dai sensori, in gradi Celsius", "Temperatura" },
                    { 2, "Pressioni di esercizio degli impianti, in bar", "Pressione" },
                    { 3, "Umidità relativa ambientale, in percentuale", "Umidità" },
                    { 4, "Consumi elettrici degli impianti, in kWh", "Consumo energetico" }
                });

            migrationBuilder.InsertData(
                table: "Ruoli",
                columns: new[] { "Id", "Nome" },
                values: new object[,]
                {
                    { 1, "data.company.owner" },
                    { 2, "data.company.reader" }
                });

            migrationBuilder.InsertData(
                table: "Dati",
                columns: new[] { "Id", "AziendaId", "CategoriaId", "Timestamp", "Value" },
                values: new object[,]
                {
                    { 1, 1, 1, new DateTime(2025, 5, 10, 8, 0, 0, 0, DateTimeKind.Unspecified), 21.50m },
                    { 2, 1, 1, new DateTime(2025, 5, 10, 14, 0, 0, 0, DateTimeKind.Unspecified), 22.30m },
                    { 3, 1, 2, new DateTime(2025, 5, 10, 8, 0, 0, 0, DateTimeKind.Unspecified), 1.75m },
                    { 4, 1, 3, new DateTime(2025, 5, 11, 8, 0, 0, 0, DateTimeKind.Unspecified), 45.00m },
                    { 5, 1, 4, new DateTime(2025, 5, 11, 8, 0, 0, 0, DateTimeKind.Unspecified), 120.40m },
                    { 6, 2, 1, new DateTime(2025, 5, 10, 8, 0, 0, 0, DateTimeKind.Unspecified), 19.80m },
                    { 7, 2, 2, new DateTime(2025, 5, 10, 9, 30, 0, 0, DateTimeKind.Unspecified), 2.10m },
                    { 8, 2, 3, new DateTime(2025, 5, 11, 10, 0, 0, 0, DateTimeKind.Unspecified), 55.20m },
                    { 9, 2, 4, new DateTime(2025, 5, 11, 10, 0, 0, 0, DateTimeKind.Unspecified), 98.75m },
                    { 10, 2, 1, new DateTime(2025, 5, 12, 8, 0, 0, 0, DateTimeKind.Unspecified), 23.10m }
                });

            migrationBuilder.InsertData(
                table: "Ordini",
                columns: new[] { "Id", "AziendaId", "Data", "Numero" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ORD-2025-001" },
                    { 2, 2, new DateTime(2025, 4, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "ORD-2025-002" }
                });

            migrationBuilder.InsertData(
                table: "Utenti",
                columns: new[] { "Id", "Attivo", "AziendaId", "Email", "RuoloId", "Username" },
                values: new object[,]
                {
                    { 1, true, 1, "mario.rossi@acme.it", 1, "mario.rossi" },
                    { 2, true, 1, "laura.bianchi@acme.it", 2, "laura.bianchi" },
                    { 3, true, 2, "giulia.verdi@globex.it", 1, "giulia.verdi" },
                    { 4, true, 2, "paolo.neri@globex.it", 2, "paolo.neri" }
                });

            migrationBuilder.InsertData(
                table: "RigheOrdine",
                columns: new[] { "Id", "CategoriaId", "Descrizione", "OrdineId", "PrezzoUnitario", "Quantita" },
                values: new object[,]
                {
                    { 1, 1, "Sensore temperatura TX-100", 1, 79.90m, 5 },
                    { 2, 2, "Sensore pressione PX-20", 1, 149.50m, 2 },
                    { 3, 3, "Igrometro ambientale HX-5", 2, 59.00m, 3 },
                    { 4, 4, "Contatore energia EX-1", 2, 320.00m, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aziende_P_IVA",
                table: "Aziende",
                column: "P_IVA",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categorie_Nome",
                table: "Categorie",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dati_AziendaId",
                table: "Dati",
                column: "AziendaId");

            migrationBuilder.CreateIndex(
                name: "IX_Dati_CategoriaId",
                table: "Dati",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ordini_AziendaId",
                table: "Ordini",
                column: "AziendaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ordini_Numero",
                table: "Ordini",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RigheOrdine_CategoriaId",
                table: "RigheOrdine",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_RigheOrdine_OrdineId",
                table: "RigheOrdine",
                column: "OrdineId");

            migrationBuilder.CreateIndex(
                name: "IX_Ruoli_Nome",
                table: "Ruoli",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utenti_AziendaId",
                table: "Utenti",
                column: "AziendaId");

            migrationBuilder.CreateIndex(
                name: "IX_Utenti_RuoloId",
                table: "Utenti",
                column: "RuoloId");

            migrationBuilder.CreateIndex(
                name: "IX_Utenti_Username",
                table: "Utenti",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dati");

            migrationBuilder.DropTable(
                name: "RigheOrdine");

            migrationBuilder.DropTable(
                name: "Utenti");

            migrationBuilder.DropTable(
                name: "Categorie");

            migrationBuilder.DropTable(
                name: "Ordini");

            migrationBuilder.DropTable(
                name: "Ruoli");

            migrationBuilder.DropTable(
                name: "Aziende");
        }
    }
}
