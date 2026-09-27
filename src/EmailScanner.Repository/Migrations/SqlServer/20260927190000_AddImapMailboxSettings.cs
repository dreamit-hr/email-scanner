using EmailScanner.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailScanner.Repository.Migrations.SqlServer;

[DbContext(typeof(EmailScannerDbContext))]
[Migration("20260927190000_AddImapMailboxSettings")]
public partial class AddImapMailboxSettings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "ImapCredentialReference", table: "MailboxConnections", type: "nvarchar(512)", maxLength: 512, nullable: true);
        migrationBuilder.AddColumn<string>(name: "ImapHost", table: "MailboxConnections", type: "nvarchar(255)", maxLength: 255, nullable: true);
        migrationBuilder.AddColumn<int>(name: "ImapPort", table: "MailboxConnections", type: "int", nullable: true);
        migrationBuilder.AddColumn<string>(name: "ImapUsername", table: "MailboxConnections", type: "nvarchar(320)", maxLength: 320, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ImapCredentialReference", table: "MailboxConnections");
        migrationBuilder.DropColumn(name: "ImapHost", table: "MailboxConnections");
        migrationBuilder.DropColumn(name: "ImapPort", table: "MailboxConnections");
        migrationBuilder.DropColumn(name: "ImapUsername", table: "MailboxConnections");
    }
}
