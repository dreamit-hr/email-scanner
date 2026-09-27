using EmailScanner.Application.Abstractions;
using EmailScanner.Application.MailboxConnections;
using EmailScanner.Application.MailboxConnections.GetMailboxConnection;
using EmailScanner.Application.MailboxConnections.GetMailboxConnections;
using EmailScanner.Application.MailboxConnections.UpdateMailboxConnection;
using EmailScanner.Application.MailboxConnections.DeleteMailboxConnection;
using EmailScanner.Application.MailboxConnections.EnableMailboxConnection;
using EmailScanner.Application.MailboxConnections.DisableMailboxConnection;
using Microsoft.AspNetCore.Mvc;

namespace EmailScanner.Api.Controllers;

[ApiController]
[Route("api/v1/mailbox-connections")]
public sealed class MailboxConnectionsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromServices] GetMailboxConnectionsFeature feature, CancellationToken cancellationToken)
    {
        var result = await feature.ExecuteAsync(new GetMailboxConnectionsRequest(), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, [FromServices] GetMailboxConnectionFeature feature, CancellationToken cancellationToken)
    {
        var result = await feature.ExecuteAsync(new GetMailboxConnectionRequest(id), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public Task<IActionResult> Create(
        [FromBody] CreateMailboxConnectionRequest request,
        [FromServices] CreateMailboxConnectionFeature feature,
        CancellationToken cancellationToken) => ExecuteAsync(feature, request, cancellationToken);

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] MailboxConnectionUpdateBody body, [FromServices] UpdateMailboxConnectionFeature feature, CancellationToken cancellationToken)
    {
        var request = new UpdateMailboxConnectionRequest(id, body.DisplayName, body.Folder, body.SyncMode, body.ImapHost, body.ImapPort, body.ImapUsername, body.ImapCredentialReference);
        var result = await feature.ExecuteAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, [FromServices] DeleteMailboxConnectionFeature feature, CancellationToken cancellationToken)
    {
        var result = await feature.ExecuteAsync(new DeleteMailboxConnectionRequest(id), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{id:guid}/enable")]
    public async Task<IActionResult> Enable(Guid id, [FromServices] EnableMailboxConnectionFeature feature, CancellationToken cancellationToken)
    {
        var result = await feature.ExecuteAsync(new EnableMailboxConnectionRequest(id), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{id:guid}/disable")]
    public async Task<IActionResult> Disable(Guid id, [FromServices] DisableMailboxConnectionFeature feature, CancellationToken cancellationToken)
    {
        var result = await feature.ExecuteAsync(new DisableMailboxConnectionRequest(id), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    private async Task<IActionResult> ExecuteAsync(IFeature<CreateMailboxConnectionRequest, Guid> feature, CreateMailboxConnectionRequest request, CancellationToken cancellationToken)
    {
        var result = await feature.ExecuteAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
