using Microsoft.EntityFrameworkCore.Migrations;

namespace ProgrammingCoursesApp.Data.Migrations
{
    public partial class AnswerResult : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Results_Tasks_TaskId",
                table: "Results");

            migrationBuilder.AlterColumn<int>(
                name: "TaskId",
                table: "Results",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserAnswerId",
                table: "Results",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Results_UserAnswerId",
                table: "Results",
                column: "UserAnswerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Results_PossibleAnswers_UserAnswerId",
                table: "Results",
                column: "UserAnswerId",
                principalTable: "PossibleAnswers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Tasks_TaskId",
                table: "Results",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Results_PossibleAnswers_UserAnswerId",
                table: "Results");

            migrationBuilder.DropForeignKey(
                name: "FK_Results_Tasks_TaskId",
                table: "Results");

            migrationBuilder.DropIndex(
                name: "IX_Results_UserAnswerId",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "UserAnswerId",
                table: "Results");

            migrationBuilder.AlterColumn<int>(
                name: "TaskId",
                table: "Results",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Tasks_TaskId",
                table: "Results",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
