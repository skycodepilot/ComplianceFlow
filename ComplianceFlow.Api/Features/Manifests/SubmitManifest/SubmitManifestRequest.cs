namespace ComplianceFlow.Api.Features.Manifests.SubmitManifest;

// Request doesn't come with a ManifestId (generated server-side to ensure uniqueness)
public record SubmitManifestRequest(
    string ReferenceNumber, 
    string[] HtsCodes
);