using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechChallenge.OS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMessagingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ordem_servico_compensacao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_servico_unique_code = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordem_servico_compensacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "processed_event",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    routing_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ordem_servico_unique_code = table.Column<Guid>(type: "uuid", nullable: false),
                    resultado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    processado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_event", x => x.event_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ordem_servico_compensacao_ordem_servico_unique_code",
                table: "ordem_servico_compensacao",
                column: "ordem_servico_unique_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ordem_servico_compensacao");

            migrationBuilder.DropTable(
                name: "processed_event");
        }
    }
}
