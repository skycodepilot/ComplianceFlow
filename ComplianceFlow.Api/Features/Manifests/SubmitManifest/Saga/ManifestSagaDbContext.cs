using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplianceFlow.Api.Features.Manifests.SubmitManifest.Saga;

// This isn't your main AppDbContext. This is SPECIALIZED for Sagas.
// CQRS: We separate the "Orchestration DB" from the "Query DB" logic.
public class ManifestSagaDbContext : SagaDbContext
{
    public ManifestSagaDbContext(DbContextOptions<ManifestSagaDbContext> options)
        : base(options)
    {
    }

    protected override IEnumerable<ISagaClassMap> Configurations
    {
        get { yield return new ManifestStateMap(); }
    }
}

// The Map tells EF how to build the table.
public class ManifestStateMap : SagaClassMap<ManifestState>
{
    protected override void Configure(EntityTypeBuilder<ManifestState> entity, ModelBuilder model)
    {
        base.Configure(entity, model);
        
        // Customize column widths if needed
        entity.Property(x => x.CurrentState).HasMaxLength(64);
        entity.Property(x => x.ReferenceNumber).HasMaxLength(256);
    }
}