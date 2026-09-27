using FluentValidation;

namespace EmailScanner.Application.MailboxConnections.UpdateMailboxConnection;

public sealed class UpdateMailboxConnectionValidator : AbstractValidator<UpdateMailboxConnectionRequest>
{
    public UpdateMailboxConnectionValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.DisplayName).MaximumLength(256);
        RuleFor(x => x.Folder).NotEmpty().MaximumLength(512);
        RuleFor(x => x.SyncMode).IsInEnum();
        RuleFor(x => x.ImapHost).MaximumLength(255);
        RuleFor(x => x.ImapPort).InclusiveBetween(1, 65535).When(x => x.ImapPort.HasValue);
        RuleFor(x => x.ImapUsername).MaximumLength(320);
        RuleFor(x => x.ImapCredentialReference).MaximumLength(512);
    }
}
