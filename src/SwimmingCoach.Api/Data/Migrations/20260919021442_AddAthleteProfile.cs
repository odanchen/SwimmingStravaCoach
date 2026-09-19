using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace SwimmingCoach.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAthleteProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AthleteProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    TimeZoneId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    PreferredUnits = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    PoolLength = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PoolLengthUnit = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    SwimmingExperience = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    RunningExperience = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    TrainingDaysPerWeek = table.Column<int>(type: "int", nullable: false),
                    TypicalSessionMinutes = table.Column<int>(type: "int", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AthleteProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AthleteProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AthleteProfiles_UserId",
                table: "AthleteProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AthleteProfiles");
        }
    }
}
