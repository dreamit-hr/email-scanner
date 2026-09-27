using EmailScanner.Application.EmailProcessing.WatchedSenders;
using Microsoft.AspNetCore.Mvc;

namespace EmailScanner.Api.Controllers;

[ApiController]
[Route("api/v1/watched-senders")]
public sealed class WatchedSendersController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromServices] ListWatchedSendersFeature feature, CancellationToken cancellationToken)
    {
        var result = await feature.ExecuteAsync(cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, [FromServices] GetWatchedSenderFeature feature, CancellationToken cancellationToken)
    {
        var result = await feature.ExecuteAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWatchedSenderRequest request, [FromServices] CreateWatchedSenderFeature feature, CancellationToken cancellationToken)
    {
        var result = await feature.ExecuteAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWatchedSenderBody body, [FromServices] UpdateWatchedSenderFeature feature, CancellationToken cancellationToken)
    {
        var result = await feature.ExecuteAsync(new UpdateWatchedSenderRequest(id, body.Name, body.EmailDomain, body.IsEnabled), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, [FromServices] DeleteWatchedSenderFeature feature, CancellationToken cancellationToken)
    {
        var result = await feature.ExecuteAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}

public sealed record UpdateWatchedSenderBody(string Name, string EmailDomain, bool IsEnabled);
