using ComplianceFlow.Api.Features.Manifests.SubmitManifest.Saga;
using ComplianceFlow.Contracts.Messages;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComplianceFlow.Api.Features.Manifests.SubmitManifest;

[ApiController]
[Route("api/manifests")]
public class SubmitManifestController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;

    // We inject IPublishEndpoint. 
    // This is MassTransit's way of saying "I don't know who is listening, and I don't care."
    public SubmitManifestController(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }
    
    // The "Query" side of CQRS
    // In a real app, this might be a separate "ManifestQueryController"
    [HttpGet("{manifestId}")]
    public async Task<IActionResult> GetStatus(Guid manifestId, [FromServices] ManifestSagaDbContext db)
    {
        // Direct Query to the Saga Table
        // We use AsNoTracking() because we are just reading, not modifying.
        var state = await db.Set<ManifestState>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CorrelationId == manifestId);

        if (state == null) return NotFound();

        return Ok(state);
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitManifestRequest request)
    {
        if (request == null) return BadRequest("Payload is null"); // Safety check
        
        // 1. Create the Correlation ID (The "Saga ID")
        var manifestId = Guid.NewGuid();

        // 2. Publish the Event (Fire and Forget)
        // We use the Contract we just defined.
        await _publishEndpoint.Publish(new ManifestSubmitted(
            manifestId,
            request.ReferenceNumber,
            request.HtsCodes
        ));

        // 3. Return 202 Accepted
        // We don't return 200 OK because the work isn't done. 
        // We just promised to *start* it.
        return Accepted(new { ManifestId = manifestId });
    }
}