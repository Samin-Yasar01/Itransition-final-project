using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormsApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTemplateAccessDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemplateAccesses_AspNetUsers_UserId",
                table: "TemplateAccesses");

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateAccesses_AspNetUsers_UserId",
                table: "TemplateAccesses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemplateAccesses_AspNetUsers_UserId",
                table: "TemplateAccesses");

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateAccesses_AspNetUsers_UserId",
                table: "TemplateAccesses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
