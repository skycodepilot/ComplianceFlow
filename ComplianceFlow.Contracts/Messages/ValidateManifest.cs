namespace ComplianceFlow.Contracts.Messages;

public record ValidateManifest
{
    // Default constructor for Serializer
    public ValidateManifest() { }

    // Primary constructor for You
    public ValidateManifest(Guid manifestId, string referenceNumber, string[] htsCodes)
    {
        ManifestId = manifestId;
        ReferenceNumber = referenceNumber;
        HtsCodes = htsCodes;
    }

    public Guid ManifestId { get; init; }
    public string ReferenceNumber { get; init; } = null!;
    public string[] HtsCodes { get; init; } = Array.Empty<string>();
}