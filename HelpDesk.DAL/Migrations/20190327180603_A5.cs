using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HelpDesk.DAL.Migrations
{
    public partial class A5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActivationCode",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarPath",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TechnicianStatus",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Surveys",
                columns: table => new
                {
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    CreatedUserId = table.Column<string>(maxLength: 450, nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUserId = table.Column<string>(maxLength: 450, nullable: true),
                    Id = table.Column<string>(nullable: false),
                    Satisfaction = table.Column<double>(nullable: false),
                    TechPoint = table.Column<double>(nullable: false),
                    Speed = table.Column<double>(nullable: false),
                    Pricing = table.Column<double>(nullable: false),
                    Solving = table.Column<double>(nullable: false),
                    Suggestions = table.Column<string>(maxLength: 200, nullable: true),
                    IsDone = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Surveys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Failures",
                columns: table => new
                {
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    CreatedUserId = table.Column<string>(maxLength: 450, nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUserId = table.Column<string>(maxLength: 450, nullable: true),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FailureName = table.Column<string>(maxLength: 100, nullable: false),
                    ProductModel = table.Column<string>(maxLength: 100, nullable: false),
                    Description = table.Column<string>(maxLength: 300, nullable: false),
                    OperationStatus = table.Column<int>(nullable: false),
                    OperationTime = table.Column<DateTime>(nullable: true),
                    RepairProcess = table.Column<int>(nullable: true),
                    StartingTime = table.Column<DateTime>(nullable: true),
                    FinishingTime = table.Column<DateTime>(nullable: true),
                    Address = table.Column<string>(maxLength: 100, nullable: false),
                    Latitude = table.Column<double>(nullable: true),
                    Longitude = table.Column<double>(nullable: true),
                    Price = table.Column<decimal>(nullable: false),
                    HasWarranty = table.Column<bool>(nullable: false),
                    Report = table.Column<string>(nullable: true),
                    ClientId = table.Column<string>(nullable: true),
                    TechnicianId = table.Column<string>(nullable: true),
                    OperatorId = table.Column<string>(nullable: true),
                    SurveyId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Failures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Failures_AspNetUsers_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Failures_AspNetUsers_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Failures_Surveys_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Surveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Failures_AspNetUsers_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Operations",
                columns: table => new
                {
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    CreatedUserId = table.Column<string>(maxLength: 450, nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUserId = table.Column<string>(maxLength: 450, nullable: true),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Message = table.Column<string>(nullable: true),
                    FromWhom = table.Column<int>(nullable: false),
                    FailureId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Operations_Failures_FailureId",
                        column: x => x.FailureId,
                        principalTable: "Failures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    CreatedUserId = table.Column<string>(maxLength: 450, nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUserId = table.Column<string>(maxLength: 450, nullable: true),
                    Id = table.Column<string>(nullable: false),
                    Path = table.Column<string>(nullable: false),
                    FailureId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Photos_Failures_FailureId",
                        column: x => x.FailureId,
                        principalTable: "Failures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Failures_ClientId",
                table: "Failures",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Failures_OperatorId",
                table: "Failures",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Failures_SurveyId",
                table: "Failures",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_Failures_TechnicianId",
                table: "Failures",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_FailureId",
                table: "Operations",
                column: "FailureId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_FailureId",
                table: "Photos",
                column: "FailureId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Operations");

            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropTable(
                name: "Failures");

            migrationBuilder.DropTable(
                name: "Surveys");

            migrationBuilder.DropColumn(
                name: "ActivationCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AvatarPath",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TechnicianStatus",
                table: "AspNetUsers");
        }
    }
}
