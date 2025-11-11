using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMonoStereo.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddArtistCoverImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "CoverImage",
                table: "Artists",
                type: "BLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverImage",
                table: "Artists");
        }
    }
}