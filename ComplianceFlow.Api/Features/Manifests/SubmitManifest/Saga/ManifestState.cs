using MassTransit;

namespace ComplianceFlow.Api.Features.Manifests.SubmitManifest.Saga;

// 1. Must implement SagaStateMachineInstance
// This tells MassTransit: "This class can be saved to a DB and tracked."
public class ManifestState : SagaStateMachineInstance
{
    // The Unique ID (This matches the ManifestId we generated in the Controller)
    public Guid CorrelationId { get; set; }

    // The "Cursor" (Where are we? "Initial", "Received", "Validated", "Failed")
    public string CurrentState { get; set; } = null!;

    // Business Data we want to remember (so we don't have to look it up constantly)
    public string ReferenceNumber { get; set; } = null!;
    
    // Audit timestamps are "Senior Dev" candy. Marcus loves them.
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Note: We don't store the full list of HTS codes here unless we need them for routing.
    // Ideally, the Saga just orchestrates; it doesn't hoard data.
}