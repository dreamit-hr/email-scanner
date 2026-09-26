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
    }
}
