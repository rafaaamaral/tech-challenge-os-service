using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TechChallenge.OS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "os_numero_seq");

            migrationBuilder.CreateTable(
                name: "ordem_servico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cliente_id = table.Column<int>(type: "integer", nullable: false),
                    veiculo_id = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    unique_code = table.Column<Guid>(type: "uuid", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    criado_por = table.Column<Guid>(type: "uuid", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_por = table.Column<Guid>(type: "uuid", nullable: true),
                    data_alteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordem_servico", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ordem_servico_item_peca_insumo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ordem_servico_id = table.Column<int>(type: "integer", nullable: false),
                    peca_insumo_id = table.Column<int>(type: "integer", nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: false),
                    unique_code = table.Column<Guid>(type: "uuid", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    criado_por = table.Column<Guid>(type: "uuid", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_por = table.Column<Guid>(type: "uuid", nullable: true),
                    data_alteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordem_servico_item_peca_insumo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ordem_servico_item_peca_insumo_ordem_servico_ordem_servico_~",
                        column: x => x.ordem_servico_id,
                        principalTable: "ordem_servico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ordem_servico_item_servico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ordem_servico_id = table.Column<int>(type: "integer", nullable: false),
                    servico_id = table.Column<int>(type: "integer", nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    unique_code = table.Column<Guid>(type: "uuid", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    criado_por = table.Column<Guid>(type: "uuid", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_por = table.Column<Guid>(type: "uuid", nullable: true),
                    data_alteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordem_servico_item_servico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ordem_servico_item_servico_ordem_servico_ordem_servico_id",
                        column: x => x.ordem_servico_id,
                        principalTable: "ordem_servico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ordem_servico_numero",
                table: "ordem_servico",
                column: "numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ordem_servico_unique_code",
                table: "ordem_servico",
                column: "unique_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ordem_servico_item_peca_insumo_ordem_servico_id",
                table: "ordem_servico_item_peca_insumo",
                column: "ordem_servico_id");

            migrationBuilder.CreateIndex(
                name: "IX_ordem_servico_item_peca_insumo_unique_code",
                table: "ordem_servico_item_peca_insumo",
                column: "unique_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ordem_servico_item_servico_ordem_servico_id",
                table: "ordem_servico_item_servico",
                column: "ordem_servico_id");

            migrationBuilder.CreateIndex(
                name: "IX_ordem_servico_item_servico_unique_code",
                table: "ordem_servico_item_servico",
                column: "unique_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ordem_servico_item_peca_insumo");

            migrationBuilder.DropTable(
                name: "ordem_servico_item_servico");

            migrationBuilder.DropTable(
                name: "ordem_servico");

            migrationBuilder.DropSequence(
                name: "os_numero_seq");
        }
    }
}
