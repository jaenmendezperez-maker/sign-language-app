using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SignLanguageApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SignLinkInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.CheckConstraint("CK_Users_Role", "\"Role\" IN ('Administrator','MedicalStaff')");
                });

            migrationBuilder.CreateTable(
                name: "Consultations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consultations", x => x.Id);
                    table.CheckConstraint("CK_Consultations_Status", "\"Status\" IN ('Active','Paused','Finished')");
                    table.ForeignKey(
                        name: "FK_Consultations_Users_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Signs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Word = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    HandCount = table.Column<short>(type: "smallint", nullable: false),
                    VisualUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signs", x => x.Id);
                    table.CheckConstraint("CK_Signs_HandCount", "\"HandCount\" IN (1, 2)");
                    table.ForeignKey(
                        name: "FK_Signs_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Signs_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GeometricReferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SignId = table.Column<Guid>(type: "uuid", nullable: false),
                    Distances = table.Column<string>(type: "jsonb", nullable: false),
                    Angles = table.Column<string>(type: "jsonb", nullable: false),
                    NormalizedLandmarks = table.Column<string>(type: "jsonb", nullable: false),
                    SampleCount = table.Column<short>(type: "smallint", nullable: false),
                    CapturedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CapturedById = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeometricReferences", x => x.Id);
                    table.CheckConstraint("CK_GeometricReferences_SampleCount", "\"SampleCount\" >= 5");
                    table.ForeignKey(
                        name: "FK_GeometricReferences_Signs_SignId",
                        column: x => x.SignId,
                        principalTable: "Signs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeometricReferences_Users_CapturedById",
                        column: x => x.CapturedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TranslationRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SignId = table.Column<Guid>(type: "uuid", nullable: true),
                    Word = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    RecognizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SimilarityScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    CertaintyLevel = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranslationRecords", x => x.Id);
                    table.CheckConstraint("CK_TranslationRecords_Level", "\"CertaintyLevel\" IN ('High','Medium','Low')");
                    table.CheckConstraint("CK_TranslationRecords_NotRecognized", "\"Status\" <> 'NotRecognized' OR (\"SignId\" IS NULL AND \"Word\" IS NULL)");
                    table.CheckConstraint("CK_TranslationRecords_Score", "\"SimilarityScore\" BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_TranslationRecords_Status", "\"Status\" IN ('Accepted','Pending','Confirmed','NotConfirmed','Discarded','NotRecognized')");
                    table.ForeignKey(
                        name: "FK_TranslationRecords_Consultations_ConsultationId",
                        column: x => x.ConsultationId,
                        principalTable: "Consultations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TranslationRecords_Signs_SignId",
                        column: x => x.SignId,
                        principalTable: "Signs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_DoctorId_StartedAt",
                table: "Consultations",
                columns: new[] { "DoctorId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GeometricReferences_CapturedById",
                table: "GeometricReferences",
                column: "CapturedById");

            migrationBuilder.CreateIndex(
                name: "IX_GeometricReferences_SignId",
                table: "GeometricReferences",
                column: "SignId");

            migrationBuilder.CreateIndex(
                name: "IX_Signs_CategoryId",
                table: "Signs",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Signs_CreatedById",
                table: "Signs",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Signs_Word",
                table: "Signs",
                column: "Word",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TranslationRecords_ConsultationId_RecognizedAt",
                table: "TranslationRecords",
                columns: new[] { "ConsultationId", "RecognizedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TranslationRecords_SignId",
                table: "TranslationRecords",
                column: "SignId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeometricReferences");

            migrationBuilder.DropTable(
                name: "TranslationRecords");

            migrationBuilder.DropTable(
                name: "Consultations");

            migrationBuilder.DropTable(
                name: "Signs");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
