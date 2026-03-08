using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMonoStereo.Core.Migrations
{
    /// <inheritdoc />
    public partial class ChangeIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tracks_AlbumId",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_TrackNumber",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Artists_DateAdded",
                table: "Artists");

            migrationBuilder.DropIndex(
                name: "IX_Albums_Year",
                table: "Albums");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tracks",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Artists",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Albums",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_AlbumId_TrackNumber",
                table: "Tracks",
                columns: new[] { "AlbumId", "TrackNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Albums_Rating",
                table: "Albums",
                column: "Rating");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tracks_AlbumId_TrackNumber",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Albums_Rating",
                table: "Albums");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tracks",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200,
                oldCollation: "NOCASE");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Artists",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200,
                oldCollation: "NOCASE");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Albums",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200,
                oldCollation: "NOCASE");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_AlbumId",
                table: "Tracks",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_TrackNumber",
                table: "Tracks",
                column: "TrackNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_DateAdded",
                table: "Artists",
                column: "DateAdded");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_Year",
                table: "Albums",
                column: "Year");
        }
    }
}
