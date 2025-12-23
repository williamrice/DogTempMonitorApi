using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TempMonitor.Migrations
{
    /// <inheritdoc />
    public partial class AddWeatherReadings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeatherReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TemperatureReadingId = table.Column<int>(type: "integer", nullable: false),
                    Temperature = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    FeelsLike = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Humidity = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Pressure = table.Column<int>(type: "integer", nullable: false),
                    WeatherMain = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WeatherDescription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    WindSpeed = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    WindDegree = table.Column<int>(type: "integer", nullable: true),
                    Clouds = table.Column<int>(type: "integer", nullable: false),
                    Visibility = table.Column<int>(type: "integer", nullable: true),
                    Sunrise = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Sunset = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeatherReadings_TemperatureReadings_TemperatureReadingId",
                        column: x => x.TemperatureReadingId,
                        principalTable: "TemperatureReadings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeatherReadings_TemperatureReadingId",
                table: "WeatherReadings",
                column: "TemperatureReadingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeatherReadings_Timestamp",
                table: "WeatherReadings",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeatherReadings");
        }
    }
}
