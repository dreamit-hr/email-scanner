namespace EmailScanner.Domain.ValueObjects;

public sealed record MimeContentType
{
    private MimeContentType(string value) => Value = value;
    public string Value { get; }
    public static MimeContentType Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim().ToLowerInvariant();
        var parts = normalized.Split('/', StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || parts.Any(string.IsNullOrWhiteSpace) || normalized.Any(char.IsControl)) throw new ArgumentException("MIME content type is invalid.", nameof(value));
        return new MimeContentType(normalized);
    }
}
