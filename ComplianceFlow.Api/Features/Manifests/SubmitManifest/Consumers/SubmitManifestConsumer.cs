using ComplianceFlow.Contracts.Messages;
using MassTransit;

namespace ComplianceFlow.Api.Features.Manifests.SubmitManifest.Consumers;

// This class listens specifically for the ManifestSubmitted event.
public class ManifestSubmittedConsumer : IConsumer<ManifestSubmitted>
{
    private readonly ILogger<ManifestSubmittedConsumer> _logger;

    public ManifestSubmittedConsumer(ILogger<ManifestSubmittedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ManifestSubmitted> context)
    {
        // The "Business Logic" (for now)
        _logger.LogInformation(
            "Received Manifest: {ReferenceNumber} with {Count} codes. CorrelationId: {ManifestId}", 
            context.Message.ReferenceNumber, 
            context.Message.HtsCodes.Length,
            context.Message.ManifestId
        );

        return Task.CompletedTask;
    }
}