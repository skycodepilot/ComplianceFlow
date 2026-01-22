using ComplianceFlow.Contracts.Messages;
using MassTransit;

namespace ComplianceFlow.Api.Features.Manifests.SubmitManifest.Saga;

public class ManifestStateMachine : MassTransitStateMachine<ManifestState>
{
    // 1. New States
    public State Validating { get; private set; } // Waiting for the Validator
    public State Validated { get; private set; }  // Success
    public State Rejected { get; private set; }   // Failure

    // 2. New Events
    public Event<ManifestSubmitted> ManifestSubmitted { get; private set; }
    public Event<ManifestValid> ManifestValid { get; private set; }
    public Event<ManifestInvalid> ManifestInvalid { get; private set; }

    public ManifestStateMachine()
    {
        InstanceState(x => x.CurrentState);

        // 3. Correlate Events
        Event(() => ManifestSubmitted, x => x.CorrelateById(m => m.Message.ManifestId));
        Event(() => ManifestValid, x => x.CorrelateById(m => m.Message.ManifestId));
        Event(() => ManifestInvalid, x => x.CorrelateById(m => m.Message.ManifestId));

        // 4. The "Happy Path" Flow
        Initially(
            When(ManifestSubmitted)
                .Then(context =>
                {
                    context.Saga.ReferenceNumber = context.Message.ReferenceNumber;
                    context.Saga.CreatedAt = DateTime.UtcNow;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                // SEND THE COMMAND
                .Publish(context => new ValidateManifest(
                    context.Saga.CorrelationId, 
                    context.Saga.ReferenceNumber, 
                    context.Message.HtsCodes
                ))
                .TransitionTo(Validating) // Move to "Waiting" state
        );

        // 5. Handling Responses
        During(Validating,
            // Scenario A: It passed
            When(ManifestValid)
                .Then(context => 
                {
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .TransitionTo(Validated),

            // Scenario B: It failed
            When(ManifestInvalid)
                .Then(context => 
                {
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    // We could store the "Reason" in the Saga data if we added a property for it.
                })
                .TransitionTo(Rejected)
        );
    }
}