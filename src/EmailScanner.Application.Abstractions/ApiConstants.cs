namespace EmailScanner.Application.Abstractions;

public static class HeaderNames
{
    public const string ClientId = "X-Client-Id";
    public const string ClientSecret = "X-Client-Secret";
    public const string CorrelationId = "X-Correlation-Id";
}

public static class ClaimNames
{
    public const string ClientId = "client_id";
    public const string TenantId = "tenant_id";
    public const string Permission = "permission";
}

public static class Permissions
{
    public const string MailboxRead = "mailboxes:read";
    public const string MailboxWrite = "mailboxes:write";
    public const string EmailRead = "emails:read";
    public const string EmailWrite = "emails:write";
    public const string RulesWrite = "rules:write";
    public const string WebhooksWrite = "webhooks:write";
}
