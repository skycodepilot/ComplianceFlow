namespace ComplianceFlow.Contracts.Messages;

public record ManifestInvalid(
    Guid ManifestId, 
    string Reason
);