using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UserForm.Migrations
{
    /// <inheritdoc />
    public partial class modform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Options_Questions_QuestionEntityId",
                table: "Options");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Forms_FormEntityId",
                table: "Questions");

            migrationBuilder.RenameColumn(
                name: "FormEntityId",
                table: "Questions",
                newName: "FormId");

            migrationBuilder.RenameIndex(
                name: "IX_Questions_FormEntityId",
                table: "Questions",
                newName: "IX_Questions_FormId");

            migrationBuilder.RenameColumn(
                name: "Text",
                table: "Options",
                newName: "OptionText");

            migrationBuilder.RenameColumn(
                name: "QuestionEntityId",
                table: "Options",
                newName: "QuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_Options_QuestionEntityId",
                table: "Options",
                newName: "IX_Options_QuestionId");

            migrationBuilder.CreateTable(
                name: "FormResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FormId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormResponses_Forms_FormId",
                        column: x => x.FormId,
                        principalTable: "Forms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuestionId = table.Column<int>(type: "integer", nullable: false),
                    AnswerText = table.Column<string>(type: "text", nullable: true),
                    FormResponseId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Answers_FormResponses_FormResponseId",
                        column: x => x.FormResponseId,
                        principalTable: "FormResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Answers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Answers_FormResponseId",
                table: "Answers",
                column: "FormResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_Answers_QuestionId",
                table: "Answers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_FormResponses_FormId",
                table: "FormResponses",
                column: "FormId");

            migrationBuilder.AddForeignKey(
                name: "FK_Options_Questions_QuestionId",
                table: "Options",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Forms_FormId",
                table: "Questions",
                column: "FormId",
                principalTable: "Forms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Options_Questions_QuestionId",
                table: "Options");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Forms_FormId",
                table: "Questions");

            migrationBuilder.DropTable(
                name: "Answers");

            migrationBuilder.DropTable(
                name: "FormResponses");

            migrationBuilder.RenameColumn(
                name: "FormId",
                table: "Questions",
                newName: "FormEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Questions_FormId",
                table: "Questions",
                newName: "IX_Questions_FormEntityId");

            migrationBuilder.RenameColumn(
                name: "QuestionId",
                table: "Options",
                newName: "QuestionEntityId");

            migrationBuilder.RenameColumn(
                name: "OptionText",
                table: "Options",
                newName: "Text");

            migrationBuilder.RenameIndex(
                name: "IX_Options_QuestionId",
                table: "Options",
                newName: "IX_Options_QuestionEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Options_Questions_QuestionEntityId",
                table: "Options",
                column: "QuestionEntityId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Forms_FormEntityId",
                table: "Questions",
                column: "FormEntityId",
                principalTable: "Forms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
