using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIProduct.Migrations
{
    /// <inheritdoc />
    public partial class @new : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tbl_user_type",
                columns: table => new
                {
                    user_type_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_type_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    user_type_level = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_user_type", x => x.user_type_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tbl_user",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_persaonal_email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_official_email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_phone_no = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_token = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_password = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fk_user_type = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_user", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_tbl_user_tbl_user_type_fk_user_type",
                        column: x => x.fk_user_type,
                        principalTable: "tbl_user_type",
                        principalColumn: "user_type_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "tbl_user_type",
                columns: new[] { "user_type_id", "is_active", "user_type_level", "user_type_name" },
                values: new object[] { new Guid("2382c7f5-f680-40da-a639-ed0638f57c7d"), true, 0, "System Admin" });

            migrationBuilder.InsertData(
                table: "tbl_user",
                columns: new[] { "user_id", "fk_user_type", "user_name", "user_official_email", "user_password", "user_persaonal_email", "user_phone_no", "user_token" },
                values: new object[] { new Guid("5638959b-70eb-40f0-8410-0046b1faaad7"), new Guid("2382c7f5-f680-40da-a639-ed0638f57c7d"), "Administrator", "admin@enexol.com", "WZRHGrsBESr8wYFZ9sx0tPURuZgG2lmzyvWpwXPKz8U=", null, "", "" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_fk_user_type",
                table: "tbl_user",
                column: "fk_user_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_user");

            migrationBuilder.DropTable(
                name: "tbl_user_type");
        }
    }
}
