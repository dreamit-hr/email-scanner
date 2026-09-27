using EmailScanner.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailScanner.Repository.Migrations.SqlServer;

[DbContext(typeof(EmailScannerDbContext))]
[Migration("20260927200000_AddEmailClassification")]
public partial class AddEmailClassification : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "WatchedSenders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                EmailDomain = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                IsEnabled = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_WatchedSenders", x => x.Id));

        migrationBuilder.CreateTable(
            name: "EmailExtractedDocuments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EmailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Category = table.Column<int>(type: "int", nullable: false),
                DocumentType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Confidence = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                Summary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EmailExtractedDocuments", x => x.Id);
                table.ForeignKey("FK_EmailExtractedDocuments_Emails_EmailId", x => x.EmailId, "Emails", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "EmailExtractedEntities",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EmailExtractedDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                Confidence = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EmailExtractedEntities", x => x.Id);
                table.ForeignKey("FK_EmailExtractedEntities_EmailExtractedDocuments_EmailExtractedDocumentId", x => x.EmailExtractedDocumentId, "EmailExtractedDocuments", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_WatchedSenders_TenantId_EmailDomain", table: "WatchedSenders", columns: new[] { "TenantId", "EmailDomain" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_EmailExtractedDocuments_EmailId", table: "EmailExtractedDocuments", column: "EmailId", unique: true);
        migrationBuilder.CreateIndex(name: "IX_EmailExtractedEntities_EmailExtractedDocumentId", table: "EmailExtractedEntities", column: "EmailExtractedDocumentId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "EmailExtractedEntities");
        migrationBuilder.DropTable(name: "WatchedSenders");
        migrationBuilder.DropTable(name: "EmailExtractedDocuments");
    }
}
