using Microsoft.EntityFrameworkCore.Migrations;

namespace ProgrammingCoursesApp.Data.Migrations
{
    public partial class TopicBlockForeignKeyForTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TopicBlocks_Tasks_TaskId",
                table: "TopicBlocks");

            migrationBuilder.DropIndex(
                name: "IX_TopicBlocks_TaskId",
                table: "TopicBlocks");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "TopicBlocks");

            migrationBuilder.AddColumn<int>(
                name: "TopicBlockId",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TopicBlockId",
                table: "Tasks",
                column: "TopicBlockId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_TopicBlocks_TopicBlockId",
                table: "Tasks",
                column: "TopicBlockId",
                principalTable: "TopicBlocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_TopicBlocks_TopicBlockId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_TopicBlockId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TopicBlockId",
                table: "Tasks");

            migrationBuilder.AddColumn<int>(
                name: "TaskId",
                table: "TopicBlocks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TopicBlocks_TaskId",
                table: "TopicBlocks",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_TopicBlocks_Tasks_TaskId",
                table: "TopicBlocks",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
