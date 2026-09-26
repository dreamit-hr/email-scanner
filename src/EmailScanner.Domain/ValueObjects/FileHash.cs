namespace EmailScanner.Domain.ValueObjects;

public sealed record FileHash
{
    private FileHash(string algorithm, string value) { Algorithm = algorithm; Value = value; }
    public string Algorithm { get; }
    public string Value { get; }
    public static FileHash Create(string algorithm, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(algorithm);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var expectedLength = algorithm.ToUpperInvariant() switch { "SHA256" => 64, "SHA512" => 128, "MD5" => 32, _ => throw new ArgumentException("Hash algorithm is not supported.", nameof(algorithm)) };
        if (value.Length != expectedLength || value.Any(x => !Uri.IsHexDigit(x))) throw new ArgumentException("Hash must be a hexadecimal digest of the selected algorithm.", nameof(value));
        return new FileHash(algorithm.ToUpperInvariant(), value.ToLowerInvariant());
    }
}
