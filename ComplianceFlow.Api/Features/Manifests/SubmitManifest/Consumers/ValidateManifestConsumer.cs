using ComplianceFlow.Contracts.Messages;
using MassTransit;

namespace ComplianceFlow.Api.Features.Manifests.SubmitManifest.Consumers;

public class ValidateManifestConsumer : IConsumer<ValidateManifest>
{
    private readonly ILogger<ValidateManifestConsumer> _logger;

    public ValidateManifestConsumer(ILogger<ValidateManifestConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ValidateManifest> context)
    {
        var message = context.Message;
        _logger.LogInformation("Validating Manifest: {Ref}", message.ReferenceNumber);

        // 1. The Logic (Simulated)
        // If the manifest contains the "Forbidden Code", we reject it.
        bool hasRestrictedItem = message.HtsCodes.Contains("9999.99"); // The "Bad" Code

        if (hasRestrictedItem)
        {
            _logger.LogWarning("Manifest {Id} Rejected: Restricted Item found.", message.ManifestId);
            
            // 2. Publish Failure
            await context.Publish(new ManifestInvalid(
                message.ManifestId, 
                "Contains Restricted HTS Code: 9999.99"
            ));
        }
        else
        {
            _logger.LogInformation("Manifest {Id} Validated Successfully.", message.ManifestId);
            
            // 3. Publish Success
            await context.Publish(new ManifestValid(message.ManifestId));
        }
    }
}