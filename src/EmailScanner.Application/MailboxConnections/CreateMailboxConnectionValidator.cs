using FluentValidation;

namespace EmailScanner.Application.MailboxConnections;

public sealed class CreateMailboxConnectionValidator : AbstractValidator<CreateMailboxConnectionRequest>
{
    public CreateMailboxConnectionValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EmailAddress).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.DisplayName).MaximumLength(256);
        RuleFor(x => x.Folder).NotEmpty().MaximumLength(512);
        RuleFor(x => x.Provider).IsInEnum();
    }
}
