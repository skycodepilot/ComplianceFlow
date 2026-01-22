namespace ComplianceFlow.Contracts.Messages;

// "ManifestId" correlates the saga. 
// "ReferenceNumber" is the user's ID (e.g., "SHIP-001").
// "HtsCodes" are the raw codes we need to validate later.
public record ManifestSubmitted
{
    // Default constructor for Serializer
    public ManifestSubmitted() { }

    // Primary constructor for You
    public ManifestSubmitted(Guid manifestId, string referenceNumber, string[] htsCodes)
    {
        ManifestId = manifestId;
        ReferenceNumber = referenceNumber;
        HtsCodes = htsCodes;
    }

    public Guid ManifestId { get; init; }
    public string ReferenceNumber { get; init; } = null!;
    public string[] HtsCodes { get; init; } = Array.Empty<string>();
}