using EmailScanner.Domain;

namespace EmailScanner.Application.EmailProcessing.Classification;

public static class WatchedSenderMatcher
{
    public static bool Matches(string senderAddress, string watchedDomain)
    {
        if (string.IsNullOrWhiteSpace(senderAddress) || string.IsNullOrWhiteSpace(watchedDomain)) return false;
        var at = senderAddress.LastIndexOf('@');
        if (at < 0 || at == senderAddress.Length - 1) return false;
        string domain;
        try
        {
            domain = WatchedSender.NormalizeDomain(watchedDomain);
        }
        catch (ArgumentException)
        {
            return false;
        }

        var senderDomain = senderAddress[(at + 1)..].TrimEnd('.');
        return senderDomain.Equals(domain, StringComparison.OrdinalIgnoreCase)
            || senderDomain.EndsWith($".{domain}", StringComparison.OrdinalIgnoreCase);
    }
}
