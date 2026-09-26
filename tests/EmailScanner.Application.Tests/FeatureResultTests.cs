using EmailScanner.Application.Abstractions;
using FluentAssertions;
using Xunit;

namespace EmailScanner.Application.Tests;

public sealed class FeatureResultTests
{
    [Fact]
    public void NotFound_uses_http_status_and_error_code()
    {
        var result = FeatureResult<string>.NotFound();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("not_found");
    }
}
